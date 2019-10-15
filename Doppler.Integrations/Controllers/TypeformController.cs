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

            if (string.IsNullOrWhiteSpace(accountName))
            {
                _log.LogError("Account Name should not be Null or empty");
                return BadRequest("{\"ErrorMessage\":\"An account name must be provided\",\"HelpLink\":\"https://help.fromdoppler.com/en/how-integrate-doppler-typeform\"}");
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _log.LogError("API Key should not be Null or empty");
                return BadRequest("{\"ErrorMessage\":\"An API Key must be provided\",\"HelpLink\":\"https://help.fromdoppler.com/en/how-integrate-doppler-typeform\"}");
            }

            var accountN = accountName.Replace(' ', '+');

            try
            {
                
                var itemList = await _dopplerService.GetFields(apiKey, accountN);//we get the user's custom fields
                var subscriber = _mapper.TypeFormToSubscriberDTO(subscriberDto, itemList);
                var origin = "Typeform";
                var result = await _dopplerService.CreateNewSubscriberAsync(apiKey, accountN, idList, subscriber,origin);

                return result;
            }
            catch (Exception ex)
            {
                _log.LogError(new EventId(), ex, string.Format("AccountName: {0} | ID_List: {1} | Status: Add subscriber has failed", accountN, idList));
                return new BadRequestResult();
            }
        }
    }
}
