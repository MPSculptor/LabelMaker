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

            //CreationUtilities.createLabel.createTheLabel(queueString, labelString, defaultsString);

            Form formLabel = new formLabel(queueString, labelString, defaultsString);
            formLabel.Visible = false;
            formLabel.ShowDialog();
            

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
    }
}
