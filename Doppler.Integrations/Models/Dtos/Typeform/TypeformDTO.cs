using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Doppler.Integrations.Models.Dtos.Typeform
{
    public class TypeformDTO
    {
        public FormResponse form_response { get; set; }
        public string event_id { get; set; }
    }
}
