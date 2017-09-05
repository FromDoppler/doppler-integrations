using Doppler.Integrations.Helpers.Interfaces;

namespace Doppler.Integrations.Helpers
{
    /// <inheritdoc/>
    public class DopplerURLs: IDopplerURLs
    {
        private const string IMPORT_SUBSCRIBERS_URL = "/accounts/{0}/lists/{1}/subscribers";
        private const string FIELDS_URL = "/accounts/{0}/fields";

        public DopplerURLs(string baseURL)
        {
            BaseURL = baseURL;
        }

        private string BaseURL { get; set; }

        public string GetImportSubscriversURL(string accountName, long idList)
        {
            var url = BaseURL + string.Format(IMPORT_SUBSCRIBERS_URL, accountName, idList.ToString());
            return url;
        }

        public string GetFieldListURL(string accountName)
        {
            var url = BaseURL + string.Format(FIELDS_URL, accountName);
            return url;
        }
    }
}
