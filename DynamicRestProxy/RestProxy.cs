using System;
using System.Text;
using System.Dynamic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DynamicRestProxy
{
    public abstract class RestProxy : DynamicObject
    {
        protected RestProxy(RestProxy parent, string name)
        {
            Parent = parent;
            Name = name;
        }

        public RestProxy Parent { get; private set; }

        public string Name { get; private set; }

        public int Index
        {
            get
            {
                return Parent != null ? Parent.Index + 1 : -1; // the root is the main url - does not represent a url segment
            }
        }

        protected abstract string BaseUrl { get; }

        protected abstract RestProxy CreateProxyNode(RestProxy parent, string name);

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            Debug.Assert(binder != null);
            Debug.Assert(args != null);

            if (args.Length != 1)
                throw new InvalidOperationException("The segment escape sequence must have exactly 1 unnamed parameter");

            // this is called when the dynamic object is invoked like a delegate
            // dynamic segment1 = proxy.segment1;
            // dynamic chain = segment1("escaped"); <- this calls TryInvoke
            result = CreateProxyNode(this, args[0].ToString());

            return true;
        }

        protected abstract Task<dynamic> CreateVerbAsyncTask(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs);

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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(binder != null);

            // this gets invoked when a dynamic property is accessed
            // example: proxy.locations will invoke here with a binder named locations
            // each dynamic property is treated as a url segment
            result = CreateProxyNode(this, binder.Name);

            return true;
        }

        private void ToString(StringBuilder builder)
        {
            if (Parent != null)
                Parent.ToString(builder); // go all the way up to the root and then back down

            if (string.IsNullOrEmpty(Name)) // if _name is null we are the root
            {
                builder.Append(BaseUrl);
            }
            else
            {
                builder.Append("/").Append(Name);
            }
        }

        protected void GetEndPointPath(StringBuilder builder)
        {
            if (Parent != null)
            { 
                Parent.GetEndPointPath(builder); // go all the way up to the root and then back down
            }

            builder.Append(Name);
            if (Parent != null)
            {
                builder.Append("/");
            }
        }

        protected string GetEndPointPath()
        {
            var builder = new StringBuilder();
            GetEndPointPath(builder);
            return builder.ToString();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            ToString(builder);
            return builder.ToString();
        }
    }
}
