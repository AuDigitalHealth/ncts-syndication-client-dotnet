using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml;
using DigitalHealth.Ncts.Client.Models;
using Newtonsoft.Json;

namespace DigitalHealth.Ncts.Client
{
    public class NctsFileDownloader: INctsFileDownloader
    {
        // NCTS client ID and secret
        private readonly string _clientId;
        private readonly string _clientSecret;

        // Bearer token
        private string _token;

        /// <summary>
        /// Default token endpoint.
        /// </summary>
        private readonly string _tokenUrl = "https://api.healthterminologies.gov.au/oauth2/token";

        /// <summary>
        /// Feed endpoint.
        /// </summary>
        private readonly string _feedUrl = "https://api.healthterminologies.gov.au/syndication/v1/syndication.xml";

        /// <summary>
        /// Sets NCTS client ID and secret required to get the bearer token.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public NctsFileDownloader(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        /// <summary>
        /// Overrides default URLs and sets the NCTS client ID and secret required to get the bearer token.
        /// </summary>
        /// <param name="tokenUrl"></param>
        /// <param name="feedUrl"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        public NctsFileDownloader(string tokenUrl, string feedUrl, string clientId, string clientSecret): this(clientId, clientSecret)
        {
            _tokenUrl = tokenUrl;
            _feedUrl = feedUrl;
        }

        /// <summary>
        /// Retrieves the bearer token from the auth server for downloading terminology files.
        /// </summary>
        /// <returns>Bearer token</returns>
        private async Task<string> GetBearerTokenFromAuthServer()
        {
            if (_token == null)
            {
                try
                {
                    // Create the query string
                    string queryString = $"?grant_type=client_credentials&client_id={_clientId}&client_secret={_clientSecret}";

                    // Create the request
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_tokenUrl + queryString);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";

                    // NOTE needed for .NET 4.5 (4.6 it is set by default)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string responseText;
                    using (var response = await request.GetResponseAsync() as HttpWebResponse)
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            throw new SyndicationFeedException($"Server error (HTTP {response.StatusCode}: {response.StatusDescription})");
                        }

                        Stream stream = response.GetResponseStream();
                        StreamReader streamReader = new StreamReader(stream);

                        responseText = streamReader.ReadToEnd();
                    }

                    // Process the JSON response
                    OAuthResponse oAuthResponse = JsonConvert.DeserializeObject<OAuthResponse>(responseText);
                    _token = oAuthResponse.AccessToken;

                }
                catch (Exception ex)
                {
                    throw new AuthenticationException("Could not get token from authentication server", ex);
                }
            }

            return _token;
        }

        /// <summary>
        /// Returns list of zip files available to download with optional filtering.
        /// </summary>
        /// <param name="categoryFilter">Category filter</param>
        /// <returns></returns>
        public async Task<List<AtomEntry>> GetListOfEntries(string categoryFilter = null)
        {
            string bearerToken = await GetBearerTokenFromAuthServer();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_feedUrl);
            request.Method = "GET";
            request.Headers.Add("Authorization", $"Bearer {bearerToken}");

            List<AtomEntry> entries;
            try
            {
                string responseText;
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new SyndicationFeedException($"Server error (HTTP {response.StatusCode}: {response.StatusDescription})");
                    }

                    Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream);

                    responseText = streamReader.ReadToEnd();
                }

                entries = ProcessEntries(responseText, categoryFilter);
            }
            catch (Exception ex)
            {
                throw new SyndicationFeedException("Error getting list of entries", ex);
            }

            return entries;
        }

        /// <summary>
        /// Returns list of zip files available to download with optional filtering.
        /// </summary>
        /// <param name="categoryFilter">Category filter</param>
        /// <returns></returns>
        public async Task<AtomEntry> GetLatestEntryForCategory(string categoryFilter)
        {
            if (string.IsNullOrWhiteSpace(categoryFilter))
            {
                throw new ArgumentException("'categoryFilter' cannot be null", nameof(categoryFilter));
            }

            List<AtomEntry> entries = await GetListOfEntries(categoryFilter);

            AtomEntry entry = entries.FirstOrDefault();

            return entry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="downloadUrl"></param>
        /// <param name="sha256"></param>
        /// <param name="fileLength"></param>
        /// <param name="downloadDirectory"></param>
        /// <returns></returns>
        public async Task<bool> DownloadFile(string downloadUrl, string sha256, string fileLength, string downloadDirectory)
        {
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                throw new ArgumentException("'downloadUrl' cannot be null", nameof(downloadUrl));
            }

            if (string.IsNullOrWhiteSpace(sha256))
            {
                throw new ArgumentException("'sha256' cannot be null", nameof(sha256));
            }

            if (string.IsNullOrWhiteSpace(fileLength))
            {
                throw new ArgumentException("'fileLength' cannot be null", nameof(fileLength));
            }

            if (string.IsNullOrWhiteSpace(downloadDirectory))
            {
                throw new ArgumentException("'downloadDirectory' cannot be null", nameof(downloadDirectory));
            }

            string filename = downloadDirectory + downloadUrl.Split('/').Last();

            // Delete file before downloading
            try
            {
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            catch (Exception ex)
            {
                throw new SyndicationFeedException("File already exists and cannot be deleted", ex);
            }

            bool downloadSuccessStatus = false;
            try
            {
                string bearerToken = await GetBearerTokenFromAuthServer();

                // Create the request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                request.Method = "GET";
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json; charset=utf-8";
                request.Headers.Add("Authorization", $"Bearer {bearerToken}");

                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new SyndicationFeedException($"Server error (HTTP {response.StatusCode}: {response.StatusDescription})");
                    }

                    Stream responseStream = response.GetResponseStream();

                    using (Stream outputStream = File.Create(filename))
                    {
                        responseStream.CopyTo(outputStream);
                    }
                }

                byte[] fileBytes = File.ReadAllBytes(filename);
                if (fileBytes.Length.ToString() == fileLength && CalculateSha256(fileBytes) == sha256)
                {
                    downloadSuccessStatus = true;
                }
                else
                {
                    // Delete file as it fails SHA256 hash check and length check
                    File.Delete(filename);
                }
            }
            catch (Exception ex)
            {
                throw new SyndicationFeedException("Error downloading file", ex);
            }

            return downloadSuccessStatus;
        }

        /// <summary>
        /// Process xml returned into simplified atom list
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="categoryFilter"></param>
        /// <returns></returns>
        private static List<AtomEntry> ProcessEntries(string stream, string categoryFilter)
        {
            List<AtomEntry> entries = new List<AtomEntry>();

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                var namespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
                namespaceManager.AddNamespace("a", "http://www.w3.org/2005/Atom");
                namespaceManager.AddNamespace("e", "http://ns.electronichealth.net.au/ncts/syndication/asf/extensions/1.0.0");
                xmlDocument.LoadXml(stream);

                XmlNodeList nodeList = xmlDocument.SelectNodes("/a:feed/a:entry", namespaceManager);

                foreach (XmlNode node in nodeList)
                {
                    AtomEntry entry = new AtomEntry();
                    XmlNode titleNode = node.SelectSingleNode("./a:title", namespaceManager);
                    XmlNode hrefNode = node.SelectSingleNode("./a:link/@href", namespaceManager);
                    XmlNode lengthNode = node.SelectSingleNode("./a:link/@length", namespaceManager);
                    XmlNode shaNode = node.SelectSingleNode("./a:link/@e:sha256Hash", namespaceManager);
                    XmlNode typeNode = node.SelectSingleNode("./a:link/@type", namespaceManager);
                    XmlNode categoryNode = node.SelectSingleNode("./a:category/@term", namespaceManager);

                    entry.Title = titleNode?.InnerText ?? "";
                    entry.Category = categoryNode != null ? categoryNode.Value : "";

                    entry.Link = new AtomLink
                    {
                        Href = hrefNode != null ? hrefNode.Value : "",
                        Length = lengthNode != null ? lengthNode.Value : "",
                        Sha256Hash = shaNode != null ? shaNode.Value : "",
                        Type = typeNode != null ? typeNode.Value : ""
                    };

                    // Filtering on category
                    if (!string.IsNullOrWhiteSpace(categoryFilter))
                    {
                        if (entry.Category.Contains(categoryFilter))
                        {
                            entries.Add(entry);
                        }
                    }
                    else
                    {
                        entries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SyndicationFeedException("Error retrieving list of files available.", ex);
            }

            // Sort the results in descending order
            entries.Sort((p, q) => string.Compare(q.Link.Href, p.Link.Href, StringComparison.Ordinal));

            return entries;
        }

        /// <summary>
        /// Validates the file downloaded matchs the sha256 hash
        /// </summary>
        /// <param name="contentToHash"></param>
        /// <returns></returns>
        private static string CalculateSha256(byte[] contentToHash)
        {
            var cryptoTransformSha = new SHA256CryptoServiceProvider();
            byte[] hashBytes = cryptoTransformSha.ComputeHash(contentToHash);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }        
    }
}
