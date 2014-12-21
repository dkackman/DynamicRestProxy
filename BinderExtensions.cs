using System;
using System.Linq;
using System.Reflection;
using System.Dynamic;
using System.Diagnostics;
using System.Collections.Generic;

namespace DynamicRestProxy
{
    static class BinderExtensions
    {
        // this is private field of the InvokeMemberBinder base class that is used to access
        // the generic type arguments
        private static FieldInfo _typeArgumentsField;

        private static readonly object _sync = new object();

        internal static readonly string[] _verbs = new string[] { "post", "get", "delete", "put", "patch" }; // currently supported verbs

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

        /// <summary>Extension method allowing to easyly extract generic type arguments from <see cref="InvokeMemberBinder"/>.</summary>
        /// <param name="binder">Binder from which get type arguments.</param>
        /// <returns>List of types passed as generic parameters.</returns>
        public static IEnumerable<Type> GetGenericTypeArguments(this InvokeMemberBinder binder)
        {
            lock (_sync)
            {
                // if we haven't cached the private field of the base class do so now
                if (_typeArgumentsField == null)
                {
                    // using reflection get the FieldInfo of the private typeArguments field
                    // mono and MS .net use different naming for this field
                    string fieldName = Type.GetType("Mono.Runtime") != null ? "typeArguments" : "m_typeArguments";
                    _typeArgumentsField = binder.GetType().GetTypeInfo().GetDeclaredField(fieldName);
                }
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
