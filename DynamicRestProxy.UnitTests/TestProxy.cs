using System.Collections.Generic;
using System.Threading.Tasks;
using System.Dynamic;

namespace DynamicRestProxy.UnitTests
{
    /// <summary>
    /// This guy is used for unit testing the abstract base class logic
    /// </summary>
    public class TestProxy : RestProxy
    {
        private string _baseUrl;

        public TestProxy(string baseUrl)
            : this(baseUrl, null, null)
        {
        }

        internal TestProxy(string baseUrl, RestProxy parent, string name)
            : base(parent, name)
        {
            _baseUrl = baseUrl;
        }

        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new TestProxy(_baseUrl, parent, name);
        }

        protected override Task<dynamic> CreateVerbAsyncTask(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs)
        {
            // just pass the arguments back as a task to they can be tested
            return Task.Factory.StartNew<dynamic>(() =>
                {
                    dynamic expando = new ExpandoObject();
                    expando.Verb = verb;
                    expando.UnnamedArgs = unnamedArgs;
                    expando.NamedArgs = namedArgs;
                    return expando;
                });
        }

        protected override string BaseUri
        {
            get { return _baseUrl; }
        }
    }
}
