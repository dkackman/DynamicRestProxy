
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

        public PostUrlParam(object v)
        {
            Value = v;
        }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "";
        }

        public override int GetHashCode()
        {
            return Value != null ? Value.GetHashCode() : 0;
        }

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
