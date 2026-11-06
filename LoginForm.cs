using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Trabajo_Janier
{
    public class LoginForm : Form
    {
        private static readonly Color Fondo = Tema.FondoGeneral;
        private static readonly Color FondoCampo = Tema.FondoCampo;
        private static readonly Color BordeCampo = Tema.BordeCampo;
        private static readonly Color Texto = Tema.TextoOscuro;
        private static readonly Color TextoSubtitulo = Tema.TextoMuted;
        private static readonly Color TextoPlaceholder = Tema.TextoPlaceholder;
        private static readonly Color VerdeInstitucional = Tema.VerdeOscuro;
        private static readonly Color VerdeOscuro = Tema.VerdeBienvenida;
        private static readonly Color VerdeAccion = Tema.VerdeOscuro;
        private static readonly Color VerdeHover = Tema.VerdeHover;

        private const string PlaceholderUsuario = "Usuario o correo";
        private const string PlaceholderPassword = "Contraseña";

        private readonly TextBox txtUsuario;
        private readonly TextBox txtPassword;
        private readonly Label lblError;
        private readonly Panel panelCampoUsuario;
        private readonly Panel panelCampoPassword;
        private readonly Panel panelBienvenida;
        private bool arrastrando;
        private Point desplazamientoArrastre;

        public LoginForm()
        {
            Text = "Agrosoft SENA | Iniciar sesión";
            ClientSize = new Size(850, 500);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            BackColor = Fondo;
            DoubleBuffered = true;
            KeyPreview = true;

            txtUsuario = CrearCampo(PlaceholderUsuario);
            txtPassword = CrearCampo(PlaceholderPassword);
            lblError = new Label();
            panelCampoUsuario = CrearContenedorCampo(txtUsuario);
            panelCampoPassword = CrearContenedorCampo(txtPassword);
            panelBienvenida = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = VerdeOscuro,
                Padding = new Padding(46, 80, 34, 36)
            };

            ConstruirInterfaz();
            ConfigurarPlaceholders();
            HabilitarArrastre(this);
            HabilitarArrastre(Controls[0]);
        }

        private void ConstruirInterfaz()
        {
            var split = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Fondo,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var panelLogin = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Fondo,
                Padding = new Padding(42, 38, 42, 28)
            };

            var layoutLogin = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 11,
                BackColor = Fondo,
                Margin = new Padding(0)
            };
            layoutLogin.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 16F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            layoutLogin.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

            var marca = new Label
            {
                Dock = DockStyle.Fill,
                Text = "AGROSOFT SENA",
                ForeColor = VerdeInstitucional,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var titulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Iniciar Sesión",
                ForeColor = Texto,
                Font = new Font("Segoe UI", 25F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var subtitulo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Usa tus credenciales de Agrosoft SENA",
                ForeColor = TextoSubtitulo,
                Font = new Font("Segoe UI", 10F),
                TextAlign = ContentAlignment.TopLeft
            };
            var etiquetaUsuario = CrearEtiquetaCampo("USUARIO O CORREO");
            var etiquetaPassword = CrearEtiquetaCampo("CONTRASEÑA");

            var btnOlvido = new LinkLabel
            {
                Dock = DockStyle.Fill,
                Text = "¿Olvidaste tu contraseña?",
                BackColor = Fondo,
                LinkColor = Tema.VerdeHover,
                ActiveLinkColor = Tema.VerdeOscuro,
                VisitedLinkColor = Tema.VerdeHover,
                LinkBehavior = LinkBehavior.HoverUnderline,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0)
            };
            btnOlvido.LinkClicked += (_, _) => MessageBox.Show(
                this,
                "Comunícate con el administrador del sistema para restablecer tu contraseña.",
                "Recuperación de acceso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            lblError.Dock = DockStyle.Fill;
            lblError.ForeColor = Tema.Rojo;
            lblError.Font = new Font("Segoe UI", 9F);
            lblError.TextAlign = ContentAlignment.MiddleLeft;
            lblError.Visible = false;

            var btnIngresar = new Button
            {
                Dock = DockStyle.Fill,
                Text = "INICIAR SESIÓN",
                FlatStyle = FlatStyle.Flat,
                BackColor = VerdeAccion,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnIngresar.FlatAppearance.BorderSize = 0;
            btnIngresar.FlatAppearance.MouseDownBackColor = VerdeAccion;
            btnIngresar.MouseEnter += (_, _) => btnIngresar.BackColor = VerdeHover;
            btnIngresar.MouseLeave += (_, _) => btnIngresar.BackColor = VerdeAccion;
            btnIngresar.Click += BtnIngresar_Click;

            layoutLogin.Controls.Add(marca, 0, 0);
            layoutLogin.Controls.Add(titulo, 0, 1);
            layoutLogin.Controls.Add(subtitulo, 0, 2);
            layoutLogin.Controls.Add(etiquetaUsuario, 0, 3);
            layoutLogin.Controls.Add(panelCampoUsuario, 0, 4);
            layoutLogin.Controls.Add(etiquetaPassword, 0, 6);
            layoutLogin.Controls.Add(panelCampoPassword, 0, 7);
            layoutLogin.Controls.Add(btnOlvido, 0, 8);
            layoutLogin.Controls.Add(lblError, 0, 9);
            layoutLogin.Controls.Add(btnIngresar, 0, 10);
            panelLogin.Controls.Add(layoutLogin);

            var layoutBienvenida = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = VerdeOscuro,
                Margin = new Padding(0)
            };
            layoutBienvenida.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutBienvenida.RowStyles.Add(new RowStyle(SizeType.Percent, 36F));
            layoutBienvenida.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
            layoutBienvenida.RowStyles.Add(new RowStyle(SizeType.Absolute, 90F));
            layoutBienvenida.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            layoutBienvenida.RowStyles.Add(new RowStyle(SizeType.Percent, 64F));

            var tituloBienvenida = new Label
            {
                Dock = DockStyle.Fill,
                Text = "¡Bienvenido!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 25F, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft
            };
            var descripcion = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Sistema integral de gestión y monitoreo agrícola Agrosoft SENA.",
                ForeColor = Tema.TextoSobreVerde,
                Font = new Font("Segoe UI", 10.5F),
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            var btnSaberMas = new Button
            {
                Size = new Size(142, 42),
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Text = "Saber más",
                FlatStyle = FlatStyle.Flat,
                BackColor = VerdeOscuro,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnSaberMas.FlatAppearance.BorderColor = Tema.Blanco;
            btnSaberMas.FlatAppearance.BorderSize = 1;
            btnSaberMas.FlatAppearance.MouseOverBackColor = Color.FromArgb(64, 255, 255, 255);
            btnSaberMas.Click += (_, _) => MessageBox.Show(
                this,
                "Agrosoft SENA integra herramientas para consultar sensores, conexiones y datos de gestión agrícola.",
                "Acerca de Agrosoft SENA",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            layoutBienvenida.Controls.Add(tituloBienvenida, 0, 1);
            layoutBienvenida.Controls.Add(descripcion, 0, 2);
            layoutBienvenida.Controls.Add(btnSaberMas, 0, 3);
            panelBienvenida.Controls.Add(layoutBienvenida);
            panelBienvenida.Resize += (_, _) => ActualizarFormaBienvenida();

            split.Controls.Add(panelLogin, 0, 0);
            split.Controls.Add(panelBienvenida, 1, 0);
            Controls.Add(split);
            Controls.Add(CrearBotonCerrar());

            HabilitarArrastre(panelLogin);
            HabilitarArrastre(layoutLogin);
            HabilitarArrastre(marca);
            HabilitarArrastre(titulo);
            HabilitarArrastre(subtitulo);
            HabilitarArrastre(panelBienvenida);
            HabilitarArrastre(layoutBienvenida);
            HabilitarArrastre(tituloBienvenida);
            HabilitarArrastre(descripcion);

            AcceptButton = btnIngresar;
            Shown += (_, _) => ActualizarFormaBienvenida();
        }

        private static TextBox CrearCampo(string placeholder)
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Text = placeholder,
                BorderStyle = BorderStyle.None,
                BackColor = FondoCampo,
                ForeColor = TextoPlaceholder,
                Font = new Font("Segoe UI", 10F),
                UseSystemPasswordChar = false,
                Margin = new Padding(0),
                TabStop = true
            };
        }

        private Panel CrearContenedorCampo(TextBox campo)
        {
            var contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = FondoCampo,
                Padding = new Padding(13, 13, 12, 8),
                Margin = new Padding(0)
            };
            contenedor.Controls.Add(campo);
            contenedor.Paint += (_, e) =>
            {
                var colorBorde = campo.Focused ? VerdeInstitucional : BordeCampo;
                using var pen = new Pen(colorBorde);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    Math.Max(0, contenedor.ClientSize.Width - 1),
                    Math.Max(0, contenedor.ClientSize.Height - 1));
            };
            campo.Enter += (_, _) => contenedor.Invalidate();
            campo.Leave += (_, _) => contenedor.Invalidate();
            campo.TextChanged += (_, _) => lblError.Visible = false;
            return contenedor;
        }

        private void ConfigurarPlaceholders()
        {
            txtUsuario.Enter += (_, _) => LimpiarPlaceholder(txtUsuario, PlaceholderUsuario, false);
            txtUsuario.Leave += (_, _) => RestaurarPlaceholder(txtUsuario, PlaceholderUsuario, false);
            txtPassword.Enter += (_, _) => LimpiarPlaceholder(txtPassword, PlaceholderPassword, true);
            txtPassword.Leave += (_, _) => RestaurarPlaceholder(txtPassword, PlaceholderPassword, true);
            txtPassword.TextChanged += (_, _) =>
            {
                txtPassword.UseSystemPasswordChar = txtPassword.Focused &&
                    txtPassword.Text.Length > 0 && txtPassword.Text != PlaceholderPassword;
            };

            txtUsuario.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtPassword.Focus();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            txtPassword.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnIngresar_Click(this, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
        }

        private static void LimpiarPlaceholder(TextBox campo, string placeholder, bool esPassword)
        {
            if (campo.Text == placeholder)
            {
                campo.Clear();
                campo.ForeColor = Texto;
            }

            if (esPassword)
            {
                campo.UseSystemPasswordChar = campo.Text.Length > 0;
            }
        }

        private static void RestaurarPlaceholder(TextBox campo, string placeholder, bool esPassword)
        {
            if (string.IsNullOrWhiteSpace(campo.Text))
            {
                campo.UseSystemPasswordChar = false;
                campo.Text = placeholder;
                campo.ForeColor = TextoPlaceholder;
            }
            else if (esPassword)
            {
                campo.UseSystemPasswordChar = true;
            }
        }

        private static Label CrearEtiquetaCampo(string texto)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = texto,
                ForeColor = Color.FromArgb(74, 85, 104),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(0, 0, 0, 4)
            };
        }

        private Button CrearBotonCerrar()
        {
            var boton = new Button
            {
                Size = new Size(38, 34),
                Location = new Point(ClientSize.Width - 48, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Text = "✕",
                FlatStyle = FlatStyle.Flat,
                BackColor = VerdeOscuro,
                ForeColor = Tema.TextoMuted,
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = VerdeOscuro;
            boton.MouseEnter += (_, _) => boton.ForeColor = Tema.RojoHover;
            boton.MouseLeave += (_, _) => boton.ForeColor = Tema.TextoMuted;
            boton.Click += (_, _) => Close();
            return boton;
        }

        private void ActualizarFormaBienvenida()
        {
            if (panelBienvenida.ClientSize.Width <= 0 || panelBienvenida.ClientSize.Height <= 0)
            {
                return;
            }

            int ancho = panelBienvenida.ClientSize.Width;
            int alto = panelBienvenida.ClientSize.Height;
            using var path = new GraphicsPath();
            path.AddBezier(54, 0, 18, alto * 0.16F, 0, alto * 0.32F, 0, alto * 0.5F);
            path.AddBezier(0, alto * 0.5F, 0, alto * 0.72F, 22, alto * 0.9F, 54, alto);
            path.AddLine(54, alto, ancho, alto);
            path.AddLine(ancho, alto, ancho, 0);
            path.CloseFigure();

            var regionAnterior = panelBienvenida.Region;
            panelBienvenida.Region = new Region(path);
            regionAnterior?.Dispose();
        }

        private void HabilitarArrastre(Control control)
        {
            control.MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    arrastrando = true;
                    desplazamientoArrastre = new Point(Cursor.Position.X - Left, Cursor.Position.Y - Top);
                }
            };
            control.MouseMove += (_, _) =>
            {
                if (arrastrando && Control.MouseButtons == MouseButtons.Left)
                {
                    Location = new Point(Cursor.Position.X - desplazamientoArrastre.X,
                        Cursor.Position.Y - desplazamientoArrastre.Y);
                }
            };
            control.MouseUp += (_, _) => arrastrando = false;
        }

        private void BtnIngresar_Click(object? sender, EventArgs e)
        {
            const string usuarioCorrecto = "admin";
            const string passwordCorrecto = "admin123";

            string usuario = txtUsuario.Text == PlaceholderUsuario ? string.Empty : txtUsuario.Text.Trim();
            string password = txtPassword.Text == PlaceholderPassword ? string.Empty : txtPassword.Text.Trim();

            if (usuario == usuarioCorrecto && password == passwordCorrecto)
            {
                string usuarioIngresado = txtUsuario.Text.Trim();
                if (string.IsNullOrWhiteSpace(usuarioIngresado) || usuarioIngresado == PlaceholderUsuario)
                {
                    usuarioIngresado = "Admin";
                }

                var mainForm = new Form1(usuarioIngresado);
                mainForm.Show();
                this.Hide();
                return;
            }

            lblError.Text = "Usuario o contraseña incorrectos. Verifica tus datos.";
            lblError.Visible = true;
            txtPassword.Clear();
            txtPassword.UseSystemPasswordChar = true;
            txtPassword.ForeColor = Texto;
            txtPassword.Focus();
        }
    }
}
