using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Doppler.Integrations.Models.Dtos.Unbounce;
using System.Threading.Tasks;
using Doppler.Integrations.Mapper.Interfaces;
using Doppler.Integrations.Services.Interfaces;

namespace Doppler.Integrations.Controllers
{
    [Route("api/[controller]")]
    public class UnbounceController : Controller
    {
        private readonly IDopplerService _dopplerService;
        private readonly IMapperSubscriber _mapper;
        private readonly ILogger _log;

        public UnbounceController(IDopplerService dopplerService, IMapperSubscriber mapper, ILogger<UnbounceController> log)
        {
            _dopplerService = dopplerService;
            _mapper = mapper;
            _log = log;
        }

        [HttpPost]
        public async Task<IActionResult> AddSubscriber(string accountName, long idList, string apiKey, [FromForm] UnbounceDto subscriberDto)
        {
            if (string.IsNullOrWhiteSpace(accountName))
            {
                _log.LogError("Account Name should not be Null or empty");
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _log.LogError("API Key should not be Null or empty");
                return BadRequest();
            }

            var accountN = accountName.Replace(' ', '+');

            try
            {
                var itemList = await _dopplerService.GetFields(apiKey, accountN);
                var subscriber = _mapper.ToDopplerSubscriberDto(subscriberDto.DataJSON, itemList);
                var result = await _dopplerService.CreateNewSubscriberAsync(apiKey, accountN, idList, subscriber);

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