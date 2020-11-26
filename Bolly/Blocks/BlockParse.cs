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
            public bool Execute(string source, string firstInput, string secondInput, out string value)
            {
                value = null;

                string pattern = firstInput + "(.*?)" + secondInput;
                var match = Regex.Match(source, pattern);

                if (match.Success)
                { 
                    value = match.Groups[1].Value.Trim();
                    return true;
                }

                return false;
            }
        }

        protected class ParseJson : IParse
        {
            public bool Execute(string source, string firstInput, string secondInput, out string value)
            {
                value = null;

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(source);

                if (jsonElement.TryGetProperty(firstInput, out var result))
                {
                    value = result.GetString();
                    return true;
                }

                return false;
            }
        }

        protected class ParseRegex : IParse
        {
            public bool Execute(string source, string firstInput, string secondInput, out string value)
            {
                value = null;

                var match = Regex.Match(source, firstInput);

                if (match.Success)
                {
                    value = match.Groups[0].Value.Trim();
                    return true;
                }

                return false;
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

            if (_parseProcess.Execute(source, _parse.FirstInput, _parse.SecondInput, out string value))
            {
                if (!botData.Variables.TryAdd(_parse.VarName, value)) botData.Variables[_parse.VarName] = value;
                if (_parse.Capture) if (!botData.Captues.TryAdd(_parse.VarName, value)) botData.Captues[_parse.VarName] = value;
            }

            await Task.CompletedTask;
        }
    }
}
