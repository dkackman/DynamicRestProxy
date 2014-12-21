using System;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Diagnostics;
using System.Collections.Generic;

namespace DynamicRestProxy
{
    static class FrameworkTools
    {
        private static FieldInfo _genericTypeArgumentsField;

        /// <summary>Extension method allowing to easyly extract generic type arguments from <see cref="InvokeMemberBinder"/>.</summary>
        /// <param name="binder">Binder from which get type arguments.</param>
        /// <returns>List of types passed as generic parameters.</returns>
        public static IEnumerable<Type> GetGenericTypeArguments(this InvokeMemberBinder binder)
        {
            if (_genericTypeArgumentsField == null)
            {
                string fieldName = Type.GetType("Mono.Runtime") != null ? "typeArguments" : "m_typeArguments";
                _genericTypeArgumentsField = binder.GetType().GetTypeInfo().GetDeclaredField(fieldName);
            }

            if (_genericTypeArgumentsField != null)
            {
                return _genericTypeArgumentsField.GetValue(binder) as IEnumerable<Type> ?? new List<Type>();
            }

            Debug.Assert(false, "Retreiveing the private collection of generic type arguments failed");
            // Sadly return empty collection if failed.
            return new List<Type>();
        }

    }
}
