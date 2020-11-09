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

            int XX = (biggest * 8)+100;
            int YY = (message.Length * 15) + 100;

            this.Width = XX;
            this.Height = YY;
            buttonHide.Left = XX - 103;
            buttonHide.Top = YY - 74;

            richTextBoxInformation.Width = XX - 40;
            richTextBoxInformation.Height = YY - 63;
            for (int i = 1; i < message.Length; i++)
            {
                richTextBoxInformation.AppendText(message[i]+ Environment.NewLine);
            }
            //this.TopMost = true;
            //Sets Focus to this form
            this.Activate();
        }


        private void buttonHide_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
