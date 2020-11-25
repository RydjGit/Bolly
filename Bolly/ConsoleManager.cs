using System;
using System.Text;
using System.Threading.Tasks;

namespace Bolly
{
    public class ConsoleManager
    {
        private readonly Checker _checker;

        public ConsoleManager(Checker checker)
        {
            _checker = checker;
        }

        public async Task StartUpdatingTitleAsync()
        {
            var title = new StringBuilder();

            while (true)
            {
                var (Invalid, Free, Success, Unknwon, Retry, CPM) = _checker.Stats;

                title.Append(_checker.Name);

                title.Append(" - Invalid ");
                title.Append(Invalid);

                title.Append(" Free ");
                title.Append(Free);

                title.Append(" Success ");
                title.Append(Success);

                title.Append(" Unknown ");
                title.Append(Unknwon);

                title.Append(" Retry ");
                title.Append(Retry);

                title.Append(" CPM ");
                title.Append(CPM);

                Console.Title = title.ToString();

                title.Clear();

                await Task.Delay(750);
            }
        }
    }
}