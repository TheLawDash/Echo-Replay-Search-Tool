using System.Text.Json.Serialization;

namespace Echo_Replay_Search_Tool
{
    public class SearchModel
    {
        [JsonPropertyName("teams")]
        public List<Teams>? TeamsList { get; set; }
    }
    public class Teams
    {
        [JsonPropertyName("players")]
        public List<Players>? Players { get; set; }
    }
    public class Players
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
