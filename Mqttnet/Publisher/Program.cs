using System;
namespace MqttNetPublisher
{
    using MQTTnet.Client.Options;
    using System.Threading.Tasks;
    using System.Threading;
    using MQTTnet.Client;
    using System.Text;
    using MQTTnet;

    class Program
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Publisher....");
            try
            {
                // Create a new MQTT client.
                var factory = new MqttFactory();
                _client = factory.CreateMqttClient();

                //configure options
                _options = new MqttClientOptionsBuilder()
                    .WithClientId("PublisherId")
                    .WithTcpServer("127.0.0.1", 1884)
                    .WithCredentials("sanjay", "%Welcome@123%")
                    .WithCleanSession()
                    .Build();

                //handlers
                _client.UseConnectedHandler(e =>
                {
                    Console.WriteLine("Connected successfully with MQTT Brokers.");
                });
                _client.UseDisconnectedHandler(e =>
                {
                    Console.WriteLine("Disconnected from MQTT Brokers.");
                });
                _client.UseApplicationMessageReceivedHandler(e =>
                {
                    try
                    {
                        string topic = e.ApplicationMessage.Topic;
                        if (string.IsNullOrWhiteSpace(topic) == false)
                        {
                            string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                            Console.WriteLine($"Topic: {topic}. Message Received: {payload}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message, ex);
                    }
                });

                //connect
                _client.ConnectAsync(_options).Wait();

                Console.WriteLine("Press key to publish message.");
                Console.ReadLine();

                //simulating publish
                SimulatePublish();

                Console.WriteLine("Simulation ended! press any key to exit.");
                Console.ReadLine();

                Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();
                _client.DisconnectAsync().Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //This method send messages to topic "test"
        static void SimulatePublish()
        {

            var counter = 0;
            while (counter < 30)
            {
                counter++;
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic("/ABB/Factory/Office")
                    .WithPayload($"\n\tSquence-no: {counter}\n\tmessage: {Guid.NewGuid()}")
                    .WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();


                if (_client.IsConnected)
                {
                    Console.WriteLine($"publishing at {DateTime.UtcNow}");
                    _client.PublishAsync(message);
                }
                Thread.Sleep(2000);
            }
        }
    }
}
