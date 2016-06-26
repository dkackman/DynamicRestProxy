using System;
using System.IO;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Wrapper class for a <see cref="Stream"/> that relates the stream to metadata (MIME type)
    /// about the stream so metadata can be added to content headers
    /// </summary>
    public sealed class StreamInfo : ContentInfo, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="mimeType">MIME type</param>
        public StreamInfo(Stream stream, string mimeType = "application/octet-stream")
            : base(stream, mimeType)
        {
        }

        /// <summary>
        /// Disposes the underlying stream when called
        /// </summary>
        public void Dispose()
        {
            if (Content != null)
            {
                ((IDisposable)Content).Dispose();
            }
        }
    }
}
