using Newtonsoft.Json;
using System.Collections.Generic;

namespace Doppler.Integrations.Models.Dtos
{
    public class DopplerSubscriberDto
    {
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public IList<CustomeFieldDto> Fields { get; set; }
    }
}
