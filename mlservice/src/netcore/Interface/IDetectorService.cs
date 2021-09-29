using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace mlservice.Interface
{
    public interface IDriftDetectorService
    {
        Task<HttpResponseMessage> OnPostAsync(JsonElement data);
    }

    public interface IOutlierDetectorService
    {
        Task<HttpResponseMessage> OnPostAsync(JsonElement data);
    }
}