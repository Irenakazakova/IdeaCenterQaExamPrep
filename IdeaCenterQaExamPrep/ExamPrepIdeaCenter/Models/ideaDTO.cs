using System.Text.Json.Serialization;

namespace ExamPrepIdeaCenter.Models
{
    internal class ideaDTO
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
