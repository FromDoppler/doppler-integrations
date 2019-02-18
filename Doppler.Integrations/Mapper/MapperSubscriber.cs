using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Doppler.Integrations.Mapper.Interfaces;
using Doppler.Integrations.Models.Dtos;
using System.Text.RegularExpressions;
using System;

namespace Doppler.Integrations.Mapper
{
    /// <inheritdoc/>
    public class MapperSubscriber : IMapperSubscriber
    {
        private readonly ILogger _log;
        private readonly HashSet<string> GENDER_FIELD_NAMES = new HashSet<string>(new[] { "GENDER", "GENERO", "SEX", "SEXO" }, StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> COUNTRY_FIELD_NAMES = new HashSet<string>(new[] { "PAIS", "COUNTRY" }, StringComparer.OrdinalIgnoreCase);
        public MapperSubscriber(ILogger<MapperSubscriber> log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public DopplerSubscriberDto ToDopplerSubscriberDto(IDictionary<string, IList<object>> rawSubscriber, ItemsDto allowedFields)
        {
            var email = GetEmailValue(rawSubscriber);
            if (rawSubscriber.ContainsKey("email"))
            {
                email = rawSubscriber["email"][0].ToString();
                rawSubscriber.Remove("email");
            }
            else
            {
                _log.LogWarning("The current user has not included an EMAIL field");
            }

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
                    else if (GENDER_FIELD_NAMES.Contains(fieldsNameAllowed[index]))
                    {
	                    value = GetGenderValue(value);
                    }
                    else if (COUNTRY_FIELD_NAMES.Contains(fieldsNameAllowed[index]))
                    {
                        value = GetCountryValue(value);
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
            string convertedGenderValue;
            if (!Dictionaries.GenderByFriendlyName.TryGetValue(genderValue, out convertedGenderValue))
            {
                convertedGenderValue = null;
            }
            return convertedGenderValue;
        }

        private string GetBooleanValue(string booleanFieldValue)
        {
            string convertedBooleanFieldValue;
            if (!Dictionaries.BooleanValueByFriendlyName.TryGetValue(booleanFieldValue, out convertedBooleanFieldValue))
            {
                convertedBooleanFieldValue = null;
            }
            return convertedBooleanFieldValue;

        }

        private string GetCountryValue(string countryValue)
        {
            string convertedcountryValue;
            if (!Dictionaries.CountriesByFriendlyName.TryGetValue(countryValue, out convertedcountryValue))
            {
                convertedcountryValue = null;
            }
            return convertedcountryValue;

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
