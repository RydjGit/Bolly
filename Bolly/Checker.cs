using Bolly.Blocks;
using Bolly.Enums;
using Bolly.Extentions;
using Bolly.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bolly
{
    public class Checker
    {
        public Tuple<int, int, int, int, int, int> GetStats
        {
            get => Tuple.Create(_invalid, _free, _success, _unknown, _retry, _cpm);
        }

        private readonly Settings _settings;
        private readonly IEnumerable<Block> _blocks;
        private readonly IEnumerable<Combo> _combos;
        private readonly HttpClientManager _httpClientManager;
        private readonly IEnumerable<Status> _validStatus;
        private readonly ReaderWriterLock _locker;

        private int _checked;
        private int _invalid;
        private int _free;
        private int _success;
        private int _unknown;
        private int _retry;
        private int _cpm;

        private const string OutputSeparator = " | ";
        private const string OutputDirectory = "Results";

        public Checker(Settings settings, IEnumerable<Block> blocks, IEnumerable<Combo> combos, HttpClientManager httpClientManager)
        {
            _settings = settings;
            _blocks = blocks;
            _combos = combos;
            _httpClientManager = httpClientManager;
            _validStatus = new[] { Status.Free, Status.Success, Status.Unknown };
            _locker = new ReaderWriterLock();
        }

        public async Task StartAsync()
        {
            Directory.CreateDirectory(OutputDirectory);

            _ = StartCpmCalculator();

            await _combos.AsyncParallelForEach(async combo =>
            {
                BotData botData;
                HttpClient httpClient;

                while (true)
                {
                    botData = new BotData();
                    httpClient = _httpClientManager.GetClient();

                    foreach (var block in _blocks)
                    {
                        await block.Execute(combo, httpClient, botData);
                        if (botData.Status == Status.Invalid)
                        {
                            Interlocked.Increment(ref _invalid);
                            break;
                        }
                        else if (botData.Status == Status.Retry)
                        {
                            Interlocked.Increment(ref _retry);
                            break;
                        }
                    }

                    if (botData.Status == Status.Retry) continue;

                    break;
                }

                if (_validStatus.Contains(botData.Status))
                {
                    string output = OutputBuilder(combo, botData);
                    string outputPath = PathBuilder(botData);

                    Save(output, outputPath);

                    switch (botData.Status)
                    {
                        case Status.Free:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine(output);
                            Interlocked.Increment(ref _free);
                            break;
                        case Status.Success:
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(output);
                            Interlocked.Increment(ref _success);
                            break;
                        case Status.Unknown:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(output);
                            Interlocked.Increment(ref _unknown);
                            break;
                    }
                }

                Interlocked.Increment(ref _checked);
            }, _settings.MaxDegreeOfParallelism);
        }

        private string OutputBuilder(Combo combo, BotData botData)
        {
            var output = new StringBuilder().Append("DATA = ").Append(combo);

            if (botData.Captues.Count == 0) return output.ToString();

            output.Append(OutputSeparator);
            output.AppendJoin(OutputSeparator, botData.Captues.Select(c => $"{c.Key} = {c.Value}")).ToString();

            return output.ToString();
        }

        private string PathBuilder(BotData botData)
        {
            return Path.Combine(OutputDirectory, botData.Status.ToString()) + ".txt";
        }

        private void Save(string data, string path)
        {
            try
            {
                _locker.AcquireWriterLock(int.MaxValue);
                File.AppendAllText(path, data + Environment.NewLine);
            }
            finally
            {
                _locker.ReleaseWriterLock();
            }
        }

        private async Task StartCpmCalculator()
        {
            while (true)
            {
                int checkedBefore = _checked;
                await Task.Delay(3000);
                int checkedAfter = _checked;

                _cpm = (checkedAfter - checkedBefore) * 20;
            }
        }
    }
}
