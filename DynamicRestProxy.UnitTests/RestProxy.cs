using System;
using System.Text;
using System.Dynamic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DynamicRestProxy
{
    /// <summary>
    /// Base proxy class. Derived classes implement the specfic rest/http communication mechanisms.
    /// Each node in a chain of proxy instances represents a specific endpoint
    /// </summary>
    public abstract class RestProxy : DynamicObject
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="parent"><see cref="DynamicRestProxy.RestProxy.Parent"/></param>
        /// <param name="name"><see cref="DynamicRestProxy.RestProxy.Name"/></param>
        protected RestProxy(RestProxy parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        /// <summary>
        /// The parent of this node
        /// </summary>
        public RestProxy Parent { get; private set; }

        /// <summary>
        /// The name of this node. This is the text used when forming the complete Url
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The numeric index of this node in the chain
        /// </summary>
        public int Index
        {
            get
            {
                return Parent != null ? Parent.Index + 1 : -1; // the root is the main url - does not represent a url segment
            }
        }

        /// <summary>
        /// The base Url of the endpoint. Overridden in derived classess to allow specific rest client to deteremine how it is stored
        /// </summary>
        protected abstract string BaseUri { get; }

        /// <summary>
        /// Factory method used to create instances of derived child nodes. Overrideen in derived classess to create derived instances
        /// </summary>
        /// <param name="parent">The parent of the newly created child</param>
        /// <param name="name">The name of the newly created child</param>
        /// <returns>Derived child instance</returns>
        protected abstract RestProxy CreateProxyNode(RestProxy parent, string name);

        /// <summary>
        /// <see cref="System.Dynamic.DynamicObject.TryInvoke(InvokeBinder, object[], out object)"/>
        /// </summary>
        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            Debug.Assert(binder != null);
            Debug.Assert(args != null);

            if (args.Length != 1)
            {
                throw new InvalidOperationException("The segment escape sequence must have exactly 1 unnamed parameter");
            }

            // this is called when the dynamic object is invoked like a delegate
            // dynamic segment1 = proxy.segment1;
            // dynamic chain = segment1("escaped"); <- this calls TryInvoke
            result = CreateProxyNode(this, args[0].ToString());

            return true;
        }

        /// <summary>
        /// Abstract method to create a Task that will execute the necessary http communication
        /// </summary>
        /// <param name="verb">The http verb to execute (must be get, post, put or delete)</param>
        /// <param name="unnamedArgs">Unnamed arguments passed to the invocation. These go into the http request body</param>
        /// <param name="namedArgs">Named arguments supplied to the invocation. These become http request parameters</param>
        /// <returns>Task{dynamic} that will execute the http call and return a dynamic object with the results</returns>
        protected abstract Task<dynamic> CreateVerbAsyncTask(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs);

        /// <summary>
        /// <see cref="System.Dynamic.DynamicObject.TryInvokeMember(InvokeMemberBinder, object[], out object)"/>
        /// </summary>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Debug.Assert(binder != null);
            Debug.Assert(args != null);

            if (binder.IsVerb())
            {
                // parse out the details of the invocation and have the derived class create a Task
                result = CreateVerbAsyncTask(binder.Name, binder.GetUnnamedArgs(args), binder.GetNamedArgs(args));
            }
            else
            {
                if (args.Length != 1)
                    throw new InvalidOperationException("The segment escape sequence must have exactly 1 unnamed parameter");

                // this is for when we escape a url segment by passing it as an argument to a method invocation
                // example: proxy.segment1("escaped")
                // here we create two new dynamic objects, 1 for "segment1" which is the method name
                // and then we create one for the escaped segment passed as an argument - "escaped" in the example
                var tmp = CreateProxyNode(this, binder.Name);
                result = CreateProxyNode(tmp, args[0].ToString());
            }

            return true;
        }

        /// <summary>
        /// <see cref="System.Dynamic.DynamicObject.TryGetMember(GetMemberBinder, out object)"/>
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(binder != null);

            // this gets invoked when a dynamic property is accessed
            // example: proxy.locations will invoke here with a binder named locations
            // each dynamic property is treated as a url segment
            result = CreateProxyNode(this, binder.Name);

            return true;
        }

        /// <summary>
        /// Used to generate a relative Url for the endpoint 
        /// </summary>
        /// <param name="builder"></param>
        protected void GetEndPointPath(StringBuilder builder)
        {
            if (Parent != null)
            {
                Parent.GetEndPointPath(builder); // go all the way up to the root and then back down
            }

            builder.Append(Name).Append("/");
        }

        /// <summary>
        /// The relative Url (minus parameters) for this endpoint
        /// </summary>
        /// <returns>The relative part of the url (relative to <see cref="DynamicRestProxy.RestProxy.BaseUri"/>)</returns>
        public string GetEndPointPath()
        {
            var builder = new StringBuilder();
            GetEndPointPath(builder);
            return builder.ToString().Trim('/');
        }

        /// <summary>
        /// <see cref="System.Object.ToString"/>
        /// </summary>
        /// <returns>The full Url of this node in the path chain</returns>
        public override string ToString()
        {
            if (BaseUri.EndsWith("/"))
            {
                return BaseUri + GetEndPointPath();
            }

            return BaseUri + "/" + GetEndPointPath();
        }

        /// <summary>
        /// <see cref="System.Object.GetHashCode"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        /// <summary>
        /// <see cref="System.Object.Equals"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>compares the complete url as a string to obj</returns>
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }

            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            return this.ToString() == obj.ToString();
        }

        /// <summary>
        /// Convert the RestProxy to its full url as a string
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static implicit operator string(RestProxy p)
        {
            return p != null ? p.ToString() : null;
        }

        /// <summary>
        /// Returns an Uri represtation of the full Url
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static explicit operator Uri(RestProxy p)
        {
            return p != null ? new Uri(p.ToString(), UriKind.Absolute) : null;
        }
    }
}
