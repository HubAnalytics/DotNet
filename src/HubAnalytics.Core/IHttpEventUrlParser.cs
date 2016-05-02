namespace HubAnalytics.Core
{
    public interface IHttpEventUrlParser
    {
        bool Parse(string url, out string domain, out string name, out string type);
    }
}
