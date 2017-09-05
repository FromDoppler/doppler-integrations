namespace Doppler.Integrations.Helpers.Interfaces
{
    /// <summary> Provides the url enabled to be used with the Doppler Relay service</summary>
    public interface IDopplerURLs
    {
        string GetFieldListURL(string accountName);

        string GetImportSubscriversURL(string accountName, long idList);
    }
}
