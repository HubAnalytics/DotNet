using System.Text.RegularExpressions;

namespace HubAnalytics.Core.HttpEventUrlParsers
{
    public class GenericParser : IHttpEventUrlParser
    {
        private static readonly Regex RegEx = new Regex(@"(https?):\/\/([^:\/]*)(?::[0-9]*)?\/?([^?]*)", RegexOptions.IgnoreCase);

        public bool Parse(string url, out string domain, out string name, out string type)
        {
            Match match = RegEx.Match(url);
            name = null;
            domain = null;
            type = null;
            if (match.Success)
            {
                domain = $"{match.Groups[2].Value}";
                name = match.Groups.Count >= 4 ? match.Groups[3].Value : null;
                type = "generic";
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                name = null;
            }

            return match.Success;
        }
    }
}
