using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using mlservice.Interface;
using Serilog;

namespace mlservice
{
    public class KafkaEventHandler : IKafkaHandler<string, string>
    {
        private readonly ILogger _logger;
        private readonly IDriftDetectorService _driftService;
        private readonly IOutlierDetectorService _outlierService;
        public KafkaEventHandler(ILogger logger, IDriftDetectorService driftDetectorService, IOutlierDetectorService outlierDetectorService)
        {
            _logger = logger;
            _driftService = driftDetectorService;
            _outlierService = outlierDetectorService;
        }
        public async Task HandleAsync(string key, string value)
        {
            _logger.Debug($"Consuming message {value}");
            dynamic message = JsonSerializer.Deserialize<ExpandoObject>(value);
            if(PropertyExsts(message, "json"))
            {
                _logger.Debug("calling drift detector");
                var driftResults = await _driftService.OnPostAsync(message.json);
                _logger.Information($"request: {value}, payload:{message.json}, response: {driftResults}");

                _logger.Debug("calling outlier detector");
                var outlierResults = await _outlierService.OnPostAsync(message.json);
                _logger.Information($"request: {value}, payload:{message.json}, response: {outlierResults}");
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