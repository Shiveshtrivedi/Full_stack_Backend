using E_commerce.Models;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace E_commerce.Services
{
    public class MQTTService : IDisposable
    {
        private IMqttClient _mqttClient;
        private IConfiguration _configuration; 
        private readonly ILogger<MQTTService> _logger;


        public MQTTService(IConfiguration configuration,ILogger<MQTTService> logger)
        {
            _logger = logger;
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            var brokerAddress = configuration["MqttSettings:BrokerAddress"];
            var port = int.Parse(configuration["MqttSettings:Port"]);

            _mqttClient.ConnectedAsync += OnConnectedAsync;
            _mqttClient.DisconnectedAsync += OnDisconnectedAsync;
            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        public async Task ConnectAsync(string brokerAddress, int port)
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId("e_commerce_updates")
                .WithWebSocketServer(brokerAddress)
                .WithCleanSession()
                .Build();

            try
            {
                await _mqttClient.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to MQTT broker: {ex.Message}");
            }
        }

        private async Task OnConnectedAsync(MqttClientConnectedEventArgs e)
        {
            _logger.LogInformation("Successfully connected to MQTT broker."); 
            await SubscribeAsync("inventory/orders");
            await SubscribeAsync("inventory/updates");
            await SubscribeAsync("sales/notifications");
            await SubscribeAsync("inventory/alerts");
            await SubscribeAsync("product/new");
            await SubscribeAsync("user/new");
            await SubscribeAsync("user/delete");
            await SubscribeAsync("user/update");
        }

        private async Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            _logger.LogWarning("Disconnected from MQTT broker. Attempting to reconnect...");

            var retryCount = 0;
            while (!_mqttClient.IsConnected && retryCount < 5)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(30 * retryCount));
                    await ConnectAsync("ws://localhost:9001", 1883);
                    if (_mqttClient.IsConnected)
                    {
                        _logger.LogInformation("Reconnected to MQTT broker.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Reconnection attempt failed: {ex.Message}");
                }
                retryCount++;
            }

            if (!_mqttClient.IsConnected)
            {
                _logger.LogError("Failed to reconnect after multiple attempts.");
            }
        }

        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            var messagePayload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            _logger.LogInformation($"Received message: {messagePayload} on topic: {e.ApplicationMessage.Topic}");

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
                case "product/new":
                    HandleNewProduct(messagePayload);
                    break;
                case "product/update":
                    HandleUpdateProduct(messagePayload);
                    break;
                case "product/delete":
                    HandleUpdateProduct(messagePayload);
                    break;
                case "user/new":
                    HandleNewUser(messagePayload);
                    break;
                case "user/update":
                    HandleUserUpdated(messagePayload);
                    break;
                case "user/delete":
                    HandleUserDelete(messagePayload);
                    break;
                case "revenue-updates":
                    HandleRevenueUpdate(messagePayload);
                    break;
                default:
                    Console.WriteLine("Unknown topic received.");
                    break;
            }

            return Task.CompletedTask;
        }

        private void HandleNewOrder(string payload)
        {
            _logger.LogInformation($"Processing new order: {payload}");
        }

        private void HandleInventoryUpdate(string payload)
        {
            _logger.LogInformation($"Updating inventory: {payload}");
        }

        private void HandleSalesNotification(string payload)
        {
            _logger.LogInformation($"Sales notification received: {payload}");
        }
        private void HandleRevenueUpdate(string payload)
        {
            _logger.LogInformation($"Revenue notification    received: {payload}");
        }

        private void HandleStockAlert(string payload)
        {
            _logger.LogInformation($"Stock alert: {payload}");
        }

        private void HandleNewProduct(string payload)
        {
            _logger.LogInformation($"New product added: {payload}");
        }
        
        private void HandleDeleteProduct(string payload)
        {
            _logger.LogInformation($"New product added: {payload}");
        }
        private void HandleUpdateProduct(string payload)
        {
            _logger.LogInformation($"product updated: {payload}");
        }
        private void HandleNewUser(string payload)
        {
            _logger.LogInformation($"New User added: {payload}");
        }
        private void HandleUserDelete(string payload)
        {
            _logger.LogInformation($"User deleted: {payload}");
        }
        private void HandleUserUpdated(string payload)
        {
            _logger.LogInformation($"User updated: {payload}");
        }

        public async Task PublishAsync(string topic, string payload)
        {
            if (_mqttClient.IsConnected)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                try
                {
                    await _mqttClient.PublishAsync(message, CancellationToken.None);
                    _logger.LogInformation($"Published message to topic {topic}: {payload}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to publish message: {ex.Message}");
                }
            }
            else
            {
                _logger.LogError("MQTT client is not connected.");
            }
        }

        public async Task SubscribeAsync(string topic)
        {
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build());
            _logger.LogInformation($"Subscribed to topic: {topic}");
        }

        public async Task DisconnectAsync()
        {
            await _mqttClient.DisconnectAsync();
            _logger.LogInformation("Disconnected from MQTT broker.");
        }

        public void Dispose()
        {
            _mqttClient?.Dispose();
        }
    }
}
