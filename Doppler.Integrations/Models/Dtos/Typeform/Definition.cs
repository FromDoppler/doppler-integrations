using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Integrations.Models.Dtos.Typeform
{
    public class Definition
    {
        public List<Field> fields { get; set; }
        public string id { get; set; }
        public string title { get; set; }
    }
}