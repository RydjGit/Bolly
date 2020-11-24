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
            public string ParseName { get; set; }
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
                int firstPosition = source.IndexOf(firstInput) + secondInput.Length;
                int secondPosition = source.IndexOf(secondInput, firstPosition);
                string result = source[firstPosition..secondPosition];
                if (!string.IsNullOrEmpty(result)) return (true, result);
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
                var match = Regex.Match(source, firstInput).Groups[0];
                if (match.Success) return (true, match.Value);
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
                botData.Variables.Add(_parse.ParseName, result);
                if (_parse.Capture) botData.Captues.Add(_parse.ParseName, result);
            }

            await Task.CompletedTask;
        }
    }
}
