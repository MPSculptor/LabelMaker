using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CreationUtilities;

namespace LabelMaker
{
    public partial class formMain : Form
    {
        public formMain()
        {
            InitializeComponent();
        }


        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'databaseLabelsDataSet.TablePlants' table. You can move, or remove it, as needed.

            this.BackColor = Color.DarkGray;

        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }
        private void button4_Click(object sender, EventArgs e)
        {
             
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TempMakeALabel();
            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void TempMakeALabel()
        {
            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";
            //file with sample queue entry;
            string name = whereFiles + "ColourQueue.txt";
            string[] queueString = CreationUtilities.dataReader.readFile(name);

            string radioChoice;
            if (radioButton3.Checked)
            {
                radioChoice = whereFiles + "acColourQueue.txt";
            }
            else if (radioButton4.Checked)
            {
                radioChoice = whereFiles + "boColourQueue.txt";
            }
            else
            {
                radioChoice = whereFiles + "bmColourQueue.txt";
            }
            queueString = CreationUtilities.dataReader.readFile(radioChoice );

            //file with a sample label definition;
            if (radioButton1.Checked)
            {
                name = whereFiles + "LabelsText.txt";
            }
            else
            {
                name = whereFiles + "LabelsColour.txt";
            }
            string[] labelString = CreationUtilities.dataReader.readFile(name);
            //file with default settings
            name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);

            Form formLabel = new formLabel(queueString, labelString, defaultsString);
            formLabel.Visible = false;
            formLabel.ShowDialog();

            LabelPreview(queueString, labelString, defaultsString);

        }

        public void LabelPreview(string[] queueData, string[] labelData, string[] defaultsString)
        {
            //Clear the panel
            foreach (Control ctrl in panelLabelPreview.Controls)
            {
                ctrl.Dispose();
            }

            //Set up the label size and shape
            int labelWidth = int.Parse(labelData[0]);
            int labelHeight = int.Parse(labelData[1]);
            string widthString = labelWidth.ToString();
            string heightString = labelHeight.ToString();
            float finalWidth = 1;
            float finalHeight = 1;

            string orientation = "portrait";
            if (labelWidth > labelHeight)
            {
                orientation = "landscape";
            }

            switch (orientation)
            {
                case "portrait":
                    float Ysizep = panelLabelPreview.ClientRectangle.Height - 4;
                    float Xsizep = Ysizep / labelHeight * labelWidth;
                    finalHeight = Ysizep;
                    finalWidth = Xsizep;
                    break;

                case "landscape":
                    float Xsizel = panelLabelPreview.ClientRectangle.Width - 4;
                    float Ysizel = Xsizel / labelWidth * labelHeight;
                    finalHeight = Ysizel;
                    finalWidth = Xsizel;
                    break;
            }

            int finalWidthInt = (int) finalWidth;
            int finalHeightInt = (int) finalHeight;

            whereToNow whereToTwo = new whereToNow(queueData, labelData, defaultsString, finalWidthInt, finalHeightInt);
            whereToTwo.BackColor = Color.White;            

            whereToTwo.Width = finalWidthInt;
            whereToTwo.Height = finalHeightInt;

            whereToTwo.Location = new Point(2, 2);
            whereToTwo.BorderStyle = BorderStyle.FixedSingle;

            panelLabelPreview.Controls.Add(whereToTwo);
        }


        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void bindingSource1_CurrentChanged(object sender, EventArgs e)
        {

        }

        private void tablePlantsBindingSource_CurrentChanged(object sender, EventArgs e)
        {
                    }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tabPageMainQueue_Click(object sender, EventArgs e)
        {
            
        }

        private void tabPageColourQueue_Click(object sender, EventArgs e)
        {
            
        }

        private void dataGridViewPlants_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void checkBoxQty_CheckedChanged(object sender, EventArgs e)
        {
            switch (checkBoxQty.Checked )
            {
                case true:
                    textBoxQtyAuto.Enabled = true;
                    textBoxQtyAuto.ForeColor = SystemColors.ActiveCaptionText ;
                    break;
                case false:
                    textBoxQtyAuto.Enabled = false;
                    textBoxQtyAuto.ForeColor = SystemColors.InactiveCaptionText;
                    break;                    
            }
        }

        private void checkBoxPrice_CheckedChanged(object sender, EventArgs e)
        {
            switch (checkBoxPrice.Checked)
            {
                case true:
                    textBoxPriceAuto.Enabled = true;
                    textBoxPriceAuto.ForeColor = SystemColors.ActiveCaptionText;
                    break;
                case false:
                    textBoxPriceAuto.Enabled = false;
                    textBoxPriceAuto.ForeColor = SystemColors.InactiveCaptionText;
                    break;
            }
        }

        private void groupBoxAlpha_Enter(object sender, EventArgs e)
        {

        }

        private void panelLabelPreview_Paint(object sender, PaintEventArgs e)
        {
            
        }
    }
}
