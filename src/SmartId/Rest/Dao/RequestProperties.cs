using System.Text.Json.Serialization;

namespace SK.SmartId.Rest.Dao
{
    public class RequestProperties
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("shareMdClientIpAddress")]
        public bool? ShareMdClientIpAddress { get; set; }

        [JsonIgnore]
        public bool HasProperties => ShareMdClientIpAddress != null;
    }
}
