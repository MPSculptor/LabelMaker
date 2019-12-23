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
        public FormInformation(string title, string[] message,int XX,int YY)
        {
            InitializeComponent();
            this.Text = title;
            this.Width = XX;
            this.Height = YY;
            richTextBoxInformation.Width = XX - 40;
            richTextBoxInformation.Height = YY - 63;
            for (int i = 0; i < message.Length; i++)
            {
                richTextBoxInformation.AppendText(message[i]+ Environment.NewLine);
            }
        }

        private void FormInformation_Load(object sender, EventArgs e)
        {
                 }

        
    }
}
