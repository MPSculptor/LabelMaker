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
            // TODO: This line of code loads data into the 'databaseLabelsDataSetColourQueue.TableColourQueue' table. You can move, or remove it, as needed.
            this.tableColourQueueTableAdapter.Fill(this.databaseLabelsDataSetColourQueue.TableColourQueue);
            // TODO: This line of code loads data into the 'databaseLabelsDataSetMainQueue.TableMainQueue' table. You can move, or remove it, as needed.
            this.tableMainQueueTableAdapter.Fill(this.databaseLabelsDataSetMainQueue.TableMainQueue);
            // TODO: This line of code loads data into the 'databaseLabelsDataSet1.TableProfiles' table. You can move, or remove it, as needed.
            this.tableProfilesTableAdapter.Fill(this.databaseLabelsDataSetProfiles.TableProfiles);
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
            indexNavigationButtons();
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
            //name = whereFiles + "ColourQueue.txt";
            string[] queueString = CreationUtilities.dataReader.readFile(name);

            int currentRow = dataGridViewPlants.CurrentCell.RowIndex;
            string[] sendData = new String[21];
            string[] findName = new String[5];
            string[] moreData = new String[12];


            // get general plant data
            for (int i = 0; i <= 20; i++)
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
            if (dataGridViewPlants.Rows[currentRow].Cells[16].Value.ToString() == "True")
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

            // customer and Order NUmber
            moreData[4] = textBoxCustomerName.Text;
            moreData[5] = textBoxOrderNumber.Text;

            // profile
            string profileName = dataGridViewPlants.Rows[currentRow].Cells[17].Value.ToString();

            DataTable table = databaseLabelsDataSetProfiles.Tables["TableProfiles"];
            string expression;
            expression = "Name = '" + profileName + "'";
            DataRow[] foundRows;

            // Use the Select method to find all rows matching the filter.
            foundRows = table.Select(expression);
            moreData[6] = foundRows[0][3].ToString(); // Font Name
            moreData[7] = foundRows[0][6].ToString(); // Font Colour
            moreData[8] = foundRows[0][4].ToString(); // Bold
            moreData[9] = foundRows[0][5].ToString(); // Italic
            moreData[10] = foundRows[0][2].ToString(); // Border Colour
            moreData[11] = foundRows[0][7].ToString(); // Back Colour

            queueString = dataReader.readQueue(sendData, sendName, moreData);


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
                sendData[i] = dataGridViewPlants.Rows[indexOfRow].Cells[1 + i].Value.ToString();
            }

            PlantNames = getPlantName(sendData);
            labelPlantName.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold | FontStyle.Italic);
            double textWidth = TextRenderer.MeasureText(PlantNames[0], labelPlantName.Font).Width;
            double labelWidth = labelPlantName.Width;

            double textSize = labelPlantName.Font.Size;
            textSize = textSize * (labelWidth / textWidth) * .9;
            float textSizeF = (float)textSize;
            if (textSizeF > 12)
            {
                textSizeF = 12;
            }
            labelPlantName.Font = new Font("Microsoft Sans Serif", textSizeF, FontStyle.Bold | FontStyle.Italic);

            labelPlantName.Text = PlantNames[0];

            //Description
            richTextBoxDesc.Text = dataGridViewPlants.Rows[indexOfRow].Cells[8].Value.ToString();

            //Thumbnails and Main Picture

            //PictureBox curPictureBox = (PictureBox)groupBoxDataPictures.Controls["pictureBoxData" + (i - 11).ToString()];
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
            TempMakeALabel(panelLabelPreview, "Main");


            //Price and Quantity
            textBoxQty.Text = "1";
            if (checkBoxQty.Checked) { textBoxQty.Text = textBoxQtyAuto.Text; }
            textBoxPrice.Text = "0";
            if (checkBoxPrice.Checked) { textBoxPrice.Text = textBoxPriceAuto.Text; }
            textBoxQty.Focus();

            //Status Buttons

            if (dataGridViewPlants.Rows[indexOfRow].Cells[10].Value.ToString() == "False")
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

            //clearPanelLabel();
            if (tabControlMain.SelectedTab == tabPagePreview)
            {
                TempMakeALabel(panelLabelTabMain, "Main");
                TempMakeALabel(panelLabelTabColour, "Colour");
            }

            if (tabControlMain.SelectedTab == tabPageDatabase)
            {
                fillDatabaseTab();
            }

            if (tabControlMain.SelectedTab == tabPageLabelProfiles)
            {
                addProfileButtons();
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

        private void fillDatabaseTab()
        {
            int indexOfRow = 513;
            indexOfRow = dataGridViewPlants.CurrentRow.Index;

            //Index
            textBoxData0.Text = dataGridViewPlants.Rows[indexOfRow].Cells[0].Value.ToString();

            //Paint two label examples
            TempMakeALabel(panelDatabaseMain, "Main");
            TempMakeALabel(panelDatabaseColour, "Colour");

            //Fill in Plant Name
            for (int i = 2; i <= 6; i++)
            {
                if (i != 3)
                { TextBox curText = (TextBox)groupBoxDataNameDetails.Controls["textBoxData" + i.ToString()];
                    curText.Text = dataGridViewPlants.Rows[indexOfRow].Cells[i].Value.ToString();
                }
            }
            ButtonData1.Text = dataGridViewPlants.Rows[indexOfRow].Cells[1].Value.ToString();
            ButtonData3.Text = dataGridViewPlants.Rows[indexOfRow].Cells[3].Value.ToString();
            //Fill in Pictures

            //get picture position
            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";
            //file with sample queue entry;
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);
            string filePlace = defaultsString[0];

            for (int i = 12; i <= 15; i++)
            {
                TextBox curText = (TextBox)groupBoxDataPictures.Controls["textBoxData" + i.ToString()];
                curText.Text = dataGridViewPlants.Rows[indexOfRow].Cells[i].Value.ToString();
                PictureBox curPictureBox = (PictureBox)groupBoxDataPictures.Controls["pictureBoxData" + (i - 11).ToString()];

                //Picture images
                try // #1
                {
                    string fileName = dataGridViewPlants.Rows[indexOfRow].Cells[i].Value.ToString();
                    string pictureFile = filePlace + fileName;
                    curPictureBox.Image = Image.FromFile(pictureFile);
                }
                catch (IOException)
                {
                    string pictureFile = "";
                    if (String.IsNullOrEmpty(dataGridViewPlants.Rows[indexOfRow].Cells[i].Value.ToString()))
                    {
                        pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                    }
                    else
                    {
                        pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                    }
                    curPictureBox.Image = Image.FromFile(pictureFile);
                }
            }




            //Fill Details
            textBoxData7.Text = dataGridViewPlants.Rows[indexOfRow].Cells[7].Value.ToString();
            textBoxData8.Text = dataGridViewPlants.Rows[indexOfRow].Cells[8].Value.ToString();
            textBoxData9.Text = dataGridViewPlants.Rows[indexOfRow].Cells[9].Value.ToString();
            textBoxData11.Text = dataGridViewPlants.Rows[indexOfRow].Cells[11].Value.ToString();
            textBoxData17.Text = dataGridViewPlants.Rows[indexOfRow].Cells[17].Value.ToString();
            textBoxData19.Text = dataGridViewPlants.Rows[indexOfRow].Cells[19].Value.ToString();

            for (int i = 0; i < databaseLabelsDataSetProfiles.TableProfiles.Rows.Count; i++)
            {
                comboBoxProfilePick.Items.Add(dataGridView1ProfileView.Rows[i].Cells[0].Value.ToString());
            }

            //FillTogles
            ButtonData10.Text = dataGridViewPlants.Rows[indexOfRow].Cells[10].Value.ToString();
            if (ButtonData10.Text == "True")
            {
                ButtonData10.BackColor = Color.LightGreen;
            }
            else
            {
                ButtonData10.BackColor = Color.DarkSalmon;
            }
            ButtonData16.Text = dataGridViewPlants.Rows[indexOfRow].Cells[16].Value.ToString();
            if (ButtonData16.Text == "True")
            {
                ButtonData16.BackColor = Color.LightGreen;
            }
            else
            {
                ButtonData16.BackColor = Color.DarkSalmon;
            }
            ButtonData18.Text = dataGridViewPlants.Rows[indexOfRow].Cells[18].Value.ToString();
            if (ButtonData18.Text == "True")
            {
                ButtonData18.BackColor = Color.DarkSalmon;
            }
            else
            {
                ButtonData18.BackColor = Color.LightGreen;
            }
            ButtonData20.Text = dataGridViewPlants.Rows[indexOfRow].Cells[20].Value.ToString();
            if (ButtonData20.Text == "True")
            {
                ButtonData20.BackColor = Color.LightGreen;
            }
            else
            {
                ButtonData20.BackColor = Color.DarkSalmon;
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
                    if (positionDecimal == (doneString.Length - 2)) { doneString = doneString + "0"; }
                }
                else
                {
                    doneString = priceSent.ToString();
                    //MessageBox.Show(doneString.Length.ToString());
                    if (doneString.Length == 3) { doneString = doneString + "0"; }
                    doneString = doneString + "p";
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
            //tabControlProfiles.Visible = true;
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
            Button[] ProfileSample = new Button[databaseLabelsDataSetProfiles.TableProfiles.Rows.Count + 1];
            //Iterate through dataset

            foreach (DataRow rowNumber in databaseLabelsDataSetProfiles.TableProfiles)
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
                ProfileSample[profileIndex].Width = 116;
                ProfileSample[profileIndex].Height = 30;
                Console.WriteLine("BackColour");
                ProfileSample[profileIndex].BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[7]));
                Console.WriteLine("ForeColour");
                ProfileSample[profileIndex].ForeColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[6]));
                ProfileSample[profileIndex].FlatStyle = FlatStyle.Flat;
                ProfileSample[profileIndex].FlatAppearance.BorderSize = 3;
                Console.WriteLine("BorderColour");
                ProfileSample[profileIndex].FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[2]));

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
            //tabControlProfiles.Visible = false;
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
                    addToQueues();
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
                    addToQueues();
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

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {   // Make a Clean Database entry sheet

            //Index
            textBoxData0.Text = "";

            //Paint two label examples
            foreach (Control ctrl in panelDatabaseColour.Controls)
            {
                ctrl.Dispose();
            }
            foreach (Control ctrl in panelDatabaseMain.Controls)
            {
                ctrl.Dispose();
            }

            //Fill in Plant Name
            for (int i = 1; i <= 6; i++)
            {
                TextBox curText = (TextBox)groupBoxDataNameDetails.Controls["textBoxData" + i.ToString()];
                curText.Text = "";
            }
            //Fill in Pictures

            for (int i = 12; i <= 15; i++)
            {
                TextBox curText = (TextBox)groupBoxDataPictures.Controls["textBoxData" + i.ToString()];
                curText.Text = "";
                PictureBox curPictureBox = (PictureBox)groupBoxDataPictures.Controls["pictureBoxData" + (i - 11).ToString()];

                //Picture images
                try // #1
                {
                    string pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                    curPictureBox.Image = Image.FromFile(pictureFile);
                }
                catch (IOException)
                {
                    string pictureFile = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";

                    curPictureBox.Image = Image.FromFile(pictureFile);
                }
            }

            //Fill Details
            textBoxData7.Text = "";
            textBoxData8.Text = "";
            textBoxData9.Text = "";
            textBoxData11.Text = "0000000000000";
            textBoxData17.Text = "Default";
            //FillTogles
            ButtonData10.Text = "True";
            ButtonData16.Text = "False";
            ButtonData18.Text = "True";
            ButtonData20.Text = "False";
        }

        private void ButtonData10_Click(object sender, EventArgs e)
        {
            if (ButtonData10.Text == "True")
            { ButtonData10.Text = "False";
                ButtonData10.BackColor = Color.DarkSalmon; }
            else
            { ButtonData10.Text = "True";
                ButtonData10.BackColor = Color.LightGreen; }
        }

        private void ButtonData20_Click(object sender, EventArgs e)
        {
            if (ButtonData20.Text == "True")
            { ButtonData20.Text = "False";
                ButtonData20.BackColor = Color.DarkSalmon;
            }
            else
            { ButtonData20.Text = "True";
                ButtonData20.BackColor = Color.LightGreen;
            }
        }

        private void ButtonData16_Click(object sender, EventArgs e)
        {
            if (ButtonData16.Text == "True")
            { ButtonData16.Text = "False";
                ButtonData16.BackColor = Color.DarkSalmon;
            }
            else
            { ButtonData16.Text = "True";
                ButtonData16.BackColor = Color.LightGreen;
            }
        }

        private void ButtonData18_Click(object sender, EventArgs e)
        {
            if (ButtonData18.Text == "True")
            { ButtonData18.Text = "False";
                ButtonData18.BackColor = Color.LightGreen;
            }
            else
            { ButtonData18.Text = "True";
                ButtonData18.BackColor = Color.DarkSalmon;
            }
        }

        private void textBoxData1_Click(object sender, EventArgs e)
        {
            if (ButtonData1.Text == "x")
            { ButtonData1.Text = ""; }
            else
            { ButtonData1.Text = "x"; }
        }

        private void textBoxData3_Click(object sender, EventArgs e)
        {
            if (ButtonData3.Text == "x")
            { ButtonData3.Text = ""; }
            else
            { ButtonData3.Text = "x"; }
        }

        // ***** QUEUE UTILITIES *****

        public string[] CollectQueueEntry()
        {
            string[] queueData = new string[25];

            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";

            //file with default settings
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);

            int currentRow = dataGridViewPlants.CurrentCell.RowIndex;
            string[] sendData = new string[21];
            string[] findName = new string[5];
            string[] moreData = new string[2];

            // get general plant data
            for (int i = 0; i <= 20; i++)
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
            if (dataGridViewPlants.Rows[currentRow].Cells[16].Value.ToString() == "True")
            {
                moreData[1] = "AGM.ico";
            }
            else
            {
                moreData[1] = "AGMBlank.ico";
            }

            // profile
            string profileName = dataGridViewPlants.Rows[currentRow].Cells[17].Value.ToString();

            DataTable table = databaseLabelsDataSetProfiles.Tables["TableProfiles"];
            string expression;
            expression = "Name = '" + profileName + "'";
            DataRow[] foundRows;
            // Use the Select method to find all rows matching the filter.
            foundRows = table.Select(expression);

            queueData[0] = sendName[0]; //Full name
            queueData[1] = textBoxQty.Text; // Qty
            if (String.IsNullOrEmpty(queueData[1])) { queueData[1] = "0"; }
            queueData[2] = formatPrice(textBoxPrice.Text); // price
            if (String.IsNullOrEmpty(queueData[2])) { queueData[2] = "0"; }
            queueData[3] = sendData[9];  //Potsize
            queueData[4] = textBoxCustomerName.Text; // Customer Name
            queueData[5] = sendData[11]; // Barcode
            queueData[6] = sendData[8]; //Description
            queueData[7] = sendData[6]; //Common Name
            queueData[8] = moreData[0]; // Main Picture
            queueData[9] = foundRows[0][3].ToString(); // Font Name
            queueData[10] = foundRows[0][6].ToString(); // Font Colour
            queueData[11] = foundRows[0][4].ToString(); // Bold
            queueData[12] = foundRows[0][5].ToString(); // Italic
            queueData[13] = foundRows[0][2].ToString(); // Border Colour
            queueData[14] = foundRows[0][7].ToString(); // Back Colour
            queueData[15] = sendData[19]; // notes
            queueData[16] = sendName[1]; // Genus
            queueData[17] = sendName[2]; // species
            queueData[18] = sendName[3];  // Variety
            queueData[19] = moreData[1]; // AGM picture to use
            queueData[20] = sendData[12]; // Picture1
            queueData[21] = sendData[13]; // Picture2
            queueData[22] = sendData[14]; // Picture3
            queueData[23] = sendData[15]; // Picture4
            queueData[24] = textBoxOrderNumber.Text; //Order Number
            if (string.IsNullOrEmpty(queueData[24]))
            { queueData[24] = ""; }
            else
            { queueData[24] = "Order No. #" + queueData[24]; }



            return queueData;
        }

        private void textBoxQty_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            
           
        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }

        private void addRowToMainQ()
        {
            string[] queue = CollectQueueEntry();
            DataRow row = databaseLabelsDataSetMainQueue.Tables[0].NewRow();

            //row["Id"] = "1";
            row["Name"] = queue[0];
            int answer = 0;
            int.TryParse(queue[1], out answer);
            row["qty"] = answer;
            row["Price"] = queue[2];
            row["PotSize"] = queue[3];
            row["Customer"] = queue[4];
            row["Barcode"] = queue[5];
            row["Description"] = queue[6];
            row["CommonName"] = queue[7];
            row["PictureFile"] = queue[8];
            row["ColourFont"] = queue[9];
            row["ColourFontColour"] = queue[10];
            row["FontBold"] = queue[11];
            row["FontItalic"] = queue[12];
            row["ColourBorderColour"] = queue[13];
            row["ColourBackgroundColour"] = queue[14];
            row["notes"] = queue[15];
            row["Genus"] = queue[16];
            row["Species"] = queue[17];
            row["Variety"] = queue[18];
            row["AGM"] = queue[19];
            row["Picture1"] = queue[20];
            row["Picture2"] = queue[21];
            row["Picture3"] = queue[22];
            row["Picture4"] = queue[23];
            row["OrderNo"] = queue[24];

            databaseLabelsDataSetMainQueue.Tables[0].Rows.Add(row);
            labelMainCount.Text = addMainQueueTotal().ToString();
        }

        private void addRowToColourQ()
        {
            string[] queue = CollectQueueEntry();
            DataRow row = databaseLabelsDataSetColourQueue.Tables[0].NewRow();

            //row["Id"] = "1";
            row["Name"] = queue[0];
            int answer = 0;
            int.TryParse(queue[1], out answer);
            row["qty"] = answer;
            row["Price"] = queue[2];
            row["PotSize"] = queue[3];
            row["Customer"] = queue[4];
            row["Barcode"] = queue[5];
            row["Description"] = queue[6];
            row["CommonName"] = queue[7];
            row["PictureFile"] = queue[8];
            row["ColourFont"] = queue[9];
            row["ColourFontColour"] = queue[10];
            row["FontBold"] = queue[11];
            row["FontItalic"] = queue[12];
            row["ColourBorderColour"] = queue[13];
            row["ColourBackgroundColour"] = queue[14];
            row["notes"] = queue[15];
            row["Genus"] = queue[16];
            row["Species"] = queue[17];
            row["Variety"] = queue[18];
            row["AGM"] = queue[19];
            row["Picture1"] = queue[20];
            row["Picture2"] = queue[21];
            row["Picture3"] = queue[22];
            row["Picture4"] = queue[23];
            row["OrderNo"] = queue[24];

            databaseLabelsDataSetColourQueue.Tables[0].Rows.Add(row);
            labelColourCount.Text = addColourQueueTotal().ToString();
        }

        private void addToQueues()
        {
            string[] queue = CollectQueueEntry();

            if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue")
            {
                addRowToColourQ();
            }
            else
            {
                addRowToMainQ();
                if (buttonAddtoColourQueue.Text == "add Colour")
                {
                    if (checkBoxColourAdd.Checked == true)
                    {
                        addRowToColourQ();
                    }
                }
            }
        }

        private void tablePlantsBindingSource_CurrentChanged_1(object sender, EventArgs e)
        {

        }

        private void dataGridView1ProfileView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void dataGridViewProfiles_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPageLabelProfiles_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxProfilePick_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxData17.Text = comboBoxProfilePick.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {

        }

        private void indexNavigationButtons()
        {
            string buttonName = "";
            string buttonNamePlus = "";
            string lastLetter = "1";
            //MessageBox.Show(dataGridViewPlants.RowCount.ToString());
            for (int i = 0; i < dataGridViewPlants.RowCount; i++)
            {
                string name = dataGridViewPlants.Rows[i].Cells[2].Value.ToString();
                //dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[i].Cells[1];
                string letter = name.Substring(0, 1);

                //MessageBox.Show(name + " , " + letter + " , " + lastLetter);
                if (letter != lastLetter)
                {
                    buttonName = "buttonAlpha" + letter;
                    buttonNamePlus = buttonName + "plus";
                    Button curButton = (Button)groupBoxAlpha.Controls[buttonName];
                    Button curButtonPlus = (Button)groupBoxAlpha.Controls[buttonNamePlus];
                    curButton.Tag = i.ToString();
                    curButton.Enabled = true;
                    curButtonPlus.Tag = i.ToString();
                    lastLetter = letter;
                }

            }
            
        }

        private void buttonAlphaA_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse (buttonAlphaA.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaB_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaB.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaC_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaC.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaD_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaD.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaE_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaE.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaF_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaF.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaG_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaG.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaH_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaH.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaI_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaI.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaJ_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaJ.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaK_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaK.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaL_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaL.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaM_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaM.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaN_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaN.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaO_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaO.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaP_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaP.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaQ_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaQ.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaR_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaR.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaS_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaS.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaT_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaT.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaU_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaU.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaV_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaV.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaW_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaW.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaX_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaX.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaY_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaY.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonAlphaZ_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaZ.Tag.ToString());
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }

        private void buttonHiddenOnly_Click(object sender, EventArgs e)
        {
            tablePlantsTableAdapter.Adapter.SelectCommand.CommandText = "SELECT Id, GenusCross, Genus, SpeciesCross, Species, Variety, Common, SKU, [Desc], PotSize, ColourQueue, Barcode, Picture1, Picture2, Picture3, Picture4, AGM, LabelColour, Hide, notes, LabelStock FROM dbo.TablePlants WHERE Hide = 'True'  ORDER BY Genus ASC, Species ASC, Variety ASC";
            tablePlantsTableAdapter.Fill(databaseLabelsDataSet.TablePlants);
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[0].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            indexNavigationButtons();
            updateMainDetails(0);
            buttonHiddenOnly.BackColor = Color.YellowGreen;
            buttonVisibleOnly.BackColor = Color.Transparent;
            buttonAllEntries.BackColor = Color.Transparent;
        }

        private void buttonVisibleOnly_Click(object sender, EventArgs e)
        {
            tablePlantsTableAdapter.Adapter.SelectCommand.CommandText = "SELECT Id, GenusCross, Genus, SpeciesCross, Species, Variety, Common, SKU, [Desc], PotSize, ColourQueue, Barcode, Picture1, Picture2, Picture3, Picture4, AGM, LabelColour, Hide, notes, LabelStock FROM dbo.TablePlants WHERE Hide = 'False'  ORDER BY Genus ASC, Species ASC, Variety ASC";
            tablePlantsTableAdapter.Fill(databaseLabelsDataSet.TablePlants);
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[0].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            indexNavigationButtons();
            updateMainDetails(0);
            buttonHiddenOnly.BackColor = Color.Transparent;
            buttonVisibleOnly.BackColor = Color.YellowGreen;
            buttonAllEntries.BackColor = Color.Transparent;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            tablePlantsTableAdapter.Adapter.SelectCommand.CommandText = "SELECT Id, GenusCross, Genus, SpeciesCross, Species, Variety, Common, SKU, [Desc], PotSize, ColourQueue, Barcode, Picture1, Picture2, Picture3, Picture4, AGM, LabelColour, Hide, notes, LabelStock FROM dbo.TablePlants ORDER BY Genus ASC, Species ASC, Variety ASC";
            tablePlantsTableAdapter.Fill(databaseLabelsDataSet.TablePlants);
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[0].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            indexNavigationButtons();
            updateMainDetails(0);
            buttonHiddenOnly.BackColor = Color.Transparent;
            buttonVisibleOnly.BackColor = Color.Transparent;
            buttonAllEntries.BackColor = Color.YellowGreen;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }



        private void button2_Click_2(object sender, EventArgs e)
        {
            
        }

        private void dataGridViewMainQ_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        } 
        private int addMainQueueTotal()
        {
            int count = 0;
            int Qvalue = 0;

            for (int i = 0; i < (dataGridViewMainQ.RowCount-1); i++)
            {
                Qvalue = int.Parse(dataGridViewMainQ.Rows[i].Cells[1].Value.ToString());
                count = count + Qvalue;
            }

            return count;
        }

        private int addColourQueueTotal()
        {
            int count = 0;
            int Qvalue = 0;

            for (int i = 0; i < (dataGridViewColourQ.RowCount-1); i++)
            {
                Qvalue = int.Parse(dataGridViewColourQ.Rows[i].Cells[1].Value.ToString());
                count = count + Qvalue;
            }

            return count;
        }
    }
}
