using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class BinderExtensions
    {
        private static readonly string[] _verbs = new string[] { "post", "get", "delete", "put", "patch" }; // currently supported verbs

        public static IEnumerable<object> GetUnnamedArgs(this InvokeMemberBinder binder, object[] args)
        {
            return args.Take(binder.UnnamedArgCount()).Where(o => o != null); // filter out nulls
        }

        public static IDictionary<string, object> GetNamedArgs(this InvokeMemberBinder binder, object[] args)
        {
            var ret = new Dictionary<string, object>();
            int unnamedArgCount = binder.UnnamedArgCount();
            for (int i = 0; i < binder.CallInfo.ArgumentNames.Count; i++)
            {
                var arg = args[i + unnamedArgCount];
                if (arg != null) // filter out null parameters
                {
                    ret.Add(binder.CallInfo.ArgumentNames[i], arg);
                }
            }
            return ret;
        }

        public static int UnnamedArgCount(this InvokeMemberBinder binder)
        {
            return binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
        }

        public static bool IsVerb(this InvokeMemberBinder binder)
        {
            return _verbs.Contains(binder.Name);
        }
    }
}
