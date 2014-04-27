using RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.UnitTests
{
    [TestClass]
    public class UrlConstructionTests
    {
        [TestMethod]
        [TestCategory("offline")]
        public void SegmentProperty()
        {
            var client = new RestClient("http://example.com");
            dynamic proxy = new RestProxy(client);
            dynamic chain = proxy.segment1;

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1", s);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void TwoSegmentProperties()
        {
            var client = new RestClient("http://example.com");
            dynamic proxy = new RestProxy(client);
            dynamic chain = proxy.segment1.segment2;

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1/segment2", s);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeSegment()
        {
            var client = new RestClient("http://example.com");
            dynamic proxy = new RestProxy(client);
            dynamic chain = proxy.segment1("escaped").segment2;

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1/escaped/segment2", s);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeTwoSequentialSegments()
        {
            var client = new RestClient("http://example.com");
            dynamic proxy = new RestProxy(client);
            dynamic chain = proxy.segment1("escaped")("escaped2");

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1/escaped/escaped2", s);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeTwoSequentialSegmentsThenProperty()
        {
            var client = new RestClient("http://example.com");
            dynamic proxy = new RestProxy(client);
            dynamic chain = proxy.segment1("escaped")("escaped2").segment2;

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1/escaped/escaped2/segment2", s);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeSegmentAsInvoke()
        {
            var client = new RestClient("http://example.com");
            dynamic proxy = new RestProxy(client);
            dynamic segment1 = proxy.segment1;
            dynamic chain = segment1("escaped");

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1/escaped", s);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeSegmentAsInvokeContinueChaining()
        {
            var client = new RestClient("http://example.com");
            dynamic proxy = new RestProxy(client);
            dynamic segment1 = proxy.segment1;
            dynamic chain = segment1("escaped")("escaped2").segment2;

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1/escaped/escaped2/segment2", s);
        }
    }
}
