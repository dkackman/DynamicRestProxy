using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class BinderExtensions
    {
        private static string[] _verbs = new string[] { "post", "get" };
        
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
