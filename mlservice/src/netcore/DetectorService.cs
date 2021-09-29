using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using mlservice.Interface;

namespace mlservice
{
    public class DriftDetectorService : IDriftDetectorService
    {
        private readonly HttpClient _client;
        private readonly string _url;
        public DriftDetectorService(IConfiguration configuration, HttpClient client)
        {
            _client = client;
            _url = configuration.GetValue<string>("DriftDetectorUrl");
        }

        public async Task<HttpResponseMessage> OnPostAsync(JsonElement data)
        {
            return await _client.PostAsJsonAsync(_url, data);
        }
    }

    public class OutlierDetectorService : IOutlierDetectorService
    {
        private readonly HttpClient _client;
        private readonly string _url;
        public OutlierDetectorService(IConfiguration configuration, HttpClient client)
        {
            _client = client;
            _url = configuration.GetValue<string>("OutlierDetectorUrl");
        }

        public async Task<HttpResponseMessage> OnPostAsync(JsonElement data)
        {
            return await _client.PostAsJsonAsync(_url, data);
        }
    }

}