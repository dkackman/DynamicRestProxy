using System.Linq;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class BinderExtensions
    {
        private static string[] _verbs = new string[] { "post", "get" }; //two verbs for now
        
        public static string Verb(this InvokeMemberBinder binder)
        {
            if (binder.IsVerb())
                return binder.Name;

            return "get";
        }

        public static bool IsVerb(this InvokeMemberBinder binder)
        {
            return _verbs.Contains(binder.Name);
        }

        public static int UrlSegmentOffset(this InvokeMemberBinder binder)
        {
            return binder.IsVerb() ? 0 : 1; // plus one is for the binder endpoint - verb binders don't count
        }
    }
}
