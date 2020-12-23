namespace Bolly.Models
{
    public class Settings
    {
        public string Name { get; set; }
        public bool UseProxies { get; set; }
        public bool UseCookies { get; set; }
        public bool AllowAutoRedirect { get; set; }
        public int MaxDegreeOfParallelism { get; set; }
    }
}
