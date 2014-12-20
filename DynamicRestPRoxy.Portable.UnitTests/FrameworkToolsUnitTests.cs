using System;
using System.Linq;
using System.Dynamic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class FrameworkToolsUnitTests
    {
        [TestMethod]
        public void GetDynamicGenericArguments()
        {
            dynamic d = new Dyn();
            Type t = d.Member<int>();

            Assert.AreEqual(t, typeof(int));
        }


        class Dyn : DynamicObject
        {
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                var types = binder.GetGenericTypeArguments();
                result = types.FirstOrDefault();
                return true;
            }
        }
    }
}
