using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace DynamicRestProxy.UnitTests
{
    static class APIKEY
    {
        //#error either load you key from a file outside of the source try like below or return it directly here
        //#warning http://sunlightfoundation.com/api/accounts/register/
        private static Dictionary<string, string> _keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static APIKEY()
        {
            AddKey("sunlight", @"d:\temp\sunlight.key.txt");
            AddKey("bing", @"d:\temp\bing.key.txt");
        }

        static void AddKey(string key, string keyPath)
        {
            try
            {
                using (var file = File.OpenRead(keyPath))
                using (var reader = new StreamReader(file))
                    _keys.Add(key, reader.ReadLine());
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
    }
}
