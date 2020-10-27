using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabelMaker
{
    public partial class FormInformation : Form
    {
        public FormInformation( string[] message)
        {
            InitializeComponent();
            this.Text = message[0];
            
            int biggest = 0;
            for (int i = 0; i < message.Length; i++)
            {
                if (message[i].Length > biggest) { biggest = message[i].Length; }
            }

            int XX = (biggest * 12)+100;
            int YY = (message.Length * 15) + 100;

            this.Width = XX;
            this.Height = YY;

            richTextBoxInformation.Width = XX - 40;
            richTextBoxInformation.Height = YY - 63;
            for (int i = 1; i < message.Length; i++)
            {
                richTextBoxInformation.AppendText(message[i]+ Environment.NewLine);
            }
        }

        private void FormInformation_Load(object sender, EventArgs e)
        {
                 }

        private void richTextBoxInformation_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
