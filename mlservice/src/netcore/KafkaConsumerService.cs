using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using mlservice.Interface;
using Serilog;

namespace mlservice
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IKafkaConsumer<string, string> _consumer;
        private readonly ILogger _logger;
        private readonly string _topic;
        public KafkaConsumerService(IKafkaConsumer<string, string> kafkaConsumer, IConfiguration configuration, ILogger logger)
        {
            _consumer = kafkaConsumer;
            _logger = logger;
            _topic = configuration.GetValue<string>("Kafka:Topic"); 
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
			{
				await _consumer.Consume(_topic, stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.Error($"{(int)HttpStatusCode.InternalServerError} ConsumeFailedOnTopic - {_topic}, {ex}");
			}
        }
    }
}