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
            Type t = d.Generic<int>();

            Assert.AreEqual(t, typeof(int));
        }

        [TestMethod]
        public void NonGenericMemberReturnsNullTypeArg()
        {
            dynamic d = new Dyn();
            Type t = d.NonGeneric();

            Assert.IsNull(t);
        }

        class Dyn : DynamicObject
        {
            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                if (binder.Name == "Generic")
                {
                    var types = binder.GetGenericTypeArguments();
                    result = types.FirstOrDefault();
                }
                else if (binder.Name == "NonGeneric")
                {
                    var types = binder.GetGenericTypeArguments();
                    result = types.FirstOrDefault();
                }
                else
                {
                    result = null;
                }

                return true;
            }
        }
    }
}
