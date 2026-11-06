using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace Trabajo_Janier
{
    public class DatosSensor
    {
        public float? Temperatura { get; set; }
        public float? Humedad { get; set; }
    }

    public class MqttService
    {
        private IMqttClient? _mqttClient;
        private MqttClientOptions? _opciones;
        private string _topicActual = "agrosoft/janier-7f3k/telemetria";
        private bool _desconexionManual = false;
        private bool _eventosConfigurados;
        private int _reconexionEnCurso;
        public event Action<DatosSensor>? OnLecturaRecibida;
        public event Action<string>? OnEstadoCambiado;

        public bool EstaConectado => _mqttClient?.IsConnected ?? false;

        public async Task IniciarAsync(string host = "broker.emqx.io", int puerto = 1883, string topic = "agrosoft/janier-7f3k/telemetria")
        {
            if (_mqttClient?.IsConnected == true)
            {
                return;
            }

            _topicActual = topic;
            _desconexionManual = false;

            var factory = new MqttFactory();
            _mqttClient ??= factory.CreateMqttClient();
            var mqttClient = _mqttClient;

            _opciones = new MqttClientOptionsBuilder()
                .WithTcpServer(host, puerto)
                .WithClientId($"Agrosoft_Client_{Guid.NewGuid():N}")
                .WithCleanSession()
                .Build();

            if (mqttClient != null && !_eventosConfigurados)
            {
                mqttClient.ConnectedAsync += async e =>
                {
                    OnEstadoCambiado?.Invoke("Conectado");
                    await mqttClient.SubscribeAsync(_topicActual);
                };

                mqttClient.DisconnectedAsync += e =>
                {
                    OnEstadoCambiado?.Invoke("Desconectado");
                    if (!_desconexionManual && Interlocked.Exchange(ref _reconexionEnCurso, 1) == 0)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                while (!_desconexionManual && !mqttClient.IsConnected)
                                {
                                    OnEstadoCambiado?.Invoke("Reconectando...");
                                    try
                                    {
                                        if (_opciones != null)
                                        {
                                            await mqttClient.ConnectAsync(_opciones);
                                        }
                                    }
                                    catch
                                    {
                                    }

                                    if (!mqttClient.IsConnected && !_desconexionManual)
                                    {
                                        await Task.Delay(TimeSpan.FromSeconds(5));
                                    }
                                }
                            }
                            finally
                            {
                                Interlocked.Exchange(ref _reconexionEnCurso, 0);
                            }
                        });
                    }

                    return Task.CompletedTask;
                };

                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    try
                    {
                        string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

                        var datos = JsonSerializer.Deserialize<DatosSensor>(payload, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (datos != null)
                        {
                            OnLecturaRecibida?.Invoke(datos);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al deserializar: {ex.Message}");
                    }

                    return Task.CompletedTask;
                };

                _eventosConfigurados = true;
            }

            try
            {
                await mqttClient!.ConnectAsync(_opciones);
            }
            catch (Exception ex)
            {
                OnEstadoCambiado?.Invoke($"Error: {ex.Message}");
            }
        }

        public async Task DesconectarAsync()
        {
            _desconexionManual = true;
            if (_mqttClient != null && _mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }
    }
}