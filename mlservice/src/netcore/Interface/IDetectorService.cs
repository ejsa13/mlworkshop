using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace mlservice.Interface
{
    public interface IDetectorService
    {
        Task<HttpResponseMessage> OnPostAsync(string url, JsonElement data);
    }
}