using Bolly.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bolly.Blocks
{
    public class BlockRequest : Block
    {
        private class Request
        {
            public string Methode { get; set; }
            public string Url { get; set; }
            public string Content { get; set; }
            public string ContentType { get; set; }
            public IEnumerable<string> Headers { get; set; }
            public bool LoadSource { get; set; }
        }

        private readonly Request _request;

        public BlockRequest(string jsonString)
        {
            _request = JsonSerializer.Deserialize<Request>(jsonString);
        }

        public override async Task Execute(Combo combo, HttpClient httpclient, BotData botData)
        {
            var method = new HttpMethod(_request.Methode);

            var uri = new Uri(ReplaceValues(_request.Url, combo, botData));

            using var httpRequestMessage = new HttpRequestMessage(method, uri);

            if (method == HttpMethod.Post) httpRequestMessage.Content = new StringContent(ReplaceValues(_request.Content, combo, botData), Encoding.UTF8, _request.ContentType);

            foreach (var header in _request.Headers)
            {
                var headerSplit = header.Split(":");
                httpRequestMessage.Headers.TryAddWithoutValidation(headerSplit[0], ReplaceValues(headerSplit[1], combo, botData));
            }

            using var httpResponseMessage = await httpclient.SendAsync(httpRequestMessage);

            botData.Address = httpResponseMessage.RequestMessage.RequestUri.ToString();

            botData.ResponseCode = (int)httpResponseMessage.StatusCode;

            if (_request.LoadSource) botData.Source = await httpResponseMessage.Content.ReadAsStringAsync();
        }
    }
}