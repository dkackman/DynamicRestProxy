using System;
using System.IO;
using System.Linq;
using System.Net.Http;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using FakeHttp;

using UnitTestHelpers;

namespace Client.Google.UnitTests
{
    [TestClass]
    [DeploymentItem(@"MockResponses\")]
    public class MockInitialization
    {
        public static HttpMessageHandler Handler { get; private set; }

        // these transient uri paramter names will not be used to hash rest query endpoint paths
        private static readonly string[] _ignoredParameterNames = new string[] { "key", "api_key" };

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // set the http message handler factory to the mode we want for the entire assmebly test execution
            MessageHandlerFactory.Mode = MessageHandlerMode.Fake;

            // folders where mock responses are stored and where captured response should be saved
            var mockFolder = context.DeploymentDirectory; // the folder where the unit tests are running
            var captureFolder = Path.Combine(context.TestRunDirectory, @"..\..\MockResponses\"); // kinda hacky but this should be the solution folder

            // here we don't want to serialize or include our api key in response lookups so
            // pass a lambda that will indicate to the serialzier to filter that param out
            var store = new FileSystemResponseStore(mockFolder, captureFolder, (name, value) => _ignoredParameterNames.Contains(name, StringComparer.InvariantCultureIgnoreCase));

            Handler = MessageHandlerFactory.CreateMessageHandler(store);
        }

        public static IGoogleOAuth2 GetAuthClient(string scope)
        {
            if (MessageHandlerFactory.Mode == MessageHandlerMode.Fake)
            {
                return new MockGoogleOAuth2();
            }

            return new GoogleOAuth2(scope);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            if (Handler != null)
            {
                Handler.Dispose();
            }
        }
    }
}
