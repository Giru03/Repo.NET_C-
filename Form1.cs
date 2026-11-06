using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trabajo_Janier
{
    public partial class Form1 : Form
    {
        private const int EspacioIconoTexto = 3;

        private readonly Panel sidebar;
        private readonly Panel areaContenido;
        private readonly Panel hostVistas;
        private readonly Label tituloVista;
        private readonly string usuarioActual;
        private readonly Font fuenteIconos;
        private readonly ToolTip toolTipMenu = new();
        private readonly DashboardForm dashboard;
        private readonly AlertaService alertaService;
        private readonly AlertasForm alertasForm;
        private readonly List<Button> botonesMenu = new();
        private readonly Dictionary<string, Form> formularios = new();
        private readonly HashSet<Button> botonesEnHover = new();
        private Button? botonActivo;
        private Button? botonAlertas;
        private Button? botonCerrarSesion;
        private TableLayoutPanel? layoutMarca;
        private Panel? logoMarca;
        private Panel? panelMarca;
        private Label? tituloMarca;
        private Form? formularioActivo;
        private int alertasNoLeidas;
        private bool sidebarColapsado;

        public Form1(string usuario = "Admin")
        {
            InitializeComponent();

            usuarioActual = string.IsNullOrWhiteSpace(usuario) ? "Admin" : usuario.Trim();
            Text = "Agrosoft SENA";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Normal;
            ClientSize = new Size(1200, 750);
            BackColor = Tema.FondoGeneral;
            DoubleBuffered = true;
            MinimumSize = new Size(1100, 680);
            fuenteIconos = new Font(ObtenerFuenteIconos(), 16F, FontStyle.Regular);

            sidebar = CrearSidebar();
            areaContenido = CrearAreaContenido(out hostVistas, out tituloVista);

            Controls.Add(areaContenido);
            Controls.Add(sidebar);

            alertaService = new AlertaService();
            dashboard = new DashboardForm(alertaService);
            alertasForm = new AlertasForm(alertaService);
            formularios["Dashboard"] = dashboard;
            formularios["Alertas"] = alertasForm;
            formularios["Conexiones"] = CrearVista("Conexiones", "Conexiones activas y servicios");
            alertaService.OnAlerta += AlertaRecibida;

            AbrirFormulario(formularios["Dashboard"]);
            SeleccionarBoton(botonesMenu[0]);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                dashboard.DesconectarMqttAsync().Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
                // Salida limpia sin bloquear el cierre de la aplicación
            }
            base.OnFormClosing(e);
        }

        private Panel CrearSidebar()
        {
            var panelSidebar = new PanelConDobleBuffer
            {
                Dock = DockStyle.Left,
                Width = 248,
                BackColor = Tema.Blanco,
                BorderStyle = BorderStyle.None
            };
            panelSidebar.Paint += (_, e) =>
            {
                using var pen = new Pen(Tema.BordeSuave);
                e.Graphics.DrawLine(pen, panelSidebar.ClientSize.Width - 1, 0,
                    panelSidebar.ClientSize.Width - 1, panelSidebar.ClientSize.Height);
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = Tema.Blanco,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

            var panelMarca = CrearPanelMarca();
            var menu = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Tema.Blanco,
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0)
            };
            menuSidebar = menu;
            menu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            string[] opciones = { "Dashboard", "Alertas", "Conexiones" };
            menu.RowCount = opciones.Length;
            for (int i = 0; i < opciones.Length; i++)
            {
                menu.RowStyles.Add(new RowStyle(SizeType.Absolute, 49F));
                var boton = CrearBotonMenu(opciones[i], opciones[i]);
                menu.Controls.Add(boton, 0, i);
                botonesMenu.Add(boton);
            }

            var separador = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Tema.BordeSuave,
                Margin = new Padding(16, 0, 16, 0)
            };
            var botonCerrar = new BotonSinFoco
            {
                Dock = DockStyle.Fill,
                Text = string.Empty,
                AccessibleName = "Cerrar sesión",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                BackColor = Tema.Blanco,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                TabStop = false,
                UseVisualStyleBackColor = false
            };
            botonCerrar.FlatAppearance.BorderSize = 0;
            botonCerrar.FlatAppearance.MouseOverBackColor = Tema.VerdeSuave;
            botonCerrar.Paint += (_, e) => DibujarBotonCerrar(botonCerrar, e.Graphics);
            botonCerrar.Click += (_, _) =>
            {
                var loginForm = new LoginForm();
                Application.Exit();
            };
            botonCerrarSesion = botonCerrar;
            toolTipMenu.SetToolTip(botonCerrar, "Cerrar sesión");

            layout.Controls.Add(panelMarca, 0, 0);
            layout.Controls.Add(menu, 0, 1);
            layout.Controls.Add(separador, 0, 2);
            layout.Controls.Add(botonCerrar, 0, 3);
            panelSidebar.Controls.Add(layout);

            return panelSidebar;
        }

        private Panel CrearPanelMarca()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Tema.Blanco,
                Padding = new Padding(18, 16, 12, 12)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Tema.Blanco,
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var logo = new Panel
            {
                Size = new Size(40, 40),
                Anchor = AnchorStyles.None,
                BackColor = Color.Transparent
            };
            logo.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var fondo = new SolidBrush(Tema.VerdeSuave);
                e.Graphics.FillEllipse(fondo, 1, 1, logo.Width - 3, logo.Height - 3);

                using var tallo = new Pen(Tema.VerdeOscuro, 2.2F)
                {
                    StartCap = LineCap.Round,
                    EndCap = LineCap.Round
                };
                e.Graphics.DrawBezier(tallo, 21, 31, 21, 25, 20, 18, 20, 11);

                using var hojaIzquierda = new GraphicsPath();
                hojaIzquierda.AddBezier(20, 22, 14, 22, 11, 18, 11, 14);
                hojaIzquierda.AddBezier(11, 14, 17, 14, 20, 17, 20, 22);
                hojaIzquierda.CloseFigure();
                using var hojaDerecha = new GraphicsPath();
                hojaDerecha.AddBezier(20, 18, 20, 12, 24, 9, 29, 9);
                hojaDerecha.AddBezier(29, 9, 29, 15, 26, 18, 20, 18);
                hojaDerecha.CloseFigure();
                using var rellenoHoja = new SolidBrush(Tema.VerdeOscuro);
                e.Graphics.FillPath(rellenoHoja, hojaIzquierda);
                e.Graphics.FillPath(rellenoHoja, hojaDerecha);
            };

            var titulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Agrosoft SENA",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            layoutMarca = layout;
            logoMarca = logo;
            panelMarca = panel;
            tituloMarca = titulo;

            layout.Controls.Add(logo, 0, 0);
            layout.Controls.Add(titulo, 1, 0);
            panel.Controls.Add(layout);
            return panel;
        }

        private Button CrearBotonMenu(string texto, string claveFormulario)
        {
            var boton = new BotonSinFoco
            {
                Dock = DockStyle.Top,
                Height = 45,
                Text = string.Empty,
                AccessibleName = texto,
                Tag = claveFormulario,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                BackColor = Tema.Blanco,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TabStop = false,
                Margin = new Padding(0, 2, 0, 2)
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseDownBackColor = Tema.VerdeSuave;
            if (claveFormulario == "Alertas")
            {
                botonAlertas = boton;
            }
            ActualizarEstiloBoton(boton);

            boton.Click += (_, _) =>
            {
                if (formularios.TryGetValue(claveFormulario, out var formHijo))
                {
                    AbrirFormulario(formHijo);
                    SeleccionarBoton(boton);
                }
            };
            boton.MouseEnter += (_, _) =>
            {
                botonesEnHover.Add(boton);
                ActualizarEstiloBoton(boton);
            };
            boton.MouseLeave += (_, _) =>
            {
                botonesEnHover.Remove(boton);
                ActualizarEstiloBoton(boton);
            };
            boton.Paint += (_, e) => DibujarBotonMenu(boton, texto, claveFormulario, e.Graphics);
            toolTipMenu.SetToolTip(boton, texto);

            return boton;
        }

        private Panel CrearAreaContenido(out Panel host, out Label titulo)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Tema.FondoGeneral,
                BorderStyle = BorderStyle.None
            };

            var barraSuperior = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Tema.Blanco,
                Padding = new Padding(28, 0, 24, 0)
            };
            barraSuperior.Paint += (_, e) =>
            {
                using var pen = new Pen(Tema.BordeSuave);
                e.Graphics.DrawLine(pen, 0, barraSuperior.Height - 1,
                    barraSuperior.Width, barraSuperior.Height - 1);
            };

            var layoutBarra = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Tema.Blanco,
                Margin = new Padding(0)
            };
            layoutBarra.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F));
            layoutBarra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutBarra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layoutBarra.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var hamburguesa = new BotonSinFoco
            {
                Anchor = AnchorStyles.None,
                Size = new Size(36, 36),
                Text = "\uE700",
                Font = new Font(fuenteIconos.FontFamily, 13F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Tema.TextoOscuro,
                BackColor = Tema.Blanco,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                TabStop = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 8, 0),
                AccessibleName = "Alternar menú lateral"
            };
            hamburguesa.FlatAppearance.BorderSize = 0;
            hamburguesa.FlatAppearance.MouseOverBackColor = Tema.VerdeSuave;
            hamburguesa.FlatAppearance.MouseDownBackColor = Tema.VerdeSuave;
            hamburguesa.Click += (_, _) => AlternarSidebar();
            toolTipMenu.SetToolTip(hamburguesa, "Alternar menú lateral");

            titulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Dashboard",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var estadoSesion = new Label
            {
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                Text = $"{DateTime.Now.ToString("dddd, d 'de' MMMM", new CultureInfo("es-CO"))}  |  Usuario: {usuarioActual}  |  Sesión activa",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Tema.TextoMuted,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0, 0, 0, 0)
            };
            layoutBarra.Controls.Add(hamburguesa, 0, 0);
            layoutBarra.Controls.Add(titulo, 1, 0);
            layoutBarra.Controls.Add(estadoSesion, 2, 0);
            barraSuperior.Controls.Add(layoutBarra);

            host = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Tema.FondoGeneral,
                BorderStyle = BorderStyle.None
            };

            panel.Controls.Add(host);
            panel.Controls.Add(barraSuperior);
            return panel;
        }

        private Form CrearVista(string titulo, string descripcion)
        {
            var form = new Form
            {
                Text = titulo,
                BackColor = Tema.FondoGeneral,
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                TopLevel = false,
                Dock = DockStyle.Fill,
                Visible = false
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Tema.FondoGeneral,
                Padding = new Padding(28, 24, 28, 28),
                Margin = new Padding(0)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var encabezado = new Label
            {
                Dock = DockStyle.Fill,
                Text = titulo,
                ForeColor = Tema.TextoOscuro,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = descripcion,
                ForeColor = Tema.TextoMuted,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var tarjeta = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Tema.Blanco,
                Padding = new Padding(22),
                Margin = new Padding(0, 12, 0, 0)
            };
            tarjeta.Paint += (_, e) =>
            {
                using var pen = new Pen(Tema.BordeSuave);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    Math.Max(0, tarjeta.ClientSize.Width - 1),
                    Math.Max(0, tarjeta.ClientSize.Height - 1));
            };

            var contenidoTarjeta = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Tema.Blanco,
                Margin = new Padding(0)
            };
            contenidoTarjeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            contenidoTarjeta.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            contenidoTarjeta.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var tituloTarjeta = new Label
            {
                Dock = DockStyle.Fill,
                Text = titulo,
                ForeColor = Tema.TextoOscuro,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var textoTarjeta = new Label
            {
                Dock = DockStyle.Fill,
                Text = $"Bienvenido a la vista de {titulo}. Aquí se mostrará la información principal relacionada con esta sección.",
                ForeColor = Color.FromArgb(75, 88, 80),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 8, 0, 0)
            };
            contenidoTarjeta.Controls.Add(tituloTarjeta, 0, 0);
            contenidoTarjeta.Controls.Add(textoTarjeta, 0, 1);
            tarjeta.Controls.Add(contenidoTarjeta);

            layout.Controls.Add(encabezado, 0, 0);
            layout.Controls.Add(subtitulo, 0, 1);
            layout.Controls.Add(tarjeta, 0, 2);
            form.Controls.Add(layout);

            return form;
        }

        private void SeleccionarBoton(Button boton)
        {
            botonActivo = boton;
            foreach (var item in botonesMenu)
            {
                ActualizarEstiloBoton(item);
                item.Invalidate();
            }
        }

        public void AbrirFormulario(Form formHijo)
        {
            if (formHijo == alertasForm)
            {
                alertasNoLeidas = 0;
                ActualizarTextoBotonAlertas();
            }

            if (formularioActivo == formHijo)
            {
                tituloVista.Text = formHijo.Text;
                return;
            }

            if (formularioActivo != null)
            {
                formularioActivo.Hide();
            }

            formHijo.TopLevel = false;
            formHijo.FormBorderStyle = FormBorderStyle.None;
            formHijo.Dock = DockStyle.Fill;

            if (formHijo.Parent != hostVistas)
            {
                hostVistas.Controls.Add(formHijo);
            }

            formHijo.Show();
            formHijo.BringToFront();
            formularioActivo = formHijo;
            tituloVista.Text = formHijo.Text;
        }

        private void AlertaRecibida(Alerta alerta)
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed || !IsHandleCreated || formularioActivo == alertasForm)
                    {
                        return;
                    }

                    alertasNoLeidas++;
                    ActualizarTextoBotonAlertas();
                }));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void ActualizarTextoBotonAlertas()
        {
            if (botonAlertas != null)
            {
                botonAlertas.AccessibleName = alertasNoLeidas > 0
                    ? $"Alertas ({alertasNoLeidas})"
                    : "Alertas";
                ActualizarEstiloBoton(botonAlertas);
                botonAlertas.Invalidate();
            }
        }

        private void ActualizarEstiloBoton(Button b)
        {
            bool esAlertasPendientes = b == botonAlertas
                && alertasNoLeidas > 0
                && formularioActivo != alertasForm;
            bool esActivo = b == botonActivo;
            bool hover = botonesEnHover.Contains(b);

            if (esAlertasPendientes)
            {
                b.BackColor = hover ? OscurecerColor(Tema.Rojo, 0.84F) : Tema.Rojo;
                b.ForeColor = Tema.Blanco;
                b.FlatAppearance.MouseOverBackColor = OscurecerColor(Tema.Rojo, 0.84F);
            }
            else if (esActivo)
            {
                b.BackColor = Tema.VerdeSuave;
                b.ForeColor = Tema.TextoOscuro;
                b.FlatAppearance.MouseOverBackColor = Tema.VerdeSuave;
            }
            else
            {
                b.BackColor = hover ? Tema.VerdeSuave : Tema.Blanco;
                b.ForeColor = Tema.TextoOscuro;
                b.FlatAppearance.MouseOverBackColor = Tema.VerdeSuave;
            }

            b.FlatAppearance.MouseDownBackColor = b == botonAlertas && esAlertasPendientes
                ? OscurecerColor(Tema.Rojo, 0.84F)
                : Tema.VerdeSuave;
        }

        private void DibujarBotonMenu(Button boton, string texto, string clave, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (boton == botonActivo)
            {
                using var marcador = new SolidBrush(Tema.VerdeOscuro);
                graphics.FillRectangle(marcador, 0, 8, 3, Math.Max(0, boton.Height - 16));
            }

            string glifo = clave switch
            {
                "Dashboard" => "\uE80F",
                "Alertas" => "\uE7BA",
                "Conexiones" => "\uE701",
                _ => string.Empty
            };
            using var pincel = new SolidBrush(boton.ForeColor);
            SizeF tamanoGlifo = graphics.MeasureString(glifo, fuenteIconos);
            float glifoX = sidebarColapsado
                ? (boton.ClientSize.Width - tamanoGlifo.Width) / 2F
                : 18F;
            float glifoY = (boton.ClientSize.Height - tamanoGlifo.Height) / 2F;
            graphics.DrawString(glifo, fuenteIconos, pincel, glifoX, glifoY);

            if (!sidebarColapsado)
            {
                string etiqueta = texto;
                if (boton == botonAlertas && alertasNoLeidas > 0)
                {
                    etiqueta += $" ({alertasNoLeidas})";
                }

                using var fuenteTexto = new Font(boton.Font, FontStyle.Bold);
                float textoX = glifoX + tamanoGlifo.Width + EspacioIconoTexto;
                graphics.DrawString(etiqueta, fuenteTexto, pincel, textoX,
                    (boton.ClientSize.Height - graphics.MeasureString(etiqueta, fuenteTexto).Height) / 2F);
            }
            else if (boton == botonAlertas && alertasNoLeidas > 0)
            {
                string conteo = alertasNoLeidas > 99 ? "99+" : alertasNoLeidas.ToString(CultureInfo.InvariantCulture);
                using var fondoInsignia = new SolidBrush(Tema.Blanco);
                using var fuenteInsignia = new Font("Segoe UI", 7F, FontStyle.Bold);
                SizeF tamanoConteo = graphics.MeasureString(conteo, fuenteInsignia);
                float ancho = Math.Max(16F, tamanoConteo.Width + 6F);
                float x = boton.ClientSize.Width - ancho - 3F;
                const float y = 2F;
                graphics.FillEllipse(fondoInsignia, x, y, ancho, 16F);
                using var textoInsignia = new SolidBrush(Tema.Rojo);
                graphics.DrawString(conteo, fuenteInsignia, textoInsignia,
                    x + (ancho - tamanoConteo.Width) / 2F,
                    y + (16F - tamanoConteo.Height) / 2F);
            }
        }

        private void DibujarBotonCerrar(Button boton, Graphics graphics)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pincel = new SolidBrush(boton.ForeColor);
            string glifo = "\uE7E8";
            SizeF tamanoGlifo = graphics.MeasureString(glifo, fuenteIconos);
            if (sidebarColapsado)
            {
                graphics.DrawString(glifo, fuenteIconos, pincel,
                    (boton.ClientSize.Width - tamanoGlifo.Width) / 2F,
                    (boton.ClientSize.Height - tamanoGlifo.Height) / 2F);
                return;
            }

            graphics.DrawString(glifo, fuenteIconos, pincel, 18F,
                (boton.ClientSize.Height - tamanoGlifo.Height) / 2F);
            using var fuenteTexto = new Font(boton.Font, FontStyle.Bold);
            const string etiqueta = "Cerrar sesión";
            float textoX = 18F + tamanoGlifo.Width + EspacioIconoTexto;
            graphics.DrawString(etiqueta, fuenteTexto, pincel, textoX,
                (boton.ClientSize.Height - graphics.MeasureString(etiqueta, fuenteTexto).Height) / 2F);
        }

        private void AlternarSidebar()
        {
            sidebarColapsado = !sidebarColapsado;
            sidebar.Width = sidebarColapsado ? 72 : 248;
            sidebar.Padding = Padding.Empty;

            if (layoutMarca != null && logoMarca != null && tituloMarca != null)
            {
                layoutMarca.ColumnCount = 2;
                layoutMarca.ColumnStyles.Clear();
                layoutMarca.ColumnStyles.Add(new ColumnStyle(
                    sidebarColapsado ? SizeType.Percent : SizeType.Absolute,
                    sidebarColapsado ? 100F : 50F));
                layoutMarca.ColumnStyles.Add(new ColumnStyle(
                    SizeType.Percent,
                    sidebarColapsado ? 0F : 100F));

                logoMarca.Anchor = AnchorStyles.None;
                tituloMarca.Visible = !sidebarColapsado;
                layoutMarca.SetColumn(logoMarca, 0);
                layoutMarca.SetColumn(tituloMarca, 1);
            }

            if (panelMarca != null)
            {
                panelMarca.Padding = sidebarColapsado
                    ? Padding.Empty
                    : new Padding(18, 16, 12, 12);
            }

            foreach (Button boton in botonesMenu)
            {
                boton.TextAlign = ContentAlignment.MiddleCenter;
                boton.Invalidate();
                toolTipMenu.SetToolTip(boton, sidebarColapsado ? (string)boton.Tag! : string.Empty);
            }
            if (menuSidebar != null)
            {
                menuSidebar.Padding = sidebarColapsado
                    ? new Padding(6, 10, 6, 10)
                    : new Padding(12, 10, 12, 10);
            }

            if (botonCerrarSesion != null)
            {
                toolTipMenu.SetToolTip(botonCerrarSesion, sidebarColapsado ? "Cerrar sesión" : string.Empty);
                botonCerrarSesion.Invalidate();
            }

            menuSidebar?.PerformLayout();
            sidebar.Invalidate(true);
            hostVistas.PerformLayout();
            hostVistas.Invalidate(true);
        }

        private TableLayoutPanel? menuSidebar;

        private static string ObtenerFuenteIconos()
        {
            using var fuentes = new InstalledFontCollection();
            foreach (FontFamily familia in fuentes.Families)
            {
                if (familia.Name == "Segoe MDL2 Assets")
                {
                    return familia.Name;
                }
            }

            return "Segoe UI Symbol";
        }

        private static Color OscurecerColor(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                (int)(color.R * factor),
                (int)(color.G * factor),
                (int)(color.B * factor));
        }

        private sealed class PanelConDobleBuffer : Panel
        {
            public PanelConDobleBuffer()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }
        }

        private sealed class BotonSinFoco : Button
        {
            protected override bool ShowFocusCues => false;
        }
    }
}