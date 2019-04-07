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
        public object dataGridView1 { get; private set; }

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
            this.tablePlantsTableAdapter.Fill(this.databaseLabelsDataSet.TablePlants);

            this.BackColor = Color.DarkGray;

            //dataGridViewPlants.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewPlants.Columns[0].Width = 0;
            dataGridViewPlants.Columns[1].Width = 10;
            dataGridViewPlants.Columns[2].Width = 100;
            dataGridViewPlants.Columns[3].Width = 10;
            dataGridViewPlants.Columns[4].Width = 100;
            dataGridViewPlants.Columns[5].Width = 100;
            dataGridViewPlants.Columns[6].Width = 100;




        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }
        private void button4_Click(object sender, EventArgs e)
        {
             
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TempMakeALabel(panelLabelPreview);
            
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void TempMakeALabel(Panel whichPanel)
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

            //Form formLabel = new formLabel(queueString, labelString, defaultsString);
            //formLabel.Visible = false;
            //formLabel.ShowDialog();

            LabelPreview(queueString, labelString, defaultsString, whichPanel);

        }

        public void LabelPreview(string[] queueData, string[] labelData, string[] defaultsString, Panel whichPanel)
        {
            //Clear the panel
            foreach (Control ctrl in whichPanel.Controls)
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
                    float Ysizep = whichPanel.ClientRectangle.Height - 4;
                    float Xsizep = Ysizep / labelHeight * labelWidth;
                    finalHeight = Ysizep;
                    finalWidth = Xsizep;
                    break;

                case "landscape":
                    float Xsizel = whichPanel.ClientRectangle.Width - 4;
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

            whichPanel.Controls.Add(whereToTwo);
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

        private void dataGridViewPlants_Click(object sender, DataGridViewCellEventArgs e)
        {
            //String NameText = "Hello";
            labelPlantName.Text = getPlantName(e); 
        }


        private void dataGridViewPlants_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //String NameText = "Hello";
            updateMainDetails(e);
        }

        public void updateMainDetails(DataGridViewCellEventArgs e)
        {
            //get picture position
            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";
            //file with sample queue entry;
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);
            string filePlace = defaultsString[0];

            //Plant name as one string
            labelPlantName.Text = getPlantName(e);
            //Description
            richTextBoxDesc.Text = dataGridViewPlants.Rows[e.RowIndex].Cells[8].Value.ToString();

            //Thumbnails and Main Picture
            try // #1
            {    
                string fileName = dataGridViewPlants.Rows[e.RowIndex].Cells[12].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxThumb1.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[e.RowIndex].Cells[12].Value.ToString()))
                    {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                }
                else
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                }
                pictureBoxThumb1.Image = Image.FromFile(pictureFile);
            }

            try // #2
            {
                string fileName = dataGridViewPlants.Rows[e.RowIndex].Cells[13].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxThumb2.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[e.RowIndex].Cells[13].Value.ToString()))
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                }
                else
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                }
                pictureBoxThumb2.Image = Image.FromFile(pictureFile);
            }

            try // #3
            {
                string fileName = dataGridViewPlants.Rows[e.RowIndex].Cells[14].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxThumb3.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[e.RowIndex].Cells[14].Value.ToString()))
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                }
                else
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                }
                pictureBoxThumb3.Image = Image.FromFile(pictureFile);
            }


            try // #4
            {
                string fileName = dataGridViewPlants.Rows[e.RowIndex].Cells[15].Value.ToString();
                string pictureFile = filePlace + fileName;
                label4.Text = pictureFile;
                pictureBoxThumb4.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[e.RowIndex].Cells[15].Value.ToString()))
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                }
                else
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                }
                pictureBoxThumb4.Image = Image.FromFile(pictureFile);
            }

            updateMainPicture(filePlace,e);


        }

        public void updateMainPicture(string filePlace, DataGridViewCellEventArgs e)
        {

            int whichOne = 12;
            if (radioButtonImage4.Checked){ whichOne = 15; }
            else if (radioButtonImage3.Checked) { whichOne = 14; }
            else if (radioButtonImage2.Checked){ whichOne = 13; }
            else { whichOne = 12; }


            try // #Main
            {
                string fileName = dataGridViewPlants.Rows[e.RowIndex].Cells[whichOne].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxMain.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[e.RowIndex].Cells[whichOne].Value.ToString()))
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                }
                else
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                }
                pictureBoxMain.Image = Image.FromFile(pictureFile);
            }
        }

        public String getPlantName(DataGridViewCellEventArgs e)
        {
            //Turn Genus etc fields into one string
            string Name = "";

            int currentRow = dataGridViewPlants.CurrentRow.Index;
            if (dataGridViewPlants.Rows[e.RowIndex].Cells[1].Value.ToString() == "x")
            {
                Name = Name + "x";
            }
            Name = Name + dataGridViewPlants.Rows[e.RowIndex].Cells[2].Value.ToString();

            if (dataGridViewPlants.Rows[e.RowIndex].Cells[3].Value.ToString() == "x")
            {
                Name = Name + " x";
            }

            Name = Name + " " + dataGridViewPlants.Rows[e.RowIndex].Cells[4].Value.ToString() ;
            Name = Name + " " + dataGridViewPlants.Rows[e.RowIndex].Cells[5].Value.ToString();
            

            return Name;
            

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

        private void groupBoxPlantData_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click_1(object sender, EventArgs e)
        {
           
        }

        private void tabControlMain_SelectedIndexChanged( object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPagePreview)
            {
                TempMakeALabel(panelLabelTab);                
            }
        }

        private void panelLabelTab_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
