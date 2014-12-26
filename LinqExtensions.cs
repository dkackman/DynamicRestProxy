using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

namespace DynamicRestProxy
{
    static class LinqExtensions
    {        
        public static bool IsOfType(this object o, IEnumerable<TypeInfo> types)
        {
            Debug.Assert(o != null);

            var typeInfo = o.GetType().GetTypeInfo();
            foreach (var t in types)
            {
                if (t.IsAssignableFrom(typeInfo))
                {
                    return true;
                }
            }

            return false;
        }

        public static T FirstOrNewInstance<T>(this IEnumerable<T> source) where T : new()
        {
            return source.Any() ? source.First() : new T();
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, T def)
        {
            return source.Any() ? source.First() : def;
        }
    }
}
