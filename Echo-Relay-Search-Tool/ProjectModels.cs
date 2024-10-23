using System.Text.Json.Serialization;

namespace Echo_Replay_Search_Tool
{
    public class SearchModel
    {
        [JsonPropertyName("client_name")]
        public string? Username { get; set; }
    }
}
