using System.Threading;
using System.Threading.Tasks;

using UnitTestHelpers;

namespace Client.Google.UnitTests
{
    class MockGoogleOAuth2 : IGoogleOAuth2
    {
        public async Task<string> Authenticate(string token)
        {
            return await Task.Run(() => "MOCK_AUTH_TOKEN");
        }

        public async Task<string> Authenticate(string token, CancellationToken cancelToken)
        {
            return await Task.Run(() => "MOCK_AUTH_TOKEN");
        }
    }
}
