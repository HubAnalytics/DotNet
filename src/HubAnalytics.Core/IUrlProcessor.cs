namespace HubAnalytics.Core
{
    public interface IUrlProcessor
    {
        /// <summary>
        /// Implement to update a URL before it is logged. If the return is set to null then nothing will be logged (no page view, external http request etc.)
        /// </summary>        
        string Process(string url);
    }
}
