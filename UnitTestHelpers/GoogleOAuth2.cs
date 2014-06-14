using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using DynamicRestProxy.PortableHttpClient;

namespace UnitTestHelpers
{
    /// <summary>
    /// Helper class to deal with google oauth for unit testing
    /// The first time a machine authenticates user interaction is required
    /// Subsequent unit test runs will use a stored token that will refresh with google
    /// </summary>
    public class GoogleOAuth2
    {
        private string _scope;

        public GoogleOAuth2(string scope)
        {
            Debug.Assert(!string.IsNullOrEmpty(scope));
            _scope = scope;
        }

        public async Task<string> Authenticate(string token)
        {
            if (!string.IsNullOrEmpty(token))
                return token;

            if (CredentialStore.ObjectExists("google.auth.json"))
            {
                var access = CredentialStore.RetrieveObject("google.auth.json");

                if (DateTime.UtcNow >= access.expiry)
                {
                    access = await RefreshAccessToken(access);
                    StoreAccess(access);
                }

                return access.access_token;
            }
            else
            {
                var access = await GetNewAccessToken();
                StoreAccess(access);
                return access.access_token;
            }
        }

        private static void StoreAccess(dynamic access)
        {
            access.expiry = DateTime.UtcNow.Add(TimeSpan.FromSeconds(access.expires_in));
            CredentialStore.StoreObject("google.auth.json", access);
        }

        private static async Task<dynamic> RefreshAccessToken(dynamic access)
        {
            dynamic key = CredentialStore.JsonKey("google").installed;

            dynamic proxy = new DynamicRestClient("https://accounts.google.com");
            var response = await proxy.o.oauth2.token.post(client_id: key.client_id, client_secret: key.client_secret, refresh_token: access.refresh_token, grant_type: "refresh_token");

            response.refresh_token = access.refresh_token; // the new access token doesn't have a new refresh token so move our current one here for long term storage
            return response;
        }

        private async Task<dynamic> GetNewAccessToken()
        {
            dynamic key = CredentialStore.JsonKey("google").installed;

            dynamic proxy = new DynamicRestClient("https://accounts.google.com");
            var response = await proxy.o.oauth2.device.code.post(client_id: key.client_id, scope: _scope);

            Debug.WriteLine((string)response.user_code);

            // use clip.exe to put the user code on the clipboard
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = string.Format("/c echo {0} | clip", response.user_code);
            p.Start();

            // this requires user permission - open a broswer - enter the user_code which is now in the clipboard
            Process.Start((string)response.verification_url);

            int expiration = response.expires_in;
            int interval = response.interval;
            int time = interval;

            dynamic tokenResonse = null;
            // we are using the device flow so enter the code in the browser - poll google for success
            while (time < expiration)
            {
                Thread.Sleep(interval * 1000);
                tokenResonse = await proxy.o.oauth2.token.post(client_id: key.client_id, client_secret: key.client_secret, code: response.device_code, grant_type: "http://oauth.net/grant_type/device/1.0");
                if (tokenResonse.access_token != null)
                    break;
                time += interval;
            }

            return tokenResonse;
        }
    }
}
