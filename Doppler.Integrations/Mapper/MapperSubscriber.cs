using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Doppler.Integrations.Mapper.Interfaces;
using Doppler.Integrations.Models.Dtos;
using System.Text.RegularExpressions;
using System;
using Doppler.Integrations.Models.Dtos.Typeform;

namespace Doppler.Integrations.Mapper
{
    /// <inheritdoc/>
    public class MapperSubscriber : IMapperSubscriber
    {
        private readonly ILogger _log;
        private readonly HashSet<string> GENDER_FIELD_NAMES = new HashSet<string>(new[] { "GENDER", "GENERO", "SEX", "SEXO" }, StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> COUNTRY_FIELD_NAMES = new HashSet<string>(new[] { "PAIS", "COUNTRY" }, StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> BASIC_FIELD_NAMES = new HashSet<string>(new[] { "FIRSTNAME", "GENDER", "COUNTRY", "BIRTHDAY", "LASTNAME" , "CONSENT"}, StringComparer.OrdinalIgnoreCase);

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

            var fields = new List<CustomFieldDto>();
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

                    var newCustomeField = new CustomFieldDto { Name = fieldsNameAllowed[index], Value = value };
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

        public DopplerSubscriberDto TypeFormToSubscriberDTO(TypeformDTO rawSubscriber, ItemsDto allowedFields)
        {
            DopplerSubscriberDto dopplerSubscriber = new DopplerSubscriberDto();
            dopplerSubscriber.Email = rawSubscriber.form_response.answers.First(x => !String.IsNullOrEmpty(x.email)).email;

            if (String.IsNullOrEmpty(dopplerSubscriber.Email))
            {
                _log.LogWarning(String.Format("The response event: {0} to the form: {1} with ID: {2} has not included an email", rawSubscriber.event_id, rawSubscriber.form_response.definition.title, rawSubscriber.form_response.definition.id));
                throw new ArgumentNullException();
            }

            var answersById = rawSubscriber.form_response.answers.Where(x=> String.IsNullOrEmpty(x.email) ).ToDictionary(y => y.field.id);

            dopplerSubscriber.Fields = rawSubscriber.form_response.definition.fields.Where(x=> x.type !="email" ).Select(f=>
            {
                var name = GENDER_FIELD_NAMES.Contains(f.@ref) ? "GENDER" 
                : COUNTRY_FIELD_NAMES.Contains(f.@ref) ? "COUNTRY"
                : BASIC_FIELD_NAMES.Contains(f.@ref) ? f.@ref.ToUpper()
                : f.@ref;

                var questionType = GENDER_FIELD_NAMES.Contains(f.@ref) ? "gender"
                : COUNTRY_FIELD_NAMES.Contains(f.@ref) ? "country"
                : f.@ref == "consent" ? "consent"
                : Dictionaries.CustomFieldTypes.TryGetValue(f.type, out string convertedQuestionType) ? convertedQuestionType
                : null;

                var answerType = Dictionaries.CustomFieldTypes.TryGetValue(answersById[f.id].field.type, out string convertedAnswerType) ? convertedAnswerType
                : null;

                var answerValue = GetAnswerValue(answersById[f.id]);

                var value = GENDER_FIELD_NAMES.Contains(f.@ref) ? GetGenderValue(answerValue.ToString())
                    : COUNTRY_FIELD_NAMES.Contains(f.@ref) ? GetCountryValue(answerValue.ToString())
                    : answerValue;

                return new
                {
                    name,
                    questionType,
                    f.id,
                    answersById,
                    value
                };
            })
            .Where(y=> allowedFields.Items
            .Any(z=> z.Name==y.name && y.questionType==z.Type))
            .Select(h=> new CustomFieldDto
            {
                Name = h.name,
                Value = h.value
            })
            .ToList();

            return dopplerSubscriber;

        }

        private object GetAnswerValue(Answer answer)
        {
            IList<object> values = new List<object>();

            values.Add(answer.email);
            values.Add(answer.phone_number);
            values.Add(answer.boolean);
            values.Add(answer.number);
            values.Add(answer.text);
            values.Add(answer.date);
            if(answer.choice != null)
            {
                values.Add(answer.choice.label);
            }
            return values.First(x => x != null);
        }
    }
}
   