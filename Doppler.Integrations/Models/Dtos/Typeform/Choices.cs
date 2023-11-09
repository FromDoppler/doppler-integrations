using System.Collections.Generic;

namespace Doppler.Integrations.Models.Dtos.Typeform
{
    public class Choices
    {
        public List<string> ids { get; set; }
        public List<string> labels { get; set; }
        public List<string> @refs { get; set; }
    }
}