using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Dynamic;

using Newtonsoft.Json;

namespace DynamicRestProxy.UnitTests
{
    /// <summary>
    /// This guy is used for unit testing the abstract base class logic
    /// </summary>
    public class TestProxy : RestProxy
    {
        private Uri _baseUrl;

        public TestProxy(string baseUrl)
            : this(new Uri(baseUrl, UriKind.Absolute), null, null)
        {
        }

        internal TestProxy(Uri baseUrl, RestProxy parent, string name)
            : base(parent, name)
        {
            _baseUrl = baseUrl;
        }

        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new TestProxy(_baseUrl, parent, name);
        }

        protected override Task<T> CreateVerbAsyncTask<T>(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs, CancellationToken cancelToken, JsonSerializerSettings serializationSettings)
        {
            // just pass the arguments back as a task to they can be tested
            return Task.Factory.StartNew<T>(() =>
                {
                    dynamic expando = new ExpandoObject();
                    expando.Verb = verb;
                    expando.UnnamedArgs = unnamedArgs;
                    expando.NamedArgs = namedArgs;
                    expando.CancelToken = cancelToken;
                    expando.SerializationSettings = serializationSettings;
                    expando.ReturnType = typeof(T);

                    return expando;
                });
        }

        protected override Uri BaseUri
        {
            get { return _baseUrl; }
        }
    }
}
