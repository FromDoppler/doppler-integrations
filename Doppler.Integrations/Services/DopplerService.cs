using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Doppler.Integrations.Services.Interfaces;
using Doppler.Integrations.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Doppler.Integrations.Helpers.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;

namespace Doppler.Integrations.Services
{
    
    public class DopplerService: IDopplerService
    {
        private readonly HttpClient _client;
        private readonly IDopplerURLs _dopplerURLs;
        private readonly ILogger _log;
		private readonly Regex _cannotSuscribeErrorRegex = new Regex("\"type\": \"/docs/errors/400\\.[9|6]-");

        public DopplerService(IDopplerURLs dopplerURLs, ILogger<DopplerService> log)
        {
            _dopplerURLs = dopplerURLs;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _log = log;
        }

        private void UpdateApiKey(string apiKey)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", apiKey);
        }

        /// <inheritdoc/>
        public async Task<IActionResult> CreateNewSubscriberAsync(string apiKey, string accountName, long idList, DopplerSubscriberDto subscriber)
        {
            UpdateApiKey(apiKey);

            var url = _dopplerURLs.GetImportSubscriversURL(accountName, idList);
            var subscriberObjectString = new StringContent(JsonConvert.SerializeObject(subscriber), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, subscriberObjectString);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode && !_cannotSuscribeErrorRegex.IsMatch(responseBody))
            {
                _log.LogError(responseBody);
                throw new Exception(responseBody);
            }

			if (!response.IsSuccessStatusCode && _cannotSuscribeErrorRegex.IsMatch(responseBody))
			{
				// We introduced this patch because in these scenarios we do not want to send an error to unbounce
				_log.LogError(responseBody);
				var contentResult = new ContentResult
				{
					Content = responseBody,
					ContentType = response.Headers.ToString(),
					StatusCode = (int)response.StatusCode
					// Maybe it will be necessary:
					// StatusCode = 200
				};
				return new OkObjectResult(contentResult);
			}
			else
			{
				var contentResult = new ContentResult
				{
					Content = responseBody,
					ContentType = response.Headers.ToString(),
					StatusCode = (int)response.StatusCode
				};
				return new OkObjectResult(contentResult);
			}
        }

        /// <inheritdoc/>
        public async Task<ItemsDto> GetFields(string apiKey, string accountName)
        {
            UpdateApiKey(apiKey);

            var url = _dopplerURLs.GetFieldListURL(accountName);
            var response = await _client.GetAsync(url);
            var jsonString = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                _log.LogError(jsonString);
                throw new Exception(jsonString);
            }
            
            var itemDto = JsonConvert.DeserializeObject<ItemsDto> (jsonString);

            return itemDto;
        }
    }
}
