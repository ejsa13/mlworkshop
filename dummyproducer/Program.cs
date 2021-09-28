using System;
using System.Threading.Tasks;
using Confluent.Kafka;

class Program
{
    public static async Task Main(string[] args)
    {
        var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

        // If serializers are not specified, default serializers from
        // `Confluent.Kafka.Serializers` will be automatically used where
        // available. Note: by default strings are encoded as UTF8.
        using (var p = new ProducerBuilder<Null, string>(config).Build())
        {
            try
            {
                var dr = await p.ProduceAsync("mlworkshop", new Message<Null, string> { Value="{\"path\": \"/\",\"headers\": {\"host\": \"logger.seldon\",\"user-agent\": \"Go-http-client/1.1\",\"content-length\": \"39\",\"ce-endpoint\": \"logging\",\"ce-id\": \"e276a0c3-a522-4499-b509-5e98e06e96fe\",\"ce-inferenceservicename\": \"model-logs\",\"ce-modelid\": \"classifier\",\"ce-namespace\": \"seldon\",\"ce-requestid\": \"33fb419e-8d6b-44e0-a084-8bd4bd4dbf7b\", \"ce-source\": \"http://:8000/\",\"ce-specversion\": \"1.0\", \"ce-time\": \"2020-11-06T09:35:09.171644169Z\",\"ce-traceparent\": \"00-9c7cc210d352f7b68be6422c1b4b78f4-a7e8a2a38709bbc9-00\",\"ce-type\": \"io.seldon.serving.inference.request\",\"content-type\": \"application/json\",\"traceparent\": \"00-9c7cc210d352f7b68be6422c1b4b78f4-b49edd5ad25552ae-00\",\"accept-encoding\": \"gzip\"},\"method\": \"POST\",\"body\": \"{\\\"data\\\": {\\\"ndarray\\\":[[1.0, 2.0, 5.0]]}}\",\"fresh\": false, \"hostname\": \"logger.seldon\",\"ip\": \"::ffff:10.244.1.65\",\"ips\": [],\"protocol\": \"http\",\"query\": {},\"subdomains\": [],\"xhr\": false,\"os\": {\"hostname\": \"logger-766f99b9b7-mqtql\"},\"connection\": {},\"json\": {\"data\": {\"ndarray\": [[1,2,5]]}}}" });
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
    }
}