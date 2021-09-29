using System;
using System.Threading;
using System.Threading.Tasks;

namespace mlservice.Interface
{
    public interface IKafkaConsumer<TKey, TValue> where TValue : class
    {
        Task Consume(string topic, CancellationToken stoppkingToken);
        void Close();
        void Dispose();
    }
}