// MqttNotificationService.cs - versione completamente aggiornata
using LogManager;
using MQTT.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;
using static MQTT.Models.MqttNotificationModel;

namespace MQTT
{
    public class MqttNotificationService : IDisposable
    {
        private IMqttClient _mqttClient;
        private string _deviceId;
        private bool _isConnected = false;
        private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);

        public MqttNotificationService(string deviceId)
        {
            _deviceId = deviceId;
            Task.Run(() => InitializeMqttClientAsync()).Wait();
        }

        private async Task InitializeMqttClientAsync()
        {
            try
            {
                // Metodo alternativo per creare il client
                _mqttClient = new MqttFactory().CreateMqttClient();

                Logger.Loggin("Client MQTT creato con successo", false, false);

                // Handler per disconnessione
                _mqttClient.DisconnectedAsync += async args =>
                {
                    _isConnected = false;
                    Logger.Loggin($"Disconnesso dal broker MQTT. Motivo: {args.Reason}", false, true);

                    await Task.Delay(5000);
                    await ConnectWithRetryAsync();
                };

                // Connessione iniziale
                await ConnectWithRetryAsync();
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore inizializzazione MQTT: {ex.Message}", true, true);
                if (ex.InnerException != null)
                {
                    Logger.Loggin($"Causa: {ex.InnerException.Message}", true, true);
                }
            }
        }

        private async Task ConnectWithRetryAsync()
        {
            await _connectionSemaphore.WaitAsync();
            try
            {
                if (_mqttClient == null)
                {
                    Logger.Loggin("Client MQTT non inizializzato", true, true);
                    return;
                }

                if (!_mqttClient.IsConnected)
                {
                    Logger.Loggin("Tentativo di connessione a broker.hivemq.com:1883...", false, false);

                    try
                    {
                        // Crea opzioni di connessione direttamente qui
                        var options = new MqttClientOptionsBuilder()
                            .WithTcpServer("broker.hivemq.com", 1883)
                            .WithClientId($"uom_publisher_{Guid.NewGuid()}")
                            .WithCleanSession()
                            .Build();

                        var result = await _mqttClient.ConnectAsync(options, CancellationToken.None);
                        _isConnected = result.ResultCode == MqttClientConnectResultCode.Success;

                        if (_isConnected)
                        {
                            await SubscribeToTopicAsync(_deviceId);
                            Logger.Loggin("Connesso al broker MQTT con successo!", false, false);
                        }
                        else
                        {
                            Logger.Loggin($"Connessione MQTT fallita: {result.ResultCode}", true, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Loggin($"Eccezione durante connessione: {ex.Message}", true, true);
                        if (ex.InnerException != null)
                        {
                            Logger.Loggin($"Causa dell'errore: {ex.InnerException.Message}", true, true);
                        }
                    }
                }
            }
            finally
            {
                _connectionSemaphore.Release();
            }
        }

        public async Task SendNotificationAsync(string title, string message, NotificationSeverity type)
        {
            try
            {
                if (_mqttClient == null)
                {
                    Logger.Loggin("Client MQTT non inizializzato", true, true);
                    return;
                }

                if (!_mqttClient.IsConnected)
                {
                    Logger.Loggin("Client MQTT non connesso. Tentativo di riconnessione...", false, true);
                    await ConnectWithRetryAsync();

                    if (!_mqttClient.IsConnected)
                    {
                        Logger.Loggin("Impossibile connettersi. Notifica non inviata.", true, true);
                        return;
                    }
                }

                var notification = new MqttNotificationModel()
                {
                    DeviceId = "ciao",
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.UtcNow
                };

                string jsonPayload = JsonSerializer.Serialize(notification);

                // Crea il messaggio MQTT
                var m = new MqttApplicationMessageBuilder()
                    .WithTopic($"uom/notifications/{notification.DeviceId}")
                    .WithPayload(Encoding.UTF8.GetBytes(jsonPayload))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                var result = await _mqttClient.PublishAsync(m, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.Loggin($"Notifica MQTT inviata a {_deviceId}: {title}", false, false);
                }
                else
                {
                    Logger.Loggin($"Errore nell'invio della notifica: {result.ReasonString}", true, false);
                }
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nell'invio della notifica MQTT: {ex.Message}", true, false);
            }
        }

        public async Task SubscribeToTopicAsync(string deviceId)
        {
            try
            {
                if (_mqttClient == null)
                {
                    Logger.Loggin("Client MQTT non inizializzato", true, true);
                    return;
                }

                if (!_mqttClient.IsConnected)
                {
                    Logger.Loggin("Client MQTT non connesso. Tentativo di riconnessione...", false, true);
                    await ConnectWithRetryAsync();

                    if (!_mqttClient.IsConnected)
                    {
                        Logger.Loggin("Impossibile connettersi. Sottoscrizione non effettuata.", true, true);
                        return;
                    }
                }

                // Configura l'handler per i messaggi in arrivo
                _mqttClient.ApplicationMessageReceivedAsync += async e =>
                {
                    try
                    {
                        var topic = e.ApplicationMessage.Topic;
                        var payload = e.ApplicationMessage.Payload;

                        if (payload != null)
                        {
                            var message = Encoding.UTF8.GetString(payload);
                            Logger.Loggin($"Messaggio ricevuto sul topic {topic}: {message}", false, false);

                            var notification = JsonSerializer.Deserialize<MqttNotificationModel>(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Loggin($"Errore nella gestione del messaggio MQTT: {ex.Message}", true, true);
                    }

                };

                // Sottoscrizione al topic specifico
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic($"uom/startstop/{deviceId}")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                var subscribeResult = await _mqttClient.SubscribeAsync(topicFilter);

                Logger.Loggin($"Sottoscrizione al topic uom/startstop/{deviceId} completata con esito: {subscribeResult.ReasonString}", false, false);
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante la sottoscrizione al topic: {ex.Message}", true, true);
            }
        }

        public void Dispose()
        {
            try
            {
                if (_mqttClient?.IsConnected == true)
                {
                    _mqttClient.DisconnectAsync().Wait();
                }
                _mqttClient?.Dispose();
                _connectionSemaphore.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nella chiusura del client MQTT: {ex.Message}", false, false);
            }
        }
    }
}