using DigitalHealth.Ncts.Client.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Xunit;

namespace DigitalHealth.Ncts.Client.Test
{
    public class NctsFileDownloaderTest
    {
        private INctsFileDownloader _client;

        private string _clientId;
        private string _clientSecret;


        public NctsFileDownloaderTest()
        {
			_clientId = "<CLIENTID>";
			_clientSecret = "<SECRET>";

			// Initialize the test client
			_client = new NctsFileDownloader(_clientId, _clientSecret);
        }

		[Fact]
		public async Task DownloadSingleFile_When_Valid_ClientId_And_Secret_Returns_Files()
        {
            // Returns list of snapshots zip files available to download
            AtomEntry entry = await _client.GetLatestEntryForCategory(Category.SctRf2Snapshot);

            string downloadPath = "c:\\temp\\";

            if (entry != null)
            {
                await _client.DownloadFile(entry.Link.Href, entry.Link.Sha256Hash, entry.Link.Length, downloadPath);
            }
        }

		[Fact]
		public async Task DownloadFile_When_Valid_ClientId_And_Secret_Returns_Files()
        {
            // Returns list of snapshots zip files available to download
            IList<AtomEntry> entries = await _client.GetListOfEntries(Category.SctRf2Snapshot);

            string downloadPath = "c:\\temp\\";

			if (entries != null && entries.Any())
            {
                //SUGGESTION: Only download latest option based from client selection of entry 
                var entry = entries.First();
                await _client.DownloadFile(entry.Link.Href, entry.Link.Sha256Hash, entry.Link.Length, downloadPath);
            }
        }

		[Fact]
		public async Task GetListOfEntries_When_No_Category_Specified_Then_Get_All()
        {
            IList<AtomEntry> entries = await _client.GetListOfEntries();

            Assert.True(entries.Count > 0);
        }

		[Fact]
		public async Task DownloadFile_When_Invalid_ClientId_And_Secret_Throws()
        {
            INctsFileDownloader fileDownloader = new NctsFileDownloader(null, null);

            // Should return 400 Bad Request 
            var exception = await Assert.ThrowsAsync<AuthenticationException>(
                async () => await fileDownloader.GetListOfEntries(Category.SctRf2Snapshot));

            Assert.Equal("Could not get token from authentication server", exception.Message);
        }
    }
}
