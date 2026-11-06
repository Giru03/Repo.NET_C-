using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trabajo_Janier
{
    public partial class Form1 : Form
    {
        private readonly Panel sidebar;
        private readonly Panel areaContenido;
        private readonly Panel hostVistas;
        private readonly Label tituloVista;
        private readonly string usuarioActual;
        private readonly DashboardForm dashboard;
        private readonly List<Button> botonesMenu = new();
        private readonly Dictionary<string, Form> formularios = new();
        private Button? botonActivo;
        private Form? formularioActivo;

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

            sidebar = CrearSidebar();
            areaContenido = CrearAreaContenido(out hostVistas, out tituloVista);

            Controls.Add(areaContenido);
            Controls.Add(sidebar);

            dashboard = new DashboardForm();
            formularios["Dashboard"] = dashboard;
            formularios["Conexiones"] = CrearVista("Conexiones", "Conexiones activas y servicios");

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
            var panelSidebar = new Panel
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
                RowCount = 2,
                BackColor = Tema.Blanco,
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0)
            };
            menu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            string[] opciones = { "Dashboard", "Conexiones" };
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
            var botonCerrar = new Button
            {
                Dock = DockStyle.Fill,
                Text = "   Cerrar sesión",
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Tema.Rojo,
                BackColor = Tema.Blanco,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            botonCerrar.FlatAppearance.BorderSize = 0;
            botonCerrar.FlatAppearance.MouseOverBackColor = Color.FromArgb(253, 242, 242);
            botonCerrar.Click += (_, _) =>
            {
                var loginForm = new LoginForm();
                Application.Exit();
            };

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

            layout.Controls.Add(logo, 0, 0);
            layout.Controls.Add(titulo, 1, 0);
            panel.Controls.Add(layout);
            return panel;
        }

        private Button CrearBotonMenu(string texto, string claveFormulario)
        {
            var boton = new Button
            {
                Dock = DockStyle.Top,
                Height = 45,
                Text = "   " + texto,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                BackColor = Tema.Blanco,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 2, 0, 2)
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseDownBackColor = Tema.VerdeSuave;
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
                if (boton != botonActivo)
                {
                    boton.BackColor = Tema.VerdeSuave;
                }
            };
            boton.MouseLeave += (_, _) =>
            {
                boton.BackColor = boton == botonActivo ? Tema.VerdeSuave : Tema.Blanco;
            };
            boton.Paint += (_, e) =>
            {
                if (boton == botonActivo)
                {
                    using var brush = new SolidBrush(Tema.VerdeOscuro);
                    e.Graphics.FillRectangle(brush, 0, 8, 3, Math.Max(0, boton.Height - 16));
                }
            };

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
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Tema.Blanco,
                Margin = new Padding(0)
            };
            layoutBarra.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutBarra.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layoutBarra.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

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
            layoutBarra.Controls.Add(titulo, 0, 0);
            layoutBarra.Controls.Add(estadoSesion, 1, 0);
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
            foreach (var item in botonesMenu)
            {
                item.BackColor = item == boton ? Tema.VerdeSuave : Tema.Blanco;
                item.Invalidate();
            }

            botonActivo = boton;
            botonActivo.Invalidate();
        }

        public void AbrirFormulario(Form formHijo)
        {
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
    }
}