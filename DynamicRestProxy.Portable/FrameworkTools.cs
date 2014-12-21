using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class FrameworkTools
    {
        private static readonly bool _isMono = Type.GetType("Mono.Runtime") != null;

        /// <summary>Extension method allowing to easyly extract generic type arguments from <see cref="InvokeMemberBinder"/>.</summary>
        /// <param name="binder">Binder from which get type arguments.</param>
        /// <returns>List of types passed as generic parameters.</returns>
        public static IList<Type> GetGenericTypeArguments(this InvokeMemberBinder binder)
        {
            if (_genericTypeField == null)
            {
                _genericTypeField = _isMono ? binder.GetType().GetField("typeArguments") : binder.GetType().GetField("m_typeArguments");
            }

            if (_genericTypeField != null)
            {
                return _genericTypeField.GetValue(binder) as IList<Type>;
            }

            // Sadly return null if failed.
            return null;
        }

        private static FieldInfo _genericTypeField;

        private static FieldInfo GetField(this Type t, string name)
        {
            return t.GetTypeInfo().DeclaredFields.First(f => f.Name == name);
        }
    }
}
