using System;
using System.Reflection;
using System.Dynamic;
using System.Diagnostics;
using System.Collections.Generic;

namespace DynamicRestProxy
{
    static class FrameworkTools
    {
        // this is private field of the InvokeMemberBinder base class that is used to access
        // the generic type arguments
        private static FieldInfo _typeArgumentsField;

        /// <summary>Extension method allowing to easyly extract generic type arguments from <see cref="InvokeMemberBinder"/>.</summary>
        /// <param name="binder">Binder from which get type arguments.</param>
        /// <returns>List of types passed as generic parameters.</returns>
        public static IEnumerable<Type> GetGenericTypeArguments(this InvokeMemberBinder binder)
        {
            // if we haven't cached the private field of the base class do so now
            if (_typeArgumentsField == null)
            {
                // using reflection get the FieldInfo of the private typeArguments field
                // mono and MS .net use different naming for this field
                string fieldName = Type.GetType("Mono.Runtime") != null ? "typeArguments" : "m_typeArguments";
                _typeArgumentsField = binder.GetType().GetTypeInfo().GetDeclaredField(fieldName);
            }

            // if the field info is still null, something changed in how .net implements the dynamic binder
            if (_typeArgumentsField != null)
            {
                return _typeArgumentsField.GetValue(binder) as IEnumerable<Type> ?? new List<Type>();
            }

            Debug.Assert(false, "Retrieving the private collection of generic type arguments failed");
            // Sadly return empty collection if failed.
            return new List<Type>();
        }
    }
}
