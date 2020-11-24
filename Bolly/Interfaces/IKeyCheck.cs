using Bolly.Models;

namespace Bolly.Interfaces
{
    public interface IKeyCheck
    {
        public KeyCheckPattern KeyCheckPattern { get; }

        public bool Execute(string source, string key);
    }
}
