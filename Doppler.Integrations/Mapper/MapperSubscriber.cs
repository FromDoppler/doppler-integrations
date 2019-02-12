using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Doppler.Integrations.Mapper.Interfaces;
using Doppler.Integrations.Models.Dtos;
using System.Text.RegularExpressions;

namespace Doppler.Integrations.Mapper
{
    /// <inheritdoc/>
    public class MapperSubscriber : IMapperSubscriber
    {
        private readonly ILogger _log;
		private readonly string[] GENDER_FIELD_NAMES = new[] { "GENDER", "GENERO", "SEX", "SEXO" };
		private readonly string[] COUNTRY_FIELD_NAMES = new[] { "PAIS", "COUNTRY"};
		public MapperSubscriber(ILogger<MapperSubscriber> log)
        {
            _log = log;
	}

        /// <inheritdoc/>
        public DopplerSubscriberDto ToDopplerSubscriberDto(IDictionary<string, IList<object>> rawSubscriber, ItemsDto allowedFields)
        {
            var email = GetEmailValue(rawSubscriber);
            var fieldsUpperNameAllowed = allowedFields.Items.Select(i => i.Name.ToUpper()).ToList();
            var fieldsNameAllowed = allowedFields.Items.Select(i => i.Name).ToList();
            var fields = new List<CustomeFieldDto>();
            var fieldsNotEnabled = new List<string>();
			
            foreach (KeyValuePair<string, IList<object>> entry in rawSubscriber)
            {
                var index = fieldsUpperNameAllowed.IndexOf(entry.Key.ToUpper());
                if (index >= 0)
                {
                    var type = allowedFields.Items[index].Type;
                    var value = entry.Value[0].ToString();
					if (type == FieldTypes.Boolean.GetDescription())
					{
						value = GetBooleanValue(value);
					}
					else if (GENDER_FIELD_NAMES.Contains(fieldsNameAllowed[index].ToUpper()))
					{
						value = GetGenderValue(value);
					}
					else if (COUNTRY_FIELD_NAMES.Contains(fieldsNameAllowed[index].ToUpper()))
					{
						CountryDictionary countryDictionary = new CountryDictionary();
						value = countryDictionary.CountryCodePairs.FirstOrDefault(x => (x.Value[0] == value || x.Value[1] == value)).Key.ToUpper();
					}


					var newCustomeField = new CustomeFieldDto { Name = fieldsNameAllowed[index], Value = value };
                    fields.Add(newCustomeField);
                }
                else
                {
                    fieldsNotEnabled.Add(entry.Key);
                }
            }

            if (fieldsNotEnabled.Count > 0)
            {
                LogFieldsRejected(fieldsNotEnabled);
            }

            return new DopplerSubscriberDto
            {
                Email = email,
                Fields = fields
            };
        }

		private string GetGenderValue(string genderValue)
		{	
			switch (genderValue.ToUpper())
			{
				case "FEMENINO":
				case "MUJER":
				case "FEMALE":
				case "WOMAN":
					return "F";
				case "MASCULINO":
				case "HOMBRE":
				case "MALE":
				case "MAN":
					return "M";
				default:
					return "N";
			}
							
		}

        private string GetBooleanValue(string value)
        {
            value = value.ToUpper();
			switch (value)
			{
				case "SI":
				case "YES":
				case "VERDADERO":
				case "TRUE":
					return "true";
				case "NO":
				case "FALSO":
				case "FALSE":
					return "false";
				default:
					return "";
			}
        }

        private string GetEmailValue(IDictionary<string, IList<object>> rawSubscriber)
        {
            var email = string.Empty;

            if (rawSubscriber.ContainsKey("email"))
            {
                email = rawSubscriber["email"][0].ToString();
                rawSubscriber.Remove("email");
            }
            else if (rawSubscriber.ContainsKey("EMAIL"))
            {
                email = rawSubscriber["EMAIL"][0].ToString();
                rawSubscriber.Remove("EMAIL");
            }
            else
            {
                _log.LogWarning("The current user has not included an EMAIL field");
            }

            return email;
        }

        private void LogFieldsRejected(List<string> valuesList)
        {
            var warningFields = valuesList.Aggregate((current, next) => current + ", " + next);
            var warningDescription = string.Format("The following fields have been rejected because they are not allowed on Doppler's subscriber: $0", warningFields);
            _log.LogWarning(warningDescription);
        }
    }
}
