using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Integrations.Models.Dtos.Typeform
{
    public class FormResponse
    {
        public Definition definition { get; set; }
        public List<Answer> answers { get; set; }
    }
}
