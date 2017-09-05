using System.Collections.Generic;
using Doppler.Integrations.Models.Dtos;

namespace Doppler.Integrations.Mapper.Interfaces
{
    /// <summary> This class provides the logic needed to convert from a DTO's object to an inner one. 
    /// I have decided not to create internal models because I consider that in this case, it would not be necessary.
    /// </summary>
    public interface IMapperSubscriber
    {
        /// <summary>
        /// This method converts a dictionary to a DopplerSubscriberDto, based on the list of allowed Fields
        /// </summary>
        /// <param name="rawSubscriber"> Dictionary with all possible fields to be included. </param>
        /// <param name="allowedFields"> List of allowed fields </param>
        /// <returns>a new DopplerSubscriberDto</returns>
        DopplerSubscriberDto ToDopplerSubscriberDto(IDictionary<string, IList<object>> rawSubscriber, ItemsDto allowedFields);
    }
}
