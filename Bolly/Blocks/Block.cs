using Bolly.Models;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public abstract class Block
    {
        private const string Pattern = "<(.*?)>";

        public abstract Task Execute(Combo combo, HttpClient httpClient, BotData botData); 

        protected string ReplaceValues(string input, Combo combo, BotData botData)
        {
            var matches = Regex.Matches(input, Pattern);

            foreach (Match match in matches)
            {
                switch (match.Groups[1].Value.ToLower())
                {
                    case "username":
                        input = input.Replace(match.Value, combo.Username);
                        break;
                    case "password":
                        input = input.Replace(match.Value, combo.Password);
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
                        if (botData.Variables.ContainsKey(match.Groups[1].Value)) input = input.Replace(match.Value, botData.Variables[match.Groups[1].Value]);
                        break;
                }
            }

            return input;
        }
    }
}
