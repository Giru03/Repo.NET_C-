using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Trabajo_Janier
{
    public sealed class AlertasForm : Form
    {
        private const int MaximoFilas = 500;
        private readonly AlertaService _alertaService;
        private readonly DataGridView _tabla;

        public AlertasForm(AlertaService alertaService)
        {
            _alertaService = alertaService;
            Text = "Alertas";
            BackColor = Tema.FondoGeneral;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopLevel = false;
            Dock = DockStyle.Fill;
            Padding = new Padding(18);

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
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var encabezado = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.Transparent,
                Margin = new Padding(8, 0, 8, 8)
            };
            encabezado.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            encabezado.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            encabezado.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
            encabezado.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Alertas",
                Font = new Font("Segoe UI", 21F, FontStyle.Bold),
                ForeColor = Tema.TextoOscuro,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            encabezado.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "Historial de alertas por topes máximos y mínimos",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Tema.TextoMuted,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            var barraAcciones = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(8, 0, 8, 6)
            };
            var limpiar = new Button
            {
                AutoSize = true,
                Dock = DockStyle.Right,
                Text = "Limpiar alertas",
                FlatStyle = FlatStyle.Flat,
                BackColor = Tema.VerdeOscuro,
                ForeColor = Tema.Blanco,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Padding = new Padding(12, 0, 12, 0),
                UseVisualStyleBackColor = false
            };
            limpiar.FlatAppearance.BorderSize = 0;
            limpiar.FlatAppearance.MouseOverBackColor = Tema.VerdeHover;
            barraAcciones.Controls.Add(limpiar);

            _tabla = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Tema.Blanco,
                BorderStyle = BorderStyle.FixedSingle,
                GridColor = Tema.BordeSuave,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeight = 38,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Tema.TextoOscuro,
                Margin = new Padding(8)
            };
            _tabla.ColumnHeadersDefaultCellStyle.BackColor = Tema.VerdeSuave;
            _tabla.ColumnHeadersDefaultCellStyle.ForeColor = Tema.VerdeOscuro;
            _tabla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _tabla.DefaultCellStyle.SelectionBackColor = Tema.VerdeSuave;
            _tabla.DefaultCellStyle.SelectionForeColor = Tema.TextoOscuro;
            _tabla.Columns.Add("FechaHora", "Fecha/Hora");
            _tabla.Columns.Add("Sensor", "Sensor");
            _tabla.Columns.Add("Tipo", "Tipo");
            _tabla.Columns.Add("Valor", "Valor");
            _tabla.Columns.Add("Umbral", "Umbral");
            _tabla.Columns.Add("Mensaje", "Mensaje");
            _tabla.Columns["FechaHora"]!.FillWeight = 105F;
            _tabla.Columns["Sensor"]!.FillWeight = 65F;
            _tabla.Columns["Tipo"]!.FillWeight = 65F;
            _tabla.Columns["Valor"]!.FillWeight = 55F;
            _tabla.Columns["Umbral"]!.FillWeight = 55F;
            _tabla.Columns["Mensaje"]!.FillWeight = 210F;
            _tabla.Columns["Mensaje"]!.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            limpiar.Click += (_, _) =>
            {
                _alertaService.Limpiar();
                _tabla.Rows.Clear();
            };

            layout.Controls.Add(encabezado, 0, 0);
            layout.Controls.Add(barraAcciones, 0, 1);
            layout.Controls.Add(_tabla, 0, 2);
            Controls.Add(layout);

            IReadOnlyList<Alerta> alertas = _alertaService.Alertas;
            for (int i = alertas.Count - 1; i >= 0; i--)
            {
                AgregarFila(alertas[i]);
            }

            _alertaService.OnAlerta += AlertaRecibida;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _alertaService.OnAlerta -= AlertaRecibida;
            }

            base.Dispose(disposing);
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
                    if (!IsDisposed && IsHandleCreated)
                    {
                        AgregarFila(alerta);
                    }
                }));
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void AgregarFila(Alerta alerta)
        {
            _tabla.Rows.Insert(0);
            DataGridViewRow fila = _tabla.Rows[0];
            fila.Cells[0].Value = alerta.Fecha.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.CurrentCulture);
            fila.Cells[1].Value = alerta.Sensor;
            fila.Cells[2].Value = alerta.Tipo;
            fila.Cells[3].Value = alerta.Valor.ToString("0.#", CultureInfo.CurrentCulture);
            fila.Cells[4].Value = alerta.Umbral.ToString("0.#", CultureInfo.CurrentCulture);
            fila.Cells[5].Value = alerta.Mensaje;
            fila.DefaultCellStyle.BackColor = alerta.Tipo == "Muy alto"
                ? Color.FromArgb(254, 242, 242)
                : Color.FromArgb(239, 246, 255);
            fila.DefaultCellStyle.SelectionBackColor = alerta.Tipo == "Muy alto"
                ? Color.FromArgb(254, 226, 226)
                : Color.FromArgb(219, 234, 254);

            while (_tabla.Rows.Count > MaximoFilas)
            {
                _tabla.Rows.RemoveAt(_tabla.Rows.Count - 1);
            }
        }
    }
}
