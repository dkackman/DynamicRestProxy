using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class BinderExtensions
    {
        private static readonly string[] _verbs = new string[] { "post", "get", "delete", "put" }; // 4 verbs for now

        public static IEnumerable<object> GetUnnamedArgs(this InvokeMemberBinder binder, object[] args)
        {
            int unnamedArgCount = binder.UnnamedArgCount();
            var ret = new List<object>();
            for (int i = 0; i < unnamedArgCount; i++)
            {                
                ret.Add(args[i]);
            }
            return ret;
        }

        public static IDictionary<string, object> GetNamedArgs(this InvokeMemberBinder binder, object[] args)
        {
            var ret = new Dictionary<string, object>();
            int unnamedArgCount = binder.UnnamedArgCount();
            for (int i = 0; i < binder.CallInfo.ArgumentNames.Count; i++)
            {
                var arg = args[i + unnamedArgCount];
                ret.Add(binder.GetArgName(i), arg);
            }
            return ret;
        }

        public static int UnnamedArgCount(this InvokeMemberBinder binder)
        {
            return binder.CallInfo.ArgumentCount - binder.CallInfo.ArgumentNames.Count;
        }

        public static string GetArgName(this InvokeMemberBinder binder, int i)
        {
            return binder.CallInfo.ArgumentNames[i];
        }

        public static bool IsVerb(this InvokeMemberBinder binder)
        {
            return _verbs.Contains(binder.Name);
        }
    }
}
