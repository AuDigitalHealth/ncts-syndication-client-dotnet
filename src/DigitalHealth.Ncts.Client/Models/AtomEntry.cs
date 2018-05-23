using Newtonsoft.Json;

namespace DigitalHealth.Ncts.Client.Models
{
    /// <summary>
    /// Atom entry.
    /// </summary>
    public class AtomEntry
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("link")]
        public AtomLink Link { get; set; }
    }
}
