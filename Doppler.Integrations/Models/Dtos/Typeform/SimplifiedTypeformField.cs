using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.Integrations.Models.Dtos.Typeform
{
    public class SimplifiedTypeformField
    {
        public string Name { get; set; }
        public string QuestionType { get; set; }
        public object Value { get; set; }
        public string AnswerType { get; set; }
        public string Id { get; set; }

    }
}
