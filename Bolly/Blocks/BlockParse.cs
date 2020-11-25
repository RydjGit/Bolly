using Bolly.Interfaces;
using Bolly.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public class BlockParse : BlockBase
    {
        protected class Parse
        {
            public string VarName { get; set; }
            public string Source { get; set; }
            public string Methode { get; set; }
            public string FirstInput { get; set; }
            public string SecondInput { get; set; }
            public bool Capture { get; set; }
        }

        protected class ParseLR : IParse
        {
            public (bool success, string result) Execute(string source, string firstInput, string secondInput)
            {
                string pattern = firstInput + "(.*?)" + secondInput;
                var match = Regex.Match(source, pattern);

                if (match.Success) return (true, match.Groups[1].Value.Trim());
                return (false, null);
            }
        }

        protected class ParseJson : IParse
        {
            public (bool success, string result) Execute(string source, string firstInput, string secondInput)
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(source);
                if (jsonElement.TryGetProperty(firstInput, out var result)) return (true, result.GetString());
                return (false, null);
            }
        }

        protected class ParseRegex : IParse
        {
            public (bool success, string result) Execute(string source, string firstInput, string secondInput)
            {
                var match = Regex.Match(source, firstInput);

                if (match.Success) return (true, match.Groups[0].Value.Trim());
                return (false, null);
            }
        }

        private readonly Parse _parse;
        private readonly IParse _parseProcess;

        public BlockParse (string jsonString)
        {
            _parse = JsonSerializer.Deserialize<Parse>(jsonString);

            switch (_parse.Methode.ToLower())
            {
                case "lr":
                    _parseProcess = new ParseLR();
                    break;
                case "json":
                    _parseProcess = new ParseJson();
                    break;
                case "regex":
                    _parseProcess = new ParseRegex();
                    break;
            }
        }

        public override async Task Execute(HttpClient httpclient, BotData botData)
        {
            string source = ReplaceValues(_parse.Source, botData);

            var (success, result) = _parseProcess.Execute(source, _parse.FirstInput, _parse.SecondInput);

            if (success)
            {
                botData.Variables.Add(_parse.VarName, result);
                if (_parse.Capture) botData.Captues.Add(_parse.VarName, result);
            }

            await Task.CompletedTask;
        }
    }
}
