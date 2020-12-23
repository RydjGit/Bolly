using Bolly.Interfaces;
using Bolly.Models;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public class BlockParse : Block
    {
        private class Parse
        {
            public string ParseName { get; set; }
            public string Source { get; set; }
            public string Methode { get; set; }
            public string FirstInput { get; set; }
            public string SecondInput { get; set; }
            public bool Capture { get; set; }
        }

        private class ParseJson : IParse
        {
            public bool IsSuccess { get; set; }
            public string Result { get; set; }

            public void Execute(string source, string firstInput, string secondInput)
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(source);
                if (jsonElement.TryGetProperty(firstInput, out var result))
                {
                    IsSuccess = true;
                    Result = result.ToString();
                }
            }
        }

        private class ParseRegex : IParse
        {
            public bool IsSuccess { get; set; }
            public string Result { get; set; }

            public void Execute(string source, string firstInput, string secondInput)
            {
                var match = Regex.Match(source, firstInput).Groups[secondInput];
                if (match.Success)
                {
                    IsSuccess = true;
                    Result = match.Value;
                }
            }
        }

        private readonly Parse _parse;
        private readonly IParse _parseProcess;

        public BlockParse (string jsonString)
        {
            _parse = JsonSerializer.Deserialize<Parse>(jsonString);

            switch (_parse.Methode.ToLower())
            {
                case "json":
                    _parseProcess = new ParseJson();
                    break;
                case "regex":
                    _parseProcess = new ParseRegex();
                    break;
            }
        }

        public override async Task Execute(Combo combo, HttpClient httpclient, BotData botData)
        {
            string source = ReplaceValues(_parse.Source, combo, botData);

            _parseProcess.Execute(source, _parse.FirstInput, _parse.SecondInput);

            if (_parseProcess.IsSuccess)
            {
                if (!botData.Variables.TryAdd(_parse.ParseName, _parseProcess.Result)) botData.Variables[_parse.ParseName] = _parseProcess.Result;
                if (_parse.Capture) if (!botData.Captues.TryAdd(_parse.ParseName, _parseProcess.Result)) botData.Captues[_parse.ParseName] = _parseProcess.Result;
            }

            await Task.CompletedTask;
        }
    }
}
