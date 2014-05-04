using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
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

            Assert.AreEqual(0, ((IEnumerable<object>)expando.UnnamedArgs).Count());

            var namedArgs = (IDictionary<string, object>)expando.NamedArgs;
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

            var unnamedArgs = (IEnumerable<object>)expando.UnnamedArgs;

            Assert.AreEqual("object", unnamedArgs.First().ToString());
        }

        [TestMethod]
        [TestCategory("offline")]
        public async Task NamedAndUnnamedArgsArePassedCorrectly()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic expando = await proxy.get("object", name: "value");

            var unnamedArgs = (IEnumerable<object>)expando.UnnamedArgs;
            var namedArgs = (IDictionary<string, object>)expando.NamedArgs;

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

            var unnamedArgs = (IEnumerable<object>)expando.UnnamedArgs;
            var namedArgs = (IDictionary<string, object>)expando.NamedArgs;

            Assert.AreEqual(0, unnamedArgs.Count());
            Assert.AreEqual(0, namedArgs.Count);
        }
    }
}
