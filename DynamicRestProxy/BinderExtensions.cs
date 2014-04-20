using System.Linq;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class BinderExtensions
    {
        private static string[] _verbs = new string[] { "post", "get", "delete", "put" }; //two verbs for now
        
        public static string Verb(this InvokeMemberBinder binder)
        {
            if (binder.IsVerb())
                return binder.Name;

            return "get";
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
