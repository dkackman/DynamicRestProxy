
namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// By default POST parameters will be form encoded.
    /// Use this to force the request to have a particular paramter encoded on the url query
    /// </summary>
    public sealed class PostUrlParam
    {
        /// <summary>
        /// The paramter's value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="v">The param value</param>
        public PostUrlParam(object v)
        {
            Value = v;
        }

        /// <summary>
        /// <see cref="System.Object.ToString"/>
        /// </summary>
        /// <returns>string representation of Value</returns>
        public override string ToString()
        {
            return Value != null ? Value.ToString() : "";
        }

        /// <summary>
        /// <see cref="System.Object.GetHashCode"/>
        /// </summary>
        /// <returns>the hascode of Value</returns>
        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

        /// <summary>
        /// <see cref="System.Object.Equals(object)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (Value != null)
            {
                return Value.Equals(obj);
            }

            return object.ReferenceEquals(obj, null);
        }
    }
}
