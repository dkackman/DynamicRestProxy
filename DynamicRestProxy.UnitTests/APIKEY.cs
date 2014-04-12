using System.IO;

namespace DynamicRestProxy.UnitTests
{
    static class APIKEY
    {
        //#error either load you key from a file outside of the source try like below or return it directly here
        //#warning http://sunlightfoundation.com/api/accounts/register/
        private static readonly string _key;

        static APIKEY()
        {
            try
            {
                using (var file = File.OpenRead(@"d:\temp\sunlight.key.txt"))
                using (var reader = new StreamReader(file))
                    _key = reader.ReadLine();
            }
            catch
            {
                _key = "";
            }
        }

        public static string Key
        {
            get
            {
                return _key;
            }
        }
    }
}
