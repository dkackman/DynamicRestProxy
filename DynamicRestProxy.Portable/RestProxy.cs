using System;
using System.Text;
using System.Linq;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    /// <summary>
    /// Base proxy class. Derived classes implement the specfic rest/http communication mechanisms.
    /// Each node in a chain of proxy instances represents a specific endpoint
    /// </summary>
    public abstract class RestProxy : DynamicObject
    {
        // currently supported verbs
        protected static readonly string[] _verbs = new string[] { "post", "get", "delete", "put", "patch" };

        // objects of these types, when passed to a verb invocation as unnamed arguments, 
        // will signal particualr behavior rather than get passed as content
        private static readonly TypeInfo[] _reservedTypes = new TypeInfo[] { typeof(CancellationToken).GetTypeInfo(), typeof(JsonSerializerSettings).GetTypeInfo(), typeof(Type).GetTypeInfo() };

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
        /// The base Url of the endpoint. Overridden in derived classes to allow specific rest client to determine how it is stored
        /// </summary>
        protected abstract Uri BaseUri { get; }

        /// <summary>
        /// Factory method used to create instances of derived child nodes. Overriden in derived classes to create derived instances
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
        /// <param name="verb">The http verb to execute (must be get, post, put, patch or delete)</param>
        /// <param name="unnamedArgs">Unnamed arguments passed to the invocation. These go into the http request body</param>
        /// <param name="namedArgs">Named arguments supplied to the invocation. These become http request parameters</param>
        /// <param name="cancelToken">A CancellationToken for the async operations</param>
        /// <param name="serializationSettings">Settings to use for response deserialization</param>
        /// <returns>Task{dynamic} that will execute the http call and return a dynamic object with the results</returns>
        protected abstract Task<T> CreateVerbAsyncTask<T>(string verb, IEnumerable<object> unnamedArgs, IEnumerable<KeyValuePair<string, object>> namedArgs, CancellationToken cancelToken, JsonSerializerSettings serializationSettings);

        /// <summary>
        /// <see cref="System.Dynamic.DynamicObject.TryInvokeMember(InvokeMemberBinder, object[], out object)"/>
        /// </summary>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (_verbs.Contains(binder.Name)) // the method name is one of our http verbs - invoke as such
            {
                var unnamedArgs = binder.GetUnnamedArgs(args);

                // filter our sentinal types out of the unnamed args to be passed on the request
                var requestArgs = unnamedArgs.Where(arg => !arg.IsOfType(_reservedTypes));

                // these are the objects that can be passed as unnamed args that we use intenrally and do not pass to the request
                var cancelToken = unnamedArgs.OfType<CancellationToken>().DefaultIfEmpty(CancellationToken.None).First();
                var serializationSettings = unnamedArgs.OfType<JsonSerializerSettings>().FirstOrNewInstance();

#if EXPERIMENTAL_GENERICS
                // dig the generic type argument out of the binder
                var returnType = binder.GetGenericTypeArguments().FirstOrDefault(); // evil exists within that method
#else
                var returnType = unnamedArgs.OfType<Type>().FirstOrDefault();
#endif
                // if no return type argument provided there is no need for late bound method dispatch
                if (returnType == null)
                {
                    // no return type argument so return result deserialized as dynamic
                    // parse out the details of the invocation and have the derived class create a Task
                    result = CreateVerbAsyncTask<dynamic>(binder.Name, requestArgs, binder.GetNamedArgs(args), cancelToken, serializationSettings);
                }
                else
                {
                    // we got a type argument (like this if experimental: client.get<SomeType>(); or like this normally: client.get(typeof(SomeType)); )
                    // make and invoke the generic implementaiton of the CreateVerbAsyncTask method
                    var methodInfo = this.GetType().GetTypeInfo().GetDeclaredMethod("CreateVerbAsyncTask");
                    var method = methodInfo.MakeGenericMethod(returnType);
                    result = method.Invoke(this, new object[] { binder.Name, requestArgs, binder.GetNamedArgs(args), cancelToken, serializationSettings });
                }
            }
            else // otherwise the method is yet another uri segment
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
            string uri = BaseUri.ToString();
            if (uri.EndsWith("/"))
            {
                return uri + GetEndPointPath();
            }

            return uri + "/" + GetEndPointPath();
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
        /// <see cref="System.Object.Equals(System.Object)"/>
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
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static implicit operator string(RestProxy proxy)
        {
            return proxy != null ? proxy.ToString() : null;
        }

        /// <summary>
        /// Returns an Uri represtation of the full Url
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public static explicit operator Uri(RestProxy proxy)
        {
            return proxy != null ? new Uri(proxy.ToString(), UriKind.Absolute) : null;
        }
    }
}
