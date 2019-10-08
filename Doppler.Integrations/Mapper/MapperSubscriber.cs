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
        private readonly HashSet<string> BASIC_FIELD_NAMES = new HashSet<string>(new[] { "FIRSTNAME", "GENDER", "COUNTRY", "BIRTHDAY", "LASTNAME" }, StringComparer.OrdinalIgnoreCase);

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
            List<SimplifiedTypeformField> simplifiedFields = new List<SimplifiedTypeformField>();

            simplifiedFields = SimplifyTypeformFields(rawSubscriber);
            simplifiedFields = ChangeFieldsTypes(simplifiedFields);
            dopplerSubscriber.Fields = MatchToDopplerFields(simplifiedFields, allowedFields);

            dopplerSubscriber.Email = simplifiedFields
                .Find(x => x.QuestionType == "email" && x.Name == "email" && !String.IsNullOrEmpty(x.Value.ToString()))
                .Value
                .ToString();

            if (String.IsNullOrEmpty(dopplerSubscriber.Email))
            {
                _log.LogWarning(String.Format("The response event: {0} to the form: {1} with ID: {2} has not included an email", rawSubscriber.event_id, rawSubscriber.form_response.definition.title, rawSubscriber.form_response.definition.id));
                throw new ArgumentNullException();
            }

            return dopplerSubscriber;

        }

        private IList<CustomFieldDto> MatchToDopplerFields(List<SimplifiedTypeformField> simplifiedFields, ItemsDto allowedFields)
        {
            //custom field section
            var fieldsThatMatch = simplifiedFields
                .Where(x => allowedFields.Items
                .Any(y => y.Name == x.Name && y.Type == x.QuestionType))
                .Select(z => new CustomFieldDto()
                {
                    Name = z.Name,
                    Value = z.Value
                })
                .ToList();

            //basic field section
            fieldsThatMatch
                .AddRange(simplifiedFields
                .Where(x => BASIC_FIELD_NAMES.Contains(x.Name))
                .Select(y => new CustomFieldDto()
                {
                    Name = y.Name.ToUpper(),
                    Value = y.Value
                }));

            //transform gender and country fields values to doppler's codes for each

            fieldsThatMatch
                .Where(x=> GENDER_FIELD_NAMES.Contains(x.Name))
                .Select(x => x.Value = GetGenderValue(x.Value.ToString()))
                .ToList();

            fieldsThatMatch
                .Where(x => COUNTRY_FIELD_NAMES.Contains(x.Name))
                .Select(x => x.Value = GetCountryValue(x.Value.ToString()))
                .ToList();

            return fieldsThatMatch;
        }

        private List<SimplifiedTypeformField> ChangeFieldsTypes(List<SimplifiedTypeformField> simplifiedFields)
        {

            foreach (SimplifiedTypeformField field in simplifiedFields)
            {
                Dictionaries.CustomFieldTypes.TryGetValue(field.AnswerType, out string convertedAnswerType);
                field.AnswerType = convertedAnswerType;
                Dictionaries.CustomFieldTypes.TryGetValue(field.QuestionType, out string convertedQuestionType);
                field.QuestionType = convertedQuestionType;
            }

            return simplifiedFields;
        }

        private List<SimplifiedTypeformField> SimplifyTypeformFields(TypeformDTO rawSubscriber)
        {
            List<SimplifiedTypeformField> simplifiedFields = new List<SimplifiedTypeformField>();

            foreach (Field f in rawSubscriber.form_response.definition.fields)
            {
                string type = rawSubscriber.form_response.answers.Select(x => x.type).ToString();
                simplifiedFields.Add(
                    new SimplifiedTypeformField
                    {
                        Name = f.@ref,
                        QuestionType = f.type,
                        Id = f.id,
                        AnswerType = rawSubscriber.form_response.answers.First(x => x.field.id == f.id && x.field.type != null).type,
                        Value = GetAnswerValue(rawSubscriber.form_response.answers.First(x => x.field.id == f.id))
                    });
            }
            return simplifiedFields;
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
            return values.FirstOrDefault(x => x != null);
        }

    }
}
   