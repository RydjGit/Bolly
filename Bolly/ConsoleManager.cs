using System;
using System.Text;
using System.Threading.Tasks;

namespace Bolly
{
    public class ConsoleManager
    {
        private readonly string _checkerName;
        private readonly Checker _checker;

        public ConsoleManager(string checkerName,  Checker checker)
        {
            _checkerName = checkerName;
            _checker = checker;
        }

        public async Task StartUpdatingTitleAsync()
        {
            var title = new StringBuilder();

            while (true)
            {
                var checkerStats = _checker.Stats;

                title.Append(_checkerName);

                title.Append(" - Invalid ");
                title.Append(checkerStats.Invalid);

                title.Append(" Free ");
                title.Append(checkerStats.Free);

                title.Append(" Success ");
                title.Append(checkerStats.Success);

                title.Append(" Unknown ");
                title.Append(checkerStats.Unknwon);

                title.Append(" Retry ");
                title.Append(checkerStats.Retry);

                title.Append(" CPM ");
                title.Append(checkerStats.CPM);

                Console.Title = title.ToString();

                title.Clear();

                await Task.Delay(250);
            }
        }
    }
}