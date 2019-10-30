using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Integrations.Models.Dtos.Typeform
{
    public class Choice
    {
        public string id { get; set; }
        public string label { get; set; }
        public string @ref { get; set; }
    }
}