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
    public partial class FormQueueList : Form
    {
        public FormQueueList(string[,] dataSetQueueList)
        {
            InitializeComponent();
            this.Load += (sender, e) => FormQueueList_Load(sender, e, dataSetQueueList);
        }

        private void FormQueueList_Load(object sender, EventArgs e, string[,] dataSetQueueList)
        {
            

            

        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
