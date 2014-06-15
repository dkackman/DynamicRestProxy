using System;
using System.IO;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// Wrapper class for a stream that relates the stream to meta-data (MIME type)
    /// about the stream so meta data can be added to content headers
    /// </summary>
    public class StreamInfo : IDisposable
    {
        /// <summary>
        /// The Stream
        /// </summary>
        public Stream Stream { get; private set; }

        /// <summary>
        /// The MIME type of the stream's contents
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="mimeType">MIME type</param>
        public StreamInfo(Stream stream, string mimeType)
        {
            Stream = stream;
            MimeType = mimeType;
        }

        /// <summary>
        /// Disposes the underlying stream when called
        /// </summary>
        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Dispose();
            }
        }
    }
}
