using Bolly.Models;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public abstract class BlockBase
    {
        private const string Pattern = "<(.*?)>";

        public abstract Task Execute(HttpClient httpClient, BotData botData);

        protected string ReplaceValues(string input, BotData botData)
        {
            var matches = Regex.Matches(input, Pattern);

            foreach (Match match in matches)
            {
                switch (match.Groups[1].Value.ToLower())
                {
                    case "username":
                        input = input.Replace(match.Value, botData.Combo.Username);
                        break;
                    case "password":
                        input = input.Replace(match.Value, botData.Combo.Password);
                        break;
                    case "source":
                        input = input.Replace(match.Value, botData.Source);
                        break;
                    case "address":
                        input = input.Replace(match.Value, botData.Address);
                        break;
                    case "responsecode":
                        input = input.Replace(match.Value, botData.ResponseCode.ToString());
                        break;
                    default:   
                        if (botData.Variables.TryGetValue(match.Groups[1].Value, out var value)) input = input.Replace(match.Value, value);
                        break;
                }
            }

            return input;
        }
    }
}
