using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.CSharp.RuntimeBinder;

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
        // the set of scopes to authorize
        private string _scope;

        public GoogleOAuth2(string scope)
        {
            Debug.Assert(!string.IsNullOrEmpty(scope));

            _scope = scope;
        }

        public async Task<string> Authenticate(string token)
        {
            // if we have a token already just use it
            if (!string.IsNullOrEmpty(token))
                return token;

            dynamic access = null;
            // if we have a stored token see use it
            if (CredentialStore.ObjectExists(_scope.GetHashCode() + ".google.auth.json"))
            {
                access = CredentialStore.RetrieveObject(_scope.GetHashCode() + ".google.auth.json");

                // if the stored token is expired refresh it
                if (DateTime.UtcNow >= access.expiry)
                {
                    access = await RefreshAccessToken(access);
                }
            }
            else
            {
                // no stored token - go get a new one
                access = await GetNewAccessToken();
            }

            StoreAccess(access);
            return access.access_token;
        }

        private void StoreAccess(dynamic access)
        {
            access.expiry = DateTime.UtcNow.Add(TimeSpan.FromSeconds(access.expires_in));
            CredentialStore.StoreObject(_scope.GetHashCode() + ".google.auth.json", access);
        }

        private static async Task<dynamic> RefreshAccessToken(dynamic access)
        {
            dynamic key = CredentialStore.RetrieveObject("google.key.json").installed;

            dynamic google = new DynamicRestClient("https://accounts.google.com/o/oauth2/");
            var response = await google.token.post(client_id: key.client_id, client_secret: key.client_secret, refresh_token: access.refresh_token, grant_type: "refresh_token");

            response.refresh_token = access.refresh_token; // the new access token doesn't have a new refresh token so move our current one here for long term storage
            return response;
        }

        /// <summary>
        /// This authenticates against user and requires user interaction to authorize the unit test to access apis
        /// This will do the auth, put the auth code on the clipboard and then open a browser with the app auth permission page
        /// The auth code needs to be sent back to google
        /// 
        /// This should only need to be done once because the access token will be stored and refreshed for future test runs
        /// </summary>
        /// <returns></returns>
        private async Task<dynamic> GetNewAccessToken()
        {
            dynamic key = CredentialStore.RetrieveObject("google.key.json").installed;

            dynamic google = new DynamicRestClient("https://accounts.google.com/o/oauth2/");
            var response = await google.device.code.post(client_id: key.client_id, scope: _scope);

            Debug.WriteLine((string)response.user_code);

            Console.WriteLine("Requested permissions are:");
            Console.WriteLine(_scope);
            Console.WriteLine("");
            Console.WriteLine("Navigate to {0} in a web browser", response.verification_url);
            Console.WriteLine("Enter this code to authorize access: " + response.user_code);
            Console.WriteLine("");

            // use clip.exe to put the user code on the clipboard
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = string.Format("/c echo {0} | clip", response.user_code);
            p.Start();

            // this requires user permission - open a broswer - enter the user_code which is now in the clipboard
            Process.Start((string)response.verification_url);

            long expiration = response.expires_in;
            long interval = response.interval;
            long time = interval;

            // we are using the device flow so enter the code in the browser
            // here poll google for success
            while (time < expiration)
            {
                Thread.Sleep((int)interval * 1000);
                dynamic tokenResonse = await google.token.post(client_id: key.client_id, client_secret: key.client_secret, code: response.device_code, grant_type: "http://oauth.net/grant_type/device/1.0");
                try
                {
                    if (tokenResonse.access_token != null)
                        return tokenResonse;
                }
                catch (RuntimeBinderException)
                {
                }

                time += interval;
            }

            throw new OperationCanceledException("Authorization from user timed out");
        }
    }
}
