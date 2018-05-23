using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalHealth.Ncts.Client.Models;

namespace DigitalHealth.Ncts.Client
{
    public interface INctsFileDownloader
    {
        Task<List<AtomEntry>> GetListOfEntries(string categoryFilter = null);
        Task<AtomEntry> GetLatestEntryForCategory(string categoryFilter);
        Task<bool> DownloadFile(string downloadUrl, string sha256, string fileLength, string downloadDirectory);
    }
}
