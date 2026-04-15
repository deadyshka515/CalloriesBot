using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CalloriesBot
{
    public class ModelRequest
    {
        public string? model { get; set; }
        public string? system_prompt { get; set; }
        public string? input { get; set; }
    }
    public class ModelResponse
    {
        [JsonPropertyName("model_instance_id")]
        public string ModelInstanceId { get; set; } = string.Empty;

        [JsonPropertyName("output")]
        public List<OutPutItem> OutPut { get; set; } = new();

        [JsonPropertyName("stats")]
        public Stats Stats { get; set; } = new();

        [JsonPropertyName("response_id")]
        public string ResponseId { get; set; } = string.Empty ;
    }
    public class OutPutItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class Stats
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("total_output_tokens")]
        public int TotalOutputTokens { get; set; }

        [JsonPropertyName("reasoning_output_tokens")]
        public int ReasoningOutputTokens { get; set; }

        [JsonPropertyName("tokens_per_second")]
        public double TokensPerSecond { get; set; }

        [JsonPropertyName("time_to_first_token_seconds")]
        public double TimeToFirstTokenSeconds { get; set; }
    }
}
