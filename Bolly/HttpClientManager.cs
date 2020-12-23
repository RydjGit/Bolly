using Bolly.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Bolly
{
    public class HttpClientManager
    {
        public HttpClient GetClient() => _useProxies ? _proxyHttpClient.ElementAt(_random.Next(_proxyHttpClient.Count())) : _singleHttpClient;

        private readonly Settings _settings;
        private readonly HttpClient _singleHttpClient;
        private readonly IEnumerable<HttpClient> _proxyHttpClient;
        private readonly Random _random;
        private readonly bool _useProxies;

        public HttpClientManager(Settings settings)
        {
            _settings = settings;
            _singleHttpClient = SetupHttpClient();
        }

        public HttpClientManager(Settings settings, IEnumerable<Proxy> proxies)
        {
            _settings = settings;
            _proxyHttpClient = proxies.Select(p => SetupHttpClient(p));
            _random = new Random();
            _useProxies = true;
        }

        private HttpClient SetupHttpClient(Proxy proxy = null)
        {
            var cookieContainer = new CookieContainer();

            var httpClientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = cookieContainer,
                UseCookies = _settings.UseCookies,
            };

            if (proxy != null)
            {
                var webProxy = new WebProxy($"http://{proxy.Host}:{proxy.Port}");

                if (proxy.WithCredentials)
                {
                    var credentials = new NetworkCredential(proxy.Username, proxy.Password);
                    webProxy.Credentials = credentials;
                }

                httpClientHandler.Proxy = webProxy;
            }

            return new HttpClient(httpClientHandler);
        }
    }
}
