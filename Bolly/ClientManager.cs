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
        public HttpClient ProxyClient { get => _useProxies ? _httpClient : _httpClients.ElementAt(_random.Next(_httpClients.Count())); }

        private readonly Config _config;
        private readonly HttpClient _httpClient;
        private readonly Random _random;
        private IEnumerable<HttpClient> _httpClients;
        private bool _useProxies;

        public ClientManager(Config config)
        {
            _config = config;
            _httpClient = SetupSingleHttpClient();
        }

        public ClientManager WithProxies(IEnumerable<Proxy> proxies)
        {
            _httpClients = proxies.Select(p => SetupProxyHttpClient(p));
            _useProxies = true;
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
