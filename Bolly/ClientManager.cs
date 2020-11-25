using Bolly.Interfaces;
using Bolly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Bolly
{
    public class ClientManager
    {
        protected class SingleClient : IClientManager
        {
            public HttpClient GetClient { get; }

            public SingleClient(HttpClient httpClient)
            {
                GetClient = httpClient;
            }
        }

        protected class ProxyCLient : IClientManager
        {
            public HttpClient GetClient { get => _httpClients.ElementAt(_random.Next(_httpClients.Count())); }

            private readonly IEnumerable<HttpClient> _httpClients;
            private readonly Random _random;

            public ProxyCLient(IEnumerable<HttpClient> httpClients)
            {
                _httpClients = httpClients;
                _random = new Random();
            }
        }

        public HttpClient GetClient { get => _clientManager.GetClient; }

        private readonly Config _config;
        private IClientManager _clientManager;

        public ClientManager(Config config)
        {
            _config = config;
            var client = SetupSingleHttpClient();
            _clientManager = new SingleClient(client);
        }

        public ClientManager WithProxies(IEnumerable<Proxy> proxies)
        {
            var clients = proxies.Select(p => SetupProxyHttpClient(p));
            _clientManager = new ProxyCLient(clients);
            return this;
        }

        private HttpClient SetupSingleHttpClient()
        {
            var cookieContainer = new CookieContainer();

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = cookieContainer,
                UseCookies = _config.UseCookies,
                AllowAutoRedirect = _config.AllowAutoRedirect
            };

            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.ConnectionClose = true;

            return httpClient;
        }

        private HttpClient SetupProxyHttpClient(Proxy proxy)
        {
            var cookieContainer = new CookieContainer();

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = cookieContainer,
                UseCookies = _config.UseCookies,
                AllowAutoRedirect = _config.AllowAutoRedirect
            };

            var webProxy = new WebProxy($"http://{proxy.Host}:{proxy.Port}");
            if (proxy.WithCredentials)
            {
                var credentials = new NetworkCredential(proxy.Username, proxy.Password);
                webProxy.Credentials = credentials;
            }
            httpClientHandler.Proxy = webProxy;

            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.ConnectionClose = true;

            return httpClient;
        }
    }
}
