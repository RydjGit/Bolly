using Bolly.Enums;
using Bolly.Extentions;
using Bolly.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bolly
{
    public class Checker
    {
        public string Name { get; }
        public (int Invalid, int Free, int Success, int Unknwon, int Retry, int CPM) Stats { get => (_invalid, _free, _success, _unknown, _retry, _cpm); }

        private readonly Config _config;
        private readonly ClientManager _clientManager;
        private readonly IEnumerable<Combo> _combos;
        private readonly IEnumerable<Status> _validStatus;
        private readonly ReaderWriterLock _writerLocker;

        private int _checked;
        private int _invalid;
        private int _free;
        private int _success;
        private int _unknown;
        private int _retry;
        private int _cpm;

        private const string Separator = " | ";
        private const string OutputDirectory = "Result";

        public Checker(Config config, IEnumerable<Combo> combos, ClientManager clientManager)
        {
            Name = config.Name;
            _config = config;
            _combos = combos;
            _clientManager = clientManager;
            _validStatus = new[] { Status.Free, Status.Success, Status.Unknown };
            _writerLocker = new ReaderWriterLock();
        }

        public async Task StartAsync()
        {
            Directory.CreateDirectory(OutputDirectory);

            _ = StartCpmCalculator();

            await _combos.AsyncParallelForEach(async combo =>
            {
                BotData botData;

                while (true)
                {
                    botData = new BotData(combo);

                    foreach (var block in _config.Blocks)
                    {
                        await block.Execute(_clientManager.Client(), botData);
                        if (botData.Status == Status.Invalid) break;
                        else if (botData.Status == Status.Retry) break;
                    }

                    if (botData.Status == Status.Retry)
                    {
                        Interlocked.Increment(ref _retry);
                        continue;
                    }

                    break;
                }

                if (botData.Status == Status.Invalid) Interlocked.Increment(ref _invalid);
                else if (_validStatus.Contains(botData.Status))
                {
                    string output = OutputBuilder(combo, botData);
                    string outputPath = PathBuilder(botData);

                    while (!TrySave(output, outputPath)) await Task.Delay(100);

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
            }, _config.MaxDegreeOfParallelism);
        }

        private string OutputBuilder(Combo combo, BotData botData)
        {
            var output = new StringBuilder()
                .Append(DateTime.Now)
                .Append(Separator)
                .Append(botData.Status)
                .Append(Separator)
                .Append(combo);

            if (botData.Captues.Count == 0) return output.ToString();

            return output
                .Append(Separator)
                .AppendJoin(Separator, botData.Captues.Select(c => $"{c.Key} = {c.Value}")).ToString();
        }

        private string PathBuilder(BotData botData) => Path.Combine(OutputDirectory, botData.Status.ToString()) + ".txt";

        private bool TrySave(string data, string path)
        {
            try
            {
                _writerLocker.AcquireWriterLock(int.MaxValue);
                File.AppendAllText(path, data + Environment.NewLine);
                _writerLocker.ReleaseWriterLock();
                return true;
            }
            catch
            {
                _writerLocker.ReleaseWriterLock();
                return false;
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
