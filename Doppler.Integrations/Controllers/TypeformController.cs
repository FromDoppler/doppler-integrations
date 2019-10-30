using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Doppler.Integrations.Models.Dtos.Typeform;
using System.Threading.Tasks;
using Doppler.Integrations.Mapper.Interfaces;
using Doppler.Integrations.Services.Interfaces;


namespace Doppler.Integrations.Controllers
{
    [Route("api/[controller]")]
    public class TypeformController : Controller
    {
        private readonly IDopplerService _dopplerService;
        private readonly IMapperSubscriber _mapper;
        private readonly ILogger _log;

        public TypeformController(IDopplerService dopplerService, IMapperSubscriber mapper, ILogger<TypeformController> log)
        {
            _dopplerService = dopplerService;
            _mapper = mapper;
            _log = log;
        }

        [HttpPost]
        public async Task<IActionResult> AddSubscriber(string accountName, long idList, string apiKey, [FromBody] TypeformDTO subscriberDto)
        {
            const string HELP_LINK = "https://help.fromdoppler.com/en/how-integrate-doppler-typeform";

            if (string.IsNullOrWhiteSpace(accountName))
            {
                _log.LogError("Account Name should not be Null or empty");
                return BadRequest(new
                {
                    ErrorMessage = "An account name must be provided",
                    HelpLink = HELP_LINK
                });
            }
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _log.LogError("API Key should not be Null or empty");
                return BadRequest(new
                {
                    ErrorMessage = "An API key must be provided",
                    HelpLink = HELP_LINK
                });
            }

            try
            {
                
                var itemList = await _dopplerService.GetFields(apiKey, accountName); //we get the user's custom fields
                var subscriber = _mapper.TypeFormToSubscriberDTO(subscriberDto, itemList);
                var requestOrigin = "Typeform";
                var result = await _dopplerService.CreateNewSubscriberAsync(apiKey, accountName, idList, subscriber, requestOrigin);

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(new EventId(), ex, string.Format("AccountName: {0} | ID_List: {1} | Status: Add subscriber has failed", accountName, idList));
                return new BadRequestResult();
            }
        }
    }
}
