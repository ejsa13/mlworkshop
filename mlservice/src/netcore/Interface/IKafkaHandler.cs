using System.Threading.Tasks;

namespace mlservice.Interface
{
    public interface IKafkaHandler<Tkey, TValue>
    {
        Task HandleAsync(Tkey key, TValue value);
    }
}