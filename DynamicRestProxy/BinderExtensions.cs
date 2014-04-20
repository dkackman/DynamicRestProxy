using System.Linq;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class BinderExtensions
    {
        private static string[] _verbs = new string[] { "post", "get", "delete", "put" }; // 4 verbs for now
        
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
