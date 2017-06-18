using System;
using System.Threading.Tasks;

using DynamicRestProxy.PortableHttpClient;

using UnitTestHelpers;

namespace GoogleStorageConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("What is the name of the storage project?"); // direct-link-612
            var project = Console.ReadLine();

            var t = EnumerateObjects(project);

            t.Wait();
        }

        private static async Task EnumerateObjects(string project)
        {
            var auth = new GoogleOAuth2("https://www.googleapis.com/auth/devstorage.read_write");
            var token = await auth.Authenticate("");

            var defaults = new DynamicRestClientDefaults()
            {
                AuthScheme = "OAuth",
                AuthToken = token
            };

            dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults);
            dynamic bucketEndPoint = google.storage.v1.b;
            dynamic buckets = await bucketEndPoint.get(project: project);

            foreach (var bucket in buckets.items)
            {
                Console.WriteLine("bucket {0}: {1}", bucket.id, bucket.name);

                dynamic contents = await bucketEndPoint(bucket.id).o.get();

                foreach (var item in contents.items)
                {
                    Console.WriteLine("\tid: {0}", item.id);
                    Console.WriteLine("\tname: {0}", item.name);
                    Console.WriteLine("\tcontent type: {0}", item.contentType);
                    Console.WriteLine("\t-----");
                }
            }
        }
    }
}
