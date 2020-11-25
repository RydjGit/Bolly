using Bolly.Enums;
using Bolly.Interfaces;
using Bolly.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public class BlockKeyCheck : BlockBase
    {
        protected class KeyCheck
        {
            public IEnumerable<KeyCheckPattern> KeyCheckPatterns { get; set; }
            public bool RetryIfNotFound { get; set; }
        }

        protected class KeyCheckEqual : IKeyCheck
        {
            public KeyCheckPattern KeyCheckPattern { get; }

            public KeyCheckEqual(KeyCheckPattern keyCheckPattern)
            {
                KeyCheckPattern = keyCheckPattern;
            }

            public bool Execute(string source, string key)
            {
                if (source == key) return true;
                return false;
            }
        }

        protected class KeyCheckContains : IKeyCheck
        {
            public KeyCheckPattern KeyCheckPattern { get; }

            public KeyCheckContains(KeyCheckPattern keyCheckPattern)
            {
                KeyCheckPattern = keyCheckPattern;
            }

            public bool Execute(string source, string key)
            {
                if (source.Contains(key)) return true;
                return false;
            }
        }

        protected class KeyCheckLessthan : IKeyCheck
        {
            public KeyCheckPattern KeyCheckPattern { get; }

            public KeyCheckLessthan(KeyCheckPattern keyCheckPattern)
            {
                KeyCheckPattern = keyCheckPattern;
            }

            public bool Execute(string source, string key)
            {
                if (int.Parse(source) < int.Parse(key)) return true;
                return false;
            }
        }

        protected class KeyCheckGreaterthan : IKeyCheck
        {
            public KeyCheckPattern KeyCheckPattern { get; }

            public KeyCheckGreaterthan(KeyCheckPattern keyCheckPattern)
            {
                KeyCheckPattern = keyCheckPattern;
            }

            public bool Execute(string source, string key)
            {
                if (int.Parse(source) > int.Parse(key)) return true;
                return false;
            }
        }

        protected class KeyCheckRegexMatch : IKeyCheck
        {
            public KeyCheckPattern KeyCheckPattern { get; }

            public KeyCheckRegexMatch(KeyCheckPattern keyCheckPattern)
            {
                KeyCheckPattern = keyCheckPattern;
            }

            public bool Execute(string source, string key)
            {
                if (Regex.IsMatch(source, key)) return true;
                return false;
            }
        }

        private readonly KeyCheck _keyCheck;
        private readonly List<IKeyCheck> _keyCheckProcess;

        public BlockKeyCheck(string jsonString)
        {
            _keyCheck = JsonSerializer.Deserialize<KeyCheck>(jsonString);

            _keyCheckProcess = new List<IKeyCheck>();

            foreach (var keyCheckPattern in _keyCheck.KeyCheckPatterns)
            {
                switch (keyCheckPattern.Condition.ToLower())
                {
                    case "equal":
                        _keyCheckProcess.Add(new KeyCheckEqual(keyCheckPattern));
                        break;
                    case "contains":
                        _keyCheckProcess.Add(new KeyCheckContains(keyCheckPattern));
                        break;
                    case "lessthan":
                        _keyCheckProcess.Add(new KeyCheckLessthan(keyCheckPattern));
                        break;
                    case "greaterthan":
                        _keyCheckProcess.Add(new KeyCheckGreaterthan(keyCheckPattern));
                        break;
                    case "regexmatch":
                        _keyCheckProcess.Add(new KeyCheckRegexMatch(keyCheckPattern));
                        break;
                }
            }
        }

        public override async Task Execute(HttpClient httpclient, BotData botData)
        {
            bool isNotFound = true;

            foreach (var keyCheck in _keyCheckProcess)
            {
                string source = ReplaceValues(keyCheck.KeyCheckPattern.Source, botData);

                string key = ReplaceValues(keyCheck.KeyCheckPattern.Key, botData);

                if (keyCheck.Execute(source, key))
                {
                    Enum.TryParse(keyCheck.KeyCheckPattern.Status, true, out Status status);     
                    botData.Status = status;
                    isNotFound = false;
                }    
            }

            if (isNotFound && _keyCheck.RetryIfNotFound) botData.Status = Status.Retry;

            await Task.CompletedTask;
        }
    }
}
