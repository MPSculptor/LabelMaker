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
            dataGridViewPlants.Columns[0].Width = 50;
            dataGridViewPlants.Columns[1].Width = 10;
            dataGridViewPlants.Columns[2].Width = 100;
            dataGridViewPlants.Columns[3].Width = 10;
            dataGridViewPlants.Columns[4].Width = 100;
            dataGridViewPlants.Columns[5].Width = 100;
            dataGridViewPlants.Columns[6].Width = 100;

            updateMainDetails(0);
            getLabelName();
            indexNavigationButtons();
        }

        
        //private void button2_Click(object sender, EventArgs e)
        //{
            //TempMakeALabel(panelLabelPreview, "Main", "database");
        //}

        private void profilesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabPageLabelProfiles;
            //addProfileButtons();
        }

        

       
        private void buttonPrint_Click(object sender, EventArgs e)
        {

            // Determine the Queue and no. entries
            int howManyLines = 0;
            string whichQueue = "";
            if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue")
            {
                whichQueue = "Colour";
                howManyLines = dataGridViewColourQ.RowCount - 1;
            }
            else
            {
                whichQueue = "Main";
                howManyLines = dataGridViewMainQ.RowCount - 1;
            }

            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";
            //file with default settings
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);

            //file with a sample label definition;
            if (whichQueue == "Colour")
            {
                name = whereFiles + "LabelsColour.txt";
            }
            else
            {
                name = whereFiles + "LabelsText.txt";
            }
            string[] labelData = CreationUtilities.dataReader.readFile(name);


            for (int i = 0; i < howManyLines; i++)
            {
                string[] queueData = collectQueueRow(i, whichQueue);

                whereToNow printWhere = new whereToNow(queueData, labelData, defaultsString, 0, 0, "print");
                //MessageBox.Show(queueData[0]);
                printWhere.Dispose();
            }
        }

        #region *** Main Tab Routines ***- routines connected with controls on the Main screen 

        #region Main Data Grid Events

        private void dataGridViewPlants_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            updateMainDetails(e.RowIndex);
        }

        #endregion

        #region * Main tabControl Events *

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPagePreview)
            {
                TempMakeALabel(panelLabelTabMain, "Main", "database");
                TempMakeALabel(panelLabelTabColour, "Colour", "database");
                TempMakeALabel(panelLabelTabChoice, "Choice", "database");
            }
            if (tabControlMain.SelectedTab == tabPageDatabase)
            {
                fillDatabaseTab();
            }
            if (tabControlMain.SelectedTab == tabPageLabelProfiles)
            {
                addProfileButtons();
            }
            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
            {
                fillQueueUtilitiesTab();
            }
            if (tabControlMain.SelectedTab == tabPageQuickPrint)
            {
                fillQuickPrint();
            }
        }

        #endregion

        #region Qty Box and Price routines

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

        #endregion

        #region Status Buttons - functions and initial colouring

        public void colourStatusButtons()
        {
            if (buttonAddtoColourQueue.Text == "add Colour")
            {
                buttonAddtoColourQueue.BackColor = Color.YellowGreen;
            }
            else
            {
                buttonAddtoColourQueue.BackColor = Color.DarkSalmon;
            }

            if (buttonAGMStatus.Text == "AGM")
            {
                buttonAGMStatus.BackColor = Color.YellowGreen;
            }
            else
            {
                buttonAGMStatus.BackColor = Color.DarkSalmon;
            }

            if (buttonLableStocks.Text == "Labels")
            {
                buttonLableStocks.BackColor = Color.YellowGreen;
            }
            else
            {
                buttonLableStocks.BackColor = Color.DarkSalmon;
            }

            if (buttonVisibleEntry.Text == "Visible")
            {
                buttonVisibleEntry.BackColor = Color.YellowGreen;
            }
            else
            {
                buttonVisibleEntry.BackColor = Color.DarkSalmon;
            }
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
                buttonAGMStatus.BackColor = Color.YellowGreen;
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
                buttonLableStocks.BackColor = Color.YellowGreen;
            }
            colourStatusButtons();
        }

        #endregion

        #region pictures

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

        #endregion

        #region Label Selection comboBox routines

        private void comboBoxLabelName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxLabelName.SelectedItem != null)
            {
                comboBoxLabelName.Text = comboBoxLabelName.SelectedItem.ToString();
                comboBoxLabelName.Refresh();
                textBoxQty.Focus();
            }
            String[] labelHeaderData = returnLabelHeaderData(comboBoxLabelName.Text.ToString());
            String[] labelData = returnLabelData(comboBoxLabelName.Text.ToString());
            labelData[0] = labelHeaderData[6];
            labelData[1] = labelHeaderData[7];
            TempMakeALabel(panelLabelPreview, "Choice", "database");
        }

        private void fillLabelCombo()
        {
            string getName = "";
            comboBoxLabelName.Items.Clear();
            LabelsLabelNamesTableAdapter.Fill(databaseLabelsDataSetLabelNames.LabelsLabelNames);

            //DataRow dRow = databaseLabelsDataSetDefaults.Tables["Defaults"].Rows[0];
            for (int i = 0; i <= (databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].Rows.Count - 1); i++)
            {
                DataRow dRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].Rows[i];
                getName = dRow.ItemArray[1].ToString();
                comboBoxLabelName.Items.Add(getName);
                //MessageBox.Show(dRow.ItemArray[i + 1].ToString());
            }
        }
        #endregion

            #region *** Navigation Buttons***
        #region Navigation Jump routines
        private void calculateTheJump(int rowBottom, int rowTop)
        {
            int whereAreWe = dataGridViewPlants.CurrentCell.RowIndex;
            int regularJump = 28;
            int rowSeek = rowBottom;

            //jump either half way to next letter or 1 screen
            if (whereAreWe < rowBottom || whereAreWe > rowTop)
            {
                rowSeek = rowBottom;
            }
            else
            {
                int jump = (rowTop - whereAreWe) / 2;
                if (jump > regularJump) { jump = regularJump; }
                rowSeek = whereAreWe + jump;
            }
            makeTheJump(rowSeek);
        }



        private void makeTheJump(int rowSeek)
        {
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[rowSeek].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            updateMainDetails(rowSeek);
        }
        #endregion
        #region plus navigation buttons
        private void buttonAlphaAplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaA.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaAplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }


        private void buttonAlphaBplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaB.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaBplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaCplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaC.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaCplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaDplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaD.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaDplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaEplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaE.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaEplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaFplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaF.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaFplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaGplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaG.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaGplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaIplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaI.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaIplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaHplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaH.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaHplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaJplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaJ.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaJplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaKplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaK.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaKplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaLplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaL.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaLplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaMplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaM.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaMplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaNplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaN.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaNplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaOplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaO.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaOplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaPplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaP.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaPplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaQplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaQ.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaQplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaRplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaR.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaRplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaSplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaS.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaSplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaTplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaT.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaTplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaUplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaU.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaUplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaVplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaV.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaVplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaWplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaW.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaWplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaXplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaX.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaXplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaYplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaY.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaYplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        private void buttonAlphaZplus_Click(object sender, EventArgs e)
        {
            int rowBottom = int.Parse(buttonAlphaZ.Tag.ToString());
            int rowTop = int.Parse(buttonAlphaZplus.Tag.ToString());
            calculateTheJump(rowBottom, rowTop);
        }

        #endregion
        #region Navigation Button Tag writers

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
                int end = dataGridViewPlants.RowCount - 1;
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
                    curButtonPlus.Enabled = true;
                    curButton.BackColor = Color.LightGray;
                    curButtonPlus.BackColor = Color.LightGray;
                    lastLetter = letter;
                }
            }
            //fill empty tags

            for (int i = 90; i >= 66; i--)
            {
                char character = (char)i;
                string letter = character.ToString();
                character = (char)(i - 1);
                string letterLess = character.ToString();
                buttonName = "buttonAlpha" + letter;
                Button curButtonPlus = (Button)groupBoxAlpha.Controls[buttonName];
                buttonName = "buttonAlpha" + letterLess;
                Button curButtonPlusLess = (Button)groupBoxAlpha.Controls[buttonName];
                if (curButtonPlusLess.Enabled == false)
                {
                    //MessageBox.Show(letter + letterLess + curButtonPlus.Tag.ToString());
                    curButtonPlusLess.Tag = curButtonPlus.Tag.ToString();
                }

            }
            //assign the plus buttons the next letter up tag
            for (int i = 65; i <= 89; i++)
            {
                char character = (char)i;
                string letter = character.ToString();
                character = (char)(i + 1);
                string letter2 = character.ToString();

                buttonName = "buttonAlpha" + letter2;
                buttonNamePlus = "buttonAlpha" + letter + "plus";
                Button curButton = (Button)groupBoxAlpha.Controls[buttonName];
                Button curButtonPlus = (Button)groupBoxAlpha.Controls[buttonNamePlus];

                curButtonPlus.Tag = curButton.Tag.ToString();

            }
            buttonAlphaZplus.Tag = (dataGridViewPlants.RowCount - 1).ToString();
        }
        #endregion
        #region Letter Navigation buttons
        private void buttonAlphaA_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaA.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaB_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaB.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaC_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaC.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaD_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaD.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaE_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaE.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaF_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaF.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaG_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaG.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaH_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaH.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaI_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaI.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaJ_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaJ.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaK_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaK.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaL_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaL.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaM_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaM.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaN_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaN.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaO_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaO.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaP_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaP.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaQ_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaQ.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaR_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaR.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaS_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaS.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaT_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaT.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaU_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaU.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaV_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaV.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaW_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaW.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaX_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaX.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaY_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaY.Tag.ToString());
            makeTheJump(rowSeek);
        }

        private void buttonAlphaZ_Click(object sender, EventArgs e)
        {
            int rowSeek = int.Parse(buttonAlphaZ.Tag.ToString());
            makeTheJump(rowSeek);
        }
        #endregion
        #endregion

        #region ***Database Filter Buttons***


        private void doSelection(string selectionText)
        {
            tablePlantsTableAdapter.Adapter.SelectCommand.CommandText = selectionText;
            tablePlantsTableAdapter.Fill(databaseLabelsDataSet.TablePlants);
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[0].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            indexNavigationButtons();
            updateMainDetails(0);
        }

        private void buttonHiddenOnly_Click(object sender, EventArgs e)
        {
            doSelection("SELECT Id, GenusCross, Genus, SpeciesCross, Species, Variety, Common, SKU, [Desc], PotSize, ColourQueue, Barcode, Picture1, Picture2, Picture3, Picture4, AGM, LabelColour, Hide, notes, LabelStock FROM dbo.TablePlants WHERE Hide = 'True'  ORDER BY Genus ASC, Species ASC, Variety ASC");
            buttonHiddenOnly.BackColor = Color.YellowGreen;
            buttonVisibleOnly.BackColor = Color.Transparent;
            buttonAllEntries.BackColor = Color.Transparent;
        }

        private void buttonVisibleOnly_Click(object sender, EventArgs e)
        {
            doSelection("SELECT Id, GenusCross, Genus, SpeciesCross, Species, Variety, Common, SKU, [Desc], PotSize, ColourQueue, Barcode, Picture1, Picture2, Picture3, Picture4, AGM, LabelColour, Hide, notes, LabelStock FROM dbo.TablePlants WHERE Hide = 'False'  ORDER BY Genus ASC, Species ASC, Variety ASC");
            buttonHiddenOnly.BackColor = Color.Transparent;
            buttonVisibleOnly.BackColor = Color.YellowGreen;
            buttonAllEntries.BackColor = Color.Transparent;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            doSelection("SELECT Id, GenusCross, Genus, SpeciesCross, Species, Variety, Common, SKU, [Desc], PotSize, ColourQueue, Barcode, Picture1, Picture2, Picture3, Picture4, AGM, LabelColour, Hide, notes, LabelStock FROM dbo.TablePlants ORDER BY Genus ASC, Species ASC, Variety ASC");
            buttonHiddenOnly.BackColor = Color.Transparent;
            buttonVisibleOnly.BackColor = Color.Transparent;
            buttonAllEntries.BackColor = Color.YellowGreen;
        }
        #endregion

        #region Updating the screen with information

        public void updateMainDetails(int indexOfRow)
        {

            //get all labels
            fillLabelCombo();

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
            TempMakeALabel(panelLabelPreview, "Main", "database");


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

            // Queue Quantities
            addMainQueueTotal();
            addColourQueueTotal();

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

        private int addColourQueueTotal()
        {
            int count = 0;
            int Qvalue = 0;

            for (int i = 0; i < (dataGridViewColourQ.RowCount - 1); i++)
            {
                Qvalue = int.Parse(dataGridViewColourQ.Rows[i].Cells[1].Value.ToString());
                count = count + Qvalue;
            }

            return count;
        }

        private int addMainQueueTotal()
        {
            int count = 0;
            int Qvalue = 0;

            for (int i = 0; i < (dataGridViewMainQ.RowCount - 1); i++)
            {
                Qvalue = int.Parse(dataGridViewMainQ.Rows[i].Cells[1].Value.ToString());
                count = count + Qvalue;
            }

            return count;
        }

        #endregion

        #endregion

        #region *** Database Entry Tab *** 

        #region Filling the Tab

        private void fillDatabaseTab()
        {
            int indexOfRow = 513;
            indexOfRow = dataGridViewPlants.CurrentRow.Index;

            //Index
            textBoxData0.Text = dataGridViewPlants.Rows[indexOfRow].Cells[0].Value.ToString();
            textBoxGridIndex.Text = indexOfRow.ToString();

            //Paint two label examples
            TempMakeALabel(panelDatabaseMain, "Main", "database");
            TempMakeALabel(panelDatabaseColour, "Colour", "database");

            //Fill in Plant Name
            for (int i = 2; i <= 6; i++)
            {
                if (i != 3)
                {
                    TextBox curText = (TextBox)groupBoxDataNameDetails.Controls["textBoxData" + i.ToString()];
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
                ButtonData10.BackColor = Color.YellowGreen;
            }
            else
            {
                ButtonData10.BackColor = Color.DarkSalmon;
            }
            ButtonData16.Text = dataGridViewPlants.Rows[indexOfRow].Cells[16].Value.ToString();
            if (ButtonData16.Text == "True")
            {
                ButtonData16.BackColor = Color.YellowGreen;
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
                ButtonData18.BackColor = Color.YellowGreen;
            }
            ButtonData20.Text = dataGridViewPlants.Rows[indexOfRow].Cells[20].Value.ToString();
            if (ButtonData20.Text == "True")
            {
                ButtonData20.BackColor = Color.YellowGreen;
            }
            else
            {
                ButtonData20.BackColor = Color.DarkSalmon;
            }
        }

        #endregion

        #region Buttons - like make a clean entry

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

        #endregion

        #region Toggles and comboBoxes for changing entry details

        private void ButtonData10_Click(object sender, EventArgs e)
        {
            //Toggle Add to Colour Queue status
            if (ButtonData10.Text == "True")
            {
                ButtonData10.Text = "False";
                ButtonData10.BackColor = Color.DarkSalmon;
            }
            else
            {
                ButtonData10.Text = "True";
                ButtonData10.BackColor = Color.YellowGreen;
            }
        }

        private void ButtonData20_Click(object sender, EventArgs e)
        {
            // Toggle Label Stocks status
            if (ButtonData20.Text == "True")
            {
                ButtonData20.Text = "False";
                ButtonData20.BackColor = Color.DarkSalmon;
            }
            else
            {
                ButtonData20.Text = "True";
                ButtonData20.BackColor = Color.YellowGreen;
            }
        }

        private void ButtonData16_Click(object sender, EventArgs e)
        {
            //Toggle AGM status
            if (ButtonData16.Text == "True")
            {
                ButtonData16.Text = "False";
                ButtonData16.BackColor = Color.DarkSalmon;
            }
            else
            {
                ButtonData16.Text = "True";
                ButtonData16.BackColor = Color.YellowGreen;
            }
        }

        private void ButtonData18_Click(object sender, EventArgs e)
        {
            //Toggle Hidden/Visible entry
            if (ButtonData18.Text == "True")
            {
                ButtonData18.Text = "False";
                ButtonData18.BackColor = Color.YellowGreen;
            }
            else
            {
                ButtonData18.Text = "True";
                ButtonData18.BackColor = Color.DarkSalmon;
            }
        }

        private void textBoxData1_Click(object sender, EventArgs e)
        {
            //Toggle Genus cross
            if (ButtonData1.Text == "x")
            { ButtonData1.Text = ""; }
            else
            { ButtonData1.Text = "x"; }
        }

        private void textBoxData3_Click(object sender, EventArgs e)
        {
            //Toggle species cross
            if (ButtonData3.Text == "x")
            { ButtonData3.Text = ""; }
            else
            { ButtonData3.Text = "x"; }
        }


        private void comboBoxProfilePick_SelectedIndexChanged(object sender, EventArgs e)
            // picks a profile name from a preselected list
        {
            textBoxData17.Text = comboBoxProfilePick.Text;
        }

        #endregion

        #endregion

        #region *** Label Preview Tab ***

        private void clearPanelLabel()
        {
            //Clear the panel
            foreach (Control ctrl in panelLabelPreview.Controls)
            {
                ctrl.Dispose();
            }
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

            whereToNow whereToTwo = new whereToNow(queueData, labelData, defaultsString, finalWidthInt, finalHeightInt, "screen");
            whereToTwo.BackColor = Color.White;

            whereToTwo.Width = finalWidthInt;
            whereToTwo.Height = finalHeightInt;

            whereToTwo.Location = new Point(2, 2);
            whereToTwo.BorderStyle = BorderStyle.FixedSingle;

            whichPanel.Controls.Add(whereToTwo);
        }


        #endregion

        #region *** Profile Tab ***

        private void buttonProfilesClose_Click(object sender, EventArgs e)
        {
            foreach (FlowLayoutPanel ctrl in groupBoxProfiles.Controls)
            {
                groupBoxProfiles.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
            //tabControlProfiles.Visible = false;
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

        #endregion

        #region *** Queue Utilities Tab *** queue  utilities Tab and stuff on actual Queue Tab 

        #region Painting the Tab 

        private void paintQueueUtilities()
        {
            labelFontColour.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(textBoxQ11.Text));
            labelBorderColour.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(textBoxQ14.Text));
            labelBackgroundColour.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(textBoxQ15.Text));

            //Fill in Pictures

            //get picture position
            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";
            //file with sample queue entry;
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);
            string filePlace = defaultsString[0];

            for (int i = 1; i <= 4; i++)
            {
                TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + (i + 20).ToString()];
                PictureBox curPictureBox = (PictureBox)panelQueueUtilities.Controls["pictureBoxQ" + (i).ToString()];

                //Picture images
                try // #1
                {
                    string fileName = curText.Text.ToString();
                    string pictureFile = filePlace + fileName;
                    curPictureBox.Image = Image.FromFile(pictureFile);
                }
                catch (IOException)
                {
                    string pictureFile = "";
                    string testString = curText.Text.ToString();
                    testString = testString.Trim();
                    if (String.IsNullOrEmpty(testString))
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
            //AGM Picture
            string fileNameAGM = textBoxQ20.Text.ToString();
            string pictureFileAGM = filePlace + fileNameAGM;
            try // #1
            {
                pictureBoxQAGM.Image = Image.FromFile(pictureFileAGM);
            }
            catch (IOException)
            {
                pictureFileAGM = "";
                if (String.IsNullOrEmpty(fileNameAGM))
                {
                    pictureFileAGM = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\blank.jpg";
                }
                else
                {
                    pictureFileAGM = "D:\\LabelMaker\\LabelMaker\\" + "PictureFiles\\NoPicture.jpg";
                }
                pictureBoxQAGM.Image = Image.FromFile(pictureFileAGM);
            }
            //Label Previews
            TempMakeALabel(panelQMain, "Main", "queue");
            TempMakeALabel(panelQColour, "Colour", "queue");
        }

        private void fillQueueUtilitiesTab()
        {

            if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageMainQueue")
            {
                if (dataGridViewMainQ.RowCount > 1)
                {
                    int indexOfRow = dataGridViewMainQ.CurrentRow.Index;
                    textBoxQ0.Text = indexOfRow.ToString();
                    //Fill in Plant Name


                    for (int i = 1; i <= 25; i++)
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        curText.Text = dataGridViewMainQ.Rows[indexOfRow].Cells[i - 1].Value.ToString();
                    }
                    swapTextBoxes(4, 8);
                    swapTextBoxes(8, 5);
                    swapTextBoxes(7, 8);
                    swapTextBoxes(6, 8);
                    swapTextBoxes(9, 8);
                }
                else
                {
                    makeNoQueueEntry();
                }
            }
            else
            {
                if (dataGridViewColourQ.RowCount > 1)
                {
                    int indexOfRow = dataGridViewColourQ.CurrentRow.Index;
                    textBoxQ0.Text = indexOfRow.ToString();
                    //Fill in Plant Name


                    for (int i = 1; i <= 25; i++)
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        curText.Text = dataGridViewColourQ.Rows[indexOfRow].Cells[i - 1].Value.ToString();
                    }
                    swapTextBoxes(4, 8);
                    swapTextBoxes(8, 5);
                    swapTextBoxes(7, 8);
                    swapTextBoxes(6, 8);
                    swapTextBoxes(9, 8);
                }
                else
                {
                    makeNoQueueEntry();
                }
            }
            paintQueueUtilities();
        }

        private void makeNoQueueEntry()
        {
            for (int i = 0; i <= 25; i++)
            {
                TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                curText.Text = "";
            }
            textBoxQ1.Text = "No Queue Entry";
            textBoxQ7.Text = "Once you have entries in the print queues you can view and amend them here.";
            textBoxQ11.Text = "10226779";
            textBoxQ14.Text = "10226779";
            textBoxQ15.Text = "10226779";
        }

        #endregion

        #region Buttons

        private void buttonQtyToSame_Click(object sender, EventArgs e)
        {
            if ((int.Parse(textBoxQtyToSame.Text.ToString()) <= 250))
                {
                if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue")
                {
                    for (int i = 0; i < databaseLabelsDataSetColourQueue.TableColourQueue.Rows.Count; i++)
                    {
                        databaseLabelsDataSetColourQueue.TableColourQueue.Rows[i].SetField(2, textBoxQtyToSame.Text.ToString());
                    }
                    try
                    {
                        tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update Colour Queue - " + ex);
                    }
                    labelColourCount.Text = addColourQueueTotal().ToString();
                }
                else
                {
                    for (int i = 0; i < databaseLabelsDataSetMainQueue.TableMainQueue.Rows.Count; i++)
                    {
                        databaseLabelsDataSetMainQueue.TableMainQueue.Rows[i].SetField(2, textBoxQtyToSame.Text.ToString());
                    }
                    try
                    {
                        tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update Main Queue - " + ex);
                    }
                    labelMainCount.Text = addMainQueueTotal().ToString();
                }
            }
            else
            {
                MessageBox.Show("Can't set a Qty above 250");
            }
        }

        private bool validateQueue()
        {
            bool valid = true;

            //Check Quantity
            var isNumeric = int.TryParse(textBoxQ2.Text.ToString(), out int n);
            if (isNumeric)
            {
                if (n<1 || n > 250)
                {
                    MessageBox.Show("Quantity should be between 1 and 250", "Update failed");
                    valid = false;
                }
            }
            else
            {
                MessageBox.Show("Quantity isn't a valid Number (should be an Integer between 0 and 250)","Update Failed");
                valid = false;
            }

            //Check Price
            String PriceBox = textBoxQ3.Text.ToString();
            if (PriceBox.Trim() != "")
            {
                var isPriceNumeric = double.TryParse(textBoxQ3.Text.ToString(), out double price);
                //convert to currency if a number, leave a string if not
                if (isPriceNumeric)
                {
                    textBoxQ3.Text = formatPrice(textBoxQ3.Text.ToString());
                }
                else
                {
                    DialogResult result = MessageBox.Show("Price is not a number (" + textBoxQ3.Text.ToString() + "). Press 'Yes' if you are happy with this, 'No' if not.", "Is the Price right ?", MessageBoxButtons.YesNo);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        valid = false;
                    }
                }
            }

            return valid;
        }

        private void buttonUpdateQLine_Click(object sender, EventArgs e)
        {
            if (validateQueue())
            {
                int indexOfRow = int.Parse(textBoxQ0.Text); //gets row to update

                if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageMainQueue")
                {
                    for (int i = 1; i <= 25; i++) //move through textboxes and update appropriate column
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        string changeText = curText.Text.ToString();
                        databaseLabelsDataSetMainQueue.TableMainQueue.Rows[indexOfRow].SetField(i, changeText);
                    }

                    try
                    {
                        tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update to Main Queue - " + ex);
                    }

                    labelMainCount.Text = addMainQueueTotal().ToString(); //updates a quantity count on screen
                }
                else
                {
                    for (int i = 1; i <= 25; i++) //move through textboxes and update appropriate column
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        string changeText = curText.Text.ToString();
                        databaseLabelsDataSetColourQueue.TableColourQueue.Rows[indexOfRow].SetField(i, changeText);
                    }

                    try
                    {
                        tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update to Colour Queue - " + ex);
                    }

                    labelColourCount.Text = addColourQueueTotal().ToString(); //updates a quantity count on screen
                }

                paintQueueUtilities(); // updates some visuals representing the textbox data

            }
        }

        private void button1_Click_3(object sender, EventArgs e)

        {
            if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageMainQueue")
            {
                int indexOfRow = dataGridViewMainQ.CurrentRow.Index;
                if (indexOfRow < dataGridViewMainQ.RowCount - 2)
                {
                    dataGridViewMainQ.CurrentCell = dataGridViewMainQ.Rows[indexOfRow + 1].Cells[0];
                    fillQueueUtilitiesTab();
                }
            }
            else
            {
                int indexOfRow = dataGridViewColourQ.CurrentRow.Index;
                if (indexOfRow < dataGridViewColourQ.RowCount - 2)
                {
                    dataGridViewColourQ.CurrentCell = dataGridViewColourQ.Rows[indexOfRow + 1].Cells[0];
                    fillQueueUtilitiesTab();
                }
            }
        }

        private void buttonMoveDownQ_Click(object sender, EventArgs e)
        {
            if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageMainQueue")
            {
                int indexOfRow = dataGridViewMainQ.CurrentRow.Index;
                if (indexOfRow > 0)
                {
                    dataGridViewMainQ.CurrentCell = dataGridViewMainQ.Rows[indexOfRow - 1].Cells[0];
                    fillQueueUtilitiesTab();
                }
            }
            else
            {
                int indexOfRow = dataGridViewColourQ.CurrentRow.Index;
                if (indexOfRow > 0)
                {
                    dataGridViewColourQ.CurrentCell = dataGridViewColourQ.Rows[indexOfRow - 1].Cells[0];
                    fillQueueUtilitiesTab();
                }
            }
        }

        private void buttonMoveLineUp_Click(object sender, EventArgs e)
        {
            if (tabControlQueue.SelectedTab == tabPageMainQueue)
            {
                int rowToMove = dataGridViewMainQ.CurrentRow.Index;
                int minRow = 1;
                if (rowToMove >= minRow)
                {
                    DataRow rowData = databaseLabelsDataSetMainQueue.TableMainQueue.NewRow();
                    string[] allTheData = new string[25];
                    for (int i = 0; i <= 24; i++)
                    {
                        allTheData[i] = dataGridViewMainQ.CurrentRow.Cells[i].Value.ToString();
                    }

                    rowData["Name"] = allTheData[0];
                    int answer = 0;
                    int.TryParse(allTheData[1], out answer);
                    rowData["qty"] = answer;
                    rowData["Price"] = allTheData[2];
                    rowData["PotSize"] = allTheData[7];
                    rowData["Customer"] = allTheData[3];
                    rowData["Barcode"] = allTheData[6];
                    rowData["Description"] = allTheData[4];
                    rowData["CommonName"] = allTheData[8];
                    rowData["PictureFile"] = allTheData[5];
                    rowData["ColourFont"] = allTheData[9];
                    rowData["ColourFontColour"] = allTheData[10];
                    rowData["FontBold"] = bool.Parse(allTheData[11]);
                    rowData["FontItalic"] = bool.Parse(allTheData[12]);
                    rowData["ColourBorderColour"] = allTheData[13];
                    rowData["ColourBackgroundColour"] = allTheData[14];
                    rowData["notes"] = allTheData[15];
                    rowData["Genus"] = allTheData[16];
                    rowData["Species"] = allTheData[17];
                    rowData["Variety"] = allTheData[18];
                    rowData["AGM"] = allTheData[19];
                    rowData["Picture1"] = allTheData[20];
                    rowData["Picture2"] = allTheData[21];
                    rowData["Picture3"] = allTheData[22];
                    rowData["Picture4"] = allTheData[23];
                    rowData["OrderNo"] = allTheData[24];


                    dataGridViewMainQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSetMainQueue.TableMainQueue.Rows.InsertAt(rowData, rowToMove - 1);
                    dataGridViewMainQ.EndEdit();

                    try
                    {
                        tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
                        //MessageBox.Show("Succeeding in deleting from Main Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Main Queue - " + ex);
                    }
                    dataGridViewMainQ.CurrentCell = dataGridViewMainQ[0, rowToMove - 1];
                    dataGridViewMainQ.Rows[rowToMove - 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewMainQ.CurrentRow.Index.ToString();
                }
            }
            else
            {
                int rowToMove = dataGridViewColourQ.CurrentRow.Index;
                int minRow = 1;
                if (rowToMove >= minRow)
                {
                    DataRow rowData = databaseLabelsDataSetColourQueue.TableColourQueue.NewRow();
                    string[] allTheData = new string[25];
                    for (int i = 0; i <= 24; i++)
                    {
                        allTheData[i] = dataGridViewColourQ.CurrentRow.Cells[i].Value.ToString();
                    }

                    rowData["Name"] = allTheData[0];
                    int answer = 0;
                    int.TryParse(allTheData[1], out answer);
                    rowData["qty"] = answer;
                    rowData["Price"] = allTheData[2];
                    rowData["PotSize"] = allTheData[7];
                    rowData["Customer"] = allTheData[3];
                    rowData["Barcode"] = allTheData[6];
                    rowData["Description"] = allTheData[4];
                    rowData["CommonName"] = allTheData[8];
                    rowData["PictureFile"] = allTheData[5];
                    rowData["ColourFont"] = allTheData[9];
                    rowData["ColourFontColour"] = allTheData[10];
                    rowData["FontBold"] = bool.Parse(allTheData[11]);
                    rowData["FontItalic"] = bool.Parse(allTheData[12]);
                    rowData["ColourBorderColour"] = allTheData[13];
                    rowData["ColourBackgroundColour"] = allTheData[14];
                    rowData["notes"] = allTheData[15];
                    rowData["Genus"] = allTheData[16];
                    rowData["Species"] = allTheData[17];
                    rowData["Variety"] = allTheData[18];
                    rowData["AGM"] = allTheData[19];
                    rowData["Picture1"] = allTheData[20];
                    rowData["Picture2"] = allTheData[21];
                    rowData["Picture3"] = allTheData[22];
                    rowData["Picture4"] = allTheData[23];
                    rowData["OrderNo"] = allTheData[24];


                    dataGridViewColourQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSetColourQueue.TableColourQueue.Rows.InsertAt(rowData, rowToMove - 1);
                    dataGridViewColourQ.EndEdit();

                    try
                    {
                        tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                        //MessageBox.Show("Succeeding in deleting from Colour Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Colour Queue - " + ex);
                    }
                    dataGridViewColourQ.CurrentCell = dataGridViewColourQ[0, rowToMove - 1];
                    dataGridViewColourQ.Rows[rowToMove - 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewColourQ.CurrentRow.Index.ToString();
                }
            }
        }

        private void buttonMoveLineDown_Click(object sender, EventArgs e)
        {
            if (tabControlQueue.SelectedTab == tabPageMainQueue)
            {
                int rowToMove = dataGridViewMainQ.CurrentRow.Index;
                int maxRow = dataGridViewMainQ.Rows.Count - 2;
                if (rowToMove < maxRow)
                {
                    DataRow rowData = databaseLabelsDataSetMainQueue.TableMainQueue.NewRow();
                    string[] allTheData = new string[25];
                    for (int i = 0; i <= 24; i++)
                    {
                        allTheData[i] = dataGridViewMainQ.CurrentRow.Cells[i].Value.ToString();
                    }

                    rowData["Name"] = allTheData[0];
                    int answer = 0;
                    int.TryParse(allTheData[1], out answer);
                    rowData["qty"] = answer;
                    rowData["Price"] = allTheData[2];
                    rowData["PotSize"] = allTheData[7];
                    rowData["Customer"] = allTheData[3];
                    rowData["Barcode"] = allTheData[6];
                    rowData["Description"] = allTheData[4];
                    rowData["CommonName"] = allTheData[8];
                    rowData["PictureFile"] = allTheData[5];
                    rowData["ColourFont"] = allTheData[9];
                    rowData["ColourFontColour"] = allTheData[10];
                    rowData["FontBold"] = bool.Parse(allTheData[11]);
                    rowData["FontItalic"] = bool.Parse(allTheData[12]);
                    rowData["ColourBorderColour"] = allTheData[13];
                    rowData["ColourBackgroundColour"] = allTheData[14];
                    rowData["notes"] = allTheData[15];
                    rowData["Genus"] = allTheData[16];
                    rowData["Species"] = allTheData[17];
                    rowData["Variety"] = allTheData[18];
                    rowData["AGM"] = allTheData[19];
                    rowData["Picture1"] = allTheData[20];
                    rowData["Picture2"] = allTheData[21];
                    rowData["Picture3"] = allTheData[22];
                    rowData["Picture4"] = allTheData[23];
                    rowData["OrderNo"] = allTheData[24];


                    dataGridViewMainQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSetMainQueue.TableMainQueue.Rows.InsertAt(rowData, rowToMove + 2);
                    dataGridViewMainQ.EndEdit();

                    try
                    {
                        tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
                        //MessageBox.Show("Succeeding in deleting from Main Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Main Queue - " + ex);
                    }
                    dataGridViewMainQ.CurrentCell = dataGridViewMainQ[0, rowToMove + 1];
                    dataGridViewMainQ.Rows[rowToMove + 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewMainQ.CurrentRow.Index.ToString();
                }
            }
            else
            {
                int rowToMove = dataGridViewColourQ.CurrentRow.Index;
                int maxRow = dataGridViewColourQ.Rows.Count - 2;
                if (rowToMove < maxRow)
                {
                    DataRow rowData = databaseLabelsDataSetColourQueue.TableColourQueue.NewRow();
                    string[] allTheData = new string[25];
                    for (int i = 0; i <= 24; i++)
                    {
                        allTheData[i] = dataGridViewColourQ.CurrentRow.Cells[i].Value.ToString();
                    }

                    rowData["Name"] = allTheData[0];
                    int answer = 0;
                    int.TryParse(allTheData[1], out answer);
                    rowData["qty"] = answer;
                    rowData["Price"] = allTheData[2];
                    rowData["PotSize"] = allTheData[7];
                    rowData["Customer"] = allTheData[3];
                    rowData["Barcode"] = allTheData[6];
                    rowData["Description"] = allTheData[4];
                    rowData["CommonName"] = allTheData[8];
                    rowData["PictureFile"] = allTheData[5];
                    rowData["ColourFont"] = allTheData[9];
                    rowData["ColourFontColour"] = allTheData[10];
                    rowData["FontBold"] = bool.Parse(allTheData[11]);
                    rowData["FontItalic"] = bool.Parse(allTheData[12]);
                    rowData["ColourBorderColour"] = allTheData[13];
                    rowData["ColourBackgroundColour"] = allTheData[14];
                    rowData["notes"] = allTheData[15];
                    rowData["Genus"] = allTheData[16];
                    rowData["Species"] = allTheData[17];
                    rowData["Variety"] = allTheData[18];
                    rowData["AGM"] = allTheData[19];
                    rowData["Picture1"] = allTheData[20];
                    rowData["Picture2"] = allTheData[21];
                    rowData["Picture3"] = allTheData[22];
                    rowData["Picture4"] = allTheData[23];
                    rowData["OrderNo"] = allTheData[24];


                    dataGridViewColourQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSetColourQueue.TableColourQueue.Rows.InsertAt(rowData, rowToMove + 2);
                    dataGridViewColourQ.EndEdit();

                    try
                    {
                        tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                        //MessageBox.Show("Succeeding in deleting from Colour Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Colour Queue - " + ex);
                    }
                    dataGridViewColourQ.CurrentCell = dataGridViewColourQ[0, rowToMove + 1];
                    dataGridViewColourQ.Rows[rowToMove + 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewColourQ.CurrentRow.Index.ToString();
                }
            }
            
        }


        #endregion

        #region Changing values in TextBoxes

        private void button2_Click_3(object sender, EventArgs e)
        {
            if (textBoxQ13.Text == "True")
            {
                textBoxQ13.Text = "False";
            }
            else
            {
                textBoxQ13.Text = "True";
            }
            paintQueueUtilities();
        }

        private void buttonQBold_Click(object sender, EventArgs e)
        {
            if (textBoxQ12.Text == "True")
            {
                textBoxQ12.Text = "False";
            }
            else
            {
                textBoxQ12.Text = "True";
            }
            paintQueueUtilities();
        }

        private void buttonQFontColour_Click(object sender, EventArgs e)
        {
            colourChangeQueue(11, labelFontColour);
        }

        private void buttonQBorderColour_Click(object sender, EventArgs e)
        {
            colourChangeQueue(14, labelBorderColour);
        }

        private void buttonQackgroundColour_Click(object sender, EventArgs e)
        {
            colourChangeQueue(15, labelBackgroundColour);
        }

        private void colourChangeQueue(int whichNo, Label whichLabel)
        {
            int storeColour = 0;
            Color oldColour = whichLabel.BackColor;
            TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + whichNo.ToString()];

            Color newColour = pickMeAColour(oldColour);
            storeColour = (newColour.B * 256 * 256) + (newColour.G * 256) + newColour.R;
            curText.Text = storeColour.ToString();
            paintQueueUtilities();
        }

        #endregion

        #region * Queue tabControl Events *

        private void tabControlQueue_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
            {
                fillQueueUtilitiesTab();
            }

            getLabelName();

            if (tabControlMain.SelectedTab == tabPagePreview)
            {
                TempMakeALabel(panelLabelTabChoice, "Choice", "database");
            }


        }
        #endregion

        #region  QUEUE UTILITIES - like collecting info from database and adding entires

        private string[] collectQueueRow(int desiredRow, string whichQueue)
        {
            string[] queueEntry = new string[25];

            //Take into account rearranged data grid
            if (whichQueue == "Main")
            {
                queueEntry[0] = dataGridViewMainQ.Rows[desiredRow].Cells[0].Value.ToString();
                queueEntry[1] = dataGridViewMainQ.Rows[desiredRow].Cells[1].Value.ToString();
                queueEntry[2] = dataGridViewMainQ.Rows[desiredRow].Cells[2].Value.ToString();
                queueEntry[3] = dataGridViewMainQ.Rows[desiredRow].Cells[7].Value.ToString();
                queueEntry[4] = dataGridViewMainQ.Rows[desiredRow].Cells[3].Value.ToString();
                queueEntry[5] = dataGridViewMainQ.Rows[desiredRow].Cells[6].Value.ToString();
                queueEntry[6] = dataGridViewMainQ.Rows[desiredRow].Cells[4].Value.ToString();
                queueEntry[7] = dataGridViewMainQ.Rows[desiredRow].Cells[8].Value.ToString();
                queueEntry[8] = dataGridViewMainQ.Rows[desiredRow].Cells[5].Value.ToString();
            }
            else
            {
                queueEntry[0] = dataGridViewColourQ.Rows[desiredRow].Cells[0].Value.ToString();
                queueEntry[1] = dataGridViewColourQ.Rows[desiredRow].Cells[1].Value.ToString();
                queueEntry[2] = dataGridViewColourQ.Rows[desiredRow].Cells[2].Value.ToString();
                queueEntry[3] = dataGridViewColourQ.Rows[desiredRow].Cells[7].Value.ToString();
                queueEntry[4] = dataGridViewColourQ.Rows[desiredRow].Cells[3].Value.ToString();
                queueEntry[5] = dataGridViewColourQ.Rows[desiredRow].Cells[6].Value.ToString();
                queueEntry[6] = dataGridViewColourQ.Rows[desiredRow].Cells[4].Value.ToString();
                queueEntry[7] = dataGridViewColourQ.Rows[desiredRow].Cells[8].Value.ToString();
                queueEntry[8] = dataGridViewColourQ.Rows[desiredRow].Cells[5].Value.ToString();
            }

            for (int i = 9; i < 25; i++)
            {
                if (whichQueue == "Main")
                {
                    queueEntry[i] = dataGridViewMainQ.Rows[desiredRow].Cells[i].Value.ToString();
                }
                else
                {
                    queueEntry[i] = dataGridViewColourQ.Rows[desiredRow].Cells[i].Value.ToString();
                }
            }
            return queueEntry;
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

            databaseLabelsDataSetMainQueue.TableMainQueue.Rows.Add(row);
            dataGridViewMainQ.EndEdit();
            try
            {
                tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to add to Main Queue - " + ex);
            }
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
            dataGridViewColourQ.EndEdit();
            try
            {
                tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to add to Colour Queue - " + ex);
            }
            labelColourCount.Text = addColourQueueTotal().ToString();
        }

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
            //if (String.IsNullOrEmpty(queueData[2])) { queueData[2] = "0"; }
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

        private void swapTextBoxes(int One, int Two)
        {
            TextBox curTextOne = (TextBox)panelQueueUtilities.Controls["textBoxQ" + One.ToString()];
            TextBox curTextTwo = (TextBox)panelQueueUtilities.Controls["textBoxQ" + Two.ToString()];
            String Swap = curTextOne.Text;
            curTextOne.Text = curTextTwo.Text;
            curTextTwo.Text = Swap;
        }


        #endregion

        #region *** Queue Buttons *** - stuff actually on the queues Tab 

        private void dataGridViewColourQ_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
                fillQueueUtilitiesTab();
        }

        private void dataGridViewMainQ_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
                fillQueueUtilitiesTab();
        }

        #region Delete Buttons

        private void deleteColourQueueLine()
        {
            foreach (DataGridViewCell oneCell in dataGridViewColourQ.SelectedCells)
            {
                if (oneCell.Selected)
                    dataGridViewColourQ.Rows.RemoveAt(oneCell.RowIndex);
            }
            try
            {
                tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to delete from Colour Queue - " + ex);
            }
            labelColourCount.Text = addColourQueueTotal().ToString();
        }

        private void deleteMainQueueLine()
        {
            foreach (DataGridViewCell oneCell in dataGridViewMainQ.SelectedCells)
            {
                if (oneCell.Selected)
                    dataGridViewMainQ.Rows.RemoveAt(oneCell.RowIndex);
            }
            try
            {
                tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to delete from Main Queue - " + ex);
            }
            labelMainCount.Text = addMainQueueTotal().ToString();
        }

        private void deleteQueue(string which)
        {
            int whichOnes = 0;
            if (which == "Both") { whichOnes = whichOnes + 32; }
            if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue") { whichOnes = whichOnes + 16; }

            if (whichOnes == 0 || whichOnes == 32)
            {
                int numRows = databaseLabelsDataSetMainQueue.TableMainQueue.Rows.Count - 1;

                for (int i = 0; i <= numRows; i++)
                {
                    //databaseLabelsDataSetMainQueue.TableMainQueue.Rows.RemoveAt(0);
                    dataGridViewMainQ.Rows.RemoveAt(0);
                }
                dataGridViewMainQ.EndEdit();
                try
                {
                    tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
                    //MessageBox.Show("Succeeding in deleting from Main Queue");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to delete from Main Queue - " + ex);
                }
                labelMainCount.Text = addMainQueueTotal().ToString();
            }

            if (whichOnes != 0)
            {
                int numRows = databaseLabelsDataSetColourQueue.TableColourQueue.Rows.Count - 1;

                for (int i = 0; i <= numRows; i++)
                {
                    //databaseLabelsDataSetColourQueue.TableColourQueue.Rows.RemoveAt(0);
                    dataGridViewColourQ.Rows.RemoveAt(0);
                }
                dataGridViewColourQ.EndEdit();
                try
                {
                    tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                    //MessageBox.Show("Succeeding in deleting from Colour Queue");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to delete from Colour Queue - " + ex);
                }
                labelColourCount.Text = addColourQueueTotal().ToString();
            }
        }

        private void buttonDeleteQLines_Click(object sender, EventArgs e)
        {
            deleteQueueLine();
        }

        private void deleteQueueLine()
        {
            if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue")
            {
                deleteColourQueueLine();
            }
            else
            {
                deleteMainQueueLine();
            }
        }

        private void buttonDeleteThisQueue_Click(object sender, EventArgs e)
        {
            deleteQueue("Single");
        }

        private void buttonDeleteBothQueues_Click(object sender, EventArgs e)
        {
            deleteQueue("Both");
        }
        #endregion

        #endregion

        #endregion

        #region *** Quick Print Tab routines ***
        private void fillQuickPrint()
        {
            DataTable quickNames = new DataTable("quickNames");
            quickNames = LabelsLabelNamesTableAdapter.GetDataByQuickPrint( true);

            //start with a clean slate
            for (int i = 1; i <= 9; i++)
            {
                GroupBox curGroupQP = (GroupBox)tabPageQuickPrint.Controls["groupBoxQP" + i.ToString()];
                TextBox curTextBoxQP = (TextBox)curGroupQP.Controls["textBoxQP" + i.ToString()];
                Button curButtonQP = (Button)curGroupQP.Controls["buttonQP" + i.ToString()];
                Panel curPanelQP = (Panel)curGroupQP.Controls["panelQP" + i.ToString()];

                curGroupQP.Text = "Quick Print " + i.ToString();
                curGroupQP.ForeColor = Color.LightSlateGray;
                curTextBoxQP.Text = "1";
                curTextBoxQP.Enabled = false;
                curButtonQP.Enabled = false;
                curPanelQP.Controls.Clear();
            }

            //only allow 9 buttons, ignore the rest
            int noRows = quickNames.Rows.Count;
            if (noRows > 9) { noRows = 9; }
            String[] allTheNames = new string[noRows];

            // collect all the names as tempMakeALabel resets dataTable
            for (int k = 0; k <= (noRows-1); k++)
            {
                DataRow dRow = quickNames.Rows[k];
                allTheNames[k] = dRow.ItemArray[1].ToString();
            }

            quickNames.Dispose();

            for (int j = 0; j <=(noRows-1); j++)
            {
                int i = j + 1;
                GroupBox curGroupQP = (GroupBox)tabPageQuickPrint.Controls["groupBoxQP" + i.ToString()];
                TextBox curTextBoxQP = (TextBox)curGroupQP.Controls["textBoxQP" + i.ToString()];
                Button curButtonQP = (Button)curGroupQP.Controls["buttonQP" + i.ToString()];
                Panel curPanelQP = (Panel)curGroupQP.Controls["panelQP" + i.ToString()];

                curGroupQP.Text = allTheNames[j];
                curGroupQP.ForeColor = Color.Black;
                curTextBoxQP.Enabled = true;
                curButtonQP.Enabled = true;

                TempMakeALabel(curPanelQP, curGroupQP.Text, "database");

            }
        }
        #endregion

        #region *** Label Stuff *** - routines to get Label Name and find the information needed and TempMakeALabel

        public void TempMakeALabel(Panel whichPanel, string whichLabel, string DatabaseOrQueue)
        {
            string whereFiles = "D:\\LabelMaker\\LabelMaker\\TextFiles\\";

            //file with default settings
            string name = whereFiles + "defaults.txt";
            string[] defaultsString = CreationUtilities.dataReader.readFile(name);
            string[] queueString = new string[25];

            if (DatabaseOrQueue == "database")
            {
                //file with sample queue entry;
                //name = whereFiles + "ColourQueue.txt";


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
            }
            else
            {
                for (int i = 0; i <= 24; i++)
                {
                    TextBox curTextBox = (TextBox)panelQueueUtilities.Controls["textBoxQ" + (i + 1).ToString()];
                    queueString[i] = curTextBox.Text.ToString();
                }
            }

            //file with a sample label definition;


            String labelChoice = "";
            if (whichLabel == "Choice")
            {
                //Take label from the one selected on the main screen
                labelChoice = comboBoxLabelName.Text.ToString();
            }
            else if (whichLabel == "Colour")
            {
                //the colour queue default
                labelChoice = "Colour 5 by 2";
                //name = whereFiles + "LabelsColour.txt";
            }
            else if (whichLabel == "Main")
            {
                //the main queue default
                labelChoice = "Sticker 1 by 1";
                //name = whereFiles + "LabelsText.txt";
            }
            else
            {
                //the labels sent as an arguement
                labelChoice = whichLabel;
            }
            String[] headerString = returnLabelHeaderData(labelChoice);
            String[] labelString = returnLabelData(labelChoice);
            labelString[0] = headerString[6];
            labelString[1] = headerString[7];

            LabelPreview(queueString, labelString, defaultsString, whichPanel);

        }


        //routines to get Label Name and find the information

        public String[] returnLabelHeaderData(string labelName)
        {
            String[] labelHeaderData = new String[18];
            //LabelsLabelNamesTableAdapter.Adapter.SelectCommand.CommandText = "SELECT Id, Name, Child, Batch, QuickPrint FROM dbo.LabelsLabelNames WHERE Name = '" +labelName+"'";
            //LabelsLabelNamesTableAdapter.Fill(databaseLabelsDataSetLabelNames.LabelsLabelNames);
            //DataRow dRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].Rows[0];
            DataTable headerDataSet = new DataTable("headerDataSet");
            headerDataSet = LabelsLabelNamesTableAdapter.GetDataByName(labelName);
            DataRow dRow = headerDataSet.Rows[0];



            //Batch or Not
            string batch = dRow.ItemArray[3].ToString().Trim();
            labelHeaderData[0] = batch;

            //Selector for next table
            String childName = dRow.ItemArray[2].ToString().Trim();

            //LabelsLabelCategoriesTableAdapter.Adapter.SelectCommand.CommandText = "SELECT * FROM dbo.LabelsLabelCategories WHERE Name = '" + childName +"'";
            LabelsLabelCategoriesTableAdapter.FillBy(databaseLabelsDataSetLabelNames.LabelsLabelCategories, childName);
            DataRow eRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelCategories"].Rows[0];
            //Header Data
            for (int i = 1; i <= 15; i++)
            {
                labelHeaderData[i] = eRow.ItemArray[i].ToString().Trim();
            }
            String printerName = eRow.ItemArray[12].ToString();
            printerName = printerName.Trim();

            //PrintersTableAdapter.Adapter.SelectCommand.CommandText = "SELECT Id, Name, OffsetDown, OffsetRight FROM dbo.Printers WHERE Name = '"+ printerName +"'";
            PrintersTableAdapter.FillBy(databaseLabelsDataSetLabelNames.Printers, printerName);
            DataRow fRow = databaseLabelsDataSetLabelNames.Tables["Printers"].Rows[0];

            labelHeaderData[16] = fRow.ItemArray[2].ToString().Trim();
            labelHeaderData[17] = fRow.ItemArray[3].ToString().Trim();

            String messageString = "";
            for (int i = 0; i <= 17; i++)
            {
                messageString = messageString + labelHeaderData[i] + "|";
            }

            //MessageBox.Show(messageString);

            return labelHeaderData;


        }


        public String[] returnLabelData(string labelName)
        {
            int howMany = 20;
            LabelsLabelFieldsTableAdapter.FillBy(databaseLabelsDataSetLabelNames.LabelsLabelFields, labelName);
            int count = databaseLabelsDataSetLabelNames.Tables["LabelsLabelFields"].Rows.Count;
            count = (count * howMany) + 2;
            String[] outputString = new string[count];

            int counter = 2; // leave space for dimensions
            outputString[0] = "90";
            outputString[1] = "50";
            for (int i = 0; i <= (databaseLabelsDataSetLabelNames.Tables["LabelsLabelFields"].Rows.Count - 1); i++)
            {
                DataRow dRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelFields"].Rows[i];
                for (int j = 1; j <= howMany; j++)
                {
                    outputString[counter] = dRow.ItemArray[j].ToString().Trim();
                    if (String.IsNullOrEmpty(outputString[counter])) { outputString[counter] = " "; }
                    counter++;
                }
            }

            return outputString;

        }

        #endregion

        #region *** Odd routines *** like the colour picker, fetching default settings, formatting price, get Plant Name

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
            Name[0] = Name[0] + sentData[1].Trim();
            Name[1] = Name[1] + sentData[1].Trim();

            if (sentData[2] == "x")
            {
                Name[0] = Name[0] + " x";
                Name[2] = Name[2] + " x";
            }

            Name[0] = Name[0] + " " + sentData[3].Trim();
            Name[0] = Name[0] + " " + sentData[4].Trim();
            Name[2] = Name[2] + " " + sentData[3].Trim();
            Name[3] = Name[3] + " " + sentData[4].Trim();


            return Name;
        }

        public string[] getDefaultSettings()
        {
            string[] defaults = new string[4];

            defaultsTableAdapter1.Fill(databaseLabelsDataSetDefaults.Defaults);
            DataRow dRow = databaseLabelsDataSetDefaults.Tables["Defaults"].Rows[0];
            for (int i = 0; i <= 3; i++)
            {
                defaults[i] = dRow.ItemArray[i + 1].ToString();
            }
            return defaults;
        }

        private void getLabelName()
        {
            string[] defaults = getDefaultSettings();
            if (tabControlQueue.SelectedTab == tabPageMainQueue)
            {
                comboBoxLabelName.Text = defaults[2];
            }
            else
            {
                comboBoxLabelName.Text = defaults[3];
            }
        }

        private Color pickMeAColour(Color oldColour)
        {
            colorDialog1.Color = oldColour;
            Color chosenColour = oldColour;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                chosenColour = colorDialog1.Color;
            }
            return chosenColour;
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








        #endregion

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private bool validateDatabase()
        {
            bool valid = true;

            // GroupBoxDataNameDetails
            textBoxData2.Text = textBoxData2.Text.Trim();
            if (textBoxData2.Text.ToString().Length > 50)
            {
                MessageBox.Show("The Genus needs to be less than 50 characters long");
                valid = false;
            }
            textBoxData4.Text = textBoxData4.Text.Trim();
            if (textBoxData4.Text.ToString().Length > 50)
            {
                MessageBox.Show("The Species needs to be less than 50 characters long");
                valid = false;
            }
            textBoxData5.Text = textBoxData5.Text.Trim();
            if (textBoxData5.Text.ToString().Length > 50)
            {
                MessageBox.Show("The Variety needs to be less than 50 characters long");
                valid = false;
            }
            if (textBoxData6.Text.ToString().Length > 100)
            {
                MessageBox.Show("The Common Name needs to be less than 100 characters long");
                valid = false;
            }

            // GroupBoxDataDetails
            if (textBoxData7.Text.ToString().Length > 20)
            {
                MessageBox.Show("The SKU needs to be less than 20 characters long");
                valid = false;
            }
            if (textBoxData8.Text.ToString().Length > 500)
            {
                MessageBox.Show("The Description needs to be less than 500 characters long");
                valid = false;
            }
            if (textBoxData9.Text.ToString().Length > 10)
            {
                MessageBox.Show("The Pot Size needs to be less than 10 characters long");
                valid = false;
            }
            if (textBoxData11.Text.ToString().Length > 13)
            {
                MessageBox.Show("The Barcode needs to be less than 13 characters long");
                valid = false;
            }
            if (textBoxData17.Text.ToString().Length > 50)
            {
                MessageBox.Show("The Label Colour needs to be less than 50" +
                    " characters long");
                valid = false;
            }
            if (textBoxData19.Text.ToString().Length > 255)
            {
                MessageBox.Show("The notes need to be less than 255 characters long");
                valid = false;
            }

            //GroupBoxDataPictures
            if (textBoxData12.Text.ToString().Length > 255)
            {
                MessageBox.Show("The path for Picture1 needs to be less than 255 characters long");
                valid = false;
            }
            if (textBoxData13.Text.ToString().Length > 255)
            {
                MessageBox.Show("The path for Picture2 needs to be less than 255 characters long");
                valid = false;
            }
            if (textBoxData14.Text.ToString().Length > 255)
            {
                MessageBox.Show("The path for Picture3 needs to be less than 255 characters long");
                valid = false;
            }
            if (textBoxData15.Text.ToString().Length > 255)
            {
                MessageBox.Show("The path for Picture4 needs to be less than 255 characters long");
                valid = false;
            }

            return valid;
        }

        private string[,] getPanelNames()
        {
            //Finds the names of the controls and the panels they are on for the database tab

            string[,] namesString = new string[2, 21];
            for (int i = 0; i <= 20; i++)
            {
                namesString[0, i] = "textBoxData";
                namesString[1, i] = "groupBoxDataNameDetails";
            }
            for (int i = 7; i <= 19; i++)
            {
                namesString[1, i] = "groupBoxDataDetails";
            }
            for (int i = 12; i <= 15; i++)
            {
                namesString[1, i] = "groupBoxDataPictures";
            }
            namesString[0, 1] = "ButtonData";
            namesString[0, 3] = "ButtonData";
            namesString[0, 8] = "RichTextData";
            namesString[0, 10] = "ButtonData";
            namesString[1, 10] = "groupBoxDataToggles";
            namesString[0, 16] = "ButtonData";
            namesString[1, 16] = "groupBoxDataToggles";
            namesString[0, 18] = "ButtonData";
            namesString[1, 18] = "groupBoxDataToggles";
            namesString[0, 19] = "RichTextData";
            namesString[0, 20] = "ButtonData";
            namesString[1, 20] = "groupBoxDataToggles";

            return namesString;

        }

        private void buttonUpdateDatabase_Click(object sender, EventArgs e)
        {
            if (validateDatabase())
            {
                //int indexOfRow = int.Parse(textBoxData0.Text); //gets row to update (as database index)
                int indexOfRow = int.Parse(textBoxGridIndex.Text); //gets row to update (as grid index)
                string[,] nameString = getPanelNames();

                    for (int i = 1; i <= 20; i++) //move through textboxes and update appropriate column
                    {
                    string changeText = "";
                    GroupBox curGroup = (GroupBox)tabPageDatabase.Controls[nameString[1, i].ToString()];
                    if (nameString[0, i].ToString() == "textBoxData")
                    {
                        TextBox curText = (TextBox)curGroup.Controls["textBoxData" + i.ToString()];
                        changeText = curText.Text.ToString();
                    }
                    else if (nameString[0, i].ToString() == "ButtonData")
                    {
                        Button curButton = (Button)curGroup.Controls["ButtonData" + i.ToString()];
                        changeText = curButton.Text.ToString();
                    }
                    else
                    {
                        RichTextBox curRichText = (RichTextBox)curGroup.Controls["textBoxData" + i.ToString()];
                        changeText = curRichText.Text.ToString();
                    }
                        databaseLabelsDataSet.TablePlants.Rows[indexOfRow].SetField(i, changeText);
                    }

                    try
                    {
                        tablePlantsTableAdapter.Update(databaseLabelsDataSet.TablePlants);
                        //MessageBox.Show("Updated Database Entry");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update to Database - " + ex);
                    }

                updateMainDetails(indexOfRow);
                tabControlMain.SelectedTab = tabPageManual;
                }
            
            }

        private void buttonAddDatabase_Click(object sender, EventArgs e)
        {
            if (validateDatabase())
            {
                //int indexOfRow = int.Parse(textBoxData0.Text); //gets row to update (as database index)
                //int indexOfRow = int.Parse(textBoxGridIndex.Text); //gets row to update (as grid index)
                string[,] nameString = getPanelNames();
                DataRow dRow = databaseLabelsDataSet.TablePlants.NewRow();
                
                for (int i = 1; i <= 20; i++) //move through textboxes and update appropriate column
                {
                    string changeText = "";
                    GroupBox curGroup = (GroupBox)tabPageDatabase.Controls[nameString[1, i].ToString()];
                    if (nameString[0, i].ToString() == "textBoxData")
                    {
                        TextBox curText = (TextBox)curGroup.Controls["textBoxData" + i.ToString()];
                        changeText = curText.Text.ToString();
                    }
                    else if (nameString[0, i].ToString() == "ButtonData")
                    {
                        Button curButton = (Button)curGroup.Controls["ButtonData" + i.ToString()];
                        changeText = curButton.Text.ToString();
                    }
                    else
                    {
                        RichTextBox curRichText = (RichTextBox)curGroup.Controls["textBoxData" + i.ToString()];
                        changeText = curRichText.Text.ToString();
                    }
                    dRow[i] = changeText;
                }
                databaseLabelsDataSet.TablePlants.Rows.Add(dRow);
                
                try
                {
                    tablePlantsTableAdapter.Update(databaseLabelsDataSet.TablePlants);
                    //MessageBox.Show("Updated Database Entry");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to update to Database - " + ex);
                }

                //refilter the grid so entry ends up sorted
                doSelection("SELECT Id, GenusCross, Genus, SpeciesCross, Species, Variety, Common, SKU, [Desc], PotSize, ColourQueue, Barcode, Picture1, Picture2, Picture3, Picture4, AGM, LabelColour, Hide, notes, LabelStock FROM dbo.TablePlants WHERE Hide = 'False'  ORDER BY Genus ASC, Species ASC, Variety ASC");
                buttonHiddenOnly.BackColor = Color.Transparent;
                buttonVisibleOnly.BackColor = Color.YellowGreen;
                buttonAllEntries.BackColor = Color.Transparent;

                //load in name details
                string[] nameTofind = new string[3];
                nameTofind[0] = textBoxData2.Text.ToString();
                nameTofind[1] = textBoxData4.Text.ToString();
                nameTofind[2] = textBoxData5.Text.ToString();

                //find the start point where new entry might be
                string firstLetter = textBoxData2.Text.ToString();
                firstLetter = firstLetter.Substring(0, 1);
                Button curAlphaButton = (Button)groupBoxAlpha.Controls["ButtonAlpha" + firstLetter];
                int rowSeek = int.Parse(curAlphaButton.Tag.ToString());
                for (int i=rowSeek; i < dataGridViewPlants.RowCount; i++)
                {
                    //check Genus
                        if (dataGridViewPlants.Rows[i].Cells[2].Value.ToString() == nameTofind[0])
                    {
                        //check species
                        if (dataGridViewPlants.Rows[i].Cells[4].Value.ToString() == nameTofind[1])
                        {
                            //check Variety
                            if (dataGridViewPlants.Rows[i].Cells[5].Value.ToString() == nameTofind[2])
                            {
                                //Bingo, set this as the right row, otherwise default to first of the letter (ie if added a hidden line)
                                rowSeek = i;
                            }
                        }
                    }
                }
                
                //go there and refresh the display
                makeTheJump(rowSeek);

                tabControlMain.SelectedTab = tabPageManual;
            }
        }
    }
}
