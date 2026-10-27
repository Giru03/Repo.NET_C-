using System.Drawing.Drawing2D;

namespace Agrosoft_Escritorio
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;

            // Tarjetas superiores
            ConectarClick(panel6, (s, e) => AbrirFormulario(new cultivos()));
            ConectarClick(panel7, (s, e) => AbrirFormulario(new sensores()));
            ConectarClick(panel8, (s, e) => AbrirFormulario(new confi()));

            // Menú lateral
            ConectarClick(panel10, (s, e) => AbrirFormulario(new dashboard()));
            ConectarClick(panel11, (s, e) => AbrirFormulario(new conexiones()));
            ConectarClick(panel12, (s, e) => AbrirFormulario(new perfil()));

            // Pantalla de inicio
            AbrirFormulario(new dashboard());

            // Esquinas redondeadas: la Region se asigna al cambiar tamaño, no al pintar
            foreach (var c in new Control[] { panel3, panel7, panel8, pictureBox5 })
            {
                c.Resize += (s, e) => Redondear((Control)s);
                Redondear(c);
            }

            // El borde sí se dibuja en Paint (sin tocar la Region)
            foreach (var p in new Panel[] { panel3, panel7, panel8 })
                p.Paint += DibujarBorde;
        }

        private static void ConectarClick(Control control, EventHandler accion)
        {
            control.Click += accion;
            foreach (Control hijo in control.Controls)
                ConectarClick(hijo, accion);
        }

        private static GraphicsPath CrearRuta(Control c, int radio)
        {
            var ruta = new GraphicsPath();
            ruta.AddArc(0, 0, radio, radio, 180, 90);
            ruta.AddArc(c.Width - radio, 0, radio, radio, 270, 90);
            ruta.AddArc(c.Width - radio, c.Height - radio, radio, radio, 0, 90);
            ruta.AddArc(0, c.Height - radio, radio, radio, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }

        private static void Redondear(Control c, int radio = 50)
        {
            if (c.Width <= 0 || c.Height <= 0) return;
            using var ruta = CrearRuta(c, radio);
            c.Region = new Region(ruta);
        }

        private void DibujarBorde(object? sender, PaintEventArgs e)
        {
            var c = (Control)sender!;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var ruta = CrearRuta(c, 50);
            using var lapiz = new Pen(Color.FromArgb(230, 230, 230), 1);
            e.Graphics.DrawPath(lapiz, ruta);
        }

        public void AbrirFormulario(Form formHijo)
        {
            // Si ya está abierto ese mismo formulario, no lo recreamos
            if (panelContenido.Controls.Count > 0 &&
                panelContenido.Controls[0].GetType() == formHijo.GetType())
            {
                formHijo.Dispose();
                return;
            }

            panelContenido.SuspendLayout();

            while (panelContenido.Controls.Count > 0)
            {
                var anterior = panelContenido.Controls[0];
                panelContenido.Controls.Remove(anterior);
                anterior.Dispose();
            }

            formHijo.TopLevel = false;
            formHijo.FormBorderStyle = FormBorderStyle.None;
            formHijo.Dock = DockStyle.Fill;
            panelContenido.Controls.Add(formHijo);
            panelContenido.Tag = formHijo;

            panelContenido.ResumeLayout();
            formHijo.Show();
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void panel11_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}