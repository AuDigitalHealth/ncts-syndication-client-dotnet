using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using DigitalHealth.Ncts.Client.Models;
using NUnit.Framework;

namespace DigitalHealth.Ncts.Client.Test
{
    [TestFixture]
    public class NctsFileDownloaderTest
    {
        private INctsFileDownloader _client;

        private string _clientId;
        private string _clientSecret;


        [SetUp]
        public void Setup()
        {
            _clientId = "INSERT_CLIENT_ID_FROM_NCTS_WEBSITE";
            _clientSecret = "INSERT_CLIENT_SECRET_FROM_NCTS_WEBSITE";

            // Initialize the test client
            _client = new NctsFileDownloader(_clientId, _clientSecret);
        }

        [Test]
        public async Task DownloadSingleFile_When_Valid_ClientId_And_Secret_Returns_Files()
        {
            // Returns list of snapshots zip files available to download
            AtomEntry entry = await _client.GetLatestEntryForCategory(Category.SctRf2Snapshot);

            string downloadPath = "ENTER_OUTPUT_DIR_PATH_HERE";

            if (entry != null)
            {
                await _client.DownloadFile(entry.Link.Href, entry.Link.Sha256Hash, entry.Link.Length, downloadPath);
            }
        }

        [Test]
        public async Task DownloadFile_When_Valid_ClientId_And_Secret_Returns_Files()
        {
            // Returns list of snapshots zip files available to download
            List<AtomEntry> entries = await _client.GetListOfEntries(Category.SctRf2Delta);

            string downloadPath = "ENTER_OUTPUT_DIR_PATH_HERE";

            if (entries != null && entries.Any())
            {
                //SUGGESTION: Only download latest option based from client selection of entry 
                var entry = entries.First();
                await _client.DownloadFile(entry.Link.Href, entry.Link.Sha256Hash, entry.Link.Length, downloadPath);
            }
        }

        [Test]
        public async Task GetListOfEntries_When_No_Category_Specified_Then_Get_All()
        {
            List<AtomEntry> entries = await _client.GetListOfEntries();

            Assert.That(entries.Count, Is.GreaterThan(0));
        }

        [Test]
        public void DownloadFile_When_Invalid_ClientId_And_Secret_Throws()
        {
            INctsFileDownloader fileDownloader = new NctsFileDownloader(null, null);

            // Should return 400 Bad Request 
            AuthenticationException exception = Assert.ThrowsAsync<AuthenticationException>(
                async () => await fileDownloader.GetListOfEntries(Category.SctRf2Snapshot));

            Assert.That(exception.Message, Is.EqualTo("Could not get token from authentication server"));
        }
    }
}
