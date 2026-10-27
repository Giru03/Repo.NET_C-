using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Agrosoft_Escritorio
{
    public partial class confi : Form
    {
        public confi()
        {
            InitializeComponent();
        }

        private void confi_Load(object sender, EventArgs e)
        {

        }

        private void panelConfiguracion_Click(object sender, EventArgs e)
        {
            // 1. Buscamos el Form1 principal que nos está conteniendo
            Form1 principal = (Form1)this.TopLevelControl;

            // 2. Creamos el formulario que queremos abrir
            confi formConfi = new confi();

            // 3. Mandamos a llamar al método de Form1 para que lo cargue en el panel principal
            principal?.AbrirFormulario(formConfi);
        }
    }
}
