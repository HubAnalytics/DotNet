using System.Text.RegularExpressions;

namespace HubAnalytics.Core.HttpEventUrlParsers
{
    internal class AzureQueueStorageParser : IHttpEventUrlParser
    {
        private static readonly Regex RegEx = new Regex(@"(https?):\/\/([a-z0-9]*)\.(queue.core.windows.net)(?::[0-9]*)?\/?([a-z0-9\-]*)(\/messages\/?)?(.*)?", RegexOptions.IgnoreCase);

        public bool Parse(string url, out string domain, out string name, out string type)
        {
            Match match = RegEx.Match(url);
            name = null;
            domain = null;
            type = null;
            if (match.Success)
            {
                domain = $"{match.Groups[2].Value}.{match.Groups[3].Value}";
                name = match.Groups.Count >= 5 ? match.Groups[4].Value : null;
                type = "azurequeue";
            }
            
            return !string.IsNullOrWhiteSpace(name);
        }
    }
}
