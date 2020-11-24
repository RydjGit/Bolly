using Bolly.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public class BlockRequest : BlockBase
    {
        protected class Request
        {
            public string Methode { get; set; }
            public string Url { get; set; }
            public string Content { get; set; }
            public string ContentType { get; set; }
            public IEnumerable<string> Headers { get; set; }
            public bool LoadSource { get; set; }
        }

        private readonly Request _request;
        private readonly HttpMethod _httpMethod;

        public BlockRequest(string jsonString)
        {
            _request = JsonSerializer.Deserialize<Request>(jsonString);
            _httpMethod = new HttpMethod(_request.Methode);
        }

        public override async Task Execute(HttpClient httpclient, BotData botData)
        {
            var uri = new Uri(ReplaceValues(_request.Url, botData));

            using var httpRequestMessage = new HttpRequestMessage(_httpMethod, uri);

            if (_httpMethod == HttpMethod.Post) httpRequestMessage.Content = new StringContent(ReplaceValues(_request.Content, botData), Encoding.UTF8, _request.ContentType);

            if (_request.Headers != null)
            {
                foreach (var header in _request.Headers)
                {
                    var headerSplit = header.Split(":");
                    httpRequestMessage.Headers.TryAddWithoutValidation(headerSplit[0], ReplaceValues(headerSplit[1], botData));
                }
            }

            using var httpResponseMessage = await httpclient.SendAsync(httpRequestMessage);

            botData.Address = httpResponseMessage.RequestMessage.RequestUri.ToString();

            botData.ResponseCode = (int)httpResponseMessage.StatusCode;

            if (_request.LoadSource) botData.Source = await httpResponseMessage.Content.ReadAsStringAsync();
        }
    }
}