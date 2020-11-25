using Bolly.Blocks;
using System.Collections.Generic;

namespace Bolly.Models
{
    public class Config
    {
        public string Name { get; set; }
        public bool UseCookies { get; set; } = true;
        public bool AllowAutoRedirect { get; set; } = true;
        public int MaxDegreeOfParallelism { get; set; }
        public IEnumerable<BlockBase> Blocks { get; set; }
    }
}
