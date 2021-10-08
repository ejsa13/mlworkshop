using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using mlservice.Interface;
using Serilog;
namespace mlservice
{
    public class KafkaEventHandler : IKafkaHandler<string, string>
    {
        private readonly ILogger _logger;
        private readonly IDetectorService _detectorService;
        private readonly IConfiguration _configuration;
        public KafkaEventHandler(ILogger logger, IDetectorService detectorService, IConfiguration configuration)
        {
            _logger = logger;
            _detectorService = detectorService;
            _configuration = configuration;
        }
        public async Task HandleAsync(string key, string value)
        {
            _logger.Debug($"Consuming message {value}");
            dynamic message = JsonSerializer.Deserialize<ExpandoObject>(value);
            if(PropertyExsts(message, "body"))
            {
                _logger.Debug("calling drift detector");
                var driftResults = await _detectorService.OnPostAsync(_configuration.GetValue<string>("Detector:DriftUrl"), message.body);
                _logger.Debug($"{{\"detector\":\"drift\", \"request\": {{{value}}}, \"payload\": {{{message.body}}}, \"response\": {{\"StatusCode\":{(int)driftResults.StatusCode}, \"Content\":{await driftResults.Content.ReadAsStringAsync()}}}}}");
                _logger.Information($"{{\"detector\":\"drift\", \"request\": {{{value}}}, \"payload\": {{{message.body}}}, \"response\": {{\"StatusCode\":{(int)driftResults.StatusCode}, \"Content\":{await driftResults.Content.ReadAsStringAsync()}}}}}");
                
                _logger.Debug("calling outlier detector");
                var outlierResults = await _detectorService.OnPostAsync(_configuration.GetValue<string>("Detector:OutlierUrl"), message.body);
                _logger.Debug($"{{\"detector\":\"outlier\", \"request\": {{{value}}}, \"payload\": {{{message.body}}}, \"response\": {{\"StatusCode\":{(int)outlierResults.StatusCode}, \"Content\":{await outlierResults.Content.ReadAsStringAsync()}}}}}");
                _logger.Information($"{{\"detector\":\"outlier\", \"request\": {{{value}}}, \"payload\": {{{message.body}}}, \"response\": {{\"StatusCode\":{(int)outlierResults.StatusCode}, \"Content\":{await outlierResults.Content.ReadAsStringAsync()}}}}}");
            }
        }

        private bool PropertyExsts(dynamic obj, string name){
            if(obj is ExpandoObject){
                return ((IDictionary<string,object>)obj).ContainsKey(name);
            }
            return obj.GetType().GetProperty(name) != null;
        }
    }
}