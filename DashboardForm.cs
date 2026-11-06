using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Trabajo_Janier
{
    public class DashboardForm : Form
    {
        private readonly Label valorSensores;
        private readonly Label estadoSensores;
        private readonly Label valorTemperatura;
        private readonly Label valorHumedad;
        private readonly Label detalleTemperatura;
        private readonly Label detalleHumedad;
        private readonly Button botonEstadoMqtt;
        private readonly MonitoringChartPanel grafico;
        private readonly System.Windows.Forms.Timer temporizadorSimulado;
        private readonly Random aleatorio = new();
        private readonly AlertaService alertaService;
        private float temperaturaActual = 26.4F;
        private float humedadActual = 68F;
        private double faseSimulacion;
        private readonly MqttService _mqtt = new();

        public DashboardForm(AlertaService alertaService, bool iniciarSimulacion = false)
        {
            this.alertaService = alertaService;
            Text = "Dashboard";
            BackColor = Tema.FondoGeneral;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopLevel = false;
            Dock = DockStyle.Fill;
            Padding = new Padding(18);
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Tema.FondoGeneral,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 194F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var header = CrearHeader(out botonEstadoMqtt);
            var tarjetas = CrearFilaTarjetas(
                out valorSensores,
                out estadoSensores,
                out valorTemperatura,
                out detalleTemperatura,
                out valorHumedad,
                out detalleHumedad);
            grafico = new MonitoringChartPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Tema.Blanco,
                Margin = new Padding(8, 8, 8, 0)
            };
            layout.Controls.Add(header, 0, 0);
            layout.Controls.Add(tarjetas, 0, 1);
            layout.Controls.Add(grafico, 0, 2);
            Controls.Add(layout);

            temporizadorSimulado = new System.Windows.Forms.Timer { Interval = 2000 };
            temporizadorSimulado.Tick += (_, _) => GenerarLecturaSimulada();
            ConfigurarSimulacion(iniciarSimulacion);

            _mqtt.OnLecturaRecibida += datos =>
            {
                if (datos.Temperatura.HasValue)
                {
                    alertaService.Evaluar("Temperatura", datos.Temperatura.Value);
                }

                if (datos.Humedad.HasValue)
                {
                    alertaService.Evaluar("Humedad", datos.Humedad.Value);
                }

                AgregarLectura(datos.Temperatura, datos.Humedad);
            };
            _mqtt.OnEstadoCambiado += estado => ActualizarEstadoMqtt(estado);
        }

        

        public void ActualizarSensor(int activos, string estado)
        {
            EjecutarEnUi(() =>
            {
                valorSensores.Text = $"{Math.Max(0, activos)} activos";
                estadoSensores.Text = string.IsNullOrWhiteSpace(estado) ? "Estado no disponible" : estado.Trim();
            });
        }

        public void ActualizarTemperatura(float valor)
        {
            EjecutarEnUi(() =>
            {
                temperaturaActual = valor;
                valorTemperatura.Text = Formatear(valor, " °C");
                detalleTemperatura.Text = "Promedio actual";
                grafico.AgregarPunto(temperaturaActual, humedadActual);
            });
        }

        public void ActualizarHumedad(float valor)
        {
            EjecutarEnUi(() =>
            {
                humedadActual = valor;
                valorHumedad.Text = Formatear(valor, " %");
                detalleHumedad.Text = "Suelo y ambiente";
                grafico.AgregarPunto(temperaturaActual, humedadActual);
            });
        }

        public void AgregarLectura(float? temp, float? hum)
        {
            EjecutarEnUi(() =>
            {
                if (temp.HasValue)
                {
                    temperaturaActual = temp.Value;
                    valorTemperatura.Text = Formatear(temp.Value, " °C");
                    detalleTemperatura.Text = "Promedio actual";
                }

                if (hum.HasValue)
                {
                    humedadActual = hum.Value;
                    valorHumedad.Text = Formatear(hum.Value, " %");
                    detalleHumedad.Text = "Suelo y ambiente";
                }

                valorTemperatura.ForeColor = alertaService.EstaFueraDeRango("Temperatura")
                    ? Tema.Rojo : Tema.TextoOscuro;
                valorHumedad.ForeColor = alertaService.EstaFueraDeRango("Humedad")
                    ? Tema.Rojo : Tema.TextoOscuro;
                grafico.AgregarPunto(temp ?? float.NaN, hum ?? float.NaN);
            });
        }

        public void ActualizarEstadoMqtt(string estado)
        {
            EjecutarEnUi(() =>
            {
                string texto = string.IsNullOrWhiteSpace(estado) ? "desconectado" : estado.Trim();
                botonEstadoMqtt.Text = $"MQTT: {texto}";
            });
        }

        public void ConfigurarSimulacion(bool activa)
        {
            EjecutarEnUi(() =>
            {
                if (activa)
                {
                    temporizadorSimulado.Start();
                }
                else
                {
                    temporizadorSimulado.Stop();
                }
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                temporizadorSimulado?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _ = Task.Run(() => _mqtt.IniciarAsync());
        }

        public Task DesconectarMqttAsync() => _mqtt.DesconectarAsync();

        private static string Formatear(float valor, string unidad)
        {
            return valor.ToString("0.0", CultureInfo.CurrentCulture) + unidad;
        }

        private void EjecutarEnUi(Action accion)
        {
            if (IsDisposed || Disposing)
            {
                return;
            }

            if (InvokeRequired)
            {
                if (IsHandleCreated)
                {
                    try
                    {
                        BeginInvoke(accion);
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }

                return;
            }

            accion();
        }

        private void GenerarLecturaSimulada()
        {
            faseSimulacion += 0.45;
            float temperatura = 26.4F + (float)Math.Sin(faseSimulacion) * 2.2F + (float)(aleatorio.NextDouble() - 0.5) * 0.3F;
            float humedad = 68F + (float)Math.Cos(faseSimulacion * 0.7) * 6F + (float)(aleatorio.NextDouble() - 0.5) * 1.2F;
            AgregarLectura(temperatura, humedad);
        }

        private static Control CrearHeader(out Button estadoMqtt)
        {
            var header = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(8, 0, 8, 8)
            };
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var textos = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
            };
            textos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            textos.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            textos.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));

            var titulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Resumen general",
                Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Monitoreo general del sistema agrícola Agrosoft SENA",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Tema.TextoMuted,
                TextAlign = ContentAlignment.MiddleLeft
            };
            textos.Controls.Add(titulo, 0, 0);
            textos.Controls.Add(subtitulo, 0, 1);

            estadoMqtt = new Button
            {
                AutoSize = true,
                MinimumSize = new Size(148, 38),
                Text = "MQTT: desconectado",
                FlatStyle = FlatStyle.Flat,
                BackColor = Tema.VerdeOscuro,
                ForeColor = Tema.Blanco,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(12, 12, 0, 0),
                Padding = new Padding(12, 0, 12, 0),
                UseVisualStyleBackColor = false
            };
            estadoMqtt.FlatAppearance.BorderSize = 0;
            estadoMqtt.FlatAppearance.MouseOverBackColor = Tema.VerdeHover;

            header.Controls.Add(textos, 0, 0);
            header.Controls.Add(estadoMqtt, 1, 0);
            return header;
        }

        private static Control CrearFilaTarjetas(
            out Label sensoresValor,
            out Label sensoresEstado,
            out Label temperaturaValor,
            out Label temperaturaDetalle,
            out Label humedadValor,
            out Label humedadDetalle)
        {
            var fila = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333F));
            fila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.334F));
            fila.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var sensores = CrearTarjeta("SISTEMA", "Sensores Activos", "24 activos", "Estado: Normal",
                out sensoresValor, out sensoresEstado);
            var temperatura = CrearTarjeta("AMBIENTE", "Temperatura", "26.4 °C", "Promedio actual",
                out temperaturaValor, out temperaturaDetalle);
            var humedad = CrearTarjeta("AMBIENTE", "Humedad", "68 %", "Suelo y ambiente",
                out humedadValor, out humedadDetalle);

            fila.Controls.Add(sensores, 0, 0);
            fila.Controls.Add(temperatura, 1, 0);
            fila.Controls.Add(humedad, 2, 0);
            return fila;
        }

        private static Control CrearTarjeta(
            string insignia,
            string titulo,
            string valor,
            string detalle,
            out Label etiquetaValor,
            out Label etiquetaDetalle)
        {
            var tarjeta = new BufferedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Tema.Blanco,
                Padding = new Padding(18, 14, 18, 12),
                Margin = new Padding(8)
            };
            tarjeta.Paint += (_, e) =>
            {
                using var pen = new Pen(Tema.BordeSuave);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    Math.Max(0, tarjeta.ClientSize.Width - 1),
                    Math.Max(0, tarjeta.ClientSize.Height - 1));
            };

            var contenido = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Tema.Blanco,
                Margin = new Padding(0)
            };
            contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            contenido.RowStyles.Add(new RowStyle(SizeType.Absolute, 23F));

            var etiqueta = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Text = insignia,
                BackColor = Tema.VerdeSuave,
                ForeColor = Tema.VerdeOscuro,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(7, 3, 7, 3),
                Margin = new Padding(0, 0, 0, 2)
            };
            var etiquetaTitulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = titulo,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Tema.TextoMuted,
                TextAlign = ContentAlignment.MiddleLeft
            };
            etiquetaValor = new Label
            {
                Dock = DockStyle.Fill,
                Text = valor,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            etiquetaDetalle = new Label
            {
                Dock = DockStyle.Fill,
                Text = detalle,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Tema.TextoMuted,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            contenido.Controls.Add(etiqueta, 0, 0);
            contenido.Controls.Add(etiquetaTitulo, 0, 1);
            contenido.Controls.Add(etiquetaValor, 0, 2);
            contenido.Controls.Add(etiquetaDetalle, 0, 3);
            tarjeta.Controls.Add(contenido);
            return tarjeta;
        }

        private sealed class BufferedPanel : Panel
        {
            public BufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        private sealed class MonitoringChartPanel : Panel
        {
            private const int MaximoPuntos = 30;
            private readonly List<float> temperaturas = new();
            private readonly List<float> humedades = new();

            public MonitoringChartPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }

            public void AgregarPunto(float temperatura, float humedad)
            {
                temperaturas.Add(temperatura);
                humedades.Add(humedad);
                if (temperaturas.Count > MaximoPuntos)
                {
                    temperaturas.RemoveAt(0);
                    humedades.RemoveAt(0);
                }

                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                Graphics graphics = e.Graphics;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Tema.Blanco);

                using var borde = new Pen(Tema.BordeSuave);
                graphics.DrawRectangle(borde, 0, 0, Math.Max(0, Width - 1), Math.Max(0, Height - 1));

                using var tituloFont = new Font("Segoe UI", 11F, FontStyle.Bold);
                using var leyendaFont = new Font("Segoe UI", 8.5F, FontStyle.Regular);
                using var textoFont = new Font("Segoe UI", 8F, FontStyle.Regular);
                using var tituloBrush = new SolidBrush(Tema.TextoOscuro);
                using var textoBrush = new SolidBrush(Tema.TextoMuted);
                graphics.DrawString("Historial de Mediciones en Tiempo Real (Temperatura / Humedad)",
                    tituloFont, tituloBrush, 20, 17);

                DibujarLeyenda(graphics, leyendaFont, tituloBrush);

                var area = new RectangleF(66, 77, Math.Max(10, Width - 132), Math.Max(10, Height - 125));
                using var cuadrillaPen = new Pen(Tema.BordeSuave, 1F);
                using var ejePen = new Pen(Color.FromArgb(190, 199, 205), 1F);

                for (int i = 0; i <= 5; i++)
                {
                    float y = area.Top + area.Height * i / 5F;
                    graphics.DrawLine(cuadrillaPen, area.Left, y, area.Right, y);
                    graphics.DrawString((50 - i * 10).ToString(CultureInfo.InvariantCulture), textoFont, textoBrush,
                        27, y - 7);
                    graphics.DrawString((100 - i * 20).ToString(CultureInfo.InvariantCulture), textoFont, textoBrush,
                        area.Right + 9, y - 7);
                }

                graphics.DrawLine(ejePen, area.Left, area.Top, area.Left, area.Bottom);
                graphics.DrawLine(ejePen, area.Left, area.Bottom, area.Right, area.Bottom);
                graphics.DrawString("°C", textoFont, textoBrush, 27, area.Top - 18);
                graphics.DrawString("%", textoFont, textoBrush, area.Right + 9, area.Top - 18);
                graphics.DrawString("Lecturas recientes", textoFont, textoBrush,
                    area.Left + area.Width / 2F - 38, area.Bottom + 8);

                if (!temperaturas.Exists(valor => !float.IsNaN(valor))
                    && !humedades.Exists(valor => !float.IsNaN(valor)))
                {
                    using var esperaBrush = new SolidBrush(Tema.TextoMuted);
                    const string mensaje = "Esperando lecturas MQTT...";
                    SizeF size = graphics.MeasureString(mensaje, textoFont);
                    graphics.DrawString(mensaje, textoFont, esperaBrush,
                        area.Left + (area.Width - size.Width) / 2F,
                        area.Top + (area.Height - size.Height) / 2F);
                    return;
                }

                DibujarSerie(graphics, temperaturas, area, 50F, Tema.VerdeOscuro);
                DibujarSerie(graphics, humedades, area, 100F, Color.FromArgb(16, 185, 129));
            }

            private static void DibujarLeyenda(Graphics graphics, Font font, Brush textoBrush)
            {
                float x = Math.Max(250, graphics.VisibleClipBounds.Width - 245);
                float y = 49;
                using var tempPen = new Pen(Tema.VerdeOscuro, 2.5F);
                using var humPen = new Pen(Color.FromArgb(16, 185, 129), 2.5F);
                graphics.DrawLine(tempPen, x, y + 6, x + 20, y + 6);
                graphics.DrawString("Temperatura", font, textoBrush, x + 25, y);
                graphics.DrawLine(humPen, x + 112, y + 6, x + 132, y + 6);
                graphics.DrawString("Humedad", font, textoBrush, x + 137, y);
            }

            private static void DibujarSerie(Graphics graphics, List<float> valores, RectangleF area, float maximo, Color color)
            {
                if (valores.Count == 0)
                {
                    return;
                }

                using var pen = new Pen(color, 2.5F)
                {
                    LineJoin = LineJoin.Round,
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                using var puntoBrush = new SolidBrush(color);
                PointF? puntoAnterior = null;

                for (int i = 0; i < valores.Count; i++)
                {
                    if (float.IsNaN(valores[i]))
                    {
                        puntoAnterior = null;
                        continue;
                    }

                    float x = valores.Count == 1
                        ? area.Left + area.Width / 2F
                        : area.Left + area.Width * i / (valores.Count - 1F);
                    float proporcion = Math.Clamp(valores[i] / maximo, 0F, 1F);
                    float y = area.Bottom - area.Height * proporcion;
                    var punto = new PointF(x, y);
                    if (puntoAnterior.HasValue)
                    {
                        graphics.DrawLine(pen, puntoAnterior.Value, punto);
                    }

                    graphics.FillEllipse(puntoBrush, punto.X - 3, punto.Y - 3, 6, 6);
                    puntoAnterior = punto;
                }
            }
        }
    }
}
