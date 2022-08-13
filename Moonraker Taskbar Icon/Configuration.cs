using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moonraker_Taskbar_Icon
{
    public partial class Configuration : Form
    {
        public Configuration()
        {
            InitializeComponent();
            Properties.Settings.Default.Reload();
            panel1.BackColor = Properties.Settings.Default.UserColor == Color.Empty ? Color.White : Properties.Settings.Default.UserColor;
            textBox1.Text = Properties.Settings.Default.IPAdress;
            DialogResult = DialogResult.Cancel;
        }

        private void panel1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panel1.BackColor = colorDialog1.Color;
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.IPAdress = textBox1.Text;
            Properties.Settings.Default.UserColor = panel1.BackColor;
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Configuration_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button_Click(sender, new EventArgs());
            }
        }
    }
}
