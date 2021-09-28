using System.Threading.Tasks;
using System;
using System.Threading;

using Microsoft.Extensions.Configuration;
using CloudNative.CloudEvents.NewtonsoftJson;

using Confluent.Kafka;

using Serilog;
using System.Dynamic;
using System.Text.Json;
using System.Collections.Generic;
using Serilog.Core;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Net.Http.Json;

namespace netcore
{
    class Program
    {
        private static IConfiguration _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        private static Logger _logger;
        private static readonly HttpClient _client = new HttpClient();
        static void Main(string[] args)
        {
            //kafka config
            
            var consumerConfig = new ConsumerConfig(){
                BootstrapServers = _config.GetValue<string>("Kafka:BootstrapServers"),
                GroupId = _config.GetValue<string>("Kafka:GroupId"),
                AutoOffsetReset = (AutoOffsetReset)Enum.Parse(typeof(AutoOffsetReset), _config.GetValue<string>("Kafka:AutoOffsetReset"), true)
            };
            
            string topic = _config.GetValue<string>("Kafka:Topic");

            //logger
            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_config)
                .CreateLogger();

            //httpclients

            CancellationTokenSource cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true; // prevent the process from terminating.
                cts.Cancel();
            };

            using (var consumer = new ConsumerBuilder<Null, string>(consumerConfig).Build())
            {
                consumer.Subscribe(topic);
                var formatter = new JsonEventFormatter();
                try {
                    while (true) {
                        var cr = consumer.Consume(cts.Token);
                        _logger.Debug($"Consumed event from topic {topic}, message: {cr.Message.Value}");
                        dynamic message = JsonSerializer.Deserialize<ExpandoObject>(cr.Message.Value);
                        if(PropertyExsts(message, "json")){
                            JsonElement data = message.json;
                            _logger.Debug($"payload: {message.json}");
                        
                            _logger.Debug($"POSTING data to outlier detector");
                            ProcessData(data,_config.GetValue<string>("OutlierDetectorUrl"), "outlier_detector");
                        
                            _logger.Debug($"POSTING data to drift detector");
                            ProcessData(data,_config.GetValue<string>("DriftDetectorUrl"), "drift_detector");
                        }
                        else {
                            _logger.Error("invalid data");                
                        }                    
                    }
                }
                catch (OperationCanceledException) {
                    // Ctrl-C was pressed.
                }
                catch (Exception ex)
                {
                    
                    //continue at the momente
                }
                finally{
                    consumer.Close();
                    _client.Dispose();
                }
            }
        }

        private static void ProcessData(JsonElement data, string url, string processName){
            //var payload = new StringContent(data, Encoding.UTF8, "application/json");
            var result = _client.PostAsJsonAsync(url, data).Result;
            var info = new { payload = data, response = result.Content.ReadAsStringAsync().Result};
            if(result.StatusCode == HttpStatusCode.Created || result.StatusCode == HttpStatusCode.OK){
                _logger.Information($"process:{processName}, payload:{data}, response:{result.Content.ReadAsStringAsync().Result}");
            }
            else {
                _logger.Error($"process:{processName}, payload:{data}");
            }
        }
        private static bool PropertyExsts(dynamic obj, string name){
            if(obj is ExpandoObject){
                return ((IDictionary<string,object>)obj).ContainsKey(name);
            }
            return obj.GetType().GetProperty(name) != null;
        }
    }
}
