using System.Collections.Generic;
using Newtonsoft.Json;

namespace Tasks.Models
{
    public sealed class BingImage
    {
        [JsonProperty("startdate")]
        public string StartDate { get; set; }
        [JsonProperty("fullstartdate")]
        public string FullStartDate { get; set; }
        [JsonProperty("enddate")]
        public string EndDate { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("urlbase")]
        public string UrlBase { get; set; }
        [JsonProperty("copyright")]
        public string Copyright { get; set; }
        [JsonProperty("copyrightlink")]
        public string CopyrightLink { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
    }

    public sealed class Bing
    {
        [JsonProperty("images")]
        public IEnumerable<BingImage> Images { get; set; }
    }
}