using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestHelpers
{
    public interface IGoogleOAuth2
    {
        Task<string> Authenticate(string token);

        Task<string> Authenticate(string token, CancellationToken cancelToken);
    }
}
