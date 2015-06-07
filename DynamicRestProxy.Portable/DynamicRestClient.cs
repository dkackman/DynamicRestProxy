using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Diagnostics;
using System.Text;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// A rest client that uses dynamic objects for invocation and result values
    /// </summary>
    public sealed class DynamicRestClient : RestProxy, IDisposable
    {
        private static readonly IDictionary<string, HttpMethod> _methods = BinderExtensions._verbs.ToDictionary(verb => verb, verb => new HttpMethod(verb.ToUpperInvariant()));

        private readonly HttpClient _httpClient;
        private readonly IEnumerable<KeyValuePair<string, object>> _defaultParameters;
        private readonly Func<HttpRequestMessage, CancellationToken, Task> _configureRequest;
        private readonly bool _disposeClient = false;
        private bool _disposed = false;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseUri">The root url for all requests</param>
        /// <param name="defaults">Default values to add to all requests</param>
        /// <param name="configure">A callback function that will be called just before any request is sent</param>
        public DynamicRestClient(string baseUri, DynamicRestClientDefaults defaults = null, Func<HttpRequestMessage, CancellationToken, Task> configure = null)
            : this(new Uri(baseUri, UriKind.Absolute), defaults, configure)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseUri">The root url for all requests</param>
        /// <param name="defaults">Default values to add to all requests</param>
        /// <param name="configure">A callback function that will be called just before any request is sent</param>
        public DynamicRestClient(Uri baseUri, DynamicRestClientDefaults defaults = null, Func<HttpRequestMessage, CancellationToken, Task> configure = null)
            : this(HttpClientFactory.CreateClient(baseUri, defaults), null, null, "", configure, true)
        {
            _defaultParameters = defaults != null ? defaults.DefaultParameters : null;
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="client">HttpClient to use for communication</param>
        /// <param name="disposeClient">Flag indicating whether to take ownership of the client instance and dispose of when this instance is disposed</param>
        /// <param name="configure">A callback function that will be called just before any request is sent</param>
        public DynamicRestClient(HttpClient client, bool disposeClient = false, Func<HttpRequestMessage, CancellationToken, Task> configure = null)
            : this(client, null, null, "", configure, disposeClient)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseUri">The root url for all requests</param>
        /// <param name="handler">HttpMessageHandler to use for communication</param>
        /// <param name="disposeHandler">Flag indicating whether to take ownership of the handler instance and dispose of when this instance is disposed</param>
        /// <param name="defaults">Default values to add to all requests</param>
        /// <param name="configure">A callback function that will be called just before any request is sent</param>
        public DynamicRestClient(string baseUri, HttpMessageHandler handler, bool disposeHandler = false, DynamicRestClientDefaults defaults = null, Func<HttpRequestMessage, CancellationToken, Task> configure = null)
            : this(new Uri(baseUri, UriKind.Absolute), handler, disposeHandler, defaults, configure)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseUri">The root url for all requests</param>
        /// <param name="handler">HttpMessageHandler to use for communication</param>
        /// <param name="disposeHandler">Flag indicating whether to take ownership of the handler instance and dispose of when this instance is disposed</param>
        /// <param name="defaults">Default values to add to all requests</param>
        /// <param name="configure">A callback function that will be called just before any request is sent</param>
        public DynamicRestClient(Uri baseUri, HttpMessageHandler handler, bool disposeHandler = false, DynamicRestClientDefaults defaults = null, Func<HttpRequestMessage, CancellationToken, Task> configure = null)
            : this(HttpClientFactory.CreateClient(baseUri, handler, disposeHandler, defaults), null, null, "", configure, true)
        {
            _defaultParameters = defaults != null ? defaults.DefaultParameters : null;
        }

        internal DynamicRestClient(HttpClient client, IEnumerable<KeyValuePair<string, object>> defaultParameters, RestProxy parent, string name, Func<HttpRequestMessage, CancellationToken, Task> configure, bool disposeClient)
            : base(parent, name)
        {
            Debug.Assert(client != null);

            _httpClient = client;
            _defaultParameters = defaultParameters;
            _configureRequest = configure;
            _disposeClient = disposeClient;
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.BaseUri"/>
        /// </summary>
        protected override Uri BaseUri
        {
            get { return _httpClient.BaseAddress; }
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.CreateProxyNode(RestProxy, string)"/>
        /// </summary>
        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new DynamicRestClient(_httpClient, _defaultParameters, parent, name, _configureRequest, _disposeClient);
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.CreateVerbAsyncTask{T}(string, IEnumerable{object}, IEnumerable{KeyValuePair{string, object}}, CancellationToken, JsonSerializerSettings)"/>
        /// </summary>
        protected async override Task<T> CreateVerbAsyncTask<T>(string verb, IEnumerable<object> unnamedArgs, IEnumerable<KeyValuePair<string, object>> namedArgs, CancellationToken cancelToken, JsonSerializerSettings serializationSettings)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("The shared HttpClient has been disposed");
            }

            if (_defaultParameters != null)
            {
                namedArgs = namedArgs.Concat(_defaultParameters);
            }

            using (var request = CreateRequest(verb, unnamedArgs, namedArgs))
            {
                // give the user code a chance to setup any other request details
                // this is especially useful for setting oauth tokens when they have different lifetimes than the rest client
                if (_configureRequest != null)
                {
                    await _configureRequest(request, cancelToken);
                }

                var response = await _httpClient.SendAsync(request, cancelToken);

                // if the client asked for the response message back do not check for success
                if (typeof(T) == typeof(HttpResponseMessage))
                {
                    return (T)(object)response;
                }

                response.EnsureSuccessStatusCode();

                // forward the JsonSerializationSettings on if passed
                T result = await response.Deserialize<T>(serializationSettings);

                // if result isn't disposable (i.e. a stream or response message) it means
                // that we have fully read and deserialized the content and can dispose the response
                // If result is disposable, then it is up to the caller to dispose of it.
                // so if the caller asks for a stream we do not dispose the response (which disposes the stream)
                if (!(result is IDisposable))
                {
                    response.Dispose();
                }

                return result;
            }
        }

        private HttpRequestMessage CreateRequest(string verb, IEnumerable<object> unnamedArgs, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            // the way the base class and this class's static contructor use BinderExtensions._verbs should prevent an unkown verb from reaching here
            Debug.Assert(_methods.ContainsKey(verb), "unrecognized verb. check the BinderExtensions _verbs array");

            var method = _methods[verb];
            return new HttpRequestMessage()
            {
                Method = method,
                RequestUri = CreateUri(method, namedArgs),
                Content = ContentFactory.CreateContent(method, unnamedArgs, namedArgs)
            };
        }

        private Uri CreateUri(HttpMethod method, IEnumerable<KeyValuePair<string, object>> namedArgs)
        {
            var builder = new StringBuilder(GetEndPointPath());

            // all methods but post place params on the url
            if (method != HttpMethod.Post)
            {
                builder.Append(namedArgs.AsQueryString());
            }
            else
            {
                // by default post uses form encoded parameters but it is allowable to have params on the url
                // see google storage api for example https://developers.google.com/storage/docs/json_api/v1/objects/insert
                // the PostUrlParam will wrap the param value and is a signal to force it onto the url and not form encode it
                builder.Append(namedArgs.Where(kvp => kvp.Value is PostUrlParam).AsQueryString());
            }

            return new Uri(builder.ToString(), UriKind.Relative);
        }

        /// <summary>
        /// Disposes the contained HttpClient
        /// </summary>
        public void Dispose()
        {
            if (!_disposed && _httpClient != null && _disposeClient)
            {
                _httpClient.Dispose();
                _disposed = true;
            }
        }
    }
}