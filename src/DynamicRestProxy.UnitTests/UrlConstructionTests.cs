﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.UnitTests
{
    [TestClass]
    public class UrlConstructionTests
    {
        [TestMethod]
        [TestCategory("offline")]
        public void SegmentProperty()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic chain = proxy.segment1;

            Assert.AreEqual("http://example.com/segment1", chain);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void TwoSegmentProperties()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic chain = proxy.segment1.segment2;

            Assert.AreEqual("http://example.com/segment1/segment2", chain);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeSegment()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic chain = proxy.segment1("escaped").segment2;

            string s = chain.ToString();
            Assert.AreEqual("http://example.com/segment1/escaped/segment2", chain);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeTwoSequentialSegments()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic chain = proxy.segment1("escaped")("escaped2");

            Assert.AreEqual("http://example.com/segment1/escaped/escaped2", chain);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeTwoSequentialSegmentsThenProperty()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic chain = proxy.segment1("escaped")("escaped2").segment2;

            Assert.AreEqual("http://example.com/segment1/escaped/escaped2/segment2", chain);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeSegmentAsInvoke()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic segment1 = proxy.segment1;
            dynamic chain = segment1("escaped");

            Assert.AreEqual("http://example.com/segment1/escaped", chain);
        }

        [TestMethod]
        [TestCategory("offline")]
        public void EscapeSegmentAsInvokeContinueChaining()
        {
            dynamic proxy = new TestProxy("http://example.com");
            dynamic segment1 = proxy.segment1;
            dynamic chain = segment1("escaped")("escaped2").segment2;

            Assert.AreEqual("http://example.com/segment1/escaped/escaped2/segment2", chain);
        }
    }
}
