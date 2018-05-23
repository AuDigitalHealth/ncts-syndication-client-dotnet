using Newtonsoft.Json;

namespace DigitalHealth.Ncts.Client.Models
{
    /// <summary>
    /// Atom link.
    /// </summary>
    public class AtomLink
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("length")]
        public string Length { get; set; }

        [JsonProperty("sha256Hash")]
        public string Sha256Hash { get; set; }
    }
}