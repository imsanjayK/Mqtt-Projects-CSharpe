namespace MqttNetSubscriber
{
    using MQTTnet.Client.Options;
    using System.Threading.Tasks;
    using System.Threading;
    using MQTTnet.Client;
    using System.Text;
    using MQTTnet;
    using System;
    class Program
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting Subsriber....");
                
                //create subscriber client
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                //configure options
                _options = new MqttClientOptionsBuilder()
                    .WithClientId("SubscriberId")
                    .WithTcpServer("127.0.0.1", 1884)
                    .WithCredentials("sanjay", "%Welcome@123%")
                    .WithCleanSession()
                    .Build();

                //Handlers
                _client.UseConnectedHandler(e =>
                {
                    Console.WriteLine("Connected successfully with MQTT Brokers.");

                    //Subscribe to topic
                    _client.SubscribeAsync(new TopicFilterBuilder().WithTopic("/ABB/Factory/Office").Build()).Wait();
                    _client.SubscribeAsync(new TopicFilterBuilder().WithTopic("mqttdotnet/pubtest").Build()).Wait();
                });
                _client.UseDisconnectedHandler(e =>
                {
                    Console.WriteLine("Disconnected from MQTT Brokers.");
                });
                _client.UseApplicationMessageReceivedHandler(e =>
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();
                    //Task.Run(() => _client.PublishAsync("hello/world"));
                });

                //actually connect
                _client.ConnectAsync(_options).Wait();

                Console.WriteLine("Press key to exit");
                Console.ReadLine();

                Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();
                _client.DisconnectAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
