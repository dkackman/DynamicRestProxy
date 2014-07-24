using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.UnitTests
{
    /// <summary>
    /// Summary description for ParameterPassingTests
    /// </summary>
    [TestClass]
    public class ParameterPassingTests
    {
        [TestMethod]
        [TestCategory("offline")]
        public async Task NamedArgsArePassedCorrectly()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic expando = await proxy.get(name: "value");

            Assert.IsFalse(((IEnumerable<object>)expando.UnnamedArgs).Any());

            IDictionary<string, object> namedArgs = expando.NamedArgs;
            Assert.IsTrue(namedArgs.ContainsKey("name"));
            Assert.AreEqual("value", namedArgs["name"]);
        }

        [TestMethod]
        [TestCategory("offline")]
        public async Task UnnamedArgsArePassedCorrectly()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic expando = await proxy.get("object");

            Assert.AreEqual(0, ((IDictionary<string, object>)expando.NamedArgs).Count);

            IEnumerable<object> unnamedArgs = expando.UnnamedArgs;

            Assert.AreEqual("object", unnamedArgs.First().ToString());
        }

        [TestMethod]
        [TestCategory("offline")]
        public async Task NamedAndUnnamedArgsArePassedCorrectly()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic expando = await proxy.get("object", name: "value");

            IEnumerable<object> unnamedArgs = expando.UnnamedArgs;
            IDictionary<string, object> namedArgs = expando.NamedArgs;

            Assert.AreEqual(1, unnamedArgs.Count());
            Assert.AreEqual(1, namedArgs.Count);

            Assert.AreEqual("object", unnamedArgs.First().ToString());

            Assert.IsTrue(namedArgs.ContainsKey("name"));
            Assert.AreEqual("value", namedArgs["name"]);
        }

        [TestMethod]
        [TestCategory("offline")]
        public async Task NoArgsPassedCorrectly()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic expando = await proxy.get();

            IEnumerable<object> unnamedArgs = expando.UnnamedArgs;
            IDictionary<string, object> namedArgs = expando.NamedArgs;

            Assert.AreEqual(0, unnamedArgs.Count());
            Assert.AreEqual(0, namedArgs.Count);
        }
    }
}
