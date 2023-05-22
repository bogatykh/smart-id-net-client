using System.Text.Json.Serialization;

namespace SK.SmartId.Rest.Dao
{
    public class RequestProperties
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ShareMdClientIpAddress;

        [JsonIgnore]
        public bool HasProperties => ShareMdClientIpAddress != null;
    }
}