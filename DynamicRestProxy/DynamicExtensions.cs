using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Dynamic;

namespace DynamicRestProxy
{
    static class DynamicExtensions
    {
        public static dynamic DeserializeDynamic(this string value)
        {
            var serializer = new JavaScriptSerializer();
            var dictionary = serializer.Deserialize<Dictionary<string, object>>(value);
            return GetExpando(dictionary);
        }

        private static ExpandoObject GetExpando(IDictionary<string, object> dictionary)
        {
            var expando = (IDictionary<string, object>)new ExpandoObject();

            foreach (var item in dictionary)
            {
                var innerDictionary = item.Value as IDictionary<string, object>;
                if (innerDictionary != null)
                {
                    expando.Add(item.Key, GetExpando(innerDictionary));
                }
                else
                {
                    expando.Add(item.Key, item.Value);
                }
            }

            return (ExpandoObject)expando;
        }
    }
}
