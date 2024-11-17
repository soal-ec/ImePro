using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImePro
{
    public partial class PopupForm : Form
    {
        public Control InputControl
        {
            get;
            private set;
        }
        public string PopupTitle
        {
            get => label1.Text;
            set => label1.Text = value;
        }
        public PopupForm()
        {
            InitializeComponent();
        }

        public void SetInputControl(Control control)
        {
            if (InputControl != null)
            {
                controlFlowLayoutPanel.Controls.Remove(InputControl);
            }
            InputControl = control;

            controlFlowLayoutPanel.Controls.Add(InputControl);
        }

        public string ifPanelNull()
        {
            //if (controlFlowLayoutPanel != null)
            //{
            //    return "not empty panel";
            //}
            //return "empty panel";
            return "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult |= DialogResult.Cancel;
            this.Close();
        }

    }
}
