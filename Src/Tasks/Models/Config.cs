using Newtonsoft.Json;

namespace Tasks.Models
{
    public sealed class Config
    {
        [JsonProperty("access_key")]
        public string AccessKeyUnsplash { get; set; }
        
        [JsonProperty("access_key_pexels")]
        public string AccessKeyPexels { get; set; }
    }
}