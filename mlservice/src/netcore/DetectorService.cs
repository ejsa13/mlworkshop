using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using mlservice.Interface;

namespace mlservice
{
    public class DetectorService : IDetectorService
    {
        private readonly HttpClient _client;
        public DetectorService(HttpClient client)
        {
            _client = client;
        }

        public async Task<HttpResponseMessage> OnPostAsync(string url, JsonElement data)
        {
            return await _client.PostAsJsonAsync(url, data);
        }
    }
}