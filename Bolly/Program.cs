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

            string configFile = args[0];

            CheckExistenceFile(configFile);

            var settings = SetupSettings(configFile);

            var blocks = SetupBlocks(configFile);

            string combosFile = args[1];

            CheckExistenceFile(combosFile);

            var combos = SetupCombos(combosFile);

            var httpClientManager = SetupHttpClientManager(args, settings);

            var checker = new Checker(settings, blocks, combos, httpClientManager);

            var consoleManager = new ConsoleManager(settings, checker);

            _ = consoleManager.StartUpdatingTitleAsync();

            await checker.StartAsync();

            consoleManager.End();
        }

        private static Settings SetupSettings(string file)
        {
            string contentFile = File.ReadAllText(file);

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(contentFile).GetProperty("Settings");

            return JsonSerializer.Deserialize<Settings>(jsonElement.ToString());
        }

        private static IEnumerable<Block> SetupBlocks(string file)
        {
            string contentFile = File.ReadAllText(file);

            var jsonElements = JsonSerializer.Deserialize<JsonElement>(contentFile).GetProperty("Blocks").EnumerateArray();

            var blocks = new List<Block>();

            foreach (var jsonElement in jsonElements)
            {
                string block = jsonElement.GetProperty("Block").GetString();

                switch (block.ToLower())
                {
                    case "request":
                        blocks.Add(new BlockRequest(jsonElement.ToString()));
                        break;
                    case "parse":
                        blocks.Add(new BlockParse(jsonElement.ToString()));
                        break;
                    case "keycheck":
                        blocks.Add(new BlockKeyCheck(jsonElement.ToString()));
                        break;
                }
            }

            return blocks;
        }

        private static IEnumerable<Combo> SetupCombos(string file)
        {
            var linesOfCombos = File.ReadAllLines(file);
            return linesOfCombos.Select(c => new Combo(c)).Where(c => c.IsValid);
        }

        private static HttpClientManager SetupHttpClientManager(string[] args, Settings settings)
        {
            if (settings.UseProxies)
            {
                string proxyFile = args[2];
                CheckExistenceFile(proxyFile);
                var proxies = SetupProxies(proxyFile);
                return new HttpClientManager(settings, proxies);
            }
            return new HttpClientManager(settings);
        }

        private static IEnumerable<Proxy> SetupProxies(string file)
        {
            var linesOfProxies = File.ReadAllLines(file);
            return linesOfProxies.Select(p => new Proxy(p)).Where(p => p.IsValid);
        }

        private static void CheckExistenceFile(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException($"{file} not found");
        }
    }
}
