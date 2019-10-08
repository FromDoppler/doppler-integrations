using Newtonsoft.Json;

namespace Doppler.Integrations.Models.Dtos
{
    public class CustomFieldDto
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }
    }
}
