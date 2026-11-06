using System;
using System.Collections.Generic;
using System.Globalization;

namespace Trabajo_Janier
{
    public sealed class Alerta
    {
        public DateTime Fecha { get; set; }
        public string Sensor { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public float Valor { get; set; }
        public float Umbral { get; set; }
        public string Mensaje { get; set; } = string.Empty;
    }

    public sealed class AlertaService
    {
        public const float TemperaturaMin = 15F;
        public const float TemperaturaMax = 40F;
        public const float HumedadMin = 30F;
        public const float HumedadMax = 80F;

        private const int MaximoAlertas = 500;
        private readonly object _sync = new();
        private readonly List<Alerta> _alertas = new();
        private readonly Dictionary<string, Estado> _estados = new(StringComparer.OrdinalIgnoreCase);

        public event Action<Alerta>? OnAlerta;

        public IReadOnlyList<Alerta> Alertas
        {
            get
            {
                lock (_sync)
                {
                    return _alertas.ToArray();
                }
            }
        }

        public void Evaluar(string sensor, float valor)
        {
            string nombre;
            string unidad;
            float minimo;
            float maximo;
            switch (sensor)
            {
                case "Temperatura":
                    nombre = "Temperatura";
                    unidad = "°C";
                    minimo = TemperaturaMin;
                    maximo = TemperaturaMax;
                    break;
                case "Humedad":
                    nombre = "Humedad";
                    unidad = "%";
                    minimo = HumedadMin;
                    maximo = HumedadMax;
                    break;
                default:
                    throw new ArgumentException("El sensor debe ser Temperatura o Humedad.", nameof(sensor));
            }

            Estado nuevoEstado = valor > maximo ? Estado.Alto
                : valor < minimo ? Estado.Bajo
                : Estado.Normal;
            Alerta? alerta = null;

            lock (_sync)
            {
                _estados.TryGetValue(nombre, out Estado estadoAnterior);
                if (nuevoEstado != Estado.Normal && nuevoEstado != estadoAnterior)
                {
                    bool alto = nuevoEstado == Estado.Alto;
                    float umbral = alto ? maximo : minimo;
                    string tipo = alto ? "Muy alto" : "Muy bajo";
                    string limite = alto ? "máximo permitido" : "mínimo permitido";
                    alerta = new Alerta
                    {
                        Fecha = DateTime.Now,
                        Sensor = nombre,
                        Tipo = tipo,
                        Valor = valor,
                        Umbral = umbral,
                        Mensaje = $"{nombre} {(alto ? "muy alta" : "muy baja")}: {valor.ToString("0.#", CultureInfo.CurrentCulture)} {unidad} ({limite} {umbral.ToString("0.#", CultureInfo.CurrentCulture)} {unidad})"
                    };
                    _alertas.Insert(0, alerta);
                    if (_alertas.Count > MaximoAlertas)
                    {
                        _alertas.RemoveAt(_alertas.Count - 1);
                    }
                }

                _estados[nombre] = nuevoEstado;
            }

            if (alerta != null)
            {
                OnAlerta?.Invoke(alerta);
            }
        }

        public bool EstaFueraDeRango(string sensor)
        {
            lock (_sync)
            {
                return _estados.TryGetValue(sensor, out Estado estado) && estado != Estado.Normal;
            }
        }

        public void Limpiar()
        {
            lock (_sync)
            {
                _alertas.Clear();
            }
        }

        private enum Estado
        {
            Normal,
            Alto,
            Bajo
        }
    }
}
