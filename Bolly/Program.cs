using Bolly.Blocks;
using Bolly.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bolly
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2) throw new ArgumentOutOfRangeException("Need to enter at least two arguments");

            var config = SetupConfig(args[0]);

            var combos = SetupCombos(args[1]);

            ClientManager clientManager;

            if (args.Length == 3)
            {
                var proxies = SetupProxies(args[2]);
                clientManager = new ClientManager(config).WithProxies(proxies);
            }
            else clientManager = new ClientManager(config);

            var checker = new Checker(config, combos, clientManager);

            var consoleManager = new ConsoleManager(checker);

            _ = consoleManager.StartUpdatingTitleAsync();

            await checker.StartAsync();

            Console.ResetColor();
            Console.WriteLine("END");

            await Task.Delay(-1);
        }

        private static Config SetupConfig(string configPath)
        {
            if (!File.Exists(configPath)) throw new FileNotFoundException($"{configPath} not found");

            string configContent = File.ReadAllText(configPath);

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(configContent);

            var settingsJsonString = jsonElement.GetProperty("Settings").ToString();

            var config = JsonSerializer.Deserialize<Config>(settingsJsonString);

            var blockJsonElements = jsonElement.GetProperty("Blocks").EnumerateArray();

            var blocks = new List<BlockBase>();

            foreach (var blockJsonElement in blockJsonElements)
            {
                string blockType = blockJsonElement.GetProperty("Block").GetString();

                switch (blockType.ToLower())
                {
                    case "request":
                        blocks.Add(new BlockRequest(blockJsonElement.ToString()));
                        break;
                    case "parse":
                        blocks.Add(new BlockParse(blockJsonElement.ToString()));
                        break;
                    case "captchasoler":
                        blocks.Add(new BlockCaptchaSolver(blockJsonElement.ToString()));
                        break;
                    case "keycheck":
                        blocks.Add(new BlockKeyCheck(blockJsonElement.ToString()));
                        break;
                }
            }

            config.Blocks = blocks;

            return config;
        }

        private static IEnumerable<Combo> SetupCombos(string combosPath)
        {
            if (!File.Exists(combosPath)) throw new FileNotFoundException($"{combosPath} not found");

            var lines = File.ReadAllLines(combosPath);
            return lines.Select(c => new Combo(c)).Where(c => c.IsValid);
        }

        private static IEnumerable<Proxy> SetupProxies(string proxiesPath)
        {
            if (!File.Exists(proxiesPath)) throw new FileNotFoundException($"{proxiesPath} not found");

            var lines = File.ReadAllLines(proxiesPath);
            return lines.Select(p => new Proxy(p)).Where(p => p.IsValid);
        }
    }
}
