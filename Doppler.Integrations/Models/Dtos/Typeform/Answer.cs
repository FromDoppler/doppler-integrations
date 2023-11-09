using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Doppler.Integrations.Models.Dtos.Typeform
{
    public class Answer
    {
        public string type { get; set; }
        public string email { get; set; }
        public Field field { get; set; }
        public string text { get; set; }
        public Choice choice { get; set; }
        public Choices choices { get; set; }
        public string phone_number { get; set; }
        public bool? boolean { get; set; }
        public int? number { get; set; }
        public string date { get; set; }
        public string url { get; set; }
    }
}