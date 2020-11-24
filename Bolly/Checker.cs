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
        public (int Invalid, int Free, int Success, int Unknwon, int Retry, int CPM) Stats { get => (_invalid, _free, _success, _unknown, _retry, _cpm); }

        private readonly CheckerSettings _checkerSettings;
        private readonly Config _config;
        private readonly ClientManager _clientManager;
        private readonly IEnumerable<Combo> _combos;
        private readonly IEnumerable<Status> _validStatus;
        private readonly ReaderWriterLock _locker;

        private int _checked;
        private int _invalid;
        private int _free;
        private int _success;
        private int _unknown;
        private int _retry;
        private int _cpm;

        public Checker(CheckerSettings checkerSettings, Config config, IEnumerable<Combo> combos, ClientManager clientManager)
        {
            _checkerSettings = checkerSettings;
            _config = config;
            _combos = combos;
            _clientManager = clientManager;
            _validStatus = new[] { Status.Free, Status.Success, Status.Unknown };
            _locker = new ReaderWriterLock();
        }

        public async Task StartAsync()
        {
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
                    }

                    if (botData.Status == Status.Retry)
                    {
                        Interlocked.Increment(ref _retry);
                        continue;
                    }

                    break;
                }

                if (botData.Status == Status.Invalid) Interlocked.Increment(ref _invalid);
                else if(botData.Status == Status.Free) Interlocked.Increment(ref _free);
                else if (botData.Status == Status.Success) Interlocked.Increment(ref _success);
                else if (botData.Status == Status.Unknown) Interlocked.Increment(ref _unknown);

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
                            break;
                        case Status.Success:
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(output);
                            break;
                        case Status.Unknown:
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine(output);
                            break;
                    }
                }

                Interlocked.Increment(ref _checked);
            }, _config.MaxDegreeOfParallelism);
        }

        private string OutputBuilder(Combo combo, BotData botData)
        {
            if (botData.Captues.Count == 0) return combo.ToString();

            return new StringBuilder(combo.ToString())
                .Append(_checkerSettings.OutputSeparator)
                .AppendJoin(_checkerSettings.OutputSeparator, botData.Captues.Select(c => $"{c.Key} = {c.Value}")).ToString();
        }

        private string PathBuilder(BotData botData) => Path.Combine(_checkerSettings.OutputDirectory, botData.Status.ToString()) + ".txt";

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
