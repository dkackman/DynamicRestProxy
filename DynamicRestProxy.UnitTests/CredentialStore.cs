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
    static class CredentialStore
    {
        //#error either load you key from a file outside of the source try like below or return it directly here
        //#warning http://sunlightfoundation.com/api/accounts/register/
        private static Dictionary<string, string> _keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static string _root = @"d:\temp\";

        static CredentialStore()
        {
            AddKey("sunlight", _root + "sunlight.key.txt");
            AddKey("bing", _root + "bing.key.txt");
            AddKey("google", _root + "google.key.json");
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

        public static bool Exists(string name)
        {
            return File.Exists(_root + name);
        }

        public static dynamic Retreive(string name)
        {
            try
            {
                using (var file = File.OpenRead(_root + name))
                using (var reader = new StreamReader(file))
                {
                    string json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<dynamic>(json);
                }
            }
            catch (Exception e)
            {
                Debug.Assert(false, e.Message);
            }

            return null;
        }

        public static void Store(string name, dynamic o)
        {
            using (var file = File.OpenWrite(_root + name))
            using (var writer = new StreamWriter(file))
            {                
                string json = JsonConvert.SerializeObject(o);
                writer.Write(json);
            }
        }

        public static dynamic JsonKey(string api)
        {
            return JsonConvert.DeserializeObject<dynamic>(CredentialStore.Key(api));
        }
    }
}
