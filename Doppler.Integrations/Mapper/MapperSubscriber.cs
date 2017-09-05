using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Doppler.Integrations.Mapper.Interfaces;
using Doppler.Integrations.Models.Dtos;

namespace Doppler.Integrations.Mapper
{
    /// <inheritdoc/>
    public class MapperSubscriber: IMapperSubscriber
    {
        private readonly ILogger _log;

        public MapperSubscriber(ILogger<MapperSubscriber> log)
        {
            _log = log;
        }

        /// <inheritdoc/>
        public DopplerSubscriberDto ToDopplerSubscriberDto(IDictionary<string, IList<object>> rawSubscriber, ItemsDto allowedFields)
        {
            var email = string.Empty;
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
                if ( index >= 0 )
                {
                    fields.Add(new CustomeFieldDto { Name = fieldsNameAllowed[index], Value = entry.Value[0] });
                }
                else
                {
                    fieldsNotEnabled.Add(entry.Key);
                }
            }

            if (fieldsNotEnabled.Count > 0)
            {
                var warningFields = fieldsNotEnabled.Aggregate((current, next) => current + ", " + next);
                var warningDescription = string.Format("The following fields have been rejected because they are not allowed on Doppler's subscriber: $0", warningFields);
                _log.LogWarning(warningDescription);
            }

            return new DopplerSubscriberDto
            {
                Email = email,
                Fields = fields
            };
        }
    }
}
