using LogManager;
using MQTT.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;

namespace MQTT
{
    public class MqttNotificationService : IDisposable
    {
        private IMqttClient _mqttClient;
        private bool _isConnected = false;
        private readonly SemaphoreSlim _connectionSemaphore = new SemaphoreSlim(1, 1);
        public Action<bool> Run { get; set; }
        public Action Logout { get; set; }

        string _deviceId = string.Empty;
        private bool _smartphoneConnected = false;
        public event EventHandler<bool> SmartphoneConnectionChanged;
        // Proprietà pubblica per esporre lo stato
        public bool SmartphoneConnected
        {
            get => _smartphoneConnected;
            private set
            {
                if (_smartphoneConnected != value)
                {
                    _smartphoneConnected = value;
                    // Notifica il cambiamento
                    SmartphoneConnectionChanged?.Invoke(this, value);
                }
            }
        }
        public MqttNotificationService()
        {
        }

        public async Task InitializeMqttClientAsync(string deviceId)
        {
            try
            {
                _deviceId = deviceId;
                // Metodo alternativo per creare il client
                _mqttClient = new MqttFactory().CreateMqttClient();

                Logger.Loggin("Client MQTT creato con successo", false, false);

                // Handler per disconnessione
                _mqttClient.DisconnectedAsync += async args =>
                {
                    _isConnected = false;
                    Logger.Loggin($"Disconnesso dal broker MQTT. Motivo: {args.Reason}", false, true);

                    await Task.Delay(5000);
                    await ConnectWithRetryAsync(deviceId);
                };

                // Connessione iniziale
                await ConnectWithRetryAsync(deviceId);
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

        private async Task ConnectWithRetryAsync(string deviceId)
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
                            await SubscribeToTopicAsync(deviceId);
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

        public async Task SendNotificationAsync(MqttNotificationModel message)
        {
           
            message.DeviceId = _deviceId;
            try
            {
                if (_mqttClient == null)
                {
                    Logger.Loggin("Client MQTT non inizializzato", true, true);
                    return;
                }

                if (!_smartphoneConnected)
                {
                    Logger.Loggin("Connessione con smartphone non avvenuta", false, false);
                    return;
                }

                if (!_mqttClient.IsConnected)
                {
                    Logger.Loggin("Client MQTT non connesso. Tentativo di riconnessione...", false, true);
                    await ConnectWithRetryAsync(message.DeviceId);

                    if (!_mqttClient.IsConnected)
                    {
                        Logger.Loggin("Impossibile connettersi. Notifica non inviata.", true, true);
                        return;
                    }
                }


                string jsonPayload = JsonSerializer.Serialize(message);

                // Crea il messaggio MQTT
                var m = new MqttApplicationMessageBuilder()
                    .WithTopic($"uom/notifications/{message.DeviceId}")
                    .WithPayload(Encoding.UTF8.GetBytes(jsonPayload))
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag(false)
                    .Build();

                var result = await _mqttClient.PublishAsync(m, CancellationToken.None);

                if (!result.IsSuccess)
                {
                    Logger.Loggin($"Errore nell'invio della notifica: {result.ReasonString}", false, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore nell'invio della notifica MQTT: {ex.Message}", false, true);
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
                    await ConnectWithRetryAsync(deviceId);

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
                            if(notification != null)
                            {
                                if (notification.Title.ToLower().Contains("start"))
                                {

                                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                                        Run?.Invoke(true);
                                    });
                                }
                                else if (notification.Title.ToLower().Contains("stop"))
                                    Run?.Invoke(false);
                                else if (notification.Title.ToLower().Contains("logout"))
                                {
                                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() => {
                                        Logout?.Invoke();
                                    });
                                }
                                else if (notification.Message.ToLower().Contains("connect"))
                                {
                                    SendNotificationAsync(new MqttNotificationModel() { Title = "CONNECT-OK", Message = "CONNECT-OK", Type = MqttNotificationModel.NotificationSeverity.Info });
                                    SmartphoneConnected = true;
                                }
                                else
                                    Logger.Loggin($"Messaggio non riconosciuto: {notification.Message}", false, false);
                            }
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


                var smartphoneConnection = new MqttTopicFilterBuilder()
                   .WithTopic($"uom/connection/{deviceId}")
                   .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                   .Build();

                var subscribeConnectionResult = await _mqttClient.SubscribeAsync(smartphoneConnection);

                Logger.Loggin($"Sottoscrizione al topic uom/startstop/{deviceId} completata con esito: {subscribeResult.ReasonString}", false, false);
                Logger.Loggin($"Sottoscrizione al topic uom/connection/{deviceId} completata con esito: {subscribeResult.ReasonString}", false, false);

            }
            catch (Exception ex)
            {
                Logger.Loggin($"Errore durante la sottoscrizione al topic: {ex.Message}", true, true);
            }
        }

        public void ResetSmartphoneConnection()
        {
            SmartphoneConnected = false;
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