using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using mlservice.Interface;
using Serilog;

namespace mlservice
{
    public class KafkaConsumer<TKey, TValue> : IKafkaConsumer<TKey, TValue> where TValue : class
    {
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private string _topic;
        private IKafkaHandler<TKey, TValue> _handler;
        private IConsumer<TKey, TValue> _consumer;
        
        public KafkaConsumer(IConfiguration config, IServiceScopeFactory serviceScopeFactory, ILogger logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _config = config;
            _logger = logger;
        }
        
        public void Close()
        {
            _consumer.Close();
        }

        public async Task Consume(string topic, CancellationToken stoppingToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            _handler = scope.ServiceProvider.GetRequiredService<IKafkaHandler<TKey, TValue>>();
            _topic = topic;

            var consumerConfig = new ConsumerConfig(){
                BootstrapServers = _config.GetValue<string>("Kafka:BootstrapServers"),
                GroupId = _config.GetValue<string>("Kafka:GroupId"),
                AutoOffsetReset = (AutoOffsetReset)Enum.Parse(typeof(AutoOffsetReset), _config.GetValue<string>("Kafka:AutoOffsetReset"), true)
            };

            _consumer = new ConsumerBuilder<TKey, TValue>(consumerConfig).Build();
                
            await Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }

        private async Task StartConsumerLoop(CancellationToken cancellationToken)
        {
            _consumer.Subscribe(_topic);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(cancellationToken);

                    if (result != null)
                    {
                        await _handler.HandleAsync(result.Message.Key, result.Message.Value);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException e)
                {
                    // Consumer errors should generally be ignored (or logged) unless fatal.
                    _logger.Error($"Consume error: {e.Error.Reason}");

                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    _logger.Error($"Unexpected error: {e}");
                    break;
                }
            }
        }
    }
}