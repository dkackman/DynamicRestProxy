
namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// By default POST parameters will be form encoded.
    /// Use this to force the request to have a particular parameter encoded on the url query
    /// </summary>
    public sealed class PostUrlParam
    {
        /// <summary>
        /// The parameter's value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="v">The param value</param>
        public PostUrlParam(object v) => Value = v;

        /// <summary>
        /// <see cref="System.Object.ToString"/>
        /// </summary>
        /// <returns>string representation of Value</returns>
        public override string ToString() => Value != null ? Value.ToString() : "";

        /// <summary>
        /// <see cref="System.Object.GetHashCode"/>
        /// </summary>
        /// <returns>the hashcode of Value</returns>
        public override int GetHashCode() => Value != null ? Value.GetHashCode() : 0;

        /// <summary>
        /// <see cref="System.Object.Equals(object)"/>
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            PostUrlParam p = obj as PostUrlParam;

            // if obj is a PostUrlParam, compare values
            if (p != null)
            {
                return p.Value == Value;
            }

            // compare everything else to Value
            if (Value != null)
            {
                return Value.Equals(obj);
            }

            // Value is null so we are equal if obj is too
            return object.ReferenceEquals(obj, null);
        }
    }
}
