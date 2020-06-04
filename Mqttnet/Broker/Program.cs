namespace MqttNetBroker
{
    using System.Threading.Tasks;
    using MQTTnet.Protocol;
    using System.Threading;
    using MQTTnet.Server;
    using System.Text;
    using System.Linq;
    using System.Net;
    using MQTTnet;
    using System;
    class Program
    {
        static void Main(string[] args)
        {
                var optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionValidator(c =>
                {
                    Console.WriteLine($"{c.ClientId} connection validator for c.Endpoint: {c.Endpoint}");
                    c.ReasonCode = MqttConnectReasonCode.Success;
                })
                .WithApplicationMessageInterceptor(context =>
                {
                    Console.WriteLine("WithApplicationMessageInterceptor block merging data");
                    var newData = Encoding.UTF8.GetBytes(DateTime.Now.ToString("O"));
                    var oldData = context.ApplicationMessage.Payload;
                    var mergedData = newData.Concat(oldData).ToArray();
                    context.ApplicationMessage.Payload = mergedData;
                })
                .WithDefaultEndpointBoundIPAddress(IPAddress.Loopback) // Set IP address
                .WithConnectionBacklog(100)
                .WithDefaultEndpointPort(1884);

            //start server
            var mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder.Build());

            Console.WriteLine($"Broker is Running: Host: {mqttServer.Options.DefaultEndpointOptions.BoundInterNetworkAddress} Port: {mqttServer.Options.DefaultEndpointOptions.Port}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();

            Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();

            mqttServer.StopAsync().Wait();
        }
    }
}
