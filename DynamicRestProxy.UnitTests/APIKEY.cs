using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace DynamicRestProxy.UnitTests
{
    /// <summary>
    /// This is a quick and dirty way to store service credentials in a place
    /// that is easy to acces but stored outside of the source tree so that they
    /// do not get checked into github
    /// </summary>
    static class APIKEY
    {
        //#error either load you key from a file outside of the source try like below or return it directly here
        //#warning http://sunlightfoundation.com/api/accounts/register/
        private static Dictionary<string, string> _keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static APIKEY()
        {
            AddKey("sunlight", @"d:\temp\sunlight.key.txt");
            AddKey("bing", @"d:\temp\bing.key.txt");
            AddKey("google", @"d:\temp\google.key.json");
        }

        static void AddKey(string key, string keyPath)
        {
            try
            {
                using (var file = File.OpenRead(keyPath))
                using (var reader = new StreamReader(file))
                    _keys.Add(key, reader.ReadToEnd());
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
            }
        }

        public static string Key(string api)
        {
            return _keys[api];
        }

        public static dynamic JsonKey(string api)
        {
            return JsonConvert.DeserializeObject<dynamic>(APIKEY.Key(api));
        }
    }
}
