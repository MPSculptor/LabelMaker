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
            // TODO: This line of code loads data into the 'databaseLabelsDataSet1.TableProfiles' table. You can move, or remove it, as needed.
            this.tableProfilesTableAdapter.Fill(this.databaseLabelsDataSet1.TableProfiles);
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

            updateMainDetails(0);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            TempMakeALabel(panelLabelPreview, "Main");
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void TempMakeALabel(Panel whichPanel, string whichLabel)
        {
            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";
            
            //file with default settings
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);

            
            //file with sample queue entry;
            name = whereFiles + "ColourQueue.txt";
            string[] queueString = CreationUtilities.dataReader.readFile(name);

            int currentRow = dataGridViewPlants.CurrentCell.RowIndex;
            string[] sendData = new String[21];
            string[] findName = new String[5];
            string[] moreData = new String[12];

            // get general plant data
            for (int i=0; i <= 20; i++)
            {
                sendData[i] = dataGridViewPlants.Rows[currentRow].Cells[i].Value.ToString();
            }

            // get various concatenated Name strings 
            for (int i = 0; i <= 4; i++)
            {
                findName[i] = dataGridViewPlants.Rows[currentRow].Cells[1 + i].Value.ToString();
            }
            string[] sendName = getPlantName(findName);

            //get main pcture
            if (radioButtonImage1.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[12].Value.ToString(); }
            if (radioButtonImage2.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[13].Value.ToString(); }
            if (radioButtonImage3.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[14].Value.ToString(); }
            if (radioButtonImage4.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[15].Value.ToString(); }

            // Check AGM Status
            if ( dataGridViewPlants.Rows[currentRow].Cells[16].Value.ToString() == "True")
            {
                moreData[1] = "AGM.ico";
            }
            else
            {
                moreData[1] = "AGMBlank.ico";
            }

            // qty and price

            moreData[2] = textBoxQty.Text;
            moreData[3] = formatPrice(textBoxPrice.Text);
 
            queueString = dataReader.readQueue(sendData,sendName, moreData);

            //file with a sample label definition;
            if (whichLabel == "Colour")
            {

                name = whereFiles + "LabelsColour.txt";
            }
            else
            {
                name = whereFiles + "LabelsText.txt";
            }
            string[] labelString = CreationUtilities.dataReader.readFile(name);
            
            

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

            int finalWidthInt = (int)finalWidth;
            int finalHeightInt = (int)finalHeight;

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
            
        }


        private void dataGridViewPlants_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            updateMainDetails(e.RowIndex);
        }

        public void updateMainDetails(int indexOfRow)
        {
            //get picture position
            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";
            //file with sample queue entry;
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);
            string filePlace = defaultsString[0];

            //Plant name as one string
            string[] PlantNames = new String[5];
            string[] sendData = new String[5];
            for (int i = 0; i <= 4; i++)
            {
                sendData[i] = dataGridViewPlants.Rows[indexOfRow].Cells[1+i].Value.ToString();
            }

            PlantNames = getPlantName(sendData);
            labelPlantName.Text = PlantNames[0];
            //Description
            richTextBoxDesc.Text = dataGridViewPlants.Rows[indexOfRow].Cells[8].Value.ToString();

            //Thumbnails and Main Picture
            try // #1
            {
                string fileName = dataGridViewPlants.Rows[indexOfRow].Cells[12].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxThumb1.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[indexOfRow].Cells[12].Value.ToString()))
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
                string fileName = dataGridViewPlants.Rows[indexOfRow].Cells[13].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxThumb2.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[indexOfRow].Cells[13].Value.ToString()))
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
                string fileName = dataGridViewPlants.Rows[indexOfRow].Cells[14].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxThumb3.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[indexOfRow].Cells[14].Value.ToString()))
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
                string fileName = dataGridViewPlants.Rows[indexOfRow].Cells[15].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxThumb4.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[indexOfRow].Cells[15].Value.ToString()))
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                }
                else
                {
                    pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                }
                pictureBoxThumb4.Image = Image.FromFile(pictureFile);
            }

            updateMainPicture(filePlace, indexOfRow);
            TempMakeALabel( panelLabelPreview, "Main");

            
            //Price and Quantity
            textBoxQty.Text = "1";
            if (checkBoxQty.Checked ) { textBoxQty.Text = textBoxQtyAuto.Text; }
            textBoxPrice.Text = "0";
            if (checkBoxPrice.Checked) { textBoxPrice.Text = textBoxPriceAuto.Text; }
            textBoxQty.Focus();

            //Status Buttons

            if ( dataGridViewPlants.Rows[indexOfRow].Cells[10].Value.ToString() == "False")
            {
                buttonAddtoColourQueue.Text = "no Colour";
            }
            else
            {
                buttonAddtoColourQueue.Text = "add Colour";
            }
            
            if (dataGridViewPlants.Rows[indexOfRow].Cells[18].Value.ToString() == "True")
            {
                buttonVisibleEntry.Text = "Hidden";
            }
            else
            {
                buttonVisibleEntry.Text = "Visible";
            }
            
            if (dataGridViewPlants.Rows[indexOfRow].Cells[16].Value.ToString() == "False")
            {
                buttonAGMStatus.Text = "no AGM";
            }
            else
            {
                buttonAGMStatus.Text = "AGM";
            }
            
            if (dataGridViewPlants.Rows[indexOfRow].Cells[20].Value.ToString() == "False")
            {
                buttonLableStocks.Text = "no Labels";
            }
            else
            {
                buttonLableStocks.Text = "Labels";
            }
            //Set Colour on status buttons
            colourStatusButtons();


        }

        public void updateMainPicture(string filePlace, int indexOfRow)
        { 

            int whichOne = 12;
            if (radioButtonImage4.Checked) { whichOne = 15; }
            else if (radioButtonImage3.Checked) { whichOne = 14; }
            else if (radioButtonImage2.Checked) { whichOne = 13; }
            else { whichOne = 12; }


            try // #Main
            {
                string fileName = dataGridViewPlants.Rows[indexOfRow].Cells[whichOne].Value.ToString();
                string pictureFile = filePlace + fileName;
                pictureBoxMain.Image = Image.FromFile(pictureFile);
            }
            catch (IOException)
            {
                string pictureFile = "";
                if (String.IsNullOrEmpty(dataGridViewPlants.Rows[indexOfRow].Cells[whichOne].Value.ToString()))
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

        public String[] getPlantName(string[] sentData)
        {
            //Turn Genus etc fields into one string
            string[] Name = new String[4];

            //int currentRow = dataGridViewPlants.CurrentRow.Index;
            if (sentData[0] == "x")
            {
                Name[0] = Name[0] + "x";
                Name[1] = Name[1] + "x";
            }
            Name[0] = Name[0] + sentData[1];
            Name[1] = Name[1] + sentData[1];

            if (sentData[2] == "x")
            {
                Name[0] = Name[0] + " x";
                Name[2] = Name[2] + " x";
            }

            Name[0] = Name[0] + " " + sentData[3];
            Name[0] = Name[0] + " " + sentData[4];
            Name[2] = Name[2] + " " + sentData[3];
            Name[3] = Name[3] + " " + sentData[4];


            return Name;


        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void checkBoxQty_CheckedChanged(object sender, EventArgs e)
        {
            switch (checkBoxQty.Checked)
            {
                case true:
                    textBoxQtyAuto.Enabled = true;
                    textBoxQtyAuto.ForeColor = SystemColors.ActiveCaptionText;
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

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearPanelLabel();
            if (tabControlMain.SelectedTab == tabPagePreview)
            {
                TempMakeALabel(panelLabelTabMain, "Main");
                TempMakeALabel(panelLabelTabColour, "Colour");
            }

        }

        private void clearPanelLabel()
        {
            //Clear the panel
            foreach (Control ctrl in panelLabelPreview.Controls)
            {
                ctrl.Dispose();
            }
        }

        private void panelLabelTab_Paint(object sender, PaintEventArgs e)
        {

        }
        public string formatPrice(string priceString)
        {
            // takes a string that represtent a number and converts it to a price format

            double priceSent;
            string doneString = "";
            Double.TryParse(priceString, out priceSent);
            Math.Round(priceSent, 2);
            //MessageBox.Show(priceSent.ToString());
            if (priceSent > 0)
            {
                if (priceSent >= 1)
                {
                    doneString = "£ " + priceSent.ToString();
                    int positionDecimal = doneString.IndexOf(".");
                    //MessageBox.Show(positionDecimal.ToString() + " , " + doneString.Length);
                    if (positionDecimal == -1) { doneString = doneString + ".00"; }
                    if (positionDecimal == (doneString.Length - 2 )) { doneString = doneString + "0"; }
                }
                else
                {
                    doneString = priceSent.ToString();
                    //MessageBox.Show(doneString.Length.ToString());
                    if (doneString.Length == 3) { doneString = doneString + "0"; }
                    doneString=doneString  + "p";
                    doneString = doneString.Substring(2);
                }
            }           

            return doneString;
        }

        public void colourStatusButtons()
        {
            if (buttonAddtoColourQueue.Text == "add Colour")
            {
                buttonAddtoColourQueue.BackColor = Color.LightGreen;
            }
            else
            {
                buttonAddtoColourQueue.BackColor = Color.DarkSalmon;
            }

            if (buttonAGMStatus.Text == "AGM")
            {
                buttonAGMStatus.BackColor = Color.LightGreen;
            }
            else
            {
                buttonAGMStatus.BackColor = Color.DarkSalmon;
            }

            if (buttonLableStocks.Text == "Labels")
            {
                buttonLableStocks.BackColor = Color.LightGreen;
            }
            else
            {
                buttonLableStocks.BackColor = Color.DarkSalmon;
            }

            if (buttonVisibleEntry.Text == "Visible")
            {
                buttonVisibleEntry.BackColor = Color.LightGreen;
            }
            else
            {
                buttonVisibleEntry.BackColor = Color.DarkSalmon;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void buttonAddtoColourQueue_Click(object sender, EventArgs e)
        {
            if (buttonAddtoColourQueue.Text == "add Colour")
            {
                buttonAddtoColourQueue.Text = "no Colour";
            }
            else
            {
                buttonAddtoColourQueue.Text = "add Colour";
            }
            colourStatusButtons();
        }

        

        private void buttonVisibleEntry_Click(object sender, EventArgs e)
        {
            if (buttonVisibleEntry.Text == "Visible")
            {
                buttonVisibleEntry.Text = "Hidden";
            }
            else
            {
                buttonVisibleEntry.Text = "Visible";
            }
            colourStatusButtons();

        }

        private void buttonAGMStatus_Click(object sender, EventArgs e)
        {
            if (buttonAGMStatus.Text == "AGM")
            {
                buttonAGMStatus.Text = "no AGM";
                buttonAGMStatus.BackColor = Color.DarkSalmon;
            }
            else
            {
                buttonAGMStatus.Text = "AGM";
                buttonAGMStatus.BackColor = Color.LightGreen;
            }
            colourStatusButtons();
        }

        private void buttonLableStocks_Click(object sender, EventArgs e)
        {
            if (buttonLableStocks.Text == "Labels")
            {
                buttonLableStocks.Text = "no Labels";
                buttonLableStocks.BackColor = Color.DarkSalmon;
            }
            else
            {
                buttonLableStocks.Text = "Labels";
                buttonLableStocks.BackColor = Color.LightGreen;
            }
            colourStatusButtons();
        }

        private void tabPageDatabase_Click(object sender, EventArgs e)
        {

        }

        private void profilesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            tabControlProfiles.Visible = true;
            addProfileButtons();            
            
        }
         
        public void addProfileButtons()
        {
            FlowLayoutPanel flowLayoutPanelProfiles = new FlowLayoutPanel();

            flowLayoutPanelProfiles.Width = groupBoxProfiles.Width - 20;
            flowLayoutPanelProfiles.Height = groupBoxProfiles.Height - 40;
            flowLayoutPanelProfiles.Left = 10;
            flowLayoutPanelProfiles.Top = 20;

            groupBoxProfiles.Controls.Add(flowLayoutPanelProfiles);


            int profileIndex = 0;
            Button[] ProfileSample = new Button[databaseLabelsDataSet1.TableProfiles.Rows.Count + 1];
            //Iterate through dataset
            
            foreach (DataRow rowNumber in databaseLabelsDataSet1.TableProfiles)
            {
                //collect one row at a a time
                int size = rowNumber.ItemArray.Count();
                String[] rowString = new string[size];
                int i = 0;
                foreach (object item in rowNumber.ItemArray)
                {
                    rowString[i] = rowNumber.ItemArray.ElementAt(i).ToString();
                    i++;
                }
                
                ProfileSample[profileIndex] = new Button();
                ProfileSample[profileIndex].Text = rowString[1];
                Console.WriteLine(rowString[1]);
                ProfileSample[profileIndex].Width = 131;
                ProfileSample[profileIndex].Height = 30;
                Console.WriteLine("BackColour");
                ProfileSample[profileIndex].BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[7]));
                Console.WriteLine("ForeColour");
                ProfileSample[profileIndex].ForeColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[6]));
                ProfileSample[profileIndex].FlatStyle = FlatStyle.Flat;
                ProfileSample[profileIndex].FlatAppearance.BorderSize=3;
                Console.WriteLine("BorderColour");
                ProfileSample[profileIndex].FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[2]));

                //ProfileSample[profileIndex].BorderColour = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[7]));

                flowLayoutPanelProfiles.Controls.Add(ProfileSample[profileIndex]);
                
                //Dispose();
                profileIndex++;
            }
        }

        private void buttonProfilesClose_Click(object sender, EventArgs e)
        {
            foreach (FlowLayoutPanel ctrl in groupBoxProfiles.Controls)
            {
                groupBoxProfiles.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
            tabControlProfiles.Visible = false;
        }

        private void tabPage1_Click_2(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox2_Enter_1(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void bindingSource1_CurrentChanged_1(object sender, EventArgs e)
        {

        }



        
        private void groupBoxImages_Enter(object sender, EventArgs e)
        {
            
        }

        private void radioButtonImage1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonImage1.Checked)
            {
                updateMainDetails(dataGridViewPlants.CurrentCell.RowIndex);
            }
        }

        private void radioButtonImage2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonImage2.Checked)
            {
                updateMainDetails(dataGridViewPlants.CurrentCell.RowIndex);
            }
        }

        private void radioButtonImage3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonImage3.Checked)
            {
                updateMainDetails(dataGridViewPlants.CurrentCell.RowIndex);
            }
        }

        private void radioButtonImage4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonImage4.Checked)
            {
                updateMainDetails(dataGridViewPlants.CurrentCell.RowIndex);
            }
        }


        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (this.ActiveControl == textBoxQty)
            {
                if (keyData == Keys.Return)
                {
                    MessageBox.Show("Enter from Qty");
                    //do something
                    return true;
                }
                else if (keyData == Keys.Tab)
                {
                    textBoxPrice.Focus();
                    //do something
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (this.ActiveControl == textBoxPrice)
            {
                if (keyData == Keys.Return)
                {
                    MessageBox.Show("Enter from Price");
                    //do something
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }
        }

        private void textBoxQty_Enter(object sender, EventArgs e)
        {
            if (checkBoxQty.Checked)
            {
                textBoxQty.Text = textBoxQtyAuto.Text;
            }
        }
        private void textBoxPrice_Enter(object sender, EventArgs e)
        {
            if (checkBoxPrice.Checked)
                {
                    textBoxPrice.Text = textBoxPriceAuto.Text;
                }
        }
    }
}
