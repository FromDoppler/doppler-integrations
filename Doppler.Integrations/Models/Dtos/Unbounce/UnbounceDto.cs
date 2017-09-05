using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Doppler.Integrations.Models.Dtos.Unbounce
{
    [ModelBinder(BinderType = typeof(UnbounceDtoModelBinder))]
    public class UnbounceDto
    {
        public Dictionary<string, IList<object>> DataJSON { get; set; }
        //public Dictionary<string, object> DataJSON { get; set; }

        public string DataXML { get; set; }

        public string Variant { get; set; }

        public string PageName { get; set; }

        public string PageID { get; set; }

        public string PageURL { get; set; }
    }
}
