using System.Collections.Generic;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Holder class for a content object and metadata about that object to be set in
    /// content headers if provided
    /// </summary>
    public class ContentInfo
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="content"></param>
        /// <param name="mimeType"></param>
        public ContentInfo(object content, string mimeType = "")
        {
            Content = content;
            MimeType = mimeType;
            ContentHeaders = new Dictionary<string, string>();
        }

        /// <summary>
        /// Additional content headers
        /// </summary>
        public IDictionary<string, string> ContentHeaders { get; private set; }

        /// <summary>
        /// The content to send
        /// </summary>
        public object Content { get; private set; }

        /// <summary>
        /// The MIME type of the content object
        /// </summary>
        public string MimeType { get; private set; }
    }
}
