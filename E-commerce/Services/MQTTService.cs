using MQTTnet;
using MQTTnet.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Services
{
    public class MQTTService : IDisposable
    {
        private IMqttClient _mqttClient; // MQTT Client instance

        public MQTTService(IConfiguration configuration)
        {
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient(); // Initialize MQTT client

            var brokerAddress = configuration["MqttSettings:BrokerAddress"];
            var port = int.Parse(configuration["MqttSettings:Port"]);

            // Manually assign event handlers for connected, disconnected, and message received
            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        // Method to connect to the MQTT broker
        public async Task ConnectAsync(string brokerAddress, int port)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId("e_commerce_updates") // Unique client ID
                .WithWebSocketServer(brokerAddress) // WebSocket server address
                .WithCleanSession() // Optional: whether to create a clean session
                .Build();

            try
            {
                await _mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to MQTT broker: {ex.Message}");
            }
        }

        // Event handler for when the client connects to the MQTT broker
        private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("Successfully connected to MQTT broker.");
            await SubscribeAsync("inventory/orders");
            await SubscribeAsync("inventory/updates");
            await SubscribeAsync("sales/notifications");
            await SubscribeAsync("inventory/alerts");
        }

        // Event handler for when the client disconnects from the MQTT broker
        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected from MQTT broker. Attempting to reconnect...");

            // Retry logic with exponential backoff
            var retryCount = 0;
            while (!_mqttClient.IsConnected && retryCount < 5) // Try up to 5 times
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2 * retryCount)); // Exponential backoff
                    await ConnectAsync("ws://localhost:9001", 1883);
                    if (_mqttClient.IsConnected)
                    {
                        Console.WriteLine("Reconnected to MQTT broker.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Reconnection attempt failed: {ex.Message}");
                }
                retryCount++;
            }

            if (!_mqttClient.IsConnected)
            {
                Console.WriteLine("Failed to reconnect after multiple attempts.");
            }
        }

        // Event handler for when a message is received on a subscribed topic
        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var messagePayload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            Console.WriteLine($"Received message: {messagePayload} on topic: {e.ApplicationMessage.Topic}");

            switch (e.ApplicationMessage.Topic)
            {
                case "inventory/orders":
                    HandleNewOrder(messagePayload);
                    break;
                case "inventory/updates":
                    HandleInventoryUpdate(messagePayload);
                    break;
                case "sales/notifications":
                    HandleSalesNotification(messagePayload);
                    break;
                case "inventory/alerts":
                    HandleStockAlert(messagePayload);
                    break;
                default:
                    Console.WriteLine("Unknown topic received.");
                    break;
            }

            return Task.CompletedTask; // Return completed task
        }

        // Handle new orders
        private void HandleNewOrder(string payload)
        {
            Console.WriteLine($"Processing new order: {payload}");
        }

        // Handle inventory updates
        private void HandleInventoryUpdate(string payload)
        {
            Console.WriteLine($"Updating inventory: {payload}");
        }

        // Handle sales notifications
        private void HandleSalesNotification(string payload)
        {
            Console.WriteLine($"Sales notification received: {payload}");
        }

        // Handle low stock alerts
        private void HandleStockAlert(string payload)
        {
            Console.WriteLine($"Stock alert: {payload}");
        }

        // Method to publish a message to a specific topic
        public async Task PublishAsync(string topic, string payload)
        {
            if (_mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // QoS 1 for exactly once
                    .Build();

                try
                {
                    await _mqttClient.PublishAsync(message, CancellationToken.None);
                    Console.WriteLine($"Published message to topic {topic}: {payload}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to publish message: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("MQTT client is not connected.");
            }
        }

        // Method to subscribe to a specific topic
        public async Task SubscribeAsync(string topic)
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce) // QoS 1 for at least once
                .Build());
            Console.WriteLine($"Subscribed to topic: {topic}");
        }

        // Method to disconnect from the MQTT broker
        public async Task DisconnectAsync()
        {
            await _mqttClient.DisconnectAsync();
            Console.WriteLine("Disconnected from MQTT broker.");
        }

        // Implement IDisposable to clean up resources
        public void Dispose()
        {
            _mqttClient?.Dispose();
        }
    }
}
