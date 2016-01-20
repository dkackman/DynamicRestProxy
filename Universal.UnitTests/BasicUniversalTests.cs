using System;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

using DynamicRestProxy.PortableHttpClient;

using Windows.Storage;
using Windows.ApplicationModel;

using FakeHttp;

namespace Universal.UnitTests
{
    [TestClass]
    public class BasicFunctionalityTests
    {
        //[TestMethod]
        //[TestCategory("Universal")]
        //public async Task CanGetStorageFile()
        //{
        //    await DumpFolder(Package.Current.InstalledLocation);


        //}

        //private async static Task DumpFolder(StorageFolder folder)
        //{
        //    Debug.WriteLine(folder.Name);

        //    foreach (var file in await folder.GetFilesAsync())
        //    {
        //        Debug.WriteLine("\t" + file.Name);
        //    }

        //    foreach (var sub in await folder.GetFoldersAsync())
        //    {
        //        await DumpFolder(sub);
        //    }
        //}

        [TestMethod]
        [TestCategory("portable")]
        [TestCategory("integration")]
        public async Task DownloadJsonFile()
        {
            using (dynamic client = new DynamicRestClient("https://storage.googleapis.com/pictureframe/settings.json"))
            {
                var result = await client.get();
                Assert.IsNotNull(result);
                Assert.AreEqual(10000, result.settingCheckInterval);
            }
        }

        [TestMethod]
        [TestCategory("Universal")]
        public async Task GetPublicBucket()
        {
            var responses = await Package.Current.InstalledLocation.GetFolderAsync("MockResponses");
            var store = new StorageFolderResponseStore(responses);
            using (var handler = new FakeHttpMessageHandler(store))
            using (dynamic google = new DynamicRestClient("https://www.googleapis.com/", handler))
            {
                dynamic bucket = google.storage.v1.b("uspto-pair");

                dynamic metaData = await bucket.get();
                Assert.IsNotNull(metaData);

                dynamic objects = await bucket.o.get();
                Assert.IsNotNull(objects);
                Assert.IsTrue(objects.items.Count > 0);
            }
        }
    }
}
