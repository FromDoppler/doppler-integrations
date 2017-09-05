using Newtonsoft.Json;

namespace Doppler.Integrations.Models.Dtos
{
    public class CustomeFieldDto
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public object Value { get; set; }
    }
}
