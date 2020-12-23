using Bolly.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Bolly
{
    public class ConsoleManager
    {
        private readonly Settings _settings;
        private readonly Checker _checker;

        public ConsoleManager(Settings settings,  Checker checker)
        {
            _settings = settings;
            _checker = checker;
        }

        public async Task StartUpdatingTitleAsync()
        {
            var title = new StringBuilder();

            while (true)
            {
                var stats = _checker.GetStats;

                title.Append(_settings.Name);

                title.Append(" - Invalid ");
                title.Append(stats.Item1);

                title.Append(" Free ");
                title.Append(stats.Item2);

                title.Append(" Success ");
                title.Append(stats.Item3);

                title.Append(" Unknown ");
                title.Append(stats.Item4);

                title.Append(" Retry ");
                title.Append(stats.Item5);

                title.Append(" CPM ");
                title.Append(stats.Item6);

                Console.Title = title.ToString();

                title.Clear();

                await Task.Delay(250);
            }
        }

        public void End()
        {
            Console.ResetColor();
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}