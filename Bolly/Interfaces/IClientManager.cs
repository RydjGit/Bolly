using System.Net.Http;

namespace Bolly.Interfaces
{
    public interface IClientManager
    {
        public HttpClient GetClient { get; }
    }
}
