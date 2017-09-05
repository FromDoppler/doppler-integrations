using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Doppler.Integrations.Models.Dtos;

namespace Doppler.Integrations.Services.Interfaces
{
    public interface IDopplerService
    {
        /// <summary>
        /// This method create a new subscriber on the list (idList) for the account (accountName)
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="accountName"> Account where we want to include a new subscriber. </param>
        /// <param name="idList"> Id of the list that we want to include a new subscriber. </param>
        /// <param name="subscriber">Subscriber to be included. </param>
        /// <returns> An Action Result with the status of the operation. </returns>
        Task<IActionResult> CreateNewSubscriberAsync(string apiKey, string accountName, long idList, DopplerSubscriberDto subscriber);

        /// <summary>
        /// Get a list of allowed fields by an Account.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="accountName"> Account to which we are going to consult. </param>
        /// <returns>A ItemDto which has a list of Fields. </returns>
        Task<ItemsDto> GetFields(string apiKey, string accountName);
    }
}
