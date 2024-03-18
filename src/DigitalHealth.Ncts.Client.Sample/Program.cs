using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalHealth.Ncts.Client.Models;

namespace DigitalHealth.Ncts.Client.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            GetLatestSnapshotUsingDefaultSettings().Wait();
            OverRideDefaultUrlsReturningAllFiles().Wait();
        }

        public static async Task GetLatestSnapshotUsingDefaultSettings()
        {
            // 1) Visit https://www.healthterminologies.gov.au/ and register/login
            // 2) Select My Profile Tab, and then Select "Client Credentials" at the bottom of the page
            // 3) Create a new client and populate the 2 variables below with the Client Id and Secret
            var clientId = "INSERT_CLIENT_ID_FROM_NCTS_WEBSITE";
            var clientSecret = "INSERT_CLIENT_SECRET_FROM_NCTS_WEBSITE";

            // Initialize Test Client
            INctsFileDownloader client = new NctsFileDownloader(clientId, clientSecret);

            // Return list of zip files available to download in descending order with latest first
            // Can filter file list returned to just one category (SNAPSHOT, FULL, DELTA, ALL or BINARY)
            AtomEntry entry = await client.GetLatestEntryForCategory(Category.SctRf2Delta);

            // If entry returned
            if (entry != null)
            {
                // Path for where to download the zip to
                var downloadPath = "ENTER_OUTPUT_DIR_PATH_HERE";

                // Only download latest option based from client selection of entry 
                await client.DownloadFile(entry.Link.Href, entry.Link.Sha256Hash, entry.Link.Length, downloadPath);
            }
        }


        public static async Task OverRideDefaultUrlsReturningAllFiles()
        {
            // 1) Visit https://www.healthterminologies.gov.au/ and register/login
            // 2) Select My Profile Tab, and then Select "Client Credentials" at the bottom of the page
            // 3) Create a new client and populate the 2 variables below with the Client Id and Secret
            var clientId = "INSERT_CLIENT_ID_FROM_NCTS_WEBSITE";
            var clientSecret = "INSERT_CLIENT_SECRET_FROM_NCTS_WEBSITE";

            // Initialize Test Client
            string tokenUrl = "https://api.healthterminologies.gov.au/oauth2/token";
            string feedUrl = "https://api.healthterminologies.gov.au/syndication/v1/syndication.xml";
            INctsFileDownloader client = new NctsFileDownloader(tokenUrl, feedUrl, clientId, clientSecret);

            // Return list of zip files available to download in descending order with latest first
            // Can filter file list returned to just one category (SNAPSHOT, FULL, DELTA, ALL or BINARY)
            List<AtomEntry> entries = await client.GetListOfEntries(Category.SctRf2Snapshot);

            if (entries != null && entries.Any())
            {
                // Path for where to download the zip to
                var downloadPath = "ENTER_OUTPUT_DIR_PATH_HERE";

                // Only download latest option based from client selection of entry 
                var entry = entries.First();
                await client.DownloadFile(entry.Link.Href, entry.Link.Sha256Hash, entry.Link.Length, downloadPath);
            }
        }
    }
}
