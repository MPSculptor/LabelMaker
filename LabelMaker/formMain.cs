using CreationUtilities;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Deployment.Application;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;

namespace LabelMaker
{


    public partial class formMain : Form
    
    {

        public formMain()

        {
           


            //  ******* IMPORTANT *********
            // Before completion search    //needs amending    to find temporary bits that need fixing or correcting
            InitializeComponent();
           

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Thread splashShow = new Thread(() => showSplashScreen());
            //splashShow.Start();
            //showSplashScreen();
            PerPixelAlphaForm splash = new PerPixelAlphaForm();
            Bitmap picture = new Bitmap(@"D:\LabelMaker\LabelMaker\PictureFiles\splash.png");
            splash.SelectBitmap(picture);
            
            splash.StartPosition = FormStartPosition.CenterScreen;
            splash.Show();



            // TODO: This line of code loads data into the 'databaseLabelsDataSet4.TablePassportQueue' table. You can move, or remove it, as needed.
            this.tablePassportQueueTableAdapter.Fill(this.databaseLabelsDataSet4.TablePassportQueue);
            // TODO: This line of code loads data into the 'databaseLabelsDataSet3.TableAddressQueue' table. You can move, or remove it, as needed.
            this.tableAddressQueueTableAdapter.Fill(this.databaseLabelsDataSet3.TableAddressQueue);
            // TODO: This line of code loads data into the 'databaseLabelsDataSetAddClean.TableAddressFilters' table. You can move, or remove it, as needed.
            this.tableAddressFiltersTableAdapter.Fill(this.databaseLabelsDataSetAddClean.TableAddressFilters);
            // TODO: This line of code loads data into the 'databaseLabelsDataSetAuto.TableAuto' table. You can move, or remove it, as needed.
            this.tableAutoTableAdapter.Fill(this.databaseLabelsDataSetAuto.TableAuto);
            // TODO: This line of code loads data into the 'databaseLabelsDataSetColourQueue.TableColourQueue' table. You can move, or remove it, as needed.
            this.tableColourQueueTableAdapter.Fill(this.databaseLabelsDataSetColourQueue.TableColourQueue);
            // TODO: This line of code loads data into the 'databaseLabelsDataSetMainQueue.TableMainQueue' table. You can move, or remove it, as needed.
            this.tableMainQueueTableAdapter.Fill(this.databaseLabelsDataSetMainQueue.TableMainQueue);
            // TODO: This line of code loads data into the 'databaseLabelsDataSet1.TableProfiles' table. You can move, or remove it, as needed.
            this.tableProfilesTableAdapter.Fill(this.databaseLabelsDataSetProfiles.TableProfiles);
            // TODO: This line of code loads data into the 'databaseLabelsDataSet.TablePlants' table. You can move, or remove it, as needed.
            this.tablePlantsTableAdapter.FillBy(this.databaseLabelsDataSet.TablePlants, false);

            this.BackColor = Color.DarkGray;

            colourQueueTab("first");
            updateManualTab();
            
            canIusePrinter.getPrinterList(); //Used to stop two threads printing to one printer at same time

            splash.Close();

        }



        private void colourQueueTab(string first)
        {
            string[] defaults = getDefaultSettings();
            tabPageColourQueue.BackColor = Color.FromName(defaults[14]);
            dataGridViewColourQ.ForeColor = Color.FromName(defaults[20]);
            tabPageMainQueue.BackColor = Color.FromName(defaults[13]);
            dataGridViewMainQ.ForeColor = Color.FromName(defaults[19]);

            tabPageAddresses.BackColor = Color.FromName(defaults[27]);
            dataGridViewAddressQ.ForeColor = Color.FromName(defaults[28]);
            tabPagePassports.BackColor = Color.FromName(defaults[29]);
            dataGridViewPassportQ.ForeColor = Color.FromName(defaults[30]);

            if (first == "first") { buttonVisibleOnly.BackColor = Color.FromName(defaults[17]); }
        }

        private void updateManualTab()
        {
            int updateNumber = 0;
            try { updateNumber = dataGridViewPlants.CurrentRow.Index; }
            catch { }

            //dataGridViewPlants.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewPlants.Columns[0].Width = 50;
            dataGridViewPlants.Columns[1].Width = 10;
            dataGridViewPlants.Columns[2].Width = 100;
            dataGridViewPlants.Columns[3].Width = 10;
            dataGridViewPlants.Columns[4].Width = 100;
            dataGridViewPlants.Columns[5].Width = 100;
            dataGridViewPlants.Columns[6].Width = 100;

            applyDefaultSetting();
            updateMainDetails(updateNumber);
            getLabelName();
            changeButtonColours();
            indexNavigationButtons();
            initialiseLabelStockGrid();
            initialiseMissingPictureGrid();
            

            // Queue Quantities
            assignQueueTotals();

            tabControlMain.BringToFront();
        }

        #region Menu Strip Events

        private void profilesToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            tabControlMain.SelectedTab = tabPageLabelProfiles;
            //addProfileButtons();
        }

        private void validateDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Do you want to trim whitespaces from all Plant names", "Clean Plant Names of White Spaces", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                //remove whitespaces from names
                string changeText = "";
                progressBarDatabase.Visible = true;
                progressBarDatabase.Maximum = dataGridViewPlants.RowCount;
                for (int i = 0; i < dataGridViewPlants.RowCount; i++)
                {
                    progressBarDatabase.Value = i;
                    progressBarDatabase.Refresh();
                    changeText = dataGridViewPlants.Rows[i].Cells[2].Value.ToString().Trim();
                    databaseLabelsDataSet.TablePlants.Rows[i].SetField(2, changeText);

                    changeText = dataGridViewPlants.Rows[i].Cells[4].Value.ToString().Trim();
                    databaseLabelsDataSet.TablePlants.Rows[i].SetField(4, changeText);

                    changeText = dataGridViewPlants.Rows[i].Cells[5].Value.ToString().Trim();
                    databaseLabelsDataSet.TablePlants.Rows[i].SetField(5, changeText);
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
                progressBarDatabase.Visible = false;
            }
        }

        #endregion

        #region Printing Routines

        private void buttonPrint_Click(object sender, EventArgs e)
        {
            doThePrinting("Main");
        }

        private void buttonAutoPrint_Click(object sender, EventArgs e)
        {
            doThePrinting("Main");
        }

        private void doThePrinting(string whichCombo)

        {   // Start by determining which label to print and what type of label it is

            string[] message = { "Printing has begun", "Printing has started", "", "", "Waiting for Windows to do some stuff" ,"","","This message will self destruct in 3 seconds"};
            Thread formShow = new Thread(() => showMessageFormWithDispose(message, 2300));
            formShow.Start();

            string[] defaultsString = getDefaultSettings();

            string name = "";
            if (whichCombo == "Main")
            {
                //Take label from Main Tab
                name = comboBoxLabelName.Text.ToString().Trim(); //whereToNow
            }
            else
            {
                //use alternative on Autolabel Tab
                name = comboBoxAutoLabelName.Text.ToString().Trim();
            }
            string[] labelData = returnLabelData(name);
            string[] labelHeader = returnLabelHeaderData(name);
            string[] printerDetails = new string[8];
            printerDetails[0] = labelHeader[12];
            printerDetails[1] = labelHeader[13];

            string whichQueue = "Main";
            int totalLabels = 0;
            int queueCount = 0; 
            
            // Determine the Queue and no. entries
                        
            if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue")
            {
                totalLabels = addColourQueueTotal();
                queueCount = dataGridViewColourQ.RowCount-1;
                whichQueue = "Colour";
            }
            else if (tabControlQueue.SelectedTab.Name == "tabPageMainQueue")
            {
                totalLabels = addMainQueueTotal();
                queueCount = dataGridViewMainQ.RowCount-1;
                whichQueue = "Main ";
            }
            else if (tabControlQueue.SelectedTab.Name == "tabPageAddresses")
            {
                totalLabels = addAddressQueueTotal();
                queueCount = dataGridViewAddressQ.RowCount-1;
                whichQueue = "Address";
            }
            else if (tabControlQueue.SelectedTab.Name == "tabPagePassports")
            {
                totalLabels = addPassportQueueTotal();
                queueCount = dataGridViewPassportQ.RowCount-1;
                whichQueue = "Passport";
            }
            int howManyLines = queueCount-1; //howManyLines = top line no for queue

            //Work out some sheet info here as outside scope of if and therefore passable
            float labelsPerSheet = (float)(int.Parse(labelHeader[3]) * int.Parse(labelHeader[4]));
            bool oddNumberOfLabels = true;
            float numberOfSheetsF = (float)totalLabels / (float)labelsPerSheet;
            int numberOfSheetsI = (int)numberOfSheetsF;
            if (numberOfSheetsF == numberOfSheetsI) { oddNumberOfLabels = false; } //find out if labels fit : false = fit, true = no fit
            DialogResult printOnlyFitting = DialogResult.No;

            if (labelHeader[2] == "Picture") //Only ask for a response for multi label sheets
            {                 
                //Get total number of labels to print. Decide if a whole number and if start and stop position
                //Work out number of labels sheets this needs
                
                if (oddNumberOfLabels) { printOnlyFitting = MessageBox.Show("Do you want to print just the labels that fit on a whole number of sheets", "Whole sheets only ?", MessageBoxButtons.YesNo); }

                if (printOnlyFitting == DialogResult.No) { numberOfSheetsI++; } // if printing odd labels, allow one more loop to do it : No = print whole sheets, Yes = print all labels

            }

            //Reset qty and Flag up if only printinga partial queue
            int qty = 0;
            string allOrQty = "All"; // default = print all labels, no need for qty
            if (labelHeader[2] == "Picture")
            {
                //qty = numberOfSheetsI * (int)labelsPerSheet;
                if (printOnlyFitting == DialogResult.Yes)
                {
                    allOrQty = "Qty";
                    qty = numberOfSheetsI * (int)labelsPerSheet;
                }
            }
                            
            //Prduce Print Dialog Box
            printerDetails[2] =  "NoPrint";

    #region pDialog to authorise print

            //get Paper Tray right
            switch (printerDetails[1])
             {
                case "9":
                    printerDetails[1] = "Multipurpose Tray";
                    break;
            }


            PrintDialog pDialog = new PrintDialog();
            //pDialog.PrinterSettings.PrinterName = labelPrinterChoice.Text.ToString().Trim();

            pDialog.PrinterSettings.PrinterName = printerDetails[0];
            pDialog.PrinterSettings.DefaultPageSettings.PrinterSettings.PrinterName = printerDetails[0];

            pDialog.Document = new System.Drawing.Printing.PrintDocument(); // set dummy document to allow papersource setting
            pDialog.Document.PrinterSettings.PrinterName = printerDetails[0];
            pDialog.Document.PrinterSettings.DefaultPageSettings.PrinterSettings.PrinterName = printerDetails[0];
            try
            {
                pDialog.Document.DefaultPageSettings.PaperSource.SourceName = printerDetails[1];
                pDialog.PrinterSettings.DefaultPageSettings.PaperSource.SourceName = printerDetails[1];
            
                string paperSourceName = printerDetails[1];
            
                //set right paper tray
                string[] sources = new string[20];

                PaperSource pkSource = pDialog.PrinterSettings.DefaultPageSettings.PaperSource;
                PaperSource pkFoundSource = pDialog.PrinterSettings.DefaultPageSettings.PaperSource;

                for (int q = 0; q < pDialog.PrinterSettings.PaperSources.Count; q++)
                {
                    pkSource = pDialog.PrinterSettings.PaperSources[q];
                    sources[q] = pkSource.SourceName;
                    if (pkSource.SourceName == listBoxPrinter.Items[15].ToString().TrimEnd()) { pkFoundSource = pkSource; }
                }

                pDialog.Document.DefaultPageSettings.PaperSource = pkFoundSource;
                pDialog.PrinterSettings.DefaultPageSettings.PaperSource = pkFoundSource;
                }
            catch
            {
                MessageBox.Show("Failed to set correct paper tray, please make sure it is set as you need");
            }

            //DialogResult msg = MessageBox.Show(printVariables.howManyLines.ToString());

            #endregion
            DialogResult printerResponse = pDialog.ShowDialog();

            printerDetails[3] = pDialog.PrinterSettings.PrinterName;
            printerDetails[4] = pDialog.PrinterSettings.DefaultPageSettings.PaperSize.Width.ToString();
            printerDetails[5] = pDialog.PrinterSettings.DefaultPageSettings.PaperSize.Height.ToString();
            printerDetails[6] = pDialog.PrinterSettings.DefaultPageSettings.HardMarginX.ToString();
            printerDetails[7] = pDialog.PrinterSettings.DefaultPageSettings.HardMarginY.ToString();
            PaperSource paperSource = pDialog.Document.DefaultPageSettings.PaperSource;

            


            if (printerResponse == DialogResult.OK)
            {
                printerDetails[2] = "Print";
            }

            if (printerDetails[2] == "Print")
            {

            //Information Box to read while printing takes place
            string wholeOrPartial = "The whole Queue of ";
            int declaredQty = queueCount;
            if (allOrQty == "Qty") { wholeOrPartial = "A partial quantity of "; declaredQty = qty; }
            
            string[] messageMain = { "Labels are Printing", "Printing has started", "", "Collecting Queue Data", "", "Using Queue - " + whichQueue,
                                    "","Printing:-",
                                    wholeOrPartial, declaredQty + " Labels from " + (howManyLines +1).ToString() + " lines" ,"Sending to Printer - " + printerDetails[3],
                                    "","","","This message will self destruct in 8 seconds"};
            Thread formShowMain = new Thread(() => showMessageFormWithDispose(messageMain, 8000));
            formShowMain.Start();

            //Collect the Queue so it can be sent and deleted
            string[,] wholeQueue = collectTheQueue(whichQueue, allOrQty, queueCount, qty);
            int labelCount = 0;
            for (int i = 0; i < ((int)(wholeQueue.Length) / 36); i++) { labelCount = labelCount + int.Parse(wholeQueue[i, 1]); }
            if (allOrQty == "Qty") { howManyLines = (wholeQueue.Length / 36); } //reduce row count for partial queues

            //Set up all the required data needed to print
            printDefaults printVariables = new printDefaults();
            printVariables.labelData = labelData;
            printVariables.whichQueue = whichQueue;
            printVariables.howManyLines = howManyLines;
            printVariables.defaultsString = defaultsString;
            printVariables.printerDetails = printerDetails;
            printVariables.paperSource = paperSource;
            printVariables.wholeQueue = wholeQueue;
            printVariables.labelCount = labelCount;
            printVariables.printerListIndex = canIusePrinter.getPrinterIndex(printerDetails[0]);

                if (labelHeader[2] == "Text")
                {
                    Thread textPrint = new Thread(() => printAsText(printVariables));    // Kick off a new thread
                    textPrint.Start();
                }
                else
                {
                    Thread colourPrint = new Thread(() => printAsColour(printVariables));
                    colourPrint.Start();
                }

            }

            
                
            }

        private string[,] collectTheQueue(string whichQueue, string allOrQty , int howManyLines, int qty)
        {
            int rowCount = 0;
            Boolean completeRow = true;
            int lastLineQty = 0;
            if (allOrQty == "Qty")
            {
                //Need to reduce howManyLines to reflect true number
                int qtyCount = 0;
                for (int i = 0; i <= howManyLines; i++)
                {
                    string[] queueData = collectQueueRow(i, whichQueue);
                    int lineQty = int.Parse(queueData[1]);
                    if (qtyCount+lineQty < qty)
                    {
                        //total not reached
                        qtyCount = qtyCount + lineQty;
                    }
                    else if (qtyCount + lineQty == qty)
                    {
                        //This Line makes it up exactly
                        rowCount = i;
                        goto SkipOut;
                    }
                    else if (qtyCount + lineQty > qty)
                    {
                        //This line tips over the quantity
                        lastLineQty = qtyCount + lineQty - qty;
                        rowCount = i;
                        completeRow = false;
                        goto SkipOut;
                    }
                }
            }
        SkipOut: //has to be outside IF statement
            if (allOrQty == "Qty") { howManyLines = rowCount; }

            //Actually collect the queue
            string[,] wholeQueue = new string[howManyLines,36];
            for (int i = 0; i < howManyLines; i++)
            {
                string[] queueData = collectQueueRow(i, whichQueue);
                for (int j=0; j <= 35; j++) { wholeQueue[i, j] = queueData[j]; }
            }

            //Delete Queue
            if (checkBoxQueueDelete.Checked)
            {
                //Delete Whole Queue if printing all
                if (allOrQty == "All")
                {
                    if (tabControlQueue.SelectedTab == tabPageMainQueue) { deleteQueue("Main Queue"); }
                    else if (tabControlQueue.SelectedTab == tabPageColourQueue) { deleteQueue("Colour Queue"); }
                    else if (tabControlQueue.SelectedTab == tabPageAddresses) { deleteQueue("Addresses Queue"); }
                    else if (tabControlQueue.SelectedTab == tabPagePassports) { deleteQueue("Passports Queue"); }
                }
                //Delete Partial Queue if printing just labels that fit
                else
                {
                    //delete most of the queue a line at a time
                    for (int i = 0; i <rowCount; i++)
                    {
                        if (tabControlQueue.SelectedTab == tabPageMainQueue) { dataGridViewMainQ.Rows[0].Cells[1].Selected = true; deleteMainQueueLine(); }
                        if (tabControlQueue.SelectedTab == tabPageColourQueue) { dataGridViewColourQ.Rows[0].Cells[1].Selected = true; deleteColourQueueLine(); }
                        if (tabControlQueue.SelectedTab == tabPageAddresses) { dataGridViewAddressQ.Rows[0].Cells[1].Selected = true; deleteAddressQueueLine(); }
                        if (tabControlQueue.SelectedTab == tabPagePassports) { dataGridViewPassportQ.Rows[0].Cells[1].Selected = true; deletePassportQueueLine(); }
                    }
                //do what is required to the last row : delete or reduce 
                        if (completeRow) //last row completes the qty
                    {
                        if (tabControlQueue.SelectedTab == tabPageMainQueue) { dataGridViewMainQ.Rows[0].Cells[1].Selected = true; deleteMainQueueLine(); }
                        if (tabControlQueue.SelectedTab == tabPageColourQueue) { dataGridViewColourQ.Rows[0].Cells[1].Selected = true; deleteColourQueueLine(); }
                        if (tabControlQueue.SelectedTab == tabPageAddresses) { dataGridViewAddressQ.Rows[0].Cells[1].Selected = true; deleteAddressQueueLine(); }
                        if (tabControlQueue.SelectedTab == tabPagePassports) { dataGridViewPassportQ.Rows[0].Cells[1].Selected = true; deletePassportQueueLine(); }
                    }
                    else //last row is a partial row
                    {
                        if (tabControlQueue.SelectedTab == tabPageMainQueue)
                        {
                            databaseLabelsDataSetMainQueue.TableMainQueue.Rows[0].SetField(2, lastLineQty);

                            try { tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue); }
                            catch (System.Exception ex) { }
                        }
                        if (tabControlQueue.SelectedTab == tabPageColourQueue)
                        {
                            databaseLabelsDataSetColourQueue.TableColourQueue.Rows[0].SetField(2, lastLineQty);

                            try { tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue); }
                            catch (System.Exception ex) { }
                        }
                        if (tabControlQueue.SelectedTab == tabPageAddresses)
                        {
                            databaseLabelsDataSet3.TableAddressQueue.Rows[0].SetField(2, lastLineQty);

                            try { tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue); }
                            catch (System.Exception ex) { }
                        }
                        if (tabControlQueue.SelectedTab == tabPagePassports)
                        {
                            databaseLabelsDataSet4.TablePassportQueue.Rows[0].SetField(2, lastLineQty);

                            try { tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue); }
                            catch (System.Exception ex) { }
                        }
                    }
                }
            }



            return wholeQueue;
        }
         

        private void DrawImage(string[] queueData, string[] labelData, string[] defaultsString, int sentWidth, int sentHeight,int marginX, int placementX,int marginY, int placementY, object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            //MessageBox.Show("DrawImage");
            whereToNow printWhere = new whereToNow(queueData, labelData, defaultsString, sentWidth, sentHeight, marginX, placementX, marginY, placementY, "print", e.Graphics);
            printWhere.Dispose();
        }

        

        private void printAsText( printDefaults printVariables)
        {
            //MessageBox.Show("Got to Text Printing");
            
            //wait around so queues don't clash on print
            int waitCounter = 0;
        Busy:
            if (canIusePrinter.inUseOrNot[printVariables.printerListIndex] == true)
                {
                Thread.Sleep(1000);
                waitCounter++;
                goto Busy;
            }
            canIusePrinter.inUseOrNot[printVariables.printerListIndex] = true;
            //MessageBox.Show("Waited for " + waitCounter + " loops");

            //Print one label at a time using multiple copies for speed
            int rowsToPrint = printVariables.howManyLines+1;
            string[] queueData = new string[36];
            for (int i = 0; i < rowsToPrint; i++)
                {
                for (int h = 0; h <= 35; h++) { queueData[h] = printVariables.wholeQueue[i, h]; }

                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = printVariables.printerDetails[3];
                    if (listBoxPrinter.Items[4].ToString() == "Landscape")
                    {
                        pd.DefaultPageSettings.Landscape = true;
                    }
                    else
                    {
                        pd.DefaultPageSettings.Landscape = false;
                    }

                    int sentWidth = int.Parse(printVariables.printerDetails[4]);
                    int sentHeight = int.Parse(printVariables.printerDetails[5]);
                    int marginX = int.Parse(printVariables.printerDetails[6]) ;
                    int marginY = int.Parse(printVariables.printerDetails[7]);
                    int placementX = 0;
                    int placementY = 0;


                    pd.PrintPage += (sender1, args) => DrawImage(queueData, printVariables.labelData, printVariables.defaultsString, sentWidth, sentHeight,marginX, placementX,marginY, placementY, sender1, args);
                    pd.PrinterSettings.Copies = short.Parse(queueData[1]);
                    
                    pd.Print();
                    pd.Dispose();

                }
            canIusePrinter.inUseOrNot[printVariables.printerListIndex] = false;
        }

        private void printAsColour(  printDefaults printerVariables)
        {
            //wait around so queues don't clash on print
            int waitCounter = 0;
        Busy:
            if (canIusePrinter.inUseOrNot[printerVariables.printerListIndex]==true)
            {
                Thread.Sleep(1000);
                waitCounter++;
                goto Busy;
            }
            canIusePrinter.inUseOrNot[printerVariables.printerListIndex] = true;
            //MessageBox.Show("Waited for " + waitCounter + " loops");

            //Print multiple labels on one sheet. 

            //Collect overall data
            int countX = 0; //label x position
                int countY = 0; //label y position
                int labelsAcross = int.Parse(listBoxPrinter.Items[2].ToString());
                int labelsDown = int.Parse(listBoxPrinter.Items[3].ToString()) ;
                int labelsPerSheet = labelsAcross * labelsDown;
                int totalLabels = printerVariables.labelCount;
                int queueCount = printerVariables.howManyLines;
                int howManyPrinted = 0;

            //collect which labels are represented by which queue entry
            int[] queuePositions = new int[totalLabels];
            int queuePositionCounter = 0;
            int queueQty = 1;
            for (int j = 0; j <= queueCount; j++)
            {
                queueQty = int.Parse(printerVariables.wholeQueue[j,1].Trim());
                    for (int k = 1; k <= queueQty; k++)
                    {
                        queuePositions[queuePositionCounter] = j;
                        queuePositionCounter++;
                    }
                
            }

            //loop through number of iterations
            int numberOfSheetsI = (int)((totalLabels-1) / labelsPerSheet)+1;
            queuePositionCounter = 0;

                for (int i = 1; i <= numberOfSheetsI; i++)

                { 
                    countX = 0;
                    countY = 0;
                    PrintDocument pd = new PrintDocument();

                    pd.PrinterSettings.PrinterName =  printerVariables.printerDetails[3];
                    pd.DefaultPageSettings.PaperSource = printerVariables.paperSource;
                    pd.PrinterSettings.DefaultPageSettings.PaperSource = printerVariables.paperSource;

                    
                    if (listBoxPrinter.Items[4].ToString().Trim() == "Landscape")
                    {
                        pd.DefaultPageSettings.Landscape = true;
                    }
                    else
                    {
                        pd.DefaultPageSettings.Landscape = false;
                    }
                

                    // put label on the sheet
                    for (int j = 1; j <= labelsPerSheet; j++)
                    {
                    string[] queueData = new string[36];
                    for (int h = 0; h <= 35; h++) { queueData[h] = printerVariables.wholeQueue[queuePositions[queuePositionCounter], h]; }
                    //string[] queueData = collectQueueRow(queuePositions[queuePositionCounter], whichQueue);

                        int sentWidth = int.Parse(printerVariables.printerDetails[4]);
                        int sentHeight = int.Parse(printerVariables.printerDetails[5]);
                        int marginX = int.Parse(printerVariables.printerDetails[6]);
                        int marginY = int.Parse(printerVariables.printerDetails[7]);

                    if (listBoxPrinter.Items[4].ToString().Trim() == "Landscape")
                        {
                            int swap = sentWidth;
                            sentWidth = sentHeight;
                            sentHeight = swap;
                        }

                        sentWidth = sentWidth / labelsAcross;
                        sentHeight = sentHeight / labelsDown;

                        int XPosition = sentWidth * countX;
                        int YPosition = sentHeight * countY;
                        
                        pd.PrintPage += (sender1, args) => DrawImage(queueData, printerVariables.labelData, printerVariables.defaultsString, sentWidth, sentHeight, marginX, XPosition, marginY,YPosition, sender1, args);

                        howManyPrinted++;
                        countX++;
                        if (countX==labelsAcross) { countX = 0; countY++; }
                        if (countY == labelsDown) { countX = 0; countY = 0; }

                    
                    if (queuePositionCounter == totalLabels) { break ; }
                    queuePositionCounter++;



                }

                    //send to print document
                    pd.Print();
                    pd.Dispose();
                    
                }
            canIusePrinter.inUseOrNot[printerVariables.printerListIndex] = false;

        }

        #region  ###  OLD PRINTING CODE FOR DELETION IF THREADING WORKS ####

        private void OLDPRINTCODEprintAsText(string[] labelData, string whichQueue, int howManyLines, string[] defaultsString, string[] printerDetails)
        // ### delete when printing swapped ###
        {
            //Print one label at a time using multiple copies for speed

            PrintDialog pDialog = new PrintDialog();
            pDialog.PrinterSettings.PrinterName = printerDetails[0];


            if (DialogResult.OK == pDialog.ShowDialog())
            {
                int count = 0;
                for (int i = 0; i < howManyLines; i++)
                {
                    // count how long each label takes
                    Stopwatch sw1 = new Stopwatch();
                    sw1.Start();

                    string[] queueData = collectQueueRow(count, whichQueue);

                    //About 0.1 %
                    Console.WriteLine("Elapsed after collecting queueData ={0}" + "  " + queueData[0], sw1.Elapsed.ToString("ss\\.ffff"));

                    PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = pDialog.PrinterSettings.PrinterName;
                    if (listBoxPrinter.Items[4].ToString() == "Landscape")
                    {
                        pd.DefaultPageSettings.Landscape = true;
                    }
                    else
                    {
                        pd.DefaultPageSettings.Landscape = false;
                    }

                    int sentWidth = (int)(pDialog.PrinterSettings.DefaultPageSettings.PaperSize.Width);
                    int sentHeight = (int)(pDialog.PrinterSettings.DefaultPageSettings.PaperSize.Height);
                    int marginX = (int)pDialog.PrinterSettings.DefaultPageSettings.HardMarginX;
                    int marginY = (int)pDialog.PrinterSettings.DefaultPageSettings.HardMarginY;
                    int placementX = 0;
                    int placementY = 0;


                    pd.PrintPage += (sender1, args) => DrawImage(queueData, labelData, defaultsString, sentWidth, sentHeight, marginX, placementX, marginY, placementY, sender1, args);
                    pd.PrinterSettings.Copies = short.Parse(queueData[1]);
                    //About a quarter
                    Console.WriteLine("Elapsed up until Print ={0}" + "  " + queueData[0], sw1.Elapsed.ToString("ss\\.ffff"));
                    //This takes 75% of time of which label creation is about 12%, ie 60%+ of time is the printer driver.
                    pd.Print();
                    //Same as end
                    //Console.WriteLine("Elapsed up until Print.Dispose ={0}" + "  " + queueData[0], sw1.Elapsed.ToString("ss\\.ffff"));
                    pd.Dispose();


                    sw1.Stop();
                    Console.WriteLine("Elapsed for whole label ={0}" + "  " + queueData[0], sw1.Elapsed.ToString("ss\\.fff"));

                    count++; //increment so move through queue if not deleting

                    //delete line
                    if (checkBoxQueueDelete.Checked)
                    {
                        if (whichQueue == "Main")
                        {
                            dataGridViewMainQ.Rows.RemoveAt(0);
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
                            dataGridViewMainQ.Refresh();
                            labelMainCount.Text = addMainQueueTotal().ToString();
                            labelMainCountQ.Text = labelMainCount.Text;
                            count--;
                        }
                        else if (whichQueue == "Address")
                        {
                            dataGridViewAddressQ.Rows.RemoveAt(0);
                            dataGridViewAddressQ.EndEdit();
                            try
                            {
                                tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
                                //MessageBox.Show("Succeeding in deleting from Address Queue");
                            }
                            catch (System.Exception ex)
                            {
                                MessageBox.Show("Failed to delete from Address Queue - " + ex);
                            }
                            dataGridViewAddressQ.Refresh();
                            textBoxAddressCount.Text = addAddressQueueTotal().ToString();
                            count--;//cancel increment if we are deleting so we always take the first item
                        }
                        else if (whichQueue == "Passport")
                        {
                            dataGridViewPassportQ.Rows.RemoveAt(0);
                            dataGridViewPassportQ.EndEdit();
                            try
                            {
                                tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
                                //MessageBox.Show("Succeeding in deleting from Passport Queue");
                            }
                            catch (System.Exception ex)
                            {
                                MessageBox.Show("Failed to delete from Passport Queue - " + ex);
                            }
                            dataGridViewPassportQ.Refresh();
                            textBoxPassportCount.Text = addPassportQueueTotal().ToString();
                            count--;//cancel increment if we are deleting so we always take the first item
                        }
                        else
                        {
                            dataGridViewColourQ.Rows.RemoveAt(0);
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
                            dataGridViewColourQ.Refresh();
                            labelColourCount.Text = addColourQueueTotal().ToString();
                            labelColourCountQ.Text = labelColourCount.Text;
                            count--;//cancel increment if we are deleting so we always take the first item
                        }
                    }
                }

                pDialog.Dispose();

            }
        }

    

        #endregion


#endregion

        #region *** Main Tab Routines ***- routines connected with controls on the Main screen 

        #region Main Data Grid Events

        private void dataGridViewPlants_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            updateMainDetails(e.RowIndex);
        }

        #endregion

        #region * Main tabControl Events *

        private void tabControlMain_Click(object sender, EventArgs e)
        {
            tabControlMain.BringToFront();
            updateManualTab();
        }


        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            tabControlMain.BringToFront();
                       

            if (tabControlMain.SelectedTab == tabPagePreview)
            {
                TempMakeALabel(panelLabelTabMain, "Main", "database","");
                TempMakeALabel(panelLabelTabColour, "Colour", "database","");
                TempMakeALabel(panelLabelTabChoice, "Choice", "database","");
            }
            if (tabControlMain.SelectedTab == tabPageDatabase)
            {
                fillDatabaseTab();
            }
            if (tabControlMain.SelectedTab == tabPageLabelProfiles)
            {
                addProfileButtons();

                string profileName = buttonMainProfile.Text;
                for (int f =0;f< dataGridView1ProfileView.RowCount; f++)
                {
                    if (dataGridView1ProfileView.Rows[f].Cells[1].Value.ToString().Trim() == profileName)
                    {
                        dataGridView1ProfileView.CurrentCell = dataGridView1ProfileView[1,f];
                        break;
                    }
                }
                                
                updateProfileSample();
                addProfilePicture("database");
            }
            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
            {
                fillQueueUtilitiesTab();
            }
            if (tabControlMain.SelectedTab == tabPageQuickPrint)
            {
                fillQuickPrint();
            }
            if (tabControlMain.SelectedTab == tabPageAuto)
            {
                string[] defaults = getDefaultSettings();
                string where = defaults[1];
                string file = defaults[18];
                labelAutoFile.Text = where + file;
                fillAutoListBox();
                checkSKUs();
            }
            if (tabControlMain.SelectedTab == tabPageManual)
            {
                
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
                    addToQueues("database");
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
                    addToQueues("database");
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
            string[] defaults = getDefaultSettings();

            Color colourTrue = Color.FromName(defaults[17]);
            Color colourFalse = Color.FromName(defaults[15]);

            if (buttonAddtoColourQueue.Text == "add Colour")
            {
                buttonAddtoColourQueue.BackColor = colourTrue;
            }
            else
            {
                buttonAddtoColourQueue.BackColor = colourFalse;
            }

            if (buttonAGMStatus.Text == "AGM")
            {
                buttonAGMStatus.BackColor = colourTrue;
            }
            else
            {
                buttonAGMStatus.BackColor = colourFalse;
            }

            if (buttonLableStocks.Text == "Labels")
            {
                buttonLableStocks.BackColor = colourTrue;
            }
            else
            {
                buttonLableStocks.BackColor = colourFalse;
            }

            if (buttonVisibleEntry.Text == "Visible")
            {
                buttonVisibleEntry.BackColor = colourTrue;
            }
            else
            {
                buttonVisibleEntry.BackColor = colourFalse;
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
            changeFlag(10);
        }


        private void changeFlag(int cellIndex)
        {
            int currentRow = dataGridViewPlants.CurrentRow.Index;
            if (databaseLabelsDataSet.TablePlants.Rows[currentRow].ItemArray[cellIndex].ToString() == "True") 
            {
                databaseLabelsDataSet.TablePlants.Rows[currentRow].SetField(cellIndex, "False");
            }
            else
            {
                databaseLabelsDataSet.TablePlants.Rows[currentRow].SetField(cellIndex, "True");
            }
            try
                {
                    tablePlantsTableAdapter.Update(databaseLabelsDataSet.TablePlants);
                    MessageBox.Show("Updated Database Entry");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to update to Database - " + ex);
                }
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
            changeFlag(18);
        }

        private void buttonAGMStatus_Click(object sender, EventArgs e)
        {
            string[] defaults = getDefaultSettings();
            if (buttonAGMStatus.Text == "AGM")
            {
                buttonAGMStatus.Text = "no AGM";
                buttonAGMStatus.BackColor = Color.FromName(defaults[15]);
            }
            else
            {
                buttonAGMStatus.Text = "AGM";
                buttonAGMStatus.BackColor = Color.FromName(defaults[17]);
            }
            colourStatusButtons();
            changeFlag(16);
        }

        private void buttonLableStocks_Click(object sender, EventArgs e)
        {
            string[] defaults = getDefaultSettings();
            if (buttonLableStocks.Text == "Labels")
            {
                buttonLableStocks.Text = "no Labels";
                buttonLableStocks.BackColor = Color.FromName(defaults[17]);
            }
            else
            {
                buttonLableStocks.Text = "Labels";
                buttonLableStocks.BackColor = Color.FromName(defaults[15]);
            }
            colourStatusButtons();
            changeFlag(20);
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
            fillPrinterDetails();
            TempMakeALabel(panelLabelPreview, "Choice", "database","");
            changeButtonColours();
            
        }

        private void fillLabelCombo()
        {
            string getName = "";
            comboBoxLabelName.Items.Clear();
            comboBoxAutoLabelName.Items.Clear();
            LabelsLabelNamesTableAdapter.Fill(databaseLabelsDataSetLabelNames.LabelsLabelNames);

            //DataRow dRow = databaseLabelsDataSetDefaults.Tables["Defaults"].Rows[0];
            for (int i = 0; i <= (databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].Rows.Count - 1); i++)
            {
                DataRow dRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].Rows[i];
                getName = dRow.ItemArray[1].ToString();
                comboBoxLabelName.Items.Add(getName);
                comboBoxAutoLabelName.Items.Add(getName);
                
            }
            string[] defaults = getDefaultSettings();
            comboBoxAutoLabelName.Text = defaults[11];

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


        private void selectByJustFillMethod()
        {
            tablePlantsTableAdapter.Fill(databaseLabelsDataSet.TablePlants);
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[0].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            indexNavigationButtons();
            updateMainDetails(0);
            
        }

        private void selectByFillMethod(Boolean hidden)
        {
            tablePlantsTableAdapter.FillBy(databaseLabelsDataSet.TablePlants, hidden);
            dataGridViewPlants.CurrentCell = dataGridViewPlants.Rows[0].Cells[1];
            dataGridViewPlants.FirstDisplayedCell = dataGridViewPlants.CurrentCell;
            dataGridViewPlants.Refresh();
            indexNavigationButtons();
            updateMainDetails(0);
        }

        

        private void buttonHiddenOnly_Click(object sender, EventArgs e)
        {
            selectByFillMethod(true);
            string[] defaults = getDefaultSettings();
            buttonHiddenOnly.BackColor = Color.FromName(defaults[17]);
            buttonVisibleOnly.BackColor = Color.Transparent;
            buttonAllEntries.BackColor = Color.Transparent;
        }

        private void buttonVisibleOnly_Click(object sender, EventArgs e)
        {
            selectByFillMethod(false);
            string[] defaults = getDefaultSettings();
            buttonHiddenOnly.BackColor = Color.Transparent;
            buttonVisibleOnly.BackColor = Color.FromName(defaults[17]);
            buttonAllEntries.BackColor = Color.Transparent;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            selectByJustFillMethod();
            string[] defaults = getDefaultSettings();
            buttonHiddenOnly.BackColor = Color.Transparent;
            buttonVisibleOnly.BackColor = Color.Transparent;
            buttonAllEntries.BackColor = Color.FromName(defaults[17]);
        }
        #endregion

        #region Updating the screen with information

        public void updateMainDetails(int indexOfRow)
        {

            //get all labels
            fillLabelCombo();

            //get picture position
            string[] defaultsString = getDefaultSettings();
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
            //Ok as screen measure
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

            //ID's for updates from other pages
            labelRealID.Text = dataGridViewPlants.Rows[indexOfRow].Cells[0].Value.ToString();
            labelGridID.Text = indexOfRow.ToString();


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
            addAddressQueueTotal();
            addPassportQueueTotal();
            if (String.IsNullOrEmpty(comboBoxLabelName.Text.Trim()))
            {
                TempMakeALabel(panelLabelPreview, "Main", "database", "");
            }
            else
            {
                TempMakeALabel(panelLabelPreview, "Choice", "database", "");
            }

            //set profile details
            string profileName = dataGridViewPlants.Rows[indexOfRow].Cells[17].Value.ToString().Trim();

            buttonMainProfile.Text = profileName;

            DataTable table = databaseLabelsDataSetProfiles.Tables["TableProfiles"];
            string expression;
            expression = "Name = '" + profileName + "'";
            DataRow[] foundRows;
            // Use the Select method to find all rows matching the filter.
            foundRows = table.Select(expression);

            buttonMainProfile.ForeColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(foundRows[0][6].ToString())); // Font Colour
            buttonMainProfile.FlatStyle = FlatStyle.Flat;
            buttonMainProfile.FlatAppearance.BorderSize = 2;
            buttonMainProfile.FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(foundRows[0][2].ToString())); // Border Colour
            buttonMainProfile.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(foundRows[0][7].ToString())); // Back Colour

            //find this

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


        private void assignQueueTotals()
        {
            labelMainCount.Text = addMainQueueTotal().ToString();
            labelMainCountQ.Text = labelMainCount.Text;
            labelColourCount.Text = addColourQueueTotal().ToString();
            labelColourCountQ.Text = labelColourCount.Text;
            textBoxAddressCount.Text = addAddressQueueTotal().ToString();
            textBoxPassportCount.Text = addPassportQueueTotal().ToString();
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
            
            //check if count is a multiple of labels per sheet
            string[] defaultsString = getDefaultSettings();
            string name = defaultsString[3];
            
            if (tabControlQueue.SelectedTab == tabPageColourQueue)
            {
                string tempName = comboBoxLabelName.Text.ToString().TrimEnd();
                if (tempName != "") { name = tempName; }
            }
            string[] headerData = returnLabelHeaderData(name);
            int perSheet = int.Parse(headerData[3]) * int.Parse(headerData[4]);
            float division = (float)count / (float)perSheet;


            if (division == (int)division) { labelColourCount.ForeColor = Color.Black; labelColourCountQ.ForeColor = Color.Black; }
            else { labelColourCount.ForeColor = Color.Firebrick; labelColourCountQ.ForeColor=Color.Firebrick; }

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
            
            //check if Main count is a multiple of labels per sheet
            string[] defaultsString = getDefaultSettings();
            string name = defaultsString[2];

            if (tabControlQueue.SelectedTab == tabPageMainQueue)
            {
                string tempName = comboBoxLabelName.Text.ToString().Trim();
                if (tempName != "") { name = tempName; }
            }
            string[] headerData = returnLabelHeaderData(name);
            int perSheet = int.Parse(headerData[3]) * int.Parse(headerData[4]);
            float division = (float)count / (float)perSheet;


            if (division == (int)division) { labelMainCount.ForeColor = Color.Black; labelMainCountQ.ForeColor = Color.Black; }
            else { labelMainCount.ForeColor = Color.Firebrick; labelMainCountQ.ForeColor = Color.Firebrick; }

            return count;
        }

        private int addAddressQueueTotal()
        {
            int count = 0;
            int Qvalue = 0;

            for (int i = 0; i < (dataGridViewAddressQ.RowCount - 1); i++)
            {
                Qvalue = int.Parse(dataGridViewAddressQ.Rows[i].Cells[1].Value.ToString());
                count = count + Qvalue;
            }

            //check if Address count is a multiple of labels per sheet
            string[] defaultsString = getDefaultSettings();
            string name = defaultsString[2];

            if (tabControlQueue.SelectedTab == tabPageAddresses)
            {
                string tempName = comboBoxLabelName.Text.ToString().Trim();
                if (tempName != "") { name = tempName; }
            }
            string[] headerData = returnLabelHeaderData(name);
            int perSheet = int.Parse(headerData[3]) * int.Parse(headerData[4]);
            float division = (float)count / (float)perSheet;


            if (division == (int)division) { textBoxAddressCount.ForeColor = Color.Black; }
            else { textBoxAddressCount.ForeColor = Color.Firebrick; }

            return count;
        }

        private int addPassportQueueTotal()
        {
            int count = 0;
            int Qvalue = 0;

            for (int i = 0; i < (dataGridViewPassportQ.RowCount - 1); i++)
            {
                Qvalue = int.Parse(dataGridViewPassportQ.Rows[i].Cells[1].Value.ToString());
                count = count + Qvalue;
            }

            //check if Passport count is a multiple of labels per sheet
            string[] defaultsString = getDefaultSettings();
            string name = defaultsString[2];

            if (tabControlQueue.SelectedTab == tabPagePassports)
            {
                string tempName = comboBoxLabelName.Text.ToString().Trim();
                if (tempName != "") { name = tempName; }
            }
            string[] headerData = returnLabelHeaderData(name);
            int perSheet = int.Parse(headerData[3]) * int.Parse(headerData[4]);
            float division = (float)count / (float)perSheet;


            if (division == (int)division) { textBoxPassportCount.ForeColor = Color.Black; }
            else { textBoxPassportCount.ForeColor = Color.Firebrick; }

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
            TempMakeALabel(panelDatabaseMain, "Main", "database","");
            TempMakeALabel(panelDatabaseColour, "Colour", "database","");

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
            string[] defaultsString = getDefaultSettings(); 
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
                comboBoxProfilePick.Items.Add(dataGridView1ProfileView.Rows[i].Cells[1].Value.ToString());
            }

            //FillTogles
            string colourTrue = defaultsString[15];
            string colourFalse = defaultsString[17];
            ButtonData10.Text = dataGridViewPlants.Rows[indexOfRow].Cells[10].Value.ToString();
            if (ButtonData10.Text == "True")
            {
                ButtonData10.BackColor = Color.FromName(colourFalse);
            }
            else
            {
                ButtonData10.BackColor = Color.FromName(colourTrue);
            }
            ButtonData16.Text = dataGridViewPlants.Rows[indexOfRow].Cells[16].Value.ToString();
            if (ButtonData16.Text == "True")
            {
                ButtonData16.BackColor = Color.FromName(colourFalse);
            }
            else
            {
                ButtonData16.BackColor = Color.FromName(colourTrue);
            }
            ButtonData18.Text = dataGridViewPlants.Rows[indexOfRow].Cells[18].Value.ToString();
            if (ButtonData18.Text == "True")
            {
                ButtonData18.BackColor = Color.FromName(colourTrue);
            }
            else
            {
                ButtonData18.BackColor = Color.FromName(colourFalse);
            }
            ButtonData20.Text = dataGridViewPlants.Rows[indexOfRow].Cells[20].Value.ToString();
            if (ButtonData20.Text == "True")
            {
                ButtonData20.BackColor = Color.FromName(colourFalse);
            }
            else
            {
                ButtonData20.BackColor = Color.FromName(colourTrue);
            }
        }

        #endregion

        #region Buttons - like make a clean entry, Add and Update

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

        private void buttonDeleteDatabase_Click(object sender, EventArgs e)
        {

            DialogResult result = MessageBox.Show("Do you really want to DELETE this Entry permanently", "Delete Database Entry", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                int rowToRemove = dataGridViewPlants.CurrentCell.RowIndex;
                dataGridViewPlants.Rows.RemoveAt(rowToRemove);

                try
                {
                    tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                    if (rowToRemove > 0)
                    {
                        updateMainDetails(rowToRemove - 1);
                    }
                    else
                    {
                        updateMainDetails(rowToRemove);
                    }
                    tabControlMain.SelectedTab = tabPageManual;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to delete from Database - " + ex);
                }
            }
        }

        private void buttonUpdateDatabase_Click(object sender, EventArgs e)
        {
            if (validateDatabase())
            {
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
                string[] defaults = getDefaultSettings();
                selectByJustFillMethod();                   
                buttonHiddenOnly.BackColor = Color.Transparent;
                buttonVisibleOnly.BackColor = Color.FromName(defaults[17]);
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
                for (int i = rowSeek; i < dataGridViewPlants.RowCount; i++)
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

        #endregion

        #region Toggles and comboBoxes for changing entry details

        private void ButtonData10_Click(object sender, EventArgs e)
        {
            string[] defaults = getDefaultSettings();
            //Toggle Add to Colour Queue status
            if (ButtonData10.Text == "True")
            {
                ButtonData10.Text = "False";
                ButtonData10.BackColor = Color.FromName(defaults[15]);
            }
            else
            {
                ButtonData10.Text = "True";
                ButtonData10.BackColor = Color.FromName(defaults[17]);
            }
        }

        private void ButtonData20_Click(object sender, EventArgs e)
        {
            string[] defaults = getDefaultSettings();
            // Toggle Label Stocks status
            if (ButtonData20.Text == "True")
            {
                ButtonData20.Text = "False";
                ButtonData20.BackColor = Color.FromName(defaults[15]);
            }
            else
            {
                ButtonData20.Text = "True";
                ButtonData20.BackColor = Color.FromName(defaults[17]);
            }
        }

        private void ButtonData16_Click(object sender, EventArgs e)
        {
            string[] defaults = getDefaultSettings();
            //Toggle AGM status
            if (ButtonData16.Text == "True")
            {
                ButtonData16.Text = "False";
                ButtonData16.BackColor = Color.FromName(defaults[15]);
            }
            else
            {
                ButtonData16.Text = "True";
                ButtonData16.BackColor = Color.FromName(defaults[17]);
            }
        }

        private void ButtonData18_Click(object sender, EventArgs e)
        {
            string[] defaults = getDefaultSettings();
            //Toggle Hidden/Visible entry
            if (ButtonData18.Text == "True")
            {
                ButtonData18.Text = "False";
                ButtonData18.BackColor = Color.FromName(defaults[17]);
            }
            else
            {
                ButtonData18.Text = "True";
                ButtonData18.BackColor = Color.FromName(defaults[15]);
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
            int labelWidth = (int)double.Parse(labelData[0]);
            int labelHeight = (int)double.Parse(labelData[1]);
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

            Graphics formGraphics = panel1.CreateGraphics();

            //add agm if not correct 
            int n = 19;
            string AGMString = queueData[n];
            switch (AGMString)
            {
                case "0":
                    queueData[n] = "AGMblank.ico";
                    break;
                case "1":
                    queueData[n] = "AGM.ico";
                    break;
                default :
                    queueData[n] = "AGMblank.ico";
                    break;
            }

            whereToNow whereToTwo = new whereToNow(queueData, labelData, defaultsString, finalWidthInt, finalHeightInt,0,0,0,0, "screen", formGraphics );
            whereToTwo.BackColor = Color.White;

            whereToTwo.Width = finalWidthInt;
            whereToTwo.Height = finalHeightInt;

            whereToTwo.Location = new Point(2, 2);
            whereToTwo.BorderStyle = BorderStyle.FixedSingle;

            whichPanel.Controls.Add(whereToTwo);
            formGraphics.Dispose();
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

            groupBoxProfiles.Controls.Clear();
            groupBoxProfiles.Controls.Add(flowLayoutPanelProfiles);


            int profileIndex = 0;
            tableProfilesTableAdapter.Fill(databaseLabelsDataSetProfiles.TableProfiles);
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
                ProfileSample[profileIndex].Width = 116;
                ProfileSample[profileIndex].Height = 30;
                ProfileSample[profileIndex].BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[7]));
                ProfileSample[profileIndex].ForeColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[6]));
                ProfileSample[profileIndex].FlatStyle = FlatStyle.Flat;
                ProfileSample[profileIndex].FlatAppearance.BorderSize = 3;
                ProfileSample[profileIndex].FlatAppearance.BorderColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(rowString[2]));

                flowLayoutPanelProfiles.Controls.Add(ProfileSample[profileIndex]);

                //Dispose();
                profileIndex++;
            }
        }

        private void addProfilePicture(string whichProfile)
        {
            if (whichProfile == "database")
            {
                TempMakeALabel(panelProfilePlantPreview, "Colour", "database", "");
            }
            else
            {
                TempMakeALabel(panelProfilePlantPreview, "Colour", "database", dataGridView1ProfileView.CurrentRow.Cells[1].Value.ToString());
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
            
            
            string[] defaultsString = getDefaultSettings(); 
            string filePlace = defaultsString[0];

            for (int i = 1; i <= 4; i++)
            {
                TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + (i + 20).ToString()];
                PictureBox curPictureBox = (PictureBox)panelQueueUtilities.Controls["pictureBoxQ" + (i).ToString()];

                //Picture images
                try // #1
                {
                    string fileName = curText.Text.ToString().Trim();
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
            TempMakeALabel(panelQMain, "Main", "queue","");
            TempMakeALabel(panelQColour, "Colour", "queue","");
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

                    for (int i = 1; i <= 34
                        ; i++)
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
            else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageAddresses")
            {
                if (dataGridViewAddressQ.RowCount > 1)
                {
                    int indexOfRow = dataGridViewAddressQ.CurrentRow.Index;
                    textBoxQ0.Text = indexOfRow.ToString();
                    //Fill in Plant Name

                    for (int i = 1; i <= 34; i++)
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        curText.Text = dataGridViewAddressQ.Rows[indexOfRow].Cells[i - 1].Value.ToString();
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
            else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPagePassports")
            {
                if (dataGridViewPassportQ.RowCount > 1)
                {
                    int indexOfRow = dataGridViewPassportQ.CurrentRow.Index;
                    textBoxQ0.Text = indexOfRow.ToString();
                    //Fill in Plant Name

                    for (int i = 1; i <= 34; i++)
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        curText.Text = dataGridViewPassportQ.Rows[indexOfRow].Cells[i - 1].Value.ToString();
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

                    for (int i = 1; i <= 34; i++)
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
            for (int i = 0; i <= 34; i++)
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
            setQueueQuantity(int.Parse(textBoxQtyToSame.Text.ToString()),"visible","Main");
        }
        private void setQueueQuantity(int howMany,string visibleOrSpecified,string specified)
        { 
            if (( howMany <= 250))
            {
                if (visibleOrSpecified == "visible")
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
                        labelColourCountQ.Text = labelColourCount.Text;
                    }
                    else if (tabControlQueue.SelectedTab.Name == "tabPageAddresses")
                    {
                        for (int i = 0; i < databaseLabelsDataSet3.TableAddressQueue.Rows.Count; i++)
                        {
                            databaseLabelsDataSet3.TableAddressQueue.Rows[i].SetField(2, textBoxQtyToSame.Text.ToString());
                        }
                        try
                        {
                            tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("Failed to update Address Queue - " + ex);
                        }
                        textBoxAddressCount.Text = addAddressQueueTotal().ToString();

                    }
                    else if (tabControlQueue.SelectedTab.Name == "tabPagePassports")
                    {
                        for (int i = 0; i < databaseLabelsDataSet4.TablePassportQueue.Rows.Count; i++)
                        {
                            databaseLabelsDataSet4.TablePassportQueue.Rows[i].SetField(2, textBoxQtyToSame.Text.ToString());
                        }
                        try
                        {
                            tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("Failed to update Passport Queue - " + ex);
                        }
                        textBoxPassportCount.Text = addPassportQueueTotal().ToString();

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
                        labelMainCountQ.Text = labelMainCount.Text;
                    }
                }
                else
                {
                    if (specified == "Colour")
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
                        labelColourCountQ.Text = labelColourCount.Text;
                    }
                    else if (specified == "Address")
                    {
                        for (int i = 0; i < databaseLabelsDataSet3.TableAddressQueue.Rows.Count; i++)
                        {
                            databaseLabelsDataSet3.TableAddressQueue.Rows[i].SetField(2, textBoxQtyToSame.Text.ToString());
                        }
                        try
                        {
                            tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("Failed to update Address Queue - " + ex);
                        }
                        textBoxAddressCount.Text = addAddressQueueTotal().ToString();

                    }
                    else if (specified == "Passport")
                    {
                        for (int i = 0; i < databaseLabelsDataSet4.TablePassportQueue.Rows.Count; i++)
                        {
                            databaseLabelsDataSet4.TablePassportQueue.Rows[i].SetField(2, textBoxQtyToSame.Text.ToString());
                        }
                        try
                        {
                            tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("Failed to update Passport Queue - " + ex);
                        }
                        textBoxPassportCount.Text = addPassportQueueTotal().ToString();

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
                        labelMainCountQ.Text = labelMainCount.Text;
                    }
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
                if (n < 1 || n > 250)
                {
                    MessageBox.Show("Quantity should be between 1 and 250", "Update failed");
                    valid = false;
                }
            }
            else
            {
                MessageBox.Show("Quantity isn't a valid Number (should be an Integer between 0 and 250)", "Update Failed");
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
                    int j = 0; // to take into account rows not lining up with boxes as they should
                    for (int i = 1; i <= 34; i++) //move through textboxes and update appropriate column
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        string changeText = curText.Text.ToString().Trim();
                        //messing about to align database and textboxes
                            j = i;
                            if (i == 26) { j = 36; }
                            if (i > 26) { j = i - 1; }
                        //MessageBox.Show("i = " + i.ToString() + " . " + changeText + " . " + j.ToString());
                        databaseLabelsDataSetMainQueue.TableMainQueue.Rows[indexOfRow].SetField(j, changeText);
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
                    labelMainCountQ.Text = labelMainCount.Text;
                }
                else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageAddresses")
                {
                    int j = 0; // to take into account rows not lining up with boxes as they should
                    for (int i = 1; i <= 34; i++) //move through textboxes and update appropriate column
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        string changeText = curText.Text.ToString().Trim();
                        
                        //messing about to align database and textboxes
                        j = i;
                        //if (i == 26) { j = 36; }
                        //if (i > 26) { j = i - 1; }
                        //MessageBox.Show("i = " + i.ToString() + " . " + changeText + " . " + j.ToString());
                        databaseLabelsDataSet3.TableAddressQueue.Rows[indexOfRow].SetField(j, changeText);
                    }

                    try
                    {
                        tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update to Address Queue - " + ex);
                    }

                    textBoxAddressCount.Text = addAddressQueueTotal().ToString(); //updates a quantity count on screen
                }
                else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPagePassports")
                {
                    int j = 0; // to take into account rows not lining up with boxes as they should
                    for (int i = 1; i <= 34; i++) //move through textboxes and update appropriate column
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        string changeText = curText.Text.ToString().Trim();
                        //MessageBox.Show("i = " + i.ToString() + " . " + databaseLabelsDataSetPassportQueue.TablePassportQueue.Rows[indexOfRow]);
                        //messing about to align database and textboxes
                        j = i;
                        //if (i == 26) { j = 36; }
                        //if (i > 26) { j = i - 1; }
                        //MessageBox.Show("i = " + i.ToString() + " . " + changeText + " . " + j.ToString());
                        databaseLabelsDataSet4.TablePassportQueue.Rows[indexOfRow].SetField(j, changeText);
                    }

                    try
                    {
                        tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update to Passport Queue - " + ex);
                    }

                    textBoxPassportCount.Text = addPassportQueueTotal().ToString(); //updates a quantity count on screen
                }
                else
                {
                    int j = 0; // to take into account rows not lining up with boxes as they should
                    for (int i = 1; i <= 34; i++) //move through textboxes and update appropriate column
                    {
                        TextBox curText = (TextBox)panelQueueUtilities.Controls["textBoxQ" + i.ToString()];
                        string changeText = curText.Text.ToString();
                        //messing about to align database and textboxes
                            j = i;
                            if (i == 26) { j = 36; }
                            if (i > 26) { j = i + 1; }
                        databaseLabelsDataSetColourQueue.TableColourQueue.Rows[indexOfRow].SetField(j, changeText);
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
                    labelColourCountQ.Text = labelColourCount.Text;
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
            else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageAddresses")
            {
                int indexOfRow = dataGridViewAddressQ.CurrentRow.Index;
                if (indexOfRow < dataGridViewAddressQ.RowCount - 2)
                {
                    dataGridViewAddressQ.CurrentCell = dataGridViewAddressQ.Rows[indexOfRow + 1].Cells[0];
                    fillQueueUtilitiesTab();
                }
            }
            else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPagePassports")
            {
                int indexOfRow = dataGridViewPassportQ.CurrentRow.Index;
                if (indexOfRow < dataGridViewPassportQ.RowCount - 2)
                {
                    dataGridViewPassportQ.CurrentCell = dataGridViewPassportQ.Rows[indexOfRow + 1].Cells[0];
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
            else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPageAddresses")
            {
                int indexOfRow = dataGridViewAddressQ.CurrentRow.Index;
                if (indexOfRow > 0)
                {
                    dataGridViewAddressQ.CurrentCell = dataGridViewAddressQ.Rows[indexOfRow - 1].Cells[0];
                    fillQueueUtilitiesTab();
                }
            }
            else if (tabControlQueue.SelectedTab.Name.ToString() == "tabPagePassports")
            {
                int indexOfRow = dataGridViewPassportQ.CurrentRow.Index;
                if (indexOfRow > 0)
                {
                    dataGridViewPassportQ.CurrentCell = dataGridViewPassportQ.Rows[indexOfRow - 1].Cells[0];
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
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewMainQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


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
            else if (tabControlQueue.SelectedTab == tabPageAddresses)
            {
                int rowToMove = dataGridViewAddressQ.CurrentRow.Index;
                int minRow = 1;
                if (rowToMove >= minRow)
                {
                    DataRow rowData = databaseLabelsDataSet3.TableAddressQueue.NewRow();
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewAddressQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


                    dataGridViewAddressQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSet3.TableAddressQueue.Rows.InsertAt(rowData, rowToMove - 1);
                    dataGridViewAddressQ.EndEdit();

                    try
                    {
                        tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
                        //MessageBox.Show("Succeeding in deleting from Colour Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Colour Queue - " + ex);
                    }
                    dataGridViewAddressQ.CurrentCell = dataGridViewAddressQ[0, rowToMove - 1];
                    dataGridViewAddressQ.Rows[rowToMove - 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewAddressQ.CurrentRow.Index.ToString();
                }
            }
            else if (tabControlQueue.SelectedTab == tabPagePassports)
            {
                int rowToMove = dataGridViewPassportQ.CurrentRow.Index;
                int minRow = 1;
                if (rowToMove >= minRow)
                {
                    DataRow rowData = databaseLabelsDataSet4.TablePassportQueue.NewRow();
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewPassportQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


                    dataGridViewPassportQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSet4.TablePassportQueue.Rows.InsertAt(rowData, rowToMove - 1);
                    dataGridViewPassportQ.EndEdit();

                    try
                    {
                        tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
                        //MessageBox.Show("Succeeding in deleting from Passport Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Passport Queue - " + ex);
                    }
                    dataGridViewPassportQ.CurrentCell = dataGridViewPassportQ[0, rowToMove - 1];
                    dataGridViewPassportQ.Rows[rowToMove - 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewPassportQ.CurrentRow.Index.ToString();
                }
            }
            else
            {
                int rowToMove = dataGridViewColourQ.CurrentRow.Index;
                int minRow = 1;
                if (rowToMove >= minRow)
                {
                    DataRow rowData = databaseLabelsDataSetColourQueue.TableColourQueue.NewRow();
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewColourQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


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
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewMainQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


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
            else if (tabControlQueue.SelectedTab == tabPageAddresses)
            {
                int rowToMove = dataGridViewAddressQ.CurrentRow.Index;
                int maxRow = dataGridViewAddressQ.Rows.Count - 2;
                if (rowToMove < maxRow)
                {
                    DataRow rowData = databaseLabelsDataSet3.TableAddressQueue.NewRow();
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewAddressQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


                    dataGridViewAddressQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSet3.TableAddressQueue.Rows.InsertAt(rowData, rowToMove + 2);
                    dataGridViewAddressQ.EndEdit();

                    try
                    {
                        tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
                        //MessageBox.Show("Succeeding in deleting from Colour Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Colour Queue - " + ex);
                    }
                    dataGridViewAddressQ.CurrentCell = dataGridViewAddressQ[0, rowToMove + 1];
                    dataGridViewAddressQ.Rows[rowToMove + 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewAddressQ.CurrentRow.Index.ToString();
                }
            }
            else if (tabControlQueue.SelectedTab == tabPagePassports)
            {
                int rowToMove = dataGridViewPassportQ.CurrentRow.Index;
                int maxRow = dataGridViewPassportQ.Rows.Count - 2;
                if (rowToMove < maxRow)
                {
                    DataRow rowData = databaseLabelsDataSet4.TablePassportQueue.NewRow();
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewPassportQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


                    dataGridViewPassportQ.Rows.RemoveAt(rowToMove);
                    databaseLabelsDataSet4.TablePassportQueue.Rows.InsertAt(rowData, rowToMove + 2);
                    dataGridViewPassportQ.EndEdit();

                    try
                    {
                        tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
                        //MessageBox.Show("Succeeding in deleting from Colour Queue");
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to move line in Colour Queue - " + ex);
                    }
                    dataGridViewPassportQ.CurrentCell = dataGridViewPassportQ[0, rowToMove + 1];
                    dataGridViewPassportQ.Rows[rowToMove + 1].Cells[0].Selected = true;

                    textBoxQ0.Text = dataGridViewPassportQ.CurrentRow.Index.ToString();
                }
            }
            else
            {
                int rowToMove = dataGridViewColourQ.CurrentRow.Index;
                int maxRow = dataGridViewColourQ.Rows.Count - 2;
                if (rowToMove < maxRow)
                {
                    DataRow rowData = databaseLabelsDataSetColourQueue.TableColourQueue.NewRow();
                    string[] allTheData = new string[37];
                    for (int i = 0; i <= 35; i++)
                    {
                        allTheData[i] = dataGridViewColourQ.CurrentRow.Cells[i].Value.ToString().Trim();
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
                    rowData["ShipName"] = allTheData[25];
                    rowData["ShipFirst"] = allTheData[26];
                    rowData["ShipLast"] = allTheData[27];
                    rowData["ShipLine1"] = allTheData[28];
                    rowData["ShipLine2"] = allTheData[29];
                    rowData["ShipCity"] = allTheData[30];
                    rowData["ShipState"] = allTheData[31];
                    rowData["ShipPostcode"] = allTheData[32];
                    rowData["OrderNotes"] = allTheData[33];
                    rowData["LabelStocks"] = allTheData[34];
                    rowData["PlantId"] = allTheData[35];


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

        private void changeButtonColours()
        {
            string[] defaults = getDefaultSettings(); 
            Color newColour = new Color();
            Color newColour1 = new Color();
            newColour = Color.FromName(defaults[13]);
            newColour1 = Color.FromName(defaults[16]);
            
            if (tabControlQueue.SelectedTab == tabPageColourQueue)
            {
                if (comboBoxLabelName.Text.Trim() == defaults[3].Trim())
                {
                    newColour = Color.FromName(defaults[14]);
                }
                if (comboBoxAutoLabelName.Text.Trim() == defaults[3].Trim())
                {
                    newColour1 = Color.FromName(defaults[14]);
                }
            }
            else if (tabControlQueue.SelectedTab == tabPageAddresses)
            {
                if (comboBoxLabelName.Text.Trim() == defaults[31].Trim())
                {
                    newColour = Color.FromName(defaults[27]);
                }
                if (comboBoxAutoLabelName.Text.Trim() == defaults[31].Trim())
                {
                    newColour1 = Color.FromName(defaults[27]);
                }
            }
            else if (tabControlQueue.SelectedTab == tabPagePassports)
            {
                if (comboBoxLabelName.Text.Trim() == defaults[32].Trim())
                {
                    newColour = Color.FromName(defaults[29]);
                }
                if (comboBoxAutoLabelName.Text.Trim() == defaults[32].Trim())
                {
                    newColour1 = Color.FromName(defaults[29]);
                }
            }
            else
            {
                if (comboBoxLabelName.Text.Trim() == defaults[2].Trim())
                {
                    newColour = Color.FromName(defaults[13]);
                }
                if (comboBoxAutoLabelName.Text.Trim() == defaults[2].Trim())
                {
                    newColour1 = Color.FromName(defaults[13]);
                }
            }
            buttonPrint.BackColor = newColour;
            buttonAutoPrint.BackColor = newColour;
            button1AutoPrint.BackColor = newColour1;
            labelAutoPrintLabel.Text = comboBoxLabelName.Text;
        }


        private void tabControlQueue_SelectedIndexChanged(object sender, EventArgs e)
        {


            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
            {
                fillQueueUtilitiesTab();
            }

            getLabelName();
            changeButtonColours();
            TempMakeALabel(panelLabelPreview, "Choice", "database", "");

            if (tabControlMain.SelectedTab == tabPagePreview)
            {
                TempMakeALabel(panelLabelTabChoice, "Choice", "database", "");
            }



            //Handle Queue Utility Tabs
            if (tabControlQueue.SelectedTab == tabPageLabelStocks)
            {
                fillLabelStocksGrid();
            }
            if (tabControlQueue.SelectedTab == tabPageMissingPictures)
            {
                fillMissingPicturesGrid();
            }
            if (tabControlQueue.SelectedTab == tabPageComparison)
            {
                fillComparisonTab();
            }
        }

            private void fillComparisonTab()
            {
                listBoxCompareMain.Items.Clear();
                listBoxCompareColour.Items.Clear();

            listBoxCompareMain.Items.Add("");
            listBoxCompareColour.Items.Add("");

            //Look for Items only on the Main Queue
            for (int i = 0; i < dataGridViewMainQ.Rows.Count - 1; i++)
                    {
                        string name = dataGridViewMainQ.Rows[i].Cells[0].Value.ToString();
                        Boolean found = false;
                        for (int j=0;j < dataGridViewColourQ.Rows.Count - 1; j++)
                        {
                            if (name == dataGridViewColourQ.Rows[j].Cells[0].Value.ToString())
                                { found = true; }
                        }
                        if (!found) { listBoxCompareMain.Items.Add("  "+name); }
                    }
            //Look for Items only on the Colour Queue
            for (int i = 0; i < dataGridViewColourQ.Rows.Count - 1; i++)
            {
                string name = dataGridViewColourQ.Rows[i].Cells[0].Value.ToString();
                Boolean found = false;
                for (int j = 0; j < dataGridViewMainQ.Rows.Count - 1; j++)
                {
                    if (name == dataGridViewMainQ.Rows[j].Cells[0].Value.ToString())
                    { found = true; }
                }
                if (!found) { listBoxCompareColour.Items.Add("  "+name); }
            }


        }



        


        #endregion

        #region Label Stocks Tab

        private void fillLabelStocksGrid()
        {
            //clear the grid first
            dataGridViewQueueList.SelectAll();
            foreach (DataGridViewCell oneCell in dataGridViewQueueList.SelectedCells)
            {
                if (oneCell.Selected)
                    dataGridViewQueueList.Rows.RemoveAt(oneCell.RowIndex);
            }
            //then fill it up

            for (int i = 0; i <= (dataGridViewColourQ.RowCount - 2); i++)
            {
                if (dataGridViewColourQ.Rows[i].Cells[34].Value.ToString() == "True")
                {
                    string[] toAdd = new string[] {
                        dataGridViewColourQ.Rows[i].Cells[0].Value.ToString(),
                        "False",
                        "False",
                        dataGridViewColourQ.Rows[i].Cells[35].Value.ToString()};
                    dataGridViewQueueList.Rows.Add(toAdd);
                }
            }
            //count numbers
            labelLabelStocks.Text = dataGridViewQueueList.RowCount.ToString();

            dataGridViewQueueList.Sort(dataGridViewQueueList.Columns[0], System.ComponentModel.ListSortDirection.Ascending);

            for (int i = 0; i < dataGridViewQueueList.RowCount; i++)
            {
                colourRow(i);
            }

            countLabelStocks();
        }
        private void countLabelStocks()
        {
            int stockCount = 0;
            int queueCount = 0;

            for (int i = 0; i <= dataGridViewQueueList.RowCount - 1; i++)
            {
                if (dataGridViewQueueList.Rows[i].Cells[1].Value.ToString() == "True") { stockCount++; }
                if (dataGridViewQueueList.Rows[i].Cells[2].Value.ToString() == "True") { queueCount++; }
            }
            labelLabelStockRemove.Text = stockCount.ToString();
            labelLabelQueueRemove.Text = queueCount.ToString();
        }

        private void countMissingPictures()
        {
            labelMissingPictures.Text = dataGridViewMissingPictures.RowCount.ToString();

            int removeCount = 0;

            for (int i = 0; i <= dataGridViewMissingPictures.RowCount - 1; i++)
            {
                if (dataGridViewMissingPictures.Rows[i].Cells[1].Value.ToString() == "True") { removeCount++; }
            }
            labelRemoveMissing.Text = removeCount.ToString();
        }


        private void initialiseLabelStockGrid()
        {
            dataGridViewQueueList.Columns.Add("Name", "Name");
            dataGridViewQueueList.Columns.Add("Label", "Remove from Stock");
            dataGridViewQueueList.Columns.Add("Remove", "Remove from Queue");
            dataGridViewQueueList.Columns.Add("Id", "Id");

            dataGridViewQueueList.Columns[0].Width = 245;
            dataGridViewQueueList.Columns[1].Width = 50;
            dataGridViewQueueList.Columns[2].Width = 50;
            dataGridViewQueueList.Columns[3].Width = 40;

        }

        private void initialiseMissingPictureGrid()
        {
            dataGridViewMissingPictures.Columns.Add("Name", "Name");
            dataGridViewMissingPictures.Columns.Add("Remove", "Remove from Queue");
            dataGridViewMissingPictures.Columns.Add("Id", "Id");

            dataGridViewMissingPictures.Columns[0].Width = 245;
            dataGridViewMissingPictures.Columns[1].Width = 50;
            dataGridViewMissingPictures.Columns[2].Width = 40;

        }

        private void fillMissingPicturesGrid()
        {
            //clear the grid first
            dataGridViewMissingPictures.SelectAll();
            foreach (DataGridViewCell oneCell in dataGridViewMissingPictures.SelectedCells)
            {
                if (oneCell.Selected)
                    dataGridViewMissingPictures.Rows.RemoveAt(oneCell.RowIndex);
            }
            //then fill it up

            for (int i = 0; i <= (dataGridViewColourQ.RowCount - 2); i++)
            {
                if (String.IsNullOrEmpty(dataGridViewColourQ.Rows[i].Cells[5].Value.ToString().Trim()))
                {
                    string[] toAdd = new string[] {
                        dataGridViewColourQ.Rows[i].Cells[0].Value.ToString(),
                        "True",
                        dataGridViewColourQ.Rows[i].Cells[26].Value.ToString()};
                    dataGridViewMissingPictures.Rows.Add(toAdd);
                }
            }
            dataGridViewMissingPictures.Sort(dataGridViewMissingPictures.Columns[0], System.ComponentModel.ListSortDirection.Ascending);

            countMissingPictures();
        }

        #endregion

        #region  QUEUE UTILITIES - like collecting info from database and adding entires

        private string[] collectQueueRow(int desiredRow, string whichQueue)
        {
            string[] queueEntry = new string[36];
            whichQueue = whichQueue.Trim();

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
            else if (whichQueue == "Address")
            {
                queueEntry[0] = dataGridViewAddressQ.Rows[desiredRow].Cells[0].Value.ToString();
                queueEntry[1] = dataGridViewAddressQ.Rows[desiredRow].Cells[1].Value.ToString();
                queueEntry[2] = dataGridViewAddressQ.Rows[desiredRow].Cells[2].Value.ToString();
                queueEntry[3] = dataGridViewAddressQ.Rows[desiredRow].Cells[7].Value.ToString();
                queueEntry[4] = dataGridViewAddressQ.Rows[desiredRow].Cells[3].Value.ToString();
                queueEntry[5] = dataGridViewAddressQ.Rows[desiredRow].Cells[6].Value.ToString();
                queueEntry[6] = dataGridViewAddressQ.Rows[desiredRow].Cells[4].Value.ToString();
                queueEntry[7] = dataGridViewAddressQ.Rows[desiredRow].Cells[8].Value.ToString();
                queueEntry[8] = dataGridViewAddressQ.Rows[desiredRow].Cells[5].Value.ToString();
            }
            else if (whichQueue == "Passport")
            {
                queueEntry[0] = dataGridViewPassportQ.Rows[desiredRow].Cells[0].Value.ToString();
                queueEntry[1] = dataGridViewPassportQ.Rows[desiredRow].Cells[1].Value.ToString();
                queueEntry[2] = dataGridViewPassportQ.Rows[desiredRow].Cells[2].Value.ToString();
                queueEntry[3] = dataGridViewPassportQ.Rows[desiredRow].Cells[7].Value.ToString();
                queueEntry[4] = dataGridViewPassportQ.Rows[desiredRow].Cells[3].Value.ToString();
                queueEntry[5] = dataGridViewPassportQ.Rows[desiredRow].Cells[6].Value.ToString();
                queueEntry[6] = dataGridViewPassportQ.Rows[desiredRow].Cells[4].Value.ToString();
                queueEntry[7] = dataGridViewPassportQ.Rows[desiredRow].Cells[8].Value.ToString();
                queueEntry[8] = dataGridViewPassportQ.Rows[desiredRow].Cells[5].Value.ToString();
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

            //maybe here
            for (int i = 9; i < 34; i++)
            {
                if (whichQueue == "Main")
                {
                    queueEntry[i] = dataGridViewMainQ.Rows[desiredRow].Cells[i].Value.ToString();
                }
                else if (whichQueue == "Address")
                {
                    queueEntry[i] = dataGridViewAddressQ.Rows[desiredRow].Cells[i].Value.ToString();
                }
                else if (whichQueue == "Passport")
                {
                    queueEntry[i] = dataGridViewPassportQ.Rows[desiredRow].Cells[i].Value.ToString();
                }
                else
                {
                    queueEntry[i] = dataGridViewColourQ.Rows[desiredRow].Cells[i].Value.ToString();
                }
            }
            return queueEntry;
        }

        private void addToQueues(string which)
        {
            string[] queue = CollectQueueEntry();
            doTheAdding(queue,which,"visible","");
        }

        private void doTheAdding(string[] queue, string which, string visibleOrSpecified,string specified)
        {
            if (visibleOrSpecified == "visible")
            {
                if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue")
                {
                    addRowToColourQ(queue, which);
                }
                else if (tabControlQueue.SelectedTab.Name == "tabPageAddresses")
                {
                    addRowToAddressQ(queue, which);
                }
                else if (tabControlQueue.SelectedTab.Name == "tabPagePassports")
                {
                    addRowToPassportQ(queue, which);
                }
                else
                {
                    addRowToMainQ(queue);
                    //if (buttonAddtoColourQueue.Text == "add Colour")
                    if (queue[36] == "add Colour")
                    {
                        if (checkBoxColourAdd.Checked == true)
                        {
                            addRowToColourQ(queue, which);
                        }
                    }
                }
            }
            else
            {
                if (specified == "Colour")
                {
                    addRowToColourQ(queue, which);
                }
                else if (specified == "Address")
                {
                    addRowToAddressQ(queue, which);
                }
                else if (specified == "Passport")
                {
                    addRowToPassportQ(queue, which);
                }
                else
                {
                    addRowToMainQ(queue);
                    //if (buttonAddtoColourQueue.Text == "add Colour")
                    if (queue[36] == "add Colour")
                    {
                        if (checkBoxColourAdd.Checked == true)
                        {
                            addRowToColourQ(queue, which);
                        }
                    }
                }
            }
        }

        private void addRowToMainQ(string[] queue)
        {
            //string[] queue = CollectQueueEntry();
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
            row["LabelStocks"] = queue[25];
            row["PlantId"] = queue[26];
            row["ShipName"] = queue[27];
            row["ShipFirst"] = queue[28];
            row["ShipLast"] = queue[29];
            row["ShipLine1"] = queue[30];
            row["ShipLine2"] = queue[31];
            row["ShipCity"] = queue[32];
            row["ShipState"] = queue[33];
            row["ShipPostcode"] = queue[34];
            row["OrderNotes"] = queue[35];

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
            labelMainCountQ.Text = labelMainCount.Text;
        }

        private void addRowToAddressQ(string[] queue, string which)
        {
            //string[] queue = CollectQueueEntry();
            DataRow row = databaseLabelsDataSet3.Tables[0].NewRow();

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
            row["LabelStocks"] = queue[25];
            row["PlantId"] = queue[26];
            row["ShipName"] = queue[27];
            row["ShipFirst"] = queue[28];
            row["ShipLast"] = queue[29];
            row["ShipLine1"] = queue[30];
            row["ShipLine2"] = queue[31];
            row["ShipCity"] = queue[32];
            row["ShipState"] = queue[33];
            row["ShipPostcode"] = queue[34];
            row["OrderNotes"] = queue[35];

            databaseLabelsDataSet3.TableAddressQueue.Rows.Add(row);
            dataGridViewAddressQ.EndEdit();
            try
            {
                tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to add to Address Queue - " + ex);
            }
            textBoxAddressCount.Text = addAddressQueueTotal().ToString();
            
        }

        private void addRowToPassportQ(string[] queue, string which)
        {
            //string[] queue = CollectQueueEntry();
            DataRow row = databaseLabelsDataSet4.Tables[0].NewRow();

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
            row["LabelStocks"] = queue[25];
            row["PlantId"] = queue[26];
            row["ShipName"] = queue[27];
            row["ShipFirst"] = queue[28];
            row["ShipLast"] = queue[29];
            row["ShipLine1"] = queue[30];
            row["ShipLine2"] = queue[31];
            row["ShipCity"] = queue[32];
            row["ShipState"] = queue[33];
            row["ShipPostcode"] = queue[34];
            row["OrderNotes"] = queue[35];

            databaseLabelsDataSet4.TablePassportQueue.Rows.Add(row);
            dataGridViewPassportQ.EndEdit();
            try
            {
                tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to add to Passport Queue - " + ex);
            }
            textBoxPassportCount.Text = addPassportQueueTotal().ToString();

        }


        private void addRowToColourQ(string[] queue, string which)
        {
            //getColour
            string[] defaults = getDefaultSettings();
            Color colourHalfWay = Color.FromName(defaults[16]);
            Color colourTrue = Color.FromName(defaults[15]);
            
            //string[] queue = CollectQueueEntry();
            DataRow row = databaseLabelsDataSetColourQueue.Tables[0].NewRow();

            //row["Id"] = "1";
            row["Name"] = queue[0];
            int answer = 0;
            int.TryParse(queue[1], out answer);
            //use single quantities for autolabel
            if (which == "autolabel")
            {
                if (checkBoxColorQSingle.Checked) { answer = 1; }
            }
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
            row["LabelStocks"] = queue[25];
            row["PlantId"] = queue[26];
            row["ShipName"] = queue[27];
            row["ShipFirst"] = queue[28];
            row["ShipLast"] = queue[29];
            row["ShipLine1"] = queue[30];
            row["ShipLine2"] = queue[31];
            row["ShipCity"] = queue[32];
            row["ShipState"] = queue[33];
            row["ShipPostcode"] = queue[34];
            row["OrderNotes"] = queue[35];

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
            labelColourCountQ.Text = labelColourCount.Text;
            if (queue[25] == "True") { dataGridViewColourQ.Rows[dataGridViewColourQ.RowCount-2].Cells[0].Style.BackColor=colourHalfWay; }
            Boolean colourRed = false;
            if (string.IsNullOrEmpty(queue[8])) { colourRed = true; }
            try
            {
                Image test = Image.FromFile(defaults[0] + queue[8]);
            }
            catch
            {
                colourRed = true;
            }
                if (colourRed) { dataGridViewColourQ.Rows[dataGridViewColourQ.RowCount - 2].Cells[0].Style.BackColor = colourTrue; }
            }

        public string[] CollectQueueEntry()
        {
            string[] queueData = new string[37];

            string[] defaultsString = getDefaultSettings(); 

            int currentRow = dataGridViewPlants.CurrentCell.RowIndex;
            string[] sendData = new string[21];
            string[] findName = new string[5];
            string[] moreData = new string[2];

            // get general plant data
            for (int i = 0; i <= 20; i++)
            {
                sendData[i] = dataGridViewPlants.Rows[currentRow].Cells[i].Value.ToString().Trim();
            }

            // get various concatenated Name strings 
            for (int i = 0; i <= 4; i++)
            {
                findName[i] = dataGridViewPlants.Rows[currentRow].Cells[1 + i].Value.ToString();
            }
            string[] sendName = getPlantName(findName);

            //get main pcture
            if (radioButtonImage1.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[12].Value.ToString().Trim(); }
            if (radioButtonImage2.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[13].Value.ToString().Trim(); }
            if (radioButtonImage3.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[14].Value.ToString().Trim(); }
            if (radioButtonImage4.Checked) { moreData[0] = dataGridViewPlants.Rows[currentRow].Cells[15].Value.ToString().Trim(); }

            // Check AGM Status
            if (dataGridViewPlants.Rows[currentRow].Cells[16].Value.ToString() == "True")
            {
                moreData[1] = "AGM.ico";
            }
            else
            {
                moreData[1] = "AGMblank.ico";
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
            queueData[25] = sendData[20];
            queueData[26] = sendData[0];
            queueData[36] = buttonAddtoColourQueue.Text;

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


        //Routines that actually delete Queue lines one by one
        #region Delete Buttons


        private void deleteColourQueueLine()
        {
            foreach (DataGridViewCell oneCell in dataGridViewColourQ.SelectedCells)
            {
                if (oneCell.Selected)
                    if (!dataGridViewColourQ.Rows[oneCell.RowIndex].IsNewRow)
                    {
                        dataGridViewColourQ.Rows.RemoveAt(oneCell.RowIndex);
                    }
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
            labelColourCountQ.Text = labelColourCount.Text;
        }

        private void deletePassportQueueLine()
        {
            foreach (DataGridViewCell oneCell in dataGridViewPassportQ.SelectedCells)
            {
                if (oneCell.Selected)
                    if (!dataGridViewPassportQ.Rows[oneCell.RowIndex].IsNewRow)
                    {
                        dataGridViewPassportQ.Rows.RemoveAt(oneCell.RowIndex);
                    }
            }
            try
            {
                tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to delete from Passport Queue - " + ex);
            }
            textBoxPassportCount.Text = addPassportQueueTotal().ToString();

        }

        private void deleteAddressQueueLine()
        {
            foreach (DataGridViewCell oneCell in dataGridViewAddressQ.SelectedCells)
            {
                if (oneCell.Selected)
                    if (!dataGridViewAddressQ.Rows[oneCell.RowIndex].IsNewRow)
                    {
                        dataGridViewAddressQ.Rows.RemoveAt(oneCell.RowIndex);
                    }
            }
            try
            {
                tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to delete from Address Queue - " + ex);
            }
            textBoxAddressCount.Text = addAddressQueueTotal().ToString();
 
        }

        private void deleteMainQueueLine()
        {
            foreach (DataGridViewCell oneCell in dataGridViewMainQ.SelectedCells)
            {
                if (oneCell.Selected)
                    if (!dataGridViewMainQ.Rows[oneCell.RowIndex].IsNewRow)
                    {
                        dataGridViewMainQ.Rows.RemoveAt(oneCell.RowIndex);
                    }
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
            labelMainCountQ.Text = labelMainCount.Text;
        }

        private void deleteQueue(string which)
        {
            
            if (which == "Main Queue" || which == "All")
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
                labelMainCountQ.Text = labelMainCount.Text;
            }

            if (which == "Colour Queue" || which == "All")
            {
                int numRows = databaseLabelsDataSetColourQueue.TableColourQueue.Rows.Count - 1;

                for (int i = 0; i <= numRows; i++)
                {
                    //databaseLabelsDataSetColourQueue.TableColourQueue.Rows.RemoveAt(0);
                    dataGridViewColourQ.Rows.RemoveAt(0);
                }
                
                try
                {
                    tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                    //MessageBox.Show("Succeeding in deleting from Colour Queue");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to delete from Colour Queue - " + ex);
                }
                dataGridViewColourQ.EndEdit();
                labelColourCount.Text = addColourQueueTotal().ToString();
                labelColourCountQ.Text = labelColourCount.Text;
            }

            if (which == "Addresses Queue" || which == "All")
            {
                int numRows = databaseLabelsDataSet3.TableAddressQueue.Rows.Count - 1;

                for (int i = 0; i <= numRows; i++)
                {
                    //databaseLabelsDataSetAddressesQueue.TableAddressesQueue.Rows.RemoveAt(0);
                    dataGridViewAddressQ.Rows.RemoveAt(0);
                }
                dataGridViewAddressQ.EndEdit();
                try
                {
                    tableAddressQueueTableAdapter.Update(databaseLabelsDataSet3.TableAddressQueue);
                    //MessageBox.Show("Succeeding in deleting from Addresses Queue");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to delete from Addresses Queue - " + ex);
                }
                textBoxAddressCount.Text = addAddressQueueTotal().ToString();
                
            }

            if (which == "Passports Queue" || which == "All")
            {
                int numRows = databaseLabelsDataSet4.TablePassportQueue.Rows.Count - 1;

                for (int i = 0; i <= numRows; i++)
                {
                    //databaseLabelsDataSetPassportQueue.TablePassportQueue.Rows.RemoveAt(0);
                    dataGridViewPassportQ.Rows.RemoveAt(0);
                }
                dataGridViewPassportQ.EndEdit();
                try
                {
                    tablePassportQueueTableAdapter.Update(databaseLabelsDataSet4.TablePassportQueue);
                    //MessageBox.Show("Succeeding in deleting from Passport Queue");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to delete from Passport Queue - " + ex);
                }
                textBoxPassportCount.Text = addPassportQueueTotal().ToString();
                
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
                DialogResult result = MessageBox.Show("Do you want to Delete all selected lines from the Colour Queue", "Colour Queue Line Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes) { deleteColourQueueLine(); }
            }
            else if (tabControlQueue.SelectedTab.Name == "tabPageAddresses")
            {
                DialogResult result = MessageBox.Show("Do you want to Delete all selected lines from the Address Queue", "Address Queue Line Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                { deleteAddressQueueLine(); }
            }
            else if (tabControlQueue.SelectedTab.Name == "tabPagePassports")
            {
                DialogResult result = MessageBox.Show("Do you want to Delete all selected lines from the Passport Queue", "Passport Queue Line Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                { deletePassportQueueLine(); }
            }
            else 
            {
                DialogResult result = MessageBox.Show("Do you want to Delete all selected lines from the Main Queue", "Main Queue Line Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                { deleteMainQueueLine(); }
            }
        }

        private void buttonDeleteThisQueue_Click(object sender, EventArgs e)
        {
            string which = "Main Queue";
            if (tabControlQueue.SelectedTab == tabPageColourQueue) { which = "Colour Queue"; }
            if (tabControlQueue.SelectedTab == tabPageAddresses) { which = "Addresses Queue"; }
            if (tabControlQueue.SelectedTab == tabPagePassports) { which = "Passports Queue"; }
            DialogResult result = MessageBox.Show("Do you want to Delete all entries from the " + which, "Delete " + which, MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            { deleteQueue(which); }
        }

        private void buttonDeleteBothQueues_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to Delete all entries from All Four Queues", "Delete All Queues", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            { deleteQueue("All"); }
        }
        #endregion

        #endregion

        #endregion

        #region *** Quick Print Tab routines ***
        private void fillQuickPrint()
        {
            DataTable quickNames = new DataTable("quickNames");
            quickNames = LabelsLabelNamesTableAdapter.GetDataByQuickPrint(true);

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
            for (int k = 0; k <= (noRows - 1); k++)
            {
                DataRow dRow = quickNames.Rows[k];
                allTheNames[k] = dRow.ItemArray[1].ToString();
            }

            quickNames.Dispose();

            for (int j = 0; j <= (noRows - 1); j++)
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

                TempMakeALabel(curPanelQP, curGroupQP.Text, "database","");

            }
        }
        #endregion

        #region *** Label Stuff *** - routines to get Label Name and find the information needed and TempMakeALabel

        public void TempMakeALabel(Panel whichPanel, string whichLabel, string DatabaseOrQueue, string whichProfile)
        {
            //must be here somewhere
            string[] defaultsString = getDefaultSettings(); 
            string[] queueString = new string[36];

            if (DatabaseOrQueue == "database")
            {
                
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
                    moreData[1] = "AGMblank.ico";
                }

                // qty and price

                moreData[2] = textBoxQty.Text;
                moreData[3] = formatPrice(textBoxPrice.Text);

                // customer and Order NUmber
                moreData[4] = textBoxCustomerName.Text;
                moreData[5] = textBoxOrderNumber.Text;

                // profile
                string profileName = dataGridViewPlants.Rows[currentRow].Cells[17].Value.ToString();
                if (whichProfile != "") { profileName = whichProfile; }

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
                for (int i = 0; i <= 33; i++)
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
                labelChoice = defaultsString[3];
            }
            else if (whichLabel == "Main")
            {
                //the main queue default
                labelChoice = defaultsString[2];
            }
            else
            {
                //the labels sent as an arguement
                labelChoice = whichLabel;
            }

            String[] getLabelString = new String[1];
            if (whichLabel == "Design")
            {
                getLabelString = CollectDesignLabelData();

                queueString[2] = "£5.50";
                queueString[4] = "Sample Customer";
                queueString[19] = "\\AGMcircle.ico";
                queueString[24] = "Order No. #12345";
                queueString[34] = "Shipping Company";
            }
            else
            {
                String[] headerString = returnLabelHeaderData(labelChoice);
                getLabelString = returnLabelData(labelChoice);
                getLabelString[0] = headerString[6];
                getLabelString[1] = headerString[7];
            }

            LabelPreview(queueString, getLabelString, defaultsString, whichPanel);

        }


        //routines to get Label Name and find the information

        public String[] returnLabelHeaderData(string labelName)
        {
            
            String[] labelHeaderData = new String[18];
            DataTable headerDataSet = new DataTable("headerDataSet");
            headerDataSet = LabelsLabelNamesTableAdapter.GetDataByName(labelName);
            DataRow dRow = headerDataSet.Rows[0];
            
            //Batch or Not
            string batch = dRow.ItemArray[3].ToString().Trim();
            labelHeaderData[0] = batch;

            //Selector for next table
            String childName = dRow.ItemArray[2].ToString().Trim();

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
            string[] defaults = new string[33];

            defaultsTableAdapter1.Fill(databaseLabelsDataSetDefaults.Defaults);
            DataRow dRow = databaseLabelsDataSetDefaults.Tables["Defaults"].Rows[0];
            for (int i = 0; i <= 11; i++)
            {
                defaults[i] = dRow.ItemArray[i + 1].ToString().Trim();
            }
            defaults[12] = dRow.ItemArray[0].ToString().Trim();
            for (int i = 13; i <= 32; i++)
            {
                defaults[i] = dRow.ItemArray[i].ToString().Trim();
            }
            return defaults;
        }

        private void applyDefaultSetting()
        {
            string[] defaults = getDefaultSettings();
            checkBoxColourAdd.Checked = bool.Parse(defaults[4].ToString());
            checkBoxQueueDelete.Checked= bool.Parse(defaults[5].ToString());
            radioButtonAutoStated.Checked = bool.Parse(defaults[6].ToString());
            radioButtonAutoModified.Checked = bool.Parse(defaults[7].ToString());
            radioButtonAddress1.Checked = bool.Parse(defaults[8].ToString());
            radioButtonAddress2.Checked= bool.Parse(defaults[9].ToString());
            checkBoxCorrectAddress.Checked = bool.Parse(defaults[10].ToString());
        }

        private void getLabelName()
        {
            string[] defaults = getDefaultSettings();
            if (tabControlQueue.SelectedTab == tabPageMainQueue)
            {
                comboBoxLabelName.Text = defaults[2];
            }
            else if (tabControlQueue.SelectedTab == tabPageColourQueue)
            {
                comboBoxLabelName.Text = defaults[3];
            }
            else if (tabControlQueue.SelectedTab == tabPageAddresses)
            {
                comboBoxLabelName.Text = defaults[31];
            }
            else if (tabControlQueue.SelectedTab == tabPagePassports)
            {
                comboBoxLabelName.Text = defaults[32];
            }
            fillPrinterDetails();
        }

        private void fillPrinterDetails()
        {
            DataTable headerDataSet = new DataTable("headerDataSet");
            headerDataSet = LabelsLabelNamesTableAdapter.GetDataByName(comboBoxLabelName.Text.ToString());
            string CategoryName = headerDataSet.Rows[0].ItemArray[2].ToString();

            LabelsLabelCategoriesTableAdapter.FillBy(databaseLabelsDataSetLabelNames.LabelsLabelCategories, CategoryName );
            DataRow dRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelCategories"].Rows[0];
            listBoxPrinter.Items.Clear();
            for (int i = 1; i <= 16; i++)
            {
                listBoxPrinter.Items.Add(dRow.ItemArray[i].ToString());
            }
            labelPrinterChoice.Text = listBoxPrinter.Items[11].ToString();
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


        #region queue modification routines ( for pre printed labels)

        private void dataGridViewQueueList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            int colIndex = e.ColumnIndex;

            
            if (dataGridViewQueueList.Rows[rowIndex].Cells[colIndex].Value.ToString() == "True")
            {
                dataGridViewQueueList.Rows[rowIndex].Cells[colIndex].Value = "False";
                
            }
            else if (dataGridViewQueueList.Rows[rowIndex].Cells[colIndex].Value.ToString() == "False")
            {
                dataGridViewQueueList.Rows[rowIndex].Cells[colIndex].Value = "True";
               
            }

            colourRow(rowIndex);
            countLabelStocks();
        }

        private void colourRow(int rowIndex)
        {
            int nameColour = 0;
            Color colourTrue = Color.MistyRose;
            Color colourMedium = Color.Cornsilk;
            Color colourFalse = Color.Honeydew;
            if (dataGridViewQueueList.Rows[rowIndex].Cells[1].Value.ToString() == "True")
            {
                dataGridViewQueueList.Rows[rowIndex].Cells[1].Style.BackColor = colourTrue;
                nameColour++;
            }
            else
            {
                dataGridViewQueueList.Rows[rowIndex].Cells[1].Style.BackColor = colourFalse;
            }

            if (dataGridViewQueueList.Rows[rowIndex].Cells[2].Value.ToString() == "True")
            {
                dataGridViewQueueList.Rows[rowIndex].Cells[2].Style.BackColor = colourTrue;
                nameColour++;
            }
            else
            {
                dataGridViewQueueList.Rows[rowIndex].Cells[2].Style.BackColor = colourFalse;
            }
            dataGridViewQueueList.Rows[rowIndex].Cells[0].Style.BackColor = colourFalse;
            if (nameColour == 1) { dataGridViewQueueList.Rows[rowIndex].Cells[0].Style.BackColor = colourMedium; }
            if (nameColour == 2) {dataGridViewQueueList.Rows[rowIndex].Cells[0].Style.BackColor = colourTrue; }
        }


        private void buttonRemoveLabelStocks_Click(object sender, EventArgs e)
        {
            if (dataGridViewQueueList.RowCount > 0)

            {
                int countLabels = 0;
                int countFlags = 0;
                //go through the list
                for (int i = 0; i <= dataGridViewQueueList.RowCount - 1; i++)
                {
                    string idFind = dataGridViewQueueList.Rows[i].Cells[3].Value.ToString();
                    //check if need to remove this one
                    if (dataGridViewQueueList.Rows[i].Cells[2].Value.ToString() == "True")
                    {
                        
                        //go through colour queue
                        for (int j = 0; j <= dataGridViewColourQ.RowCount - 2; j++)
                        {
                            if (idFind == dataGridViewColourQ.Rows[j].Cells[35].Value.ToString())
                            {
                                dataGridViewColourQ.Rows.RemoveAt(j);

                                try
                                {
                                    tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                                    countLabels++;
                                }
                                catch (System.Exception ex)
                                {
                                    MessageBox.Show("Failed to delete from Colour Queue - " + ex);
                                }
                                break;
                            }
                        }
                    }
                    //check if need to remove flag
                    if (dataGridViewQueueList.Rows[i].Cells[1].Value.ToString() == "True")
                    {

                        for (int j = 0; j <= (dataGridViewPlants.RowCount - 1); j++)
                        {
                            if (idFind == dataGridViewPlants.Rows[j].Cells[0].Value.ToString())
                            {
                                databaseLabelsDataSet.TablePlants.Rows[j].SetField(20, "False");
                                try
                                {
                                    tablePlantsTableAdapter.Update(databaseLabelsDataSet.TablePlants);
                                    countFlags++;
                                    //MessageBox.Show("Updated Database Entry");
                                }
                                catch (System.Exception ex)
                                {
                                    MessageBox.Show("Failed to update to Database - " + ex);
                                }
                                break;
                            }
                        }
                    }

                }
                string entry = " Entry";
                string flag = " flag";
                if (countLabels > 1) { entry = " Entries"; }
                if (countFlags > 1) { flag = " flags"; }
                labelColourCount.Text = addColourQueueTotal().ToString();
                labelColourCountQ.Text = labelColourCount.Text;
                fillLabelStocksGrid();
                MessageBox.Show(countLabels + entry + " removed from colour queue, " + countFlags + flag + " reset to 'False'.");
            }
        }

        private void buttonMissingRemove_Click(object sender, EventArgs e)
        {
            if (dataGridViewMissingPictures.RowCount > 0)

            {
                int count = 0;
                //go through the list
                for (int i = 0; i <= dataGridViewMissingPictures.RowCount - 1; i++)
                {
                    //check if need to remove this one
                    if (dataGridViewMissingPictures.Rows[i].Cells[1].Value.ToString() == "True")
                    {
                        string idFind = dataGridViewMissingPictures.Rows[i].Cells[2].Value.ToString();
                        //go through colour queue
                        for (int j = 0; j <= dataGridViewColourQ.RowCount - 2; j++)
                        {
                            if (idFind == dataGridViewColourQ.Rows[j].Cells[26].Value.ToString())
                            {
                                dataGridViewColourQ.Rows.RemoveAt(j);

                                try
                                {
                                    tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                                    count++;
                                }
                                catch (System.Exception ex)
                                {
                                    MessageBox.Show("Failed to delete from Colour Queue - " + ex);
                                }
                                break;
                            }
                        }
                    }
                }
                string entry = " Entry";
                if (count > 1) { entry = " Entries"; }
                labelColourCount.Text = addColourQueueTotal().ToString();
                labelColourCountQ.Text = labelColourCount.Text;
                fillMissingPicturesGrid();
                MessageBox.Show(count + entry + " removed from colour queue");
            }
        }

        private void checkForLabelStocksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControlQueue.SelectedTab = tabPageLabelStocks;
        }

        private void checkForMissingPicturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControlQueue.SelectedTab = tabPageMissingPictures;
        }

        private void dataGridViewMissingPictures_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            int colIndex = e.ColumnIndex;
            if (dataGridViewMissingPictures.Rows[rowIndex].Cells[colIndex].Value.ToString() == "True")
            {
                dataGridViewMissingPictures.Rows[rowIndex].Cells[colIndex].Value = "False";
            }
            else if (dataGridViewMissingPictures.Rows[rowIndex].Cells[colIndex].Value.ToString() == "False")
            {
                dataGridViewMissingPictures.Rows[rowIndex].Cells[colIndex].Value = "True";
            }
            countMissingPictures();
        }

        #endregion

        #region *** AutoLabel ***

        private void buttonUploadAuto_Click(object sender, EventArgs e)
        {
            csvReaderAutoBody(labelAutoFile.Text.ToString());
            dataGridViewAuto.Refresh();
            fillAutoListBox();
            checkSKUs();
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            string[] header = CreationUtilities.dataReader.csvReaderHeader(labelAutoFile.Text.ToString());
            for (int i = 0; i <= header.Length - 1; i++)
            {
                //listBoxAuto.Items.Add(header[i]);
            }

        }

        private string[] findAutoHeaderRow()
        {
            string[] returnString = new string[15];
            try
            {
                string[] header = CreationUtilities.dataReader.csvReaderHeader(labelAutoFile.Text.ToString());
                string[] headerData = new string[15];
                for (int i = 0; i <= header.Length - 1; i++)
                {

                    if (header[i] == "Order Number")
                    {
                        headerData[0] = i.ToString();
                    }
                    if (header[i] == "Product Name")
                    {
                        headerData[2] = i.ToString();
                    }
                    if (header[i] == "Product Quantity")
                    {
                        headerData[3] = i.ToString();
                    }
                    if (header[i] == "Product SKU")
                    {
                        headerData[4] = i.ToString();
                    }
                    if (header[i] == "Billing First Name")
                    {
                        headerData[5] = i.ToString();
                    }
                    if (header[i] == "Billing Last Name")
                    {
                        headerData[6] = i.ToString();
                    }
                    if (header[i] == "Shipping First Name")
                    {
                        headerData[7] = i.ToString();
                    }
                    if (header[i] == "Shipping Last Name")
                    {
                        headerData[8] = i.ToString();
                    }
                    if (header[i] == "Shipping Address Line 1")
                    {
                        headerData[9] = i.ToString();
                    }
                    if (header[i] == "Shipping Address Line 2")
                    {
                        headerData[10] = i.ToString();
                    }
                    if (header[i] == "Shipping City")
                    {
                        headerData[11] = i.ToString();
                    }
                    if (header[i] == "Shipping State")
                    {
                        headerData[12] = i.ToString();
                    }
                    if (header[i] == "Shipping Postcode")
                    {
                        headerData[13] = i.ToString();
                    }
                    if (header[i] == "Shipping Company")
                    {
                        headerData[14] = i.ToString();
                    }
                    



                }
                returnString = headerData;

            }
            catch
            {
                MessageBox.Show("Failed to Read File. Try altering the file address and refreshing the grid");
            }
            return returnString;
        }

        public void clearAutoGrid()
        {
            int numRows = databaseLabelsDataSetAuto.TableAuto.Rows.Count - 1;

            for (int i = 0; i <= numRows; i++)
            {
                dataGridViewAuto.Rows.RemoveAt(0);
            }
            dataGridViewAuto.EndEdit();
            try
            {
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to delete from Auto Queue - " + ex);
            }


        }

        private void colourAutoDataGrid()
        {
            string[] defaults = getDefaultSettings();
            Color colourTrue = Color.FromName(defaults[15]);
            Color colourFalse = Color.FromName(defaults[17]);
                for (int i = 0; i < dataGridViewAuto.RowCount-1; i++)
            {
                for (int f = 1; f <= 3; f++)
                {
                    if (dataGridViewAuto.Rows[i].Cells[f].Value.ToString() == "True")
                    {
                        dataGridViewAuto.Rows[i].Cells[f].Style.BackColor = colourTrue;
                    }
                    else
                    {
                        dataGridViewAuto.Rows[i].Cells[f].Style.BackColor = colourFalse;
                    }
                }
            }
        }

        private void fillAutoListBox()
        {
            string customerOld = "";

            listBoxAuto.Items.Clear();

            int numRows = databaseLabelsDataSetAuto.TableAuto.Rows.Count - 1;

            for (int i = 0; i <= numRows; i++)
            {
                string customer = dataGridViewAuto.Rows[i].Cells[5].Value.ToString();
                string locked = "      ";
                if (dataGridViewAuto.Rows[i].Cells[1].Value.ToString() == "True") { locked = "# "; }
                if (customer != customerOld)
                {
                    listBoxAuto.Items.Add(locked + customer);
                    customerOld = customer;
                }
            }

            sortAutoListBox();
            colourAutoDataGrid();

        }

        private void sortAutoListBox()
        {

            listBoxAuto.Sorted = true;

            //remove any duplicates
            for (int i = (listBoxAuto.Items.Count - 1); i >= 1; i--)
            {
                for (int j = (i - 1); j >= 0; j--)
                {
                    string one = listBoxAuto.Items[i].ToString();
                    one = one.SubstringSpecial(2, one.Length - 1);
                    string two = listBoxAuto.Items[j].ToString();
                    two = two.SubstringSpecial(2, two.Length - 1);

                    if (one == two)
                    {
                        listBoxAuto.Items.RemoveAt(j);
                        i--;
                    }

                }
            }

        }

        public void csvReaderAutoBody(string path)

        {
            //clear the grid first
            clearAutoGrid();

            //fetch the autolabel data and fill the grid


            string[] headerData = findAutoHeaderRow();
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                String[] currentRow;

                //Loop through all of the fields in the file. 
                //If any lines are corrupt, report an error and continue parsing. 
                while (!csvParser.EndOfData)
                {
                    try
                    {
                        currentRow = csvParser.ReadFields();

                        DataRow row = databaseLabelsDataSetAuto.Tables[0].NewRow();

                        row["LockCust"] = "True";
                        row["LockLine"] = "False";
                        row["Printed"] = "False";
                        row["ON"] = currentRow[int.Parse(headerData[0])];
                        string customer = currentRow[int.Parse(headerData[5])] + " " + currentRow[int.Parse(headerData[6])];
                        row["Customer"] = customer;

                        row["Name"] = currentRow[int.Parse(headerData[2])];
                        row["Qty"] = currentRow[int.Parse(headerData[3])];
                        row["SKU"] = currentRow[int.Parse(headerData[4])];
                        row["First"] = currentRow[int.Parse(headerData[5])];
                        row["Last"] = currentRow[int.Parse(headerData[6])];

                        row["ShipFirst"] = currentRow[int.Parse(headerData[7])];
                        row["ShipLast"] = currentRow[int.Parse(headerData[8])];
                        row["ShipLine1"] = currentRow[int.Parse(headerData[9])];
                        row["ShipLine2"] = currentRow[int.Parse(headerData[10])];
                        row["ShipCity"] = currentRow[int.Parse(headerData[11])];
                        row["ShipState"] = currentRow[int.Parse(headerData[12])];
                        row["ShipPostcode"] = currentRow[int.Parse(headerData[13])];
                        row["ShipNotes"] = currentRow[int.Parse(headerData[14])];// = Shipping Company now


                        databaseLabelsDataSetAuto.TableAuto.Rows.Add(row);
                        dataGridViewAuto.EndEdit();
                        try
                        {
                            tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("Failed to add to Auto Queue - " + ex);
                        }

                    }
                    catch (MalformedLineException e)
                    {
                        MessageBox.Show("Line " + e.Message + " is invalid.  Skipping");
                    }

                }
                sortAuto("Name");
                fillAutoListBox();


            }
        }

        public void sortAuto(string which)
        {

            dataGridViewAuto.ClearSelection();
            //Always sort by Plant so that plants are in order even if sort is by O/N or Name
            dataGridViewAuto.Sort(dataGridViewAuto.Columns[6], ListSortDirection.Ascending);
            //selectionText = "SELECT LockCust, LockLine, Printed, [ON], Customer, Name, Qty, SKU, First, Last, Id, ShipFirst, ShipLast, ShipLine1, ShipLine2, ShipCity, ShipState, ShipPostcode, ShipNotes FROM TableAuto ORDER BY Name";

            if (which == "Order")
            {// default is by Order Number
             //string selectionText = "SELECT LockCust, LockLine, Printed, [ON], Customer, Name, Qty, SKU, First, Last, Id, ShipFirst, ShipLast, ShipLine1, ShipLine2, ShipCity, ShipState, ShipPostcode, ShipNotes FROM TableAuto ORDER BY Customer, Name";
                dataGridViewAuto.Sort(dataGridViewAuto.Columns[4], ListSortDirection.Ascending);
            }

            if (which == "Customer")
            {
                dataGridViewAuto.Sort(dataGridViewAuto.Columns[5], ListSortDirection.Ascending);
                //selectionText = "SELECT LockCust, LockLine, Printed, [ON], Customer, Name, Qty, SKU, First, Last, Id, ShipFirst, ShipLast, ShipLine1, ShipLine2, ShipCity, ShipState, ShipPostcode, ShipNotes FROM TableAuto ORDER BY [ON], Name";
            }
            
            //tableAutoTableAdapter.Adapter.SelectCommand.CommandText = selectionText;
            //tableAutoTableAdapter.Fill(databaseLabelsDataSetAuto.TableAuto);
            //dataGridViewAuto.CurrentCell = dataGridViewAuto.Rows[0].Cells[1];
            //dataGridViewAuto.FirstDisplayedCell = dataGridViewAuto.CurrentCell;
            dataGridViewAuto.Refresh();
        }

        private void buttonAutoCustomer_Click(object sender, EventArgs e)
        {
            sortAuto("Customer");
            colourAutoDataGrid();
            fillAutoListBox();
            checkSKUs();
        }

        private void buttonSortAutoON_Click(object sender, EventArgs e)
        {
            sortAuto("Order");
            colourAutoDataGrid();
            fillAutoListBox();
            checkSKUs();
        }

        private void buttonSortAutoPlant_Click(object sender, EventArgs e)
        {
            sortAuto("Plant");
            colourAutoDataGrid();
            fillAutoListBox();
            checkSKUs();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Delete Printed Lines
            for (int i = dataGridViewAuto.RowCount - 2; i >= 0; i--)
            {
                if (dataGridViewAuto.Rows[i].Cells[3].Value.ToString() == "True")
                {
                    dataGridViewAuto.Rows.RemoveAt(i);
                }
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            fillAutoListBox();
        }

        private void listBoxAuto_Click(object sender, EventArgs e)
        {
            if (listBoxAuto.SelectedItem == null)
            {
                return;
            }

            Boolean locked = false;

            string selected = listBoxAuto.SelectedItem.ToString().TrimEnd();
            Boolean change = true;
            if (string.IsNullOrEmpty(selected)) { change = false; }
            if (change)
            {
                if (selected.SubstringSpecial(0, 2) == "# ")
                {
                    locked = true;
                }

                string name = selected.Substring(2);
                name=name.Trim();
                swapAutoByName(locked, name, listBoxAuto.SelectedIndex);

            }

        }

        private void swapAutoByName(Boolean locked, string name, int index)
        {
                        
            //get colours
            string[] defaults = getDefaultSettings();
            Color colourTrue = Color.FromName(defaults[15]);
            Color colourFalse = Color.FromName(defaults[17]);
            //MessageBox.Show(name + " - " + locked.ToString());
            Boolean changeTo = true;
            dataGridViewAuto.ClearSelection();

            if (locked) { changeTo = false; }
            string changeText = "      ";
            if (changeTo) { changeText = "# "; }
            listBoxAuto.Items[index] = changeText + name;

            for (int i = 0; i <= dataGridViewAuto.Rows.Count - 2; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[5].Value.ToString().TrimEnd() == name)
                {
                    dataGridViewAuto.Rows[i].Cells[1].Value = changeTo;
                    tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
                    if (changeTo)
                    {
                        dataGridViewAuto.Rows[i].Cells[1].Style.BackColor = colourTrue;
                    }
                    else
                    {
                        dataGridViewAuto.Rows[i].Cells[1].Style.BackColor = colourFalse;
                    }
                }
            }

        }
        

        private void dataGridViewAuto_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //get colours
            string[] defaults = getDefaultSettings();
            Color colourTrue = Color.FromName(defaults[15]);
            Color colourFalse = Color.FromName(defaults[17]);

            if (e.ColumnIndex == 5 | e.ColumnIndex == 1 | e.ColumnIndex == 4) //LockCustomer, Order NUmber or Name change whole order
            {
                //clicked on the name
                string name = dataGridViewAuto.Rows[e.RowIndex].Cells[5].Value.ToString().TrimEnd();
                Boolean locked = false;

                int index = 0;
                //MessageBox.Show("Looking for '" + name +"'");

                for (int i = 0; i <= listBoxAuto.Items.Count - 1; i++)
                {
                    if (listBoxAuto.Items[i].ToString().Substring(2).Trim() == name)

                    {
                        index = i;
                        //MessageBox.Show("FOUND - "+ listBoxAuto.Items[i].ToString().Substring(2).Trim()+ "     In loop, index = " + index.ToString());

                        string lockedString = listBoxAuto.Items[i].ToString().SubstringSpecial(0, 2);
                        if (lockedString == "# ") { locked = true; }
                        break;
                    }
                }
                swapAutoByName(locked, name, index);
            }
            else if (e.ColumnIndex == 2 | e.ColumnIndex == 6 | e.ColumnIndex == 7 | e.ColumnIndex == 8) //Line Lock, Plant Name, qty or SKU
            {
                string changeValue = dataGridViewAuto.Rows[e.RowIndex].Cells[2].Value.ToString();
                Boolean changeTo = true;
                dataGridViewAuto.Rows[e.RowIndex].Cells[2].Style.BackColor = colourTrue;
                if (changeValue == "True") { changeTo = false; dataGridViewAuto.Rows[e.RowIndex].Cells[2].Style.BackColor = colourFalse; }
                dataGridViewAuto.Rows[e.RowIndex].Cells[2].Value = changeTo;
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);

            }
            else if (e.ColumnIndex == 3) //Printed
            {
                string changeValue = dataGridViewAuto.Rows[e.RowIndex].Cells[3].Value.ToString();
                Boolean changeTo = true;
                dataGridViewAuto.Rows[e.RowIndex].Cells[3].Style.BackColor = colourTrue;
                if (changeValue == "True") { changeTo = false; dataGridViewAuto.Rows[e.RowIndex].Cells[3].Style.BackColor = colourFalse; }
                dataGridViewAuto.Rows[e.RowIndex].Cells[3].Value = changeTo;
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);

            }
        }

        private void buttonCreateQueue_Click(object sender, EventArgs e)
        {
            if (tabControlQueue.SelectedTab == tabPageMainQueue || tabControlQueue.SelectedTab == tabPageColourQueue)
            {
                string[] message = { "Producing Label Queue", "Producing Label Queue", "", "Please wait while the program", "collects the data for the queue", "", "", "This message will evaporate shortly" };
                Thread queueNote = new Thread(() => showMessageFormWithDispose(message, 3000));
                queueNote.Start();

                listBoxAutoErrors.Items.Clear();
                pictureBoxArrow.Visible = false;
                findAutoCustomer();
                //Create Address Queue
                createAddressList("specified", "Address");

                //Create Passport Queue
                createPassportList("specified", "Passport");

                //reset customer to prevent confusion with next manual entry
                textBoxCustomerName.Text = "";
                textBoxOrderNumber.Text = "";
            }
            else
            {
                DialogResult result = MessageBox.Show("Please Set the Queue Tab to 'Main' or 'Colour' before trying to create an automatic queue", "Wrong Queue");
            }
        }

        private void findAutoCustomer()
        {
           
            // findAutoCustomer the next printable customer
            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[1].Value.ToString() == "False" &&
                    dataGridViewAuto.Rows[i].Cells[2].Value.ToString() == "False" &&
                    dataGridViewAuto.Rows[i].Cells[3].Value.ToString() == "False")
                {
                    string customer = dataGridViewAuto.Rows[i].Cells[5].Value.ToString();
                    //MessageBox.Show("findAutoCustomer i = " + i);
                    string[][] collectedOrder = collectAutoCustomer(customer);
                    createAutoQueueEntry(collectedOrder);
                }
            }
        }

        private string[][] collectAutoCustomer(string customer)
        {
            //MessageBox.Show("collectAutoCustomer");
            // collect the whole order to an array so it can print as one and also so it can be analysed

            //work out how many lines are for that customer
            int size = 0;
            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[5].Value.ToString() == customer) { size++; }
            }
            //MessageBox.Show(size.ToString());

            //make an array to fill with this order
            string[][] collectedOrder = new string[size][];
            int counter = 0;
            DataTable tryTable = databaseLabelsDataSet.Tables["TablePlants"];

            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[5].Value.ToString() == customer)
                {
                    string[] collect = new string[19];
                    for (int j = 0; j <= 18; j++)
                    {
                        collect[j] = dataGridViewAuto.Rows[i].Cells[j].Value.ToString();
                    }
                    //set as printed unless it is locked
                    if (dataGridViewAuto.Rows[i].Cells[2].Value.ToString() == "False") { dataGridViewAuto.Rows[i].Cells[3].Value = true; }
                    try
                    {
                        tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
                    }
                    catch 
                    {
                        //MessageBox.Show("Failed to delete from Auto Queue - " + ex);
                    }

                    //reset printed flag if entry is unfindable
                        string expression;
                        expression = "SKU = '" + collect[8].TrimEnd() + "'";
                        DataRow[] foundRows;
                        // Use the Select method to find all rows matching the filter.
                        foundRows = tryTable.Select(expression);
                        try
                        {
                            //trial assignment to trigger the error
                            string Genus = foundRows[0][3].ToString();
                        }
                        catch
                        {
                            dataGridViewAuto.Rows[i].Cells[3].Value = false; 
                            try
                            {
                                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
                            }
                            catch 
                            {
                                //MessageBox.Show("Haven't found - " + collect[6]);
                            }
                        }
                    
                    collectedOrder[counter] = collect;
                    counter++;
                }
            }

            tryTable.Dispose();

            // Runs if you want to produce automatic altered values
            if (radioButtonAutoModified.Checked)
            {
                // alter quantities if required
                string[] newQuantities = new string[counter];// new string to take new quantities
                newQuantities  = alterTheQuantities(collectedOrder, newQuantities);
                //replace old quantities for new altered ones
                for (int i = 0; i < counter; i++)
                {
                    collectedOrder[i][7] = newQuantities[i];
                }
            }
            return collectedOrder;
        }

        private string[] alterTheQuantities(string[][] collectedOrder, string[] newQuantities)
        {
            // Routine to automatically change quantities to an intellligent printable form. ie single labels for most, individual for duplicated genera etc.

            double numberOfLines = newQuantities.Length;
            double totalPotCount = 0;
            string[] genera = new string[Convert.ToInt32( numberOfLines)];
            string[] generaDuplicates = new string[Convert.ToInt32(numberOfLines)];

            for (int i = 0; i < numberOfLines; i++)
            {
                genera[i] = findGenus(collectedOrder[i][6]);
                newQuantities[i] = collectedOrder[i][7]; //transfer old to new as default position
                int result = 0;
                int.TryParse(collectedOrder[i][7], out result);
                totalPotCount = totalPotCount + result;
            }
            if (genera.Length > 1)
            {
                for (int i = 0; i <= genera.Length - 1; i++)
                {
                    for (int j = i + 1; j <= genera.Length - 1; j++)
                    {
                        if (genera[i] == genera[j] )
                        { 
                            genera[j] = ""; //clear out duplicates, leaving first instance intact
                            generaDuplicates[j] = "*"; //record all instances of duplicated genera for later
                            generaDuplicates[i] = "*"; //record all instances of duplicated genera for later
                        }
                    }
                }
            }
            // Count the different Genera
            double genusCount = 0;
            for (int i = 0; i <= genera.Length - 1; i++)
            {
                if (genera[i] != "") { genusCount++; }
            }

            //  a) First test for single line orders
            if (numberOfLines == 1)
            {
                // send singles straight through
                if (totalPotCount == 1) { return newQuantities; }
                // 2-3 can be set to singles
                if (totalPotCount >1 && totalPotCount < 4)
                {
                    newQuantities[0] = "1";
                    return newQuantities;
                }
                // 4-6 can be set to two labels for two boxes
                if (totalPotCount > 3 && totalPotCount < 7)
                {
                    newQuantities[0] = "2";
                    return newQuantities;
                }
                // greater than 6 can be complicated (Digitalis and Primula and Violets are 9cm)  set to int no./ 4 
                if (totalPotCount > 6 )
                {
                    double divide = Math.Truncate(double.Parse(newQuantities[0]) / 4);
                    newQuantities[0]  = Convert.ToInt32(divide).ToString();
                    return newQuantities;
                }

            }
            
            // The rest must all be multi-line

            // b) test for multi-line orders that are all single and send straight through
            if (numberOfLines/totalPotCount == 1)
            {
                return newQuantities;
            }
            
            // c) if all the genera are different set quantities to 1
            if (numberOfLines / genusCount == 1)
            {
                //MessageBox.Show("c");
                for (int i = 0; i < newQuantities.Length; i++) { newQuantities[i] = "1"; }
                return newQuantities;
            }

            // d) the rest must have the same gunus twice - the default option.
            //MessageBox.Show("d");
            for (int i = 0; i < newQuantities.Length; i++)
            {
                //only change unduplicated genus lines
                if (generaDuplicates[i] != "*") { newQuantities[i] = "1"; }   
            }
            return newQuantities;
            
        }

        private string findGenus(string sentName)
        {
            string genus = "";
            int findSpace = sentName.IndexOf(" ");
            genus = sentName.SubstringSpecial(0, findSpace);
            //MessageBox.Show("'" + genus + "'");
            return genus;
        }

        private void createAutoQueueEntry(string[][] sentOrder)
        {
            

            DataTable table = databaseLabelsDataSet.Tables["TablePlants"];
            Boolean firstError = true;
            //pictureBoxArrow.Visible = false;

            for (int i = 0; i <= sentOrder.Length -1; i++)
            {
                if (sentOrder[i][1] == "False" &&
                    sentOrder[i][2] == "False" &&
                    sentOrder[i][3] == "False")
                    
                {
                    string sku = sentOrder[i][8];
                    try
                    {//MessageBox.Show("looking for " + i.ToString());
                        
                        string expression;
                        expression = "SKU = '" + sku + "'";
                        DataRow[] foundRows;
                        // Use the Select method to find all rows matching the filter.
                        foundRows = table.Select(expression);
                        //MessageBox.Show("Found a match " + foundRows[0][4].ToString());
                        string[] sendAutoRow = new string[21];
                        for (int j = 0; j <= 20; j++)
                        {
                            sendAutoRow[j] = foundRows[0][j].ToString();
                        }
                        string sendQty = sentOrder[i][7];
                        string sendCustomer = sentOrder[i][5];
                        string sendOrderNumber = sentOrder[i][4];

                        string[] sendAddress = new string[9];
                        for (int k = 0; k <= 7; k++)
                        {
                            sendAddress[k + 1] =  sentOrder[i][k+11];
                        }
                        sendAddress[0] =  "To: "+ sendAddress[1].Trim()+ " " + sendAddress[2].Trim();

                        string[] queue = CollectAutoQueueEntry(sendAutoRow, sendQty, sendCustomer, sendOrderNumber, sendAddress);
                        doTheAdding(queue, "autolabel","visible","Main");
                    }
                    catch (Exception ex)
                    {
                        string exception = ex.ToString();
                        MessageBox.Show("Failed to match "+ sentOrder[i][6].Trim());

                        if (firstError && pictureBoxArrow.Visible == false )
                        {
                            //listBoxAutoErrors.Items.Clear();
                            listBoxAutoErrors.Items.Add("The following Lines weren't matched in the Database :-");
                            listBoxAutoErrors.Items.Add(" ");
                        }
                        listBoxAutoErrors.Items.Add(sku.Trim() + " - " + sentOrder[i][6].Trim() + " : "+  sentOrder[i][5].Trim());
                        firstError = false;
                        pictureBoxArrow.Visible = true;
                    }
                }
            }
            table.Dispose();
        }

        
        private void checkSKUs()
        {
            //get colours
            string[] defaults = getDefaultSettings();
            Color colourTrue = Color.FromName(defaults[15]);

            DataTable table = databaseLabelsDataSet.Tables["TablePlants"];
            Boolean firstError = true;
            pictureBoxArrow.Visible = false;
            listBoxAutoErrors.Items.Clear();
            listBoxAutoErrors.Items.Add("All SKUs have a corresponding match in the Database");
            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                {
                    //MessageBox.Show("looking for " + i.ToString());
                    string sku = dataGridViewAuto.Rows[i].Cells[8].Value.ToString().Trim();

                    string expression;
                    expression = "SKU = '" + sku + "'";
                    try
                    {
                        DataRow[] foundRows;
                        // Use the Select method to find all rows matching the filter.
                        foundRows = table.Select(expression);
                        //MessageBox.Show("Found a match for" + dataGridViewAuto.Rows[i].Cells[6].Value.ToString());
                        string found = foundRows[0][2] + " " + foundRows[0][4] + " " + foundRows[0][5];
                        //MessageBox.Show("Found a match for " + found);

                    }
                    catch
                    {
                        if (firstError)
                        {
                            listBoxAutoErrors.Items.Clear();
                            listBoxAutoErrors.Items.Add("The following Lines lack a match in the Database :-");
                            listBoxAutoErrors.Items.Add(" ");
                        }
                        //MessageBox.Show("failed to match SKU = " + sku + " for " + dataGridViewAuto.Rows[i].Cells[6].Value.ToString());
                        listBoxAutoErrors.Items.Add(sku + " - " + dataGridViewAuto.Rows[i].Cells[6].Value.ToString());
                        dataGridViewAuto.Rows[i].Cells[8].Style.BackColor = colourTrue;
                        firstError = false;
                        pictureBoxArrow.Visible = true;
                    }
                }
                table.Dispose();
            }
}

        private void buttonCheckSKUs_Click(object sender, EventArgs e)
        {
            checkSKUs();
        }

        public string[] CollectAutoQueueEntry(string[] sentAutoRow, string sentQty, string sentCustomer, string sentOrderNumber, string[] sentAddress)
        {
            string[] queueData = new string[37];

            
            string[] defaultsString = getDefaultSettings();

            string[] sendData = new string[21];
            string[] findName = new string[5];
            string[] moreData = new string[2];

            // get general plant data
            for (int i = 0; i <= 20; i++)
            {
                sendData[i] = sentAutoRow[i];
            }

            // get various concatenated Name strings 
            for (int i = 0; i <= 4; i++)
            {
                findName[i] = sentAutoRow[1 + i];
            }
            string[] sendName = getPlantName(findName);

            //get main pcture
            if (radioButtonImage1.Checked) { moreData[0] = sentAutoRow[12]; }
            if (radioButtonImage2.Checked) { moreData[0] = sentAutoRow[13]; }
            if (radioButtonImage3.Checked) { moreData[0] = sentAutoRow[14]; }
            if (radioButtonImage4.Checked) { moreData[0] = sentAutoRow[15]; }

            // Check AGM Status
            if (sentAutoRow[16] == "True")
            {
                moreData[1] = "AGM.ico";
            }
            else
            {
                moreData[1] = "AGMblank.ico";
            }

            // profile
            string profileName = sentAutoRow[17];

            DataTable table = databaseLabelsDataSetProfiles.Tables["TableProfiles"];
            string expression;
            expression = "Name = '" + profileName + "'";
            DataRow[] foundRows;
            // Use the Select method to find all rows matching the filter.
            foundRows = table.Select(expression);

            queueData[0] = sendName[0]; //Full name
            queueData[1] = sentQty; // Qty
            if (String.IsNullOrEmpty(queueData[1])) { queueData[1] = "0"; }
            queueData[2] = ""; // price - set to 0
            //if (String.IsNullOrEmpty(queueData[2])) { queueData[2] = "0"; }
            queueData[3] = sendData[9];  //Potsize
            queueData[4] = sentCustomer; // Customer Name
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
            queueData[24] = sentOrderNumber; //Order Number
            if (string.IsNullOrEmpty(queueData[24]))
            { queueData[24] = ""; }
            else
            { queueData[24] = "Order No. #" + queueData[24]; }
            queueData[25] = sendData[20];
            queueData[26] = sendData[0];

            queueData[27] = sentAddress[0];
            queueData[28] = sentAddress[1];
            queueData[29] = sentAddress[2];
            queueData[30] = sentAddress[3];
            queueData[31] = sentAddress[4];
            queueData[32] = sentAddress[5];
            queueData[33] = sentAddress[6];
            queueData[34] = sentAddress[7];
            queueData[35] = sentAddress[8];
            //Transfer Add to Colour Queue flag to queueData
            queueData[36] = "No Colour";
            if (sendData[10] == "True") { queueData[36] = "add Colour"; }

            return queueData;
        }

        #endregion

        private void buttonUnlockAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                dataGridViewAuto.Rows[i].Cells[1].Value = false;
            }
            try
            {
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            catch
            {
                //MessageBox.Show("Haven't found - " + collect[6]);
            }
            fillAutoListBox();
        }

        private void labelAutoFile_Click(object sender, EventArgs e)
        {
            
            string[] defaultsString = getDefaultSettings() ;
            string filePlace = defaultsString[1];

            openFileDialog1.InitialDirectory = filePlace;
            openFileDialog1.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            DialogResult result  = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) { labelAutoFile.Text = openFileDialog1.FileName; }
        }

        private void resetLabelFlagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do You want to reset all the Label Stocks flags to 'False'", "Change Flags", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                for (int i = 0; i <= dataGridViewPlants.RowCount - 1; i++)
                {
                    dataGridViewPlants.Rows[i].Cells[20].Value = false;
                }
                try
                {
                    tablePlantsTableAdapter.Update(databaseLabelsDataSet.TablePlants);
                    //MessageBox.Show("Updated Database Entry");
                }
                catch 
                {
                    MessageBox.Show("Failed to update to Database - ");

                }
            }
        }

        private void quickPrint(int qty, string labelName)
        {
            //Print one label without reference to the queue for speed

            #region **temp make a label copy**

            string[] defaultsString = getDefaultSettings();
            string[] queueString = new string[25];


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
                moreData[1] = "AGMblank.ico";
            }

            // qty and price

            moreData[2] = qty.ToString();
            moreData[3] = "";

            // customer and Order NUmber
            moreData[4] = "";
            moreData[5] = "";

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

            string labelChoice = labelName;

            String[] headerString = returnLabelHeaderData(labelChoice);
            String[] labelString = returnLabelData(labelChoice);
            labelString[0] = headerString[6];
            labelString[1] = headerString[7];

            #endregion **end**

            quickPrintDefaults quickPrintDefaults = new quickPrintDefaults();

            quickPrintDefaults.labelString = labelString;
            quickPrintDefaults.defaultsString = defaultsString;
            quickPrintDefaults.queueString = queueString;
            quickPrintDefaults.headerString = headerString;

            Thread quickPrint = new Thread(() => doTheQuickPrint(quickPrintDefaults));    // Kick off a new thread
            quickPrint.Start();

        }

        public void doTheQuickPrint( quickPrintDefaults quickPrintDefaults)
        { 

            PrintDialog pDialog = new PrintDialog();
            pDialog.PrinterSettings.PrinterName = quickPrintDefaults.headerString[12];

            if (DialogResult.OK == pDialog.ShowDialog())
            {

                //wait around so queues don't clash on print
                int waitCounter = 0;

                int printerIndex = canIusePrinter.getPrinterIndex(quickPrintDefaults.headerString[12]);
            Busy:
                if (canIusePrinter.inUseOrNot[printerIndex] == true)
                {
                    Thread.Sleep(1000);
                    waitCounter++;
                    goto Busy;
                }
                canIusePrinter.inUseOrNot[printerIndex] = true;


                //string[] queueData = collectQueueRow(count, whichQueue);

                PrintDocument pd = new PrintDocument();
                    pd.PrinterSettings.PrinterName = pDialog.PrinterSettings.PrinterName;
                    if (listBoxPrinter.Items[4].ToString() == "Landscape")
                    {
                        pd.DefaultPageSettings.Landscape = true;
                    }
                    else
                    {
                        pd.DefaultPageSettings.Landscape = false;
                    }

                    int sentWidth = (int)(pDialog.PrinterSettings.DefaultPageSettings.PaperSize.Width);
                    int sentHeight = (int)(pDialog.PrinterSettings.DefaultPageSettings.PaperSize.Height);
                    int marginX = (int)pDialog.PrinterSettings.DefaultPageSettings.HardMarginX;
                    int marginY = (int)pDialog.PrinterSettings.DefaultPageSettings.HardMarginY;
                    int placementX = 0;
                    int placementY = 0;


                    pd.PrintPage += (sender1, args) => DrawImage(quickPrintDefaults.queueString, quickPrintDefaults.labelString, quickPrintDefaults.defaultsString, sentWidth, sentHeight, marginX, placementX, marginY, placementY, sender1, args);
                    pd.PrinterSettings.Copies = short.Parse(quickPrintDefaults.queueString[1]);
                    pd.Print();
                    pd.Dispose();
               
                pDialog.Dispose();
                canIusePrinter.inUseOrNot[printerIndex] = false;
            }
            
        }

        #region QuickPrint Buttons

        private void buttonQP1_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP1.Text.ToString()), groupBoxQP1.Text.ToString());
        }

        private void buttonQP2_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP2.Text.ToString()), groupBoxQP2.Text.ToString());
        }

        private void buttonQP3_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP3.Text.ToString()), groupBoxQP3.Text.ToString());
        }

        private void buttonQP4_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP4.Text.ToString()), groupBoxQP4.Text.ToString());
        }

        private void buttonQP5_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP5.Text.ToString()), groupBoxQP5.Text.ToString());
        }

        private void buttonQP6_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP6.Text.ToString()), groupBoxQP6.Text.ToString());
        }

        private void buttonQP7_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP7.Text.ToString()), groupBoxQP7.Text.ToString());
        }

        private void buttonQP8_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP8.Text.ToString()), groupBoxQP8.Text.ToString());
        }

        private void buttonQP9_Click(object sender, EventArgs e)
        {
            quickPrint(int.Parse(textBoxQP9.Text.ToString()), groupBoxQP9.Text.ToString());
        }

#endregion


        private string getPicture( string initialFile)
        {
            string returnString = "";
            string smallPath = "";
            int place = 0;

            if (string.IsNullOrEmpty(initialFile)) { } //ignore if no initial value 
            else //trim off any extra folders
            {
                if (initialFile.Contains("\\"))
                    {
                    place = initialFile.IndexOf("\\");
                    smallPath = initialFile.Substring(0, place + 1);
                    }
            }

            string[] defaultString = getDefaultSettings();
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.InitialDirectory = defaultString[0] + smallPath;

            DialogResult result = fileDialog.ShowDialog();

            if ( result == DialogResult.OK)
                {
                string wholeName = fileDialog.FileName;
                string returnFile = "";

                if (wholeName.Contains(defaultString[0]))
                {
                    returnFile = wholeName.Substring(defaultString[0].Length);
                }

                returnString = returnFile;
            }

            return returnString;
        }

        private void pictureBoxData1_Click(object sender, EventArgs e)
        {
            string pictureFile = getPicture( textBoxData12.Text);
            textBoxData12.Text = pictureFile;
        }

        private void pictureBoxData2_Click(object sender, EventArgs e)
        {
            string pictureFile = getPicture( textBoxData13.Text);
            textBoxData13.Text = pictureFile;
        }

        private void pictureBoxData3_Click(object sender, EventArgs e)
        {
            string pictureFile = getPicture( textBoxData14.Text);
            textBoxData14.Text = pictureFile;
        }

        private void pictureBoxData4_Click(object sender, EventArgs e)
        {
            string pictureFile = getPicture( textBoxData15.Text);
            textBoxData15.Text = pictureFile;
        }

        private void dataGridView1ProfileView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int cellColumn = dataGridView1ProfileView.CurrentCell.ColumnIndex;
            if (cellColumn == 2 | cellColumn == 6 | cellColumn==7 )
            {
                ColorDialog colourPicker = new ColorDialog();
                int oldColour = int.Parse(dataGridView1ProfileView.CurrentCell.Value.ToString());
                int B = oldColour / 256 / 256;
                int G = (oldColour - (B * 256 * 256)) / 256;
                int R = (oldColour - (B * 256 * 256) - (G * 256));
                colourPicker.Color = Color.FromArgb (255,R,G,B);
                colourPicker.AnyColor = true;
                colourPicker.FullOpen = true;
                DialogResult result = colourPicker.ShowDialog();
                
                if (result == DialogResult.OK)
                {
                    int colour = 0;
                    colour = colour + colourPicker.Color.R;
                    colour = colour + (colourPicker.Color.G * 256);
                    colour = colour + (colourPicker.Color.B * 256 * 256);
                    int row = dataGridView1ProfileView.CurrentRow.Index;
                    databaseLabelsDataSetProfiles.TableProfiles.Rows[row].SetField( cellColumn, colour.ToString());
                }
            }
            if (cellColumn == 2 | cellColumn == 3 | cellColumn == 4 | cellColumn == 6 | cellColumn == 7)
            {
                try
                {

                    tableProfilesTableAdapter.Update(databaseLabelsDataSetProfiles.TableProfiles);
                    //MessageBox.Show("Updating");
                    //dataGridView1ProfileView.Refresh();
                    addProfileButtons();

                    addProfilePicture(dataGridView1ProfileView.CurrentRow.Cells[1].Value.ToString());
                }
                catch
                {
                    MessageBox.Show("Failed to Update Profiles Table");
                }
            }
            updateProfileSample();
            addProfilePicture( dataGridView1ProfileView.CurrentRow.Cells[1].Value.ToString());
        }

        

        private void bindingNavigatorMoveNextItem_Click(object sender, EventArgs e)
        {
            updateMainDetails(dataGridViewPlants.CurrentRow.Index );
        }

        private void bindingNavigatorMovePreviousItem_Click(object sender, EventArgs e)
        {
            updateMainDetails(dataGridViewPlants.CurrentRow.Index);
        }

        private void bindingNavigatorMoveLastItem_Click(object sender, EventArgs e)
        {
            updateMainDetails(dataGridViewPlants.CurrentRow.Index);
        }

        private void bindingNavigatorMoveFirstItem_Click(object sender, EventArgs e)
        {
            updateMainDetails(dataGridViewPlants.CurrentRow.Index);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            DataTable searchTable = new DataTable("searchTable");
            searchTable = tablePlantsTableAdapter.GetDataBySearch(toolStripComboBox1.Text);
            toolStripComboBox1.Items.Clear();
            for (int i = 0;i< searchTable.Rows.Count; i++)
            {
                string[] sendData = new string[5];
                for (int j = 0; j <= 4; j++)
                {
                    sendData[j] = searchTable.Rows[i].ItemArray[j+1].ToString();
                }
                string[] names = getPlantName(sendData);
                string number = searchTable.Rows[i].ItemArray[0].ToString();
                for (int k = number.Length+1; k <= 5; k++) { number = number + " "; }
                toolStripComboBox1.Items.Add(number+names[0]);
            }
            if (toolStripComboBox1.Items.Count > 0) { toolStripComboBox1.Text = toolStripComboBox1.Items[0].ToString(); }
            searchTable.Dispose();

            string idToFind = toolStripComboBox1.Text.Substring(0, 4).Trim();
            if (idToFind.All(char.IsDigit)) { findById(); }
        }

        private void findById()
        {
            //get Id
            string idToFind = toolStripComboBox1.Text.Substring(0, 4).Trim();

            int rowSeek = 0;
            for (int i = 0; i < dataGridViewPlants.RowCount; i++)
            {
                //check id
                if (dataGridViewPlants.Rows[i].Cells[0].Value.ToString().Trim() == idToFind)
                {
                    //Bingo, set this as the right row, otherwise default to first of the letter (ie if added a hidden line)
                    rowSeek = i;
                }
            }
            //go there and refresh the display
            makeTheJump(rowSeek);
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string idToFind = toolStripComboBox1.Text.Substring(0, 4).Trim();
            if (idToFind.All(char.IsDigit)) { findById(); }
        }

        private void fillLabelNames()
        {
            comboBoxLabelsWithin.Items.Clear();
            LabelsLabelNamesTableAdapter.FillByChild(databaseLabelsDataSetLabelNames.LabelsLabelNames, textBoxCat1.Text.ToString().Trim());
            for (int i=0;i< databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows.Count; i++)
            {
                comboBoxLabelsWithin.Items.Add(databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[i].ItemArray[1]);
            }
            fillLabelNamesTextBoxes(0);
            comboBoxLabelsWithin.Text = "Find labels from this Category Here";

        }
        private void fillLabelNamesTextBoxes(int sentRow)
        {
            for (int i=0; i < 5;  i++)
            {
                TextBox curText = (TextBox)groupBoxLabelNames.Controls["textBoxLabel" + i.ToString()];
                curText.Text = "";
                try
                {
                    curText.Text = databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[sentRow].ItemArray[i].ToString();
                }
                catch { }
            }
        }

        private void tabControlDesign_Click(object sender, EventArgs e)
        {
            tabControlDesign.BringToFront();

            if (tabControlDesign.SelectedTab == tabPageAppData)
            {
                richTextBoxAppData.Clear();

                try
                {
                    richTextBoxAppData.AppendText("ApplicationDeployment.CurrentDeployment.CurrentVersion" + Environment.NewLine + Environment.NewLine);
                    richTextBoxAppData.AppendText(ApplicationDeployment.CurrentDeployment.CurrentVersion + Environment.NewLine + Environment.NewLine + Environment.NewLine);
                    richTextBoxAppData.AppendText("ApplicationDeployment.CurrentDeployment.ActivationUri" + Environment.NewLine + Environment.NewLine);
                    richTextBoxAppData.AppendText(ApplicationDeployment.CurrentDeployment.ActivationUri + Environment.NewLine + Environment.NewLine + Environment.NewLine);
                    richTextBoxAppData.AppendText("ApplicationDeployment.CurrentDeployment.DataDirectory" + Environment.NewLine + Environment.NewLine);
                    richTextBoxAppData.AppendText(ApplicationDeployment.CurrentDeployment.DataDirectory + Environment.NewLine + Environment.NewLine + Environment.NewLine);
                    richTextBoxAppData.AppendText("Application.UserAppDataPath" + Environment.NewLine + Environment.NewLine);
                    richTextBoxAppData.AppendText(Application.UserAppDataPath + Environment.NewLine + Environment.NewLine + Environment.NewLine);
                }
                catch
                {
                    MessageBox.Show("Can't fill in data as this is not a deployed version", "No data available");
                }
            }

            if (tabControlDesign.SelectedTab == tabPageCategories)
            {
                LabelsLabelCategoriesTableAdapter.Fill(databaseLabelsDataSetLabelNames.LabelsLabelCategories);
                fillCategories("tab");
                fillLabelNames();
                
            }

            if (tabControlDesign.SelectedTab == tabPageDesignFields)
            {

                dataGridViewDesign.Visible = false;
                LabelsLabelNamesTableAdapter.Fill(databaseLabelsDataSetLabelNames.LabelsLabelNames);
                comboBoxLabelsForDesign.Items.Clear();
                for (int i=0;i< databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows.Count; i++)
                    {
                    comboBoxLabelsForDesign.Items.Add(databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[i].ItemArray[1].ToString());
                    }
            }

            if (tabControlDesign.SelectedTab == tabPageDefaults)
            {
                string[] defaults = getDefaultSettings();

                textBoxDefaultsId.Text = defaults[12];
                textBoxDefaultsPictureFolder.Text = defaults[0];
                textBoxDefaultsFileFolder.Text = defaults[1];
                textBoxDefaultsMainLabel.Text = defaults[2];
                textBoxDefaultsColourLabel.Text = defaults[3];
                textBoxDefaultsAddColour.Text = defaults[4];
                textBoxDefaultsDeleteQueue.Text = defaults[5];
                textBoxDefaultsAutoStated.Text = defaults[6];
                textBoxDefaultsAutoModified.Text = defaults[7];
                textBoxDefaultsAddressUnlock.Text = defaults[8];
                textBoxDefaultsAddressAll.Text = defaults[9];
                textBoxDefaultsCorrectAdd.Text = defaults[10];
                textBoxDefaultsAutoLabel.Text = defaults[11];
                textBoxAutoLabelFile.Text = defaults[18];

                textBoxAddressSort.Text = defaults[21];
                textBoxOrderSort.Text = defaults[22];
                textBoxIncludeCourier.Text = defaults[23];
                textBoxAddressLabelQty.Text = defaults[24];
                textBoxProduceAddressLabel.Text = defaults[25];
                textBoxProducePassportLabel.Text = defaults[26];
                

                //colours
                textBoxColourMain.Text = defaults[13];
                textBoxColourColour.Text = defaults[14];
                textBoxColourTrue.Text = defaults[15];
                textBoxColourHalfway.Text = defaults[16];
                textBoxColourFalse.Text = defaults[17];
                textBoxColourMainText.Text = defaults[19];
                textBoxColourColourText.Text = defaults[20];

                buttonColourMain.BackColor = Color.FromName(textBoxColourMain.Text);
                buttonColourColour.BackColor = Color.FromName(textBoxColourColour.Text);
                buttonColourTrue.BackColor = Color.FromName(textBoxColourTrue.Text);
                buttonColourHalfway.BackColor = Color.FromName(textBoxColourHalfway.Text);
                buttonColourFalse.BackColor = Color.FromName(textBoxColourFalse.Text);
                button1ColourMainText.BackColor = Color.FromName(textBoxColourMainText.Text);
                buttonColourColourText.BackColor = Color.FromName(textBoxColourColourText.Text);

                textBoxColourAddress.Text = defaults[27];
                textBoxColourPassport.Text = defaults[29];
                textBoxColourAddressText.Text = defaults[28];
                textBoxColourPassportText.Text = defaults[30];

                textBoxDefaultAddressLabel.Text = defaults[31];
                textBoxDefaultPassportLabel.Text = defaults[32];
                
                buttonColourAddress.BackColor = Color.FromName(textBoxColourAddress.Text);
                buttonColourPassport.BackColor = Color.FromName(textBoxColourPassport.Text);
                buttonColourAddressText.BackColor = Color.FromName(textBoxColourAddressText.Text);
                buttonColourPassportText.BackColor = Color.FromName(textBoxColourPassportText.Text);
                


                string getName = "";
                comboBoxMainLabel.Items.Clear();
                comboBoxColourLabel.Items.Clear();
                comboBoxAutoLabel.Items.Clear();
                comboBoxAddressLabel.Items.Clear();
                comboBoxPassportLabel.Items.Clear();
                LabelsLabelNamesTableAdapter.Fill(databaseLabelsDataSetLabelNames.LabelsLabelNames);

                for (int i = 0; i <= (databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].Rows.Count - 1); i++)
                {
                    DataRow dRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].Rows[i];
                    getName = dRow.ItemArray[1].ToString();
                    comboBoxMainLabel.Items.Add(getName);
                    comboBoxColourLabel.Items.Add(getName);
                    comboBoxAutoLabel.Items.Add(getName);
                    comboBoxAddressLabel.Items.Add(getName);
                    comboBoxPassportLabel.Items.Add(getName);
                }

                //fill colours
                fillColourCombo();

            }
            if (tabControlDesign.SelectedTab == tabPageColours)
            {
                fillColourTab(); 
            }
        }

        private void fillColourTab()
        {
            int gap = 2;
            int panelWidth = panelColours.Width - 22;
            int panelHeight = panelColours.Height - 22;
            int availableWidth = panelWidth / 4;
            int availableHeight = panelHeight / 35;
            int controlHeight = availableHeight - gap;
            int controlWidth = ((availableWidth) - gap - gap) / 2;

            int x = 0;
            int y = 0;
            panelColours.Controls.Clear();
            Type colorType = typeof(System.Drawing.Color);
            // We take only static property to avoid properties like Name, IsSystemColor ...
            PropertyInfo[] propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (PropertyInfo propInfo in propInfos)
            {
                Point where = new Point(x, y);
                Label colourLabel = new Label();
                colourLabel.Text = propInfo.Name;
                colourLabel.Location = new Point(x*availableWidth+gap+controlWidth, y*availableHeight);
                colourLabel.Size = new Size(controlWidth, controlHeight);
                colourLabel.TextAlign = ContentAlignment.MiddleLeft;
                Button colourButton = new Button();
                colourButton.Text = "";
                colourButton.FlatStyle = FlatStyle.Flat;
                colourButton.Location = new Point(x * availableWidth, y * availableHeight);
                colourButton.Size = new Size(controlWidth, controlHeight);
                colourButton.BackColor = Color.FromName(propInfo.Name);

                panelColours.Controls.Add(colourLabel);
                panelColours.Controls.Add(colourButton);
                
                y++;
                if (y == 36)
                {
                    y = 0;
                    x++;    
                }
            }
        }



        private void fillColourCombo()
        {
            Type colorType = typeof(System.Drawing.Color);
            // We take only static property to avoid properties like Name, IsSystemColor ...
            PropertyInfo[] propInfos = colorType.GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public);
            foreach (PropertyInfo propInfo in propInfos)
            {
                comboBoxColours.Items.Add(propInfo.Name);
            }
        }


        private void fillCategories(string tabOrcombo)
        {
            int indexOfRow = 0;

            if (tabOrcombo == "tab")
            {
                indexOfRow = dataGridViewCategories.CurrentRow.Index;
            }
            else
            {
                indexOfRow = int.Parse(tabOrcombo);
            }
            for (int i = 0; i <= 16; i++)
            {
                TextBox curText = (TextBox)tabPageCategories.Controls["textBoxCat" + i.ToString()];
                curText.Text = dataGridViewCategories.Rows[indexOfRow].Cells[i].Value.ToString();
            }
            comboBoxCatType.Text = textBoxCat2.Text ;
            comboBoxCatOrient.Text = textBoxCat5.Text;
            comboBoxCatPrinter.Text = textBoxCat12.Text;
            comboBoxCatFlip.Text = textBoxCat14.Text;
            comboBoxCatRotate.Text = textBoxCat15.Text;

            panelLabelCategoryPreview.Size = new Size(501, 315);

            panelLabelCategoryPreview.Controls.Clear();

            int labelsAcross = int.Parse(textBoxCat3.Text.ToString());
            int labelsDown = int.Parse(textBoxCat4.Text.ToString());
            int labelCount = labelsAcross * labelsDown;
            Panel[] LabelSample = new Panel[labelCount];

            //work out the best fit for the space
            double labelWidth = double.Parse(textBoxCat6.Text.ToString());
            double labelHeight = double.Parse(textBoxCat7.Text.ToString());

            double labelsWidth = labelWidth*labelsAcross;
            double labelsHeight = labelHeight*labelsDown;
            double ratioAcross = panelLabelCategoryPreview.Width / labelsWidth;
            double ratioDown = panelLabelCategoryPreview.Height / labelsHeight;
            double ratio = ratioAcross;
            if (ratioDown < ratioAcross) { ratio = ratioDown; }

            int count = 0;
            int gap = 2;
            for (int x = 1; x <= labelsAcross; x++)
            {
                for (int y = 1; y <= labelsDown; y++)
                {
                    LabelSample[count] = new Panel();
                    panelLabelCategoryPreview.Controls.Add(LabelSample[count]);
                    double xPos = ((labelWidth * ratio + gap) * (x - 1)) + gap;
                    double yPos = ((labelHeight * ratio + gap) * (y - 1)) + gap;
                    LabelSample[count].Size = new Size((int)(labelWidth * ratio),(int)(labelHeight * ratio));
                    LabelSample[count].Location = new Point ((int)xPos,(int)yPos);
                    LabelSample[count].BorderStyle = BorderStyle.FixedSingle;
                    LabelSample[count].BackColor = Color.GhostWhite;
                    count++;
                }
            }
            count--;
            panelLabelCategoryPreview.Size = new Size(LabelSample[count].Right+gap+gap,LabelSample[count].Bottom+gap+gap);
        }

        private void buttonPricestoSame_Click(object sender, EventArgs e)
        {
            bool valid = true;

            //Check Price
            String PriceBox = textBoxPricestoSame.Text.ToString().Trim();
            if (PriceBox.Trim() != "")
            {
                var isPriceNumeric = double.TryParse(textBoxQ3.Text.ToString(), out double price);
                //convert to currency if a number, leave a string if not
                if (isPriceNumeric)
                {
                    textBoxPricestoSame.Text = formatPrice(textBoxPricestoSame.Text.ToString());
                }
                else
                {
                    DialogResult result = MessageBox.Show("Price is not a number (" + textBoxPricestoSame.Text.ToString() + "). Press 'Yes' if you are happy with this, 'No' if not.", "Is the Price right ?", MessageBoxButtons.YesNo);
                    if (result == System.Windows.Forms.DialogResult.No)
                    {
                        valid = false;
                    }
                }
            }

            if (valid)
            {
                if (tabControlQueue.SelectedTab.Name == "tabPageColourQueue")
                {
                    for (int i = 0; i < databaseLabelsDataSetColourQueue.TableColourQueue.Rows.Count; i++)
                    {
                        databaseLabelsDataSetColourQueue.TableColourQueue.Rows[i].SetField(3, textBoxPricestoSame.Text.ToString());
                    }
                    try
                    {
                        tableColourQueueTableAdapter.Update(databaseLabelsDataSetColourQueue.TableColourQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update Colour Queue - " + ex);
                    }
                    
                }
                else
                {
                    for (int i = 0; i < databaseLabelsDataSetMainQueue.TableMainQueue.Rows.Count; i++)
                    {
                        databaseLabelsDataSetMainQueue.TableMainQueue.Rows[i].SetField(3, textBoxPricestoSame.Text.ToString());
                    }
                    try
                    {
                        tableMainQueueTableAdapter.Update(databaseLabelsDataSetMainQueue.TableMainQueue);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("Failed to update Main Queue - " + ex);
                    }
                    
                }
            }
            else
            {
                MessageBox.Show("Can't set the Price above");
            }
        }

        private void buttonResetPrinted_Click(object sender, EventArgs e)
        {
            //Delete Printed Lines
            for (int i = dataGridViewAuto.RowCount - 2; i >= 0; i--)
            {
                dataGridViewAuto.Rows[i].Cells[3].Value = "False";
                
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            fillAutoListBox();
        }

        private void buttonLockEntries_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                dataGridViewAuto.Rows[i].Cells[1].Value = true;
            }
            try
            {
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            catch
            {
                //MessageBox.Show("Haven't found - " + collect[6]);
            }
            fillAutoListBox();
        }

        private void buttonLockPrinted_Click(object sender, EventArgs e)
        {
            //Delete Printed Lines
            for (int i = dataGridViewAuto.RowCount - 2; i >= 0; i--)
            {
                dataGridViewAuto.Rows[i].Cells[3].Value = "True";

                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            fillAutoListBox();
        }

        private void button1_Click_4(object sender, EventArgs e)
        {
            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                dataGridViewAuto.Rows[i].Cells[2].Value = true;
            }
            try
            {
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            catch
            {
                //MessageBox.Show("Haven't found - " + collect[6]);
            }
            fillAutoListBox();
        }

        private void buttonUnlockPlants_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= dataGridViewAuto.RowCount - 2; i++)
            {
                dataGridViewAuto.Rows[i].Cells[2].Value = false;
            }
            try
            {
                tableAutoTableAdapter.Update(databaseLabelsDataSetAuto.TableAuto);
            }
            catch
            {
                //MessageBox.Show("Haven't found - " + collect[6]);
            }
            fillAutoListBox();
        }

        private void buttonEditProfile_Click(object sender, EventArgs e)
        {

        }

        private void buttonUpdateProfile_Click(object sender, EventArgs e)
        {

        }

        private void buttonAssignProfile_Click(object sender, EventArgs e)
        {
            string profileName = labelProfileSampleText.Text.Trim();
            int row = dataGridViewPlants.CurrentRow.Index;
            //int.TryParse(dataGridViewPlants.Rows[row].Cells[0].Value.ToString(), out int rowIndex);
            Boolean result = int.TryParse(labelGridID.Text.ToString(),out int realRow);
            databaseLabelsDataSet.TablePlants.Rows[realRow].SetField(17, profileName);
            //MessageBox.Show(databaseLabelsDataSet.TablePlants.Rows[rowIndex].RowState.ToString());            
            if (result)
            {
                try
                {
                    tablePlantsBindingSource.EndEdit();
                    tablePlantsTableAdapter.Update(databaseLabelsDataSet.TablePlants);

                    MessageBox.Show("Updated Profile for " + labelPlantName.Text + " as " + profileName);

                }
                catch
                {
                    MessageBox.Show("Failed to update Profile for " + labelPlantName.Text);
                }
                updateMainDetails(row);
            }
            else
            {
                MessageBox.Show("Failed to update Profile for " + labelPlantName.Text + ", couldn't find right row !");
            }
        }

        private void buttonNewProfile_Click(object sender, EventArgs e)
        {

        }

        private void updateProfileSample()
        {
            labelProfileSampleText.Text = dataGridView1ProfileView.CurrentRow.Cells[1].Value.ToString();

            labelProfileSampleText.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(dataGridView1ProfileView.CurrentRow.Cells[7].Value.ToString()));
            labelProfileSampleText.ForeColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(dataGridView1ProfileView.CurrentRow.Cells[6].Value.ToString()));
            labelProfileSampleText.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(dataGridView1ProfileView.CurrentRow.Cells[7].Value.ToString()));
            panelProfileSampleBorder.BackColor= System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(dataGridView1ProfileView.CurrentRow.Cells[2].Value.ToString()));



        }

        

        private void button1_Click_5(object sender, EventArgs e)
        {
            
            //sort by customer
            sortAuto("Customer");
            //colourAutoDataGrid();
            //fillAutoListBox();
            //checkSKUs();

            createAddressList("visible","Address");

            //set numbers to 1
            
        }

        private void createAddressList(string visibleOrSpecified, string specified)
        {

            //sort by customer
            sortAuto("Customer");

            if (checkBoxCorrectAddress.Checked) { cleanAddresses(); }
            Boolean includeDPD = true; //default
            DialogResult result = MessageBox.Show( "Do you want to include courier orders as well as Post Office","Courier orders", MessageBoxButtons.YesNo);
            if (result == DialogResult.No) { includeDPD = false; }
            
            // count customers
            string customer = "";
            int count = 0;
            for (int i = 0; i < dataGridViewAuto.RowCount - 1; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[5].Value.ToString() != customer)
                {
                    count++;
                    customer = dataGridViewAuto.Rows[i].Cells[5].Value.ToString();
                }
            }
            int rememberCustomers = count;
            //gather names and counts
            string[,] customers = new string[count,3];
            customer = "";
            count = 0;
            int plantCount = 0;
            for (int i = 0; i < dataGridViewAuto.RowCount - 1; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[5].Value.ToString() != customer)
                {
                    
                    customer = dataGridViewAuto.Rows[i].Cells[5].Value.ToString();
                    plantCount = int.Parse(dataGridViewAuto.Rows[i].Cells[7].Value.ToString());
                    customers[count,0] = customer;
                    customers[count, 1] = plantCount.ToString();
                    customers[count, 2] = dataGridViewAuto.Rows[i].Cells[1].Value.ToString();
                    count++;
                }
                else
                {
                    plantCount = plantCount + int.Parse(dataGridViewAuto.Rows[i].Cells[7].Value.ToString());
                    customers[count-1, 1] = plantCount.ToString();
                }
                //MessageBox.Show(customer.Trim() + " plants - " + plantCount.ToString() + " , count - "+count.ToString());
            }

            //make the queue

            for (int i = 0; i<=rememberCustomers-1; i++)
            {
                Boolean addIt = false;

                if (radioButtonAddress2.Checked == true) { addIt = true; } //use all addresses, locked and unlocked
                //MessageBox.Show(customers[i, 2].ToString());
                if (customers[i, 2].ToString() == "False") { addIt = true; } //unlocked customers
                if (includeDPD == false) //check for DPD, false = no DPD
                {
                    if (int.Parse(customers[i,1].ToString()) > 6) { addIt = false; } //>6 = likely to be DPD
                }

                if (addIt)
                {
                    //rountine to make a line if it needs adding
                    //MessageBox.Show("Adding - " + customers[i, 0]);
                    for (int j = 0; j<= dataGridViewAuto.RowCount - 1; j++)
                    {
                        if (dataGridViewAuto.Rows[j].Cells[5].Value.ToString() == customers[i,0])
                        {
                            //MessageBox.Show("Found "+ customers[i, 0]);

                            string[] queueData = new string[37];

                            queueData[0] = customers[i,0]; //Full name
                            int qty = 1;
                            int qtySent = (int.Parse(customers[i, 1].ToString()));
                            if (qtySent > 3) { qty = 2; }
                            if (qtySent > 6) { qty = 1; }
                            queueData[1] = qty.ToString(); // Qty
                            queueData[2] = ""; // price - set to 0
                            queueData[3] = "";  //Potsize
                            queueData[4] = customers[i,0]; // Customer Name
                            queueData[5] = ""; // Barcode
                            queueData[6] = "This is an Address only Queue"; //Description
                            queueData[7] = ""; //Common Name
                            queueData[8] = ""; // Main Picture
                            queueData[9] = "Arial"; // Font Name
                            queueData[10] = "0"; // Font Colour
                            queueData[11] = "True"; // Bold
                            queueData[12] = "True"; // Italic
                            queueData[13] = "0"; // Border Colour
                            queueData[14] = "0"; // Back Colour
                            queueData[15] = ""; // notes
                            queueData[16] = ""; // Genus
                            queueData[17] = ""; // species
                            queueData[18] = "";  // Variety
                            queueData[19] = ""; // AGM picture to use
                            queueData[20] = ""; // Picture1
                            queueData[21] = ""; // Picture2
                            queueData[22] = ""; // Picture3
                            queueData[23] = ""; // Picture4
                            queueData[24] = dataGridViewAuto.Rows[j].Cells[4].Value.ToString(); //Order Number
                            if (string.IsNullOrEmpty(queueData[24]))
                            { queueData[24] = ""; }
                            else
                            { queueData[24] = "Order No. #" + queueData[24]; }
                            queueData[25] = "True";
                            queueData[26] = "1";

                            queueData[27] = "To : " + dataGridViewAuto.Rows[j].Cells[11].Value.ToString().Trim() +" "+ dataGridViewAuto.Rows[j].Cells[12].Value.ToString().Trim();
                            queueData[28] = dataGridViewAuto.Rows[j].Cells[11].Value.ToString().Trim(); 

                            queueData[29] = dataGridViewAuto.Rows[j].Cells[12].Value.ToString().Trim();
                            queueData[30] = dataGridViewAuto.Rows[j].Cells[13].Value.ToString().Trim();
                            queueData[31] = dataGridViewAuto.Rows[j].Cells[14].Value.ToString().Trim();
                            queueData[32] = dataGridViewAuto.Rows[j].Cells[15].Value.ToString().Trim();
                            queueData[33] = dataGridViewAuto.Rows[j].Cells[16].Value.ToString().Trim();
                            queueData[34] = dataGridViewAuto.Rows[j].Cells[17].Value.ToString().Trim();
                            queueData[35] = dataGridViewAuto.Rows[j].Cells[18].Value.ToString().Trim();
                            queueData[36] = "No Colour";

                            doTheAdding(queueData, "AutoLabel",visibleOrSpecified,specified);

                            break;
                        }
                    }
                }
            }
            setQueueQuantity(1, visibleOrSpecified, specified);
            string[] defaults = getDefaultSettings();
            if (defaults[21] == "Customer") { dataGridViewAddressQ.Sort(dataGridViewAddressQ.Columns[3], ListSortDirection.Ascending); }
            else if (defaults[21] == "Plant") { dataGridViewAddressQ.Sort(dataGridViewAddressQ.Columns[16], ListSortDirection.Ascending); }
            else { dataGridViewAddressQ.Sort(dataGridViewAddressQ.Columns[24], ListSortDirection.Ascending); }
        }

        private void createPassportList(string visibleOrSpecified, string specified)
        {
            // count customers
            string customer = "";
            int count = 0;
            for (int i = 0; i < dataGridViewAuto.RowCount - 1; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[5].Value.ToString() != customer)
                {
                    count++;
                    customer = dataGridViewAuto.Rows[i].Cells[5].Value.ToString();
                }
            }
            int rememberCustomers = count;
            //gather names and counts and Genera
            string[,] customers = new string[count, 4];
            customer = "";
            count = 0;
            int plantCount = 0;
            
            for (int i = 0; i < dataGridViewAuto.RowCount - 1; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[5].Value.ToString() != customer)
                {

                    customer = dataGridViewAuto.Rows[i].Cells[5].Value.ToString();
                    plantCount = int.Parse(dataGridViewAuto.Rows[i].Cells[7].Value.ToString());
                    customers[count, 0] = customer;
                    customers[count, 1] = plantCount.ToString();
                    customers[count, 2] = dataGridViewAuto.Rows[i].Cells[1].Value.ToString();

                    //Collect Genera
                    customers[count, 3] = "";
                    string Genus = "";
                    for (int q = 0; q < dataGridViewAuto.RowCount - 1; q++)
                            { 
                               if (dataGridViewAuto.Rows[q].Cells[5].Value.ToString() == customer)
                                {
                                    string GenusCompareOriginal = dataGridViewAuto.Rows[q].Cells[6].Value.ToString();
                                    int index = 0;
                                    for (int j = GenusCompareOriginal.Length - 1; j > 0; j--)
                                    {
                                        if (GenusCompareOriginal[j].ToString() == " ")
                                        {
                                            index = j;
                                        }
                                    }

                                    string GenusCompare = GenusCompareOriginal.SubstringSpecial(0, index);
                                    if (GenusCompare != Genus)
                                    {
                                    customers[count, 3] = customers[count, 3].ToString() + GenusCompare + ", ";
                                    Genus = GenusCompare;
                                    }
                                }
                            }


                    count++;
                }
                else
                {
                    plantCount = plantCount + int.Parse(dataGridViewAuto.Rows[i].Cells[7].Value.ToString());
                    customers[count - 1, 1] = plantCount.ToString();
                }

                
                }

            //make the queue

            for (int i = 0; i <= rememberCustomers - 1; i++)
            {
                Boolean addIt = false;

                if (radioButtonAddress2.Checked == true) { addIt = true; } //use all addresses, locked and unlocked
                //MessageBox.Show(customers[i, 2].ToString());
                if (customers[i, 2].ToString() == "False") { addIt = true; } //unlocked customers
                
                if (addIt)
                {
                    //rountine to make a line if it needs adding
                    //MessageBox.Show("Adding - " + customers[i, 0]);
                    for (int j = 0; j <= dataGridViewAuto.RowCount - 1; j++)
                    {
                        if (dataGridViewAuto.Rows[j].Cells[5].Value.ToString() == customers[i, 0])
                        {
                            //MessageBox.Show("Found "+ customers[i, 0]);

                            string[] queueData = new string[37];

                            queueData[0] = customers[i, 0]; //Full name
                            int qty = 1;
                            int qtySent = (int.Parse(customers[i, 1].ToString()));
                            if (qtySent > 3) { qty = 2; }
                            if (qtySent > 6) { qty = 1; }
                            queueData[1] = qty.ToString(); // Qty
                            queueData[2] = ""; // price - set to 0
                            queueData[3] = "";  //Potsize
                            queueData[4] = customers[i, 0]; // Customer Name
                            queueData[5] = ""; // Barcode
                            queueData[6] = "This is a Plant Passport only Queue"; //Description
                            queueData[7] = ""; //Common Name
                            queueData[8] = ""; // Main Picture
                            queueData[9] = "Arial"; // Font Name
                            queueData[10] = "0"; // Font Colour
                            queueData[11] = "True"; // Bold
                            queueData[12] = "True"; // Italic
                            queueData[13] = "0"; // Border Colour
                            queueData[14] = "0"; // Back Colour
                            queueData[15] = DateTime.Now.ToString("yyyy-d-M"); // notes
                            queueData[16] = customers[i,3]; // Genus
                            queueData[17] = ""; // species
                            queueData[18] = "";  // Variety
                            queueData[19] = ""; // AGM picture to use
                            queueData[20] = ""; // Picture1
                            queueData[21] = ""; // Picture2
                            queueData[22] = ""; // Picture3
                            queueData[23] = ""; // Picture4
                            queueData[24] = dataGridViewAuto.Rows[j].Cells[4].Value.ToString(); //Order Number
                            if (string.IsNullOrEmpty(queueData[24]))
                            { queueData[24] = ""; }
                            else
                            { queueData[24] = "Order No. #" + queueData[24]; }
                            queueData[25] = "True";
                            queueData[26] = "1";

                            queueData[27] = "To : " + dataGridViewAuto.Rows[j].Cells[11].Value.ToString().Trim() + " " + dataGridViewAuto.Rows[j].Cells[12].Value.ToString().Trim();
                            queueData[28] = dataGridViewAuto.Rows[j].Cells[11].Value.ToString().Trim();

                            queueData[29] = dataGridViewAuto.Rows[j].Cells[12].Value.ToString().Trim();
                            queueData[30] = dataGridViewAuto.Rows[j].Cells[13].Value.ToString().Trim();
                            queueData[31] = dataGridViewAuto.Rows[j].Cells[14].Value.ToString().Trim();
                            queueData[32] = dataGridViewAuto.Rows[j].Cells[15].Value.ToString().Trim();
                            queueData[33] = dataGridViewAuto.Rows[j].Cells[16].Value.ToString().Trim();
                            queueData[34] = dataGridViewAuto.Rows[j].Cells[17].Value.ToString().Trim();
                            queueData[35] = dataGridViewAuto.Rows[j].Cells[18].Value.ToString().Trim();
                            queueData[36] = "No Colour";

                            //add passport
                            doTheAdding(queueData, "AutoLabel", visibleOrSpecified, specified);

                            break;
                        }
                    }
                }
            }
            //set numbers to 1
            setQueueQuantity(1, visibleOrSpecified, specified);
            string[] defaults = getDefaultSettings();
            if (defaults[21] == "Customer") { dataGridViewPassportQ.Sort(dataGridViewPassportQ.Columns[3], ListSortDirection.Ascending); }
            else if (defaults[21] == "Plant") { dataGridViewPassportQ.Sort(dataGridViewPassportQ.Columns[16], ListSortDirection.Ascending); }
            else { dataGridViewPassportQ.Sort(dataGridViewPassportQ.Columns[24], ListSortDirection.Ascending); }

        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string myVersion = "";
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                myVersion = ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            else
            {
                myVersion = "Debug";
        }
            MessageBox.Show(myVersion);
        }

        private void dataGridViewCategories_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            fillCategories("tab");
            fillLabelNames();
        }

        private void comboBoxCatType_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxCat2.Text = comboBoxCatType.Text;
        }

        private void comboBoxCatOrient_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxCat5.Text = comboBoxCatOrient.Text;
        }

        private void comboBoxCatRotate_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxCat15.Text = comboBoxCatRotate.Text;
        }

        private void comboBoxCatFlip_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxCat14.Text = comboBoxCatFlip.Text;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonColourQChange_Click(object sender, EventArgs e)
        {
            if (textBoxDefaultsAddColour.Text == "True")
            { textBoxDefaultsAddColour.Text = "False"; }
            else
            { textBoxDefaultsAddColour.Text = "True"; }
        }

        private void buttonDeleteQ_Click(object sender, EventArgs e)
        {
            if (textBoxDefaultsDeleteQueue.Text == "True")
            { textBoxDefaultsDeleteQueue .Text = "False"; }
            else
            { textBoxDefaultsDeleteQueue.Text = "True"; }
        }

        private void buttonAutoQty_Click(object sender, EventArgs e)
        {
            if (textBoxDefaultsAutoStated .Text == "True")
            {
                textBoxDefaultsAutoStated.Text = "False";
                textBoxDefaultsAutoModified.Text = "True";
            }
            else
            {
                textBoxDefaultsAutoStated.Text = "True";
                textBoxDefaultsAutoModified.Text = "False";
            }
        }

        private void buttonAutoAddress_Click(object sender, EventArgs e)
        {

            if (textBoxDefaultsAddressUnlock.Text == "True")
            {
                textBoxDefaultsAddressUnlock.Text = "False";
                textBoxDefaultsAddressAll.Text = "True";
            }
            else
            {
                textBoxDefaultsAddressUnlock.Text = "True";
                textBoxDefaultsAddressAll.Text = "False";
            }
        }

        private void comboBoxMainLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDefaultsMainLabel.Text = comboBoxMainLabel.Text;
        }

        private void comboBoxColourLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDefaultsColourLabel.Text = comboBoxColourLabel.Text;
        }

        private void buttonUpdateDefaults_Click(object sender, EventArgs e)
        {
            int indexOfRow = 0; //gets row to update

            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(1, textBoxDefaultsPictureFolder.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(2, textBoxDefaultsFileFolder.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(3, textBoxDefaultsMainLabel.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(4, textBoxDefaultsColourLabel.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(5, textBoxDefaultsAddColour.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(6, textBoxDefaultsDeleteQueue.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(7, textBoxDefaultsAutoStated.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(8, textBoxDefaultsAutoModified.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(9, textBoxDefaultsAddressUnlock.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(10, textBoxDefaultsAddressAll.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(11, textBoxDefaultsCorrectAdd.Text.ToString() );
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(12, textBoxDefaultsAutoLabel.Text.ToString());

            //Colours
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(13, textBoxColourMain .Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(14, textBoxColourColour.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(15, textBoxColourTrue.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(16, textBoxColourHalfway.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(17, textBoxColourFalse.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(19, textBoxColourMainText.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(20, textBoxColourColourText.Text.ToString());

            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(27, textBoxColourAddress.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(28, textBoxColourAddressText.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(29, textBoxColourPassport.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(30, textBoxColourPassportText.Text.ToString());

            //autolabel file
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(18, textBoxAutoLabelFile.Text.ToString());

            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(21, textBoxAddressSort.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(22, textBoxOrderSort.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(23, textBoxIncludeCourier.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(24, textBoxAddressLabelQty.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(25, textBoxProduceAddressLabel.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(26, textBoxProducePassportLabel.Text.ToString());

            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(31, textBoxDefaultAddressLabel.Text.ToString());
            databaseLabelsDataSetDefaults.Defaults.Rows[indexOfRow].SetField(32, textBoxDefaultPassportLabel.Text.ToString());

            try
            {
                defaultsTableAdapter1.Update(databaseLabelsDataSetDefaults.Defaults);
                        MessageBox.Show("Default Settings Updated");
                applyDefaultSetting();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to update Default Settings - " + ex);
                }
            }

       

        private void buttonCorrectAddresses_Click(object sender, EventArgs e)
        {
            if (textBoxDefaultsCorrectAdd.Text == "True")
            { textBoxDefaultsCorrectAdd.Text = "False"; }
            else
            { textBoxDefaultsCorrectAdd.Text = "True"; }
        }

        private void comboBoxLabelsWithin_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (textBoxLabel1.Text == "Find labels from this Category Here") { return; }
            LabelsLabelNamesTableAdapter.FillByChild(databaseLabelsDataSetLabelNames.LabelsLabelNames, textBoxCat1.Text.ToString().Trim());

            int sentRow = 0;
            for (int i = 0; i < databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows.Count; i++)
            {
                if (databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[i].ItemArray[1].ToString().Trim() == comboBoxLabelsWithin.Text.Trim())
                {
                    sentRow = i;
                    break;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                TextBox curText = (TextBox)groupBoxLabelNames.Controls["textBoxLabel" + i.ToString()];
                curText.Text = "";
                try
                {
                    curText.Text = databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[sentRow].ItemArray[i].ToString();
                }
                catch { }
            }
        }

        

        private void comboBoxLabelsForDesign_SelectedIndexChanged(object sender, EventArgs e)
        {
            string label = comboBoxLabelsForDesign.Text;
            LabelsLabelFieldsTableAdapter.FillBy(databaseLabelsDataSetLabelNames.LabelsLabelFields, label);
            dataGridViewDesign.Visible = true;
        }

        private void dataGridViewDesign_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowChosen = dataGridViewDesign.CurrentRow.Index;
            for (int i = 0; i < dataGridViewDesign.ColumnCount; i++)
            {
                TextBox curText = (TextBox)tabPageDesignFields.Controls["textBoxDesign" + i.ToString()];
                curText.Text = dataGridViewDesign.Rows[rowChosen].Cells[i].Value.ToString();
            }
            labelDesignColour.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(textBoxDesign20.Text));
            textBoxDesignRow.Text = dataGridViewDesign.CurrentRow.Index.ToString();
            PaintFields();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (textBoxDesign9.Text == "True")
            {
                textBoxDesign9.Text = "False";
            }
            else
            {
                textBoxDesign9.Text = "True";
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string[] message = { "Font Colour", "Set to true or false" ,"","Determines whether the font is printed in the","profile colour (false) or the colour from","the value in box 22 (true)"};
            showMessageForm(message);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string[] message = { "Profile Defaults", "Set to true or false", "", "Determines whether the information printed", "comes from the queue data (true)","governed by the field specified in box 11","or the fixed value in box 12 (false)" };
            showMessageForm(message);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string[] message = { "Font Variable", "Set to true or false", "", "Determines whether the program is allowed", "to vary the font size (true) to maximise its size", "or use the fixed value in box 17 (false)" };
            showMessageForm(message);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string[] message = { "Font Variable", "Set to true or false", "", "Determines whether the program is allowed", "to vary reduce the number of text lines (true)","to maximise font size.","Use in conjunction with 'Font Variable'" };
            showMessageForm(message);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string[] message = { "ListBox index numbers","Determines which item from the Queue is used to fill the field.","These numbers correspond to the numbers in 'Queue Utilities' less 1", "",
                " 0. Plant Name ( full name created from Genus, species and Variety )"," 1. Quantity - how many of label to print",                                      
                " 2. Price - formatted with £ sign or pence", " 3. Pot Size", " 4. Customer - (usually from Billing Customer in Autolabel)",                    
                " 5. Barcode - not currently supported",  " 6. Description - the full plant description"," 7. Common Name",                                                                
                " 8. Main Picture","9. Font Name - from Profile",  "10. Font Colour - from Profile",                                                 
                "11. Font Bold - true/false - from Profile", "12. Font Italic - true/false - from Profile",                                    
                "13. Border Colour  - from Profile","14. Background Colour  - from Profile",                                          
                "15. Notes - for short descriptions","16. Genus","17. Species","18. Variety",
                "19. AGM - whether AGM picture is displayed or not","20. Picture no.1","21. Picture no.2","22. Picture no.3","23. Picture no.4",
                "24. Order Number - formatted as O/N #...","25. Shipping Name ","26. Shipping First Name","27. Shipping Last Name",
                "28. Shipping Address - line 1","29. Shipping Address - line 2","30. Shipping Address - City","31. Shipping Address - State",
                "32. shipping Address - Postcode","34. Shipping Company","",
                "Shipping Address lines can be rearranged automatically by the 'modify address' function within Autolabel"};
            showMessageForm(message);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string[] message = { "Fixed Value", "Used to provide a fixed value such as", "a fixed text string or the width to draw a border line" };
            showMessageForm(message);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            string[] message = { "Size and Position", "All sizes and positions are entered as a percentage", "of the label size.", "","Positions enterd a '0' designate a position","that is centred on that axis." };
            showMessageForm(message);
        }

        private void buttonFlipDesignSurface_Click(object sender, EventArgs e)
        {
            if (buttonFlipDesignSurface.Text == "Change so Fields Show")
            {
                buttonFlipDesignSurface.Text = "Change to Preview";
                panelDesignFields.BringToFront();
            }
            else
            {
                buttonFlipDesignSurface.Text = "Change so Fields Show";
                panelDesignPreview.BringToFront();
            }
        }

        private void buttonRefreshPreviews_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBoxLabelsForDesign.Text))
            {
                MessageBox.Show("There is no label selected", "Choose a label first");
                return;
            }
            TempMakeALabel(panelDesignPreview, "Design", "database", "");
            String[] labelData = CollectDesignLabelData();
            makeLabelFieldsPreview(panelDesignFields, labelData);
        }

        private String[] CollectDesignLabelData()
        {
            int fieldCount = dataGridViewDesign.Rows.Count-1;
            int howMany = 20;

            int count = (fieldCount * howMany) + 2;
            String[] outputString = new string[count];

            string[] headerString = returnLabelHeaderData(comboBoxLabelsForDesign.Text);
            outputString [0] = headerString[6];
            outputString [1] = headerString[7];

            for (int j = 0; j < fieldCount; j++)
            {
                for (int i = 0; i < 20; i++)
                {
                    outputString[2 + i + (j * 20)] = dataGridViewDesign.Rows[j].Cells[i + 1].Value.ToString().Trim();
                }
            }

            return outputString;
        }

        private void makeLabelFieldsPreview(Panel whichPanel, String[] labelData)
        {
            //Clear the panel
            foreach (Control ctrl in whichPanel.Controls)
            {
                ctrl.Dispose();
            }

            //Set up the label size and shape
            int labelWidth = (int)double.Parse(labelData[0]);
            int labelHeight = (int)double.Parse(labelData[1]);
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

            Panel panelFields = new designPanel(labelData, finalWidthInt, finalHeightInt);
            whichPanel.Controls.Add(panelFields);
        }

        public class designPanel : Panel
        {
            public designPanel(string[] labelData, int XSize,int YSize)
            {
                 this.Paint += (sender2, e2) => designPanel_Paint(sender2, e2, labelData, XSize,  YSize);
            }

            private void designPanel_Paint(object sender, PaintEventArgs e, string[] labelData, int XSize, int YSize)
            {
                this.Location = new Point(2, 2);
                this.Size = new Size(XSize, YSize);
                this.BackColor = Color.White;
                this.BorderStyle = BorderStyle.FixedSingle;
                Graphics formGraphics = this.CreateGraphics();
                String[] colours = { "Aqua", "Coral", "Violet", "Tomato", "PaleVioletRed", "BlueViolet", "Chocolate", "Salmon", "Olive", "Thistle", "RosyBrown", "Aqua", "Coral", "Violet", "Tomato", "PaleVioletRed", "BlueViolet", "Chocolate", "Salmon", "Olive", "Thistle", "RosyBrown" };

                Pen gridPen = new Pen(Color.Gainsboro);
                Pen gridPenPale = new Pen(Color.GhostWhite);
                gridPen.Width = 1;
                int flip = 0;
                for (int x = 5; x <= 95; x = x + 5)
                {
                    if (flip == 0)
                    {
                        formGraphics.DrawLine(gridPenPale, new Point(((XSize * x) / 100), 0), new Point(((XSize * x) / 100), YSize));
                    }
                    else
                    { 
                        formGraphics.DrawLine(gridPen, new Point(((XSize * x) / 100), 0), new Point(((XSize * x) / 100), YSize));
                    }
                    flip = 1 - flip;
                    }
                flip = 0;
                for (int y = 5; y <= 95; y = y + 5)
                {
                    if (flip == 0)
                    {
                        formGraphics.DrawLine(gridPenPale, new Point(0, ((YSize * y) / 100)), new Point(XSize, ((YSize * y) / 100)));
                    }
                    else
                    {
                        formGraphics.DrawLine(gridPen, new Point(0, ((YSize * y) / 100)), new Point(XSize, ((YSize * y) / 100)));
                    }
                    flip = 1 - flip;
                }
                gridPen.Dispose();
                Font textFont  = new Font("Arial", 10, FontStyle.Regular);
                SolidBrush textBrush = new SolidBrush(Color.Black);
                int noLines = ((labelData.Length - 2) / 20);
                for (int i = 0; i < noLines; i++)
                {
                    double fieldX = (double.Parse(labelData[5 + (i * 20)])) / 100 * XSize;
                    double fieldY = (double.Parse(labelData[6 + (i * 20)])) / 100 * YSize;
                    double Xpos = (double.Parse(labelData[7 + (i * 20)])) / 100 * XSize;
                    double fieldXpos = 0;
                    if (Xpos == 0) { fieldXpos = (XSize - fieldX) / 2; }
                    else {  fieldXpos = Xpos; }
                    double fieldYpos = 0;
                    double Ypos = (double.Parse(labelData[8 + (i * 20)])) / 100 * YSize;
                    if (Ypos == 0) { fieldYpos = (YSize - fieldY) / 2; }
                    else { fieldYpos = Ypos; }

                    SolidBrush myBrush = new SolidBrush(Color.FromName(colours[i]));
                    Pen myPen = new Pen(myBrush);
                    myPen.Width = 2;
                    formGraphics.DrawRectangle(myPen, new Rectangle((int)fieldXpos, (int)fieldYpos, (int)fieldX, (int)fieldY));
                    formGraphics.DrawString(labelData[3 + (i * 20)], textFont, textBrush, (float)fieldXpos, (float)fieldYpos);
                    myBrush.Dispose();
                }
                formGraphics.Dispose();
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (textBoxDesign18.Text == "True")
            {
                textBoxDesign18.Text = "False";
            }
            else
            {
                textBoxDesign18.Text = "True";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBoxDesign19.Text == "True")
            {
                textBoxDesign19.Text = "False";
            }
            else
            {
                textBoxDesign19.Text = "True";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBoxDesign15.Text == "True")
            {
                textBoxDesign15.Text = "False";
            }
            else
            {
                textBoxDesign15.Text = "True";
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBoxDesign8.Text == "True")
            {
                textBoxDesign8.Text = "False";
            }
            else
            {
                textBoxDesign8.Text = "True";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBoxDesign10.Text == "True")
            {
                textBoxDesign10.Text = "False";
            }
            else
            {
                textBoxDesign10.Text = "True";
            }
        }

        private void comboBoxOrient_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDesign14.Text = comboBoxOrient.Text;
        }

        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDesign3.Text = comboBoxType.Text;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            FontDialog chooseFont = new FontDialog();
            Font currentStyle = new Font(textBoxDesign16.Text.Trim(), int.Parse(textBoxDesign17.Text), FontStyle.Regular);
            chooseFont.Font  = currentStyle;
            if (chooseFont.ShowDialog() == DialogResult.OK)
            {
                textBoxDesign16.Text = chooseFont.Font.Name;
                textBoxDesign17.Text = chooseFont.Font.Size.ToString();
            }
        }

        private void buttonDesignColour_Click(object sender, EventArgs e)
        {
            int storeColour = 0;
            Color oldColour = labelDesignColour.BackColor;
            Color newColour = pickMeAColour(oldColour);
            storeColour = (newColour.B * 256 * 256) + (newColour.G * 256) + newColour.R;
            textBoxDesign20.Text = storeColour.ToString();
            labelDesignColour.BackColor = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(textBoxDesign20.Text));
        }

        private void labelDesignColour_Click(object sender, EventArgs e)
        {

        }

        private void buttonAddCleanAdd_Click(object sender, EventArgs e)
        {
            //Check Action is valid
            string Action = comboBoxAddressClean.Text.Trim();
            Boolean record = false;
            for (int f = 0; f < comboBoxAddressClean.Items.Count; f++)
            {
                if (Action == comboBoxAddressClean.Items[f].ToString().Trim()) { record = true; }
            }

            if (record)
            {
                DataRow newRow = databaseLabelsDataSetAddClean.TableAddressFilters.NewRow();
                newRow[1] = textBoxAddClean.Text;
                newRow[2] = comboBoxAddressClean.Text;
                databaseLabelsDataSetAddClean.TableAddressFilters.Rows.Add(newRow);
                tableAddressFiltersTableAdapter.Update(databaseLabelsDataSetAddClean.TableAddressFilters);
            }
            else
            {
                MessageBox.Show("Action is not valid, please pick from the list");
            }
        }

        private void buttonAddCleanDelete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewCell oneCell in dataGridViewAddClean.SelectedCells)
            {
                if (oneCell.Selected)
                    dataGridViewAddClean.Rows.RemoveAt(oneCell.RowIndex);
            }
            try
            {
                  tableAddressFiltersTableAdapter.Update(databaseLabelsDataSetAddClean.TableAddressFilters);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to delete from Address Filters - " + ex);
            }
        }

        
        private void buttonDesignDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewDesign.RowCount == 2) {
                MessageBox.Show("Every Label needs one field or erors occur, delete the label instead");
                return;
            }

            DialogResult result = MessageBox.Show("Do you want to Delete the selected lines. If you have selected all entries then errors will occur. Please leave one line.", "Delete field lines", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (DataGridViewCell oneCell in dataGridViewDesign.SelectedCells)
                {
                    if (oneCell.Selected)
                        dataGridViewDesign.Rows.RemoveAt(oneCell.RowIndex);
                }
                try
                {
                    LabelsLabelFieldsTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelFields);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Failed to delete from Label Fields - " + ex);
                }
            }
        }

        private void buttonDesignUpdate_Click(object sender, EventArgs e)
        {
            int indexOfRow = int.Parse(textBoxDesignRow.Text); //gets row to update

            for (int i = 1; i <= 21; i++) //move through textboxes and update appropriate column
            {
                TextBox curText = (TextBox)tabPageDesignFields.Controls["textBoxdesign" + i.ToString()];
                string changeText = curText.Text.ToString().Trim();
                try
                {
                    databaseLabelsDataSetLabelNames.LabelsLabelFields.Rows[indexOfRow].SetField(i, changeText);
                }
                catch
                {
                    MessageBox.Show("The line no longer exists", "Bad Line");
                    return;
                }
            }

            try
            {
                LabelsLabelFieldsTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelFields);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to update Label Field - " + ex);
            }
        }

        private void buttonDesignAdd_Click(object sender, EventArgs e)
        {
            DataRow newRow = databaseLabelsDataSetLabelNames.LabelsLabelFields.NewRow();
            for (int i = 1; i <= 21; i++) //move through textboxes and update appropriate column
            {
                TextBox curText = (TextBox)tabPageDesignFields.Controls["textBoxdesign" + i.ToString()];
                string changeText = curText.Text.ToString().Trim();
                newRow[i] = changeText;
            }
            try
            {
                databaseLabelsDataSetLabelNames.LabelsLabelFields.Rows.Add(newRow);
                LabelsLabelFieldsTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelFields);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Failed to Add Label Field - " + ex);
            }

        }

        private void buttonBatchLabel_Click(object sender, EventArgs e)
        {
            if (textBoxLabel3.Text == "True")
            {
                textBoxLabel3.Text = "False"; 
            }
            else
            {
                textBoxLabel3.Text = "True";
            }
        }

        private void buttonQuickPrint_Click(object sender, EventArgs e)
        {
            if (textBoxCat2.Text.Trim() == "Picture")
            {
                MessageBox.Show("Only 'Text' labels can be QuickPrint labels", "Naughty Naughty");
                return;
            }
            if (textBoxLabel4.Text == "True")
            {
                textBoxLabel4.Text = "False";
            }
            else
            {
                textBoxLabel4.Text = "True";
            }
        }

        private void PaintFields()
        {
            string[] text = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21" };
            string[] border = new string[] { "1", "2", "3", "4", "5", "6", "7", "12", "20", "21" };
            string[] colourbox = new string[] { "1", "2", "3", "4", "5", "6", "7", "20", "21" };
            string[] image = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "20", "21" };

            string which = textBoxDesign3.Text.Trim();
            switch (which)
            {
                case "text":
                    highlightFields(text);
                    break;
                case "border":
                    highlightFields(border);
                    break;
                case "colourbox":
                    highlightFields(colourbox);
                    break;
                case "image":
                    highlightFields(image);
                    break;
            }
        }


            private void highlightFields(string[] which)
            { 

                    for (int i = 0; i < 22; i++)
            {
                TextBox curText = (TextBox)tabPageDesignFields.Controls["textBoxDesign" + i.ToString()];
                curText.BackColor = Color.White;
            }
            for (int i = 0; i < which.Length; i++)
            {
                TextBox curText = (TextBox)tabPageDesignFields.Controls["textBoxDesign" + which[i]];
                curText.BackColor = Color.LemonChiffon;
            }
        }
        #region  Updating Label Types

        private void button18_Click(object sender, EventArgs e)
        {
            // Updating a Label
            MessageBox.Show("This will not change the Label Name, just the Label details", "Update Label");
            int rowIndex = int.Parse(textBoxLabel0.Text);

            //LabelsLabelNamesTableAdapter.Fill(databaseLabelsDataSetLabelNames.LabelsLabelNames);
            for (int i = 2; i < 5; i++)
            {
                TextBox curText = (TextBox)groupBoxLabelNames.Controls["textBoxLabel" + i.ToString()];
                databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[rowIndex].BeginEdit();
                databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[rowIndex].SetField(i, curText.Text);
                databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[rowIndex].EndEdit();
            }
            try
            {
                LabelsLabelNamesTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelNames);
            }
            catch
            {
                MessageBox.Show("Failed to Update Label Description");
            }

        }

        #endregion

        private void buttonAddNewLabel_Click(object sender, EventArgs e)
        {
            Boolean create = true;
            for (int i = 0; i< databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows.Count; i++)
            {
                if (databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[i].ItemArray[1].ToString() == textBoxLabel1.Text) { create = false; }
            }
            if (create)
            {
                Boolean succeed = true;
                DataRow newRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].NewRow();
                for (int i = 1; i < 5; i++)
                {
                    TextBox curText = (TextBox)groupBoxLabelNames.Controls["textBoxLabel" + i.ToString()];
                    newRow[i] = curText.Text;
                }
                try {
                    databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows.Add(newRow);
                    LabelsLabelNamesTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelNames);
                }
                catch { MessageBox.Show("Failed to Add " + textBoxLabel1.Text, "Fail");  succeed = false; }

                if (succeed)
                {
                    DataRow newField = databaseLabelsDataSetLabelNames.Tables["LabelsLabelFields"].NewRow();

                    newField[1] = textBoxLabel1.Text;
                    newField[2] = "Trial Text";
                    newField[3] = "text";
                    newField[4] = "50";
                    newField[5] = "50";
                    newField[6] = "0";
                    newField[7] = "0";
                    newField[8] = "True";
                    newField[9] = "True";
                    newField[10] = "True";
                    newField[11] = "0";
                    newField[12] = "";
                    newField[13] = "2";
                    newField[14] = "center";
                    newField[15] = "True";
                    newField[16] = "Arial";
                    newField[17] = "10";
                    newField[18] = "True";
                    newField[19] = "False";
                    newField[20] = "0";
                    newField[21] = "1";

                    try {
                        databaseLabelsDataSetLabelNames.LabelsLabelFields.Rows.Add(newField);
                        LabelsLabelFieldsTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelFields);
                    }
                    catch { MessageBox.Show("Failed to Add " + textBoxLabel1.Text+ "sample field", "Fail"); }
                }
            }
            else
            {
                MessageBox.Show("Sorry, Label already exists", "Existing Label");
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string[] message = {"New Label", "Add a Label", "", " - Adds a new Label with the label name","typed as a child of the current Category.","", "It will create 1 field within this label.",
                                 "","","Update a Label",""," - Updates just the last two fields",
                                 "","","Delete a Label","","This deletes the label and ALL of its fields","Use with Caution !"};
            FormInformation form = new FormInformation( message);
            form.Show();
        }

        private void buttonDuplicateLabel_Click(object sender, EventArgs e)
        {
            string newName = textBoxDuplicatedLabel.Text;
            string oldName = textBoxLabel1.Text;

            Boolean create = true;
            for (int i = 0; i < databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows.Count; i++)
            {
                if (databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows[i].ItemArray[1].ToString() == newName) { create = false; }
            }
            if (create)
            {
                Boolean succeed = true;
                DataRow newRow = databaseLabelsDataSetLabelNames.Tables["LabelsLabelNames"].NewRow();
                newRow[1] = newName;
                for (int i = 2; i < 5; i++)
                {
                    TextBox curText = (TextBox)groupBoxLabelNames.Controls["textBoxLabel" + i.ToString()];
                    newRow[i] = curText.Text;
                }
                try
                {
                    databaseLabelsDataSetLabelNames.LabelsLabelNames.Rows.Add(newRow);
                    LabelsLabelNamesTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelNames);
                }
                catch { MessageBox.Show("Failed to Add " + textBoxLabel1.Text, "Fail"); succeed = false; }
                if (succeed)
                {
                    //Add fields only if label created
                    LabelsLabelFieldsTableAdapter.FillBy(databaseLabelsDataSetLabelNames.LabelsLabelFields, oldName);
                    int rowCount = databaseLabelsDataSetLabelNames.LabelsLabelFields.Rows.Count;
                    for (int i=0;i< rowCount; i++)
                    {
                        DataRow newField = databaseLabelsDataSetLabelNames.Tables["LabelsLabelFields"].NewRow();

                        newField[1] = newName;
                        for (int j = 2; j < 22; j++)
                        {
                            newField[j] = databaseLabelsDataSetLabelNames.Tables["LabelsLabelFields"].Rows[i].ItemArray[j];
                        }
                        try
                        {
                            databaseLabelsDataSetLabelNames.LabelsLabelFields.Rows.Add(newField);
                            LabelsLabelFieldsTableAdapter.Update(databaseLabelsDataSetLabelNames.LabelsLabelFields);
                        }
                        catch { MessageBox.Show("Failed to Add " + textBoxLabel1.Text + "sample field", "Fail"); }
                    }
                    MessageBox.Show(newName + " Created successfully", "New Label");
                }
            }
            else
            {
                MessageBox.Show("Sorry, Label already exists", "Existing Label");
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            string[] message = { "Altering Labels","Altering Labels works live on the database.", "", "If you want to experiment, duplicate your","label. Then experiment with this and when you are ready","alter the fields so that they refer to the label", "you want to work on."
                                 };
            FormInformation form = new FormInformation( message);
            form.Show();
        }

        private void backupToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // read connectionstring from config file
            var connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString;

            string backupFolder = Application.UserAppDataPath+"\\";

            var sqlConStrBuilder = new SqlConnectionStringBuilder(connectionString);

            // set backupfilename (you will get something like: "C:/temp/MyDatabase-2013-12-07.bak")
            var backupFileName = String.Format("{0}{1}-{2}.bak",
                backupFolder, "DatabaseBackup",
                DateTime.Now.ToString("yyyy-MM-dd"));

            using (var connection = new SqlConnection(sqlConStrBuilder.ConnectionString))
            {
                var query = String.Format("BACKUP DATABASE {0} TO DISK='{1}'",
                    "DatabaseLabels.mdf", backupFileName);

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void appPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ApplicationDeployment.CurrentDeployment.DataDirectory + " , "+ Application.UserAppDataPath);
            
        }

        private void button18_Click_1(object sender, EventArgs e)
        {
            doThePrinting("Auto");
        }

        private void comboBoxAutoLabelName_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeButtonColours();
        }

        private void comboBoxAutoLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDefaultsAutoLabel.Text = comboBoxAutoLabel.Text;
        }

        private void buttonAddressClean_Click(object sender, EventArgs e)
        {
            cleanAddresses();
        }


        #region Clean Addresses

        private Boolean checkForHouseNumber(string stringToTest)
        {
            Boolean result = false;
            if (string.IsNullOrEmpty(stringToTest.Trim())) { return result; }

            // call Regex.Match.
            string testString = stringToTest.Trim();
            //test for Flat and Unit and remove if found
            if (testString.Length > 4) //only test if long enough to get Flat and 1 number in
            {
                if (testString.Substring(0, 4).ToLower() == "flat" | testString.Substring(0, 4).ToLower() == "unit")
                {
                    testString = testString.Substring(4);
                }
            }
            //test for commas and full stops and remove if necessary
            string smallTest = testString.Substring(testString.Length - 1, 1);
            if (smallTest == "," | smallTest == ";" | smallTest == ".")
            {
                testString = testString.Substring(0, testString.Length - 1);
            }
            //match Any number of decimal digits plus 0 or more word characters, from first character of string
            Match match = Regex.Match(testString.Trim(), @"(^\d+\w*$)", RegexOptions.IgnoreCase);

            // check the Match for Success.
            if (match.Success)
            {
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }


        private void cleanAddresses()
        {
            //Cleans up the Addresses in the DataGrid. Doesn't save changes in case the cleanup is imperfect.

            

            for (int i =0;i<dataGridViewAuto.RowCount - 1; i++)
            {
                Boolean sameShippingAsBilling = true; // to stop it replacing Billing name with Shipping name
                string[] address = new string[9];
                //fill address from grid

                //check for null address
                Boolean checkNull = true;
                for (int j = 0; j <= 8; j++)
                {
                    if (j == 0)
                    {
                        address[j] = dataGridViewAuto.Rows[i].Cells[j + 5].Value.ToString().Trim();                        
                    }
                    else
                    {
                        address[j] = dataGridViewAuto.Rows[i].Cells[j + 10].Value.ToString().Trim();
                        if (String.IsNullOrEmpty(address[j]) != true) { checkNull = false; }
                    }
                }
                //jump to next line if address is all null
                if (checkNull) { continue; }

                //Move Mr / Mrs and delete oddities

                for (int k = 0; k < dataGridViewAddClean.RowCount - 1; k++)
                {
                    for (int l = 3; l <= 8; l++)
                    {
                        if (address[l] == dataGridViewAddClean.Rows[k].Cells[1].Value.ToString().Trim())
                        {
                            //found a matching oddity, so delete or move
                            string action = dataGridViewAddClean.Rows[k].Cells[2].Value.ToString().Trim();
                            if (action == "Name") // move item to before first name
                            {
                                address[1] = address[l] + " " + address[1];
                                address[l] = "";
                            }
                            if (action == "Delete") // remove item
                            {
                                address[l] = "";
                            }
                        }
                    }
                }
                //remove spurious numbers from line 1
                string check = address[3].Trim();
                bool result = checkForHouseNumber(check);
                if (result)
                {
                    //test for commas and full stops and remove if necessary
                    string smallTest = check.Substring(check.Length - 1, 1);
                    if (smallTest == "," | smallTest == ";" | smallTest == ".")
                    {
                        check = check.Substring(0, check.Length - 1);
                    }
                    //found an isolated number or similar
                    int numberLength = check.Length;
                    string duplicateCheck = "";
                    if (!string.IsNullOrWhiteSpace(address[4]))
                        {
                        duplicateCheck = address[4].Substring(0, numberLength);
                        }
                    if (check == duplicateCheck)
                    {
                        address[3] = ""; // duplicate, so just delete it
                    }
                    else
                    {
                        address[3] = check + " " + address[4]; //on wrong line so move and delete
                        address[4] = "";
                    }
                }

                //remove spurious numbers from line 2
                check = address[4].Trim();
                result = checkForHouseNumber(check);
                if (result)
                {
                    //test for commas and full stops and remove if necessary
                    string smallTest = check.Substring(check.Length - 1, 1);
                    if (smallTest == "," | smallTest == ";" | smallTest == ".")
                    {
                        check = check.Substring(0, check.Length - 1);
                    }
                    //found an isolated number or similar
                    int numberLength = check.Length;
                    string duplicateCheck = address[3].Substring(0, numberLength);
                    if (check == duplicateCheck)
                    {
                        address[4] = ""; // duplicate, so just delete it
                    }
                    else
                    {
                        address[3] = check + " " + address[3]; //on wrong line so move and delete
                        address[4] = "";
                    }
                }
                //Check if Postcode appears twice
                string Postcode = address[7];
                for (int j = 1; j <= 8; j++)
                {
                    if (j != 7)  //skip postcode line
                    {
                        string newString = "";
                        //check for postcode with space
                        int textPosition1 = address[j].ToUpper().IndexOf(Postcode);
                        if (textPosition1 != -1)
                        {
                            //MessageBox.Show("Found with space " + Postcode);
                            newString = address[j].SubstringSpecial(0, textPosition1) + address[j].Substring(textPosition1 + Postcode.Length)+" ";
                        }
                        //check for postcode with space
                        string PostcodeNoSpace = RemoveWhitespace(Postcode);

                        int textPosition2 = address[j].ToUpper().IndexOf(PostcodeNoSpace);
                        if (textPosition2 != -1)
                        {
                            //MessageBox.Show("Found " + PostcodeNoSpace);
                            newString = address[j].SubstringSpecial(0, textPosition2) + address[j].Substring(textPosition2 + PostcodeNoSpace.Length)+" ";
                        }
                        if (newString != "") {
                            address[j] = newString.Trim();
                        }
                    }
                }

                    //Un-capitalise and then capitalise first letters
                    for (int j = 0; j <= 8; j++)
                {
                    TextInfo newText = CultureInfo.CurrentCulture.TextInfo;
                    //convert to LowerCase First
                    if (j != 7) { address[j] = address[j].ToLowerInvariant(); } // miss postcode
                    //Convert to Title Case
                    address[j] = newText.ToTitleCase(address[j]);
                    //check for ands and ofs ????
                }

                //split into separate address lines
                string[] splitAddress = new string[12];
                for (int f = 0; f <= 11; f++) { splitAddress[f] = ""; } //make all empty strings to prevent null
                int counter = 0;
                string stringToSplit = "";

                    //Organisation first
                    stringToSplit = address[8].Trim();
                    if (stringToSplit != "") { splitAddress = splitString(splitAddress, stringToSplit, counter); }

                    //rest of address
                    for (int j = 3; j<= 7; j++)
                    {
                        stringToSplit = address[j].Trim();
                        if (stringToSplit != "")
                        {
                            //find counter
                            counter = findCounter(splitAddress);
                            splitAddress = splitString(splitAddress, stringToSplit, counter);
                        }

                    }


                //Put back Name
                address[0] = address[1] +" "+ address[2];

                //Put back address lines

                    //Put new Address back in right string

                    //find how many address lines you have
                    counter = findCounter(splitAddress);
                    if (counter > 6)
                    {
                    do
                    {
                        //recombine some
                        string compare1="";
                        string compare2 = "";
                        int bestFit = 0;
                        int lowestLength = 1000;
                        //find smallest combination
                        for (int f = 0; f <= (counter - 3); f++)
                        {
                            compare1 = splitAddress[f].Trim();
                            compare2 = splitAddress[f + 1].Trim();
                            if ((compare1.Length + compare2.Length) < lowestLength) { bestFit = f; lowestLength = (compare1.Length + compare2.Length); }
                        }
                        splitAddress[bestFit] = splitAddress[bestFit].Trim() + ", "+ splitAddress[bestFit + 1].Trim();
                        for (int f = (bestFit+1); f <= 10; f++) //shuffle the rest down
                        {
                            splitAddress[f] = splitAddress[f + 1];
                            splitAddress[f+1] = "";
                        }

                            //MessageBox.Show("Needs recombining - " + address[2]);
                        counter = findCounter(splitAddress);
                    } while (counter > 6);
                    }
                    for (int f = 3; f <= 8; f++) { address[f] = ""; }
                    counter = findCounter(splitAddress);
                    //last one should be Postcode
                    address[7] = splitAddress[counter - 1].ToUpper();
                    //first one to organisation
                    address[8] = splitAddress[0];
                    //and the rest
                    for (int f = 1; f <= (counter - 2); f++)
                    {
                        address[f+2] = splitAddress[f];
                    }

                //put the string back in the dataGrid
                for (int j = 0; j <= 8; j++)
                {
                    if (j == 0) {  dataGridViewAuto.Rows[i].Cells[j + 5].Value = address[j].Trim() ; }
                    else {  dataGridViewAuto.Rows[i].Cells[j + 10].Value = address[j].Trim(); }
                }

                

            }
        }
        

        private int findCounter(string[] sentString)
        {
            //find counter - finds first empty string in an array
            int counter = 0;
            for (int k = sentString.Length-1; k >= 0; k--)
            {
                try { if (string.IsNullOrEmpty(sentString[k].Trim())) { counter = k; } }
                catch { }
            }
            return counter;
        }

        private string[] splitString(string[] splitAddress, string stringToSplit, int startPosition)
        {
            Boolean breakLoop = false;
            do
            {
                int commaPosition = stringToSplit.IndexOf(",");
                if (commaPosition == -1)
                {
                    splitAddress[startPosition] = stringToSplit;
                    breakLoop = true;
                }
                else
                {
                    //look for comma following house number
                    int numberFound = 0;
                    string check = stringToSplit.SubstringSpecial(commaPosition - 1, commaPosition);
                    bool result = int.TryParse(check, out numberFound); //numberFound = number if it is one and result=true;
                    if (result)
                    {
                        //found number so change the comma so it doesn't keep triggering
                        stringToSplit = stringToSplit.SubstringSpecial(0, commaPosition) + " "+ stringToSplit.Substring(commaPosition + 1);
                    }
                    else
                    {
                        //no number so split in two
                        splitAddress[startPosition] = stringToSplit.SubstringSpecial(0, commaPosition);
                        stringToSplit = stringToSplit.Substring(commaPosition + 1);
                        startPosition++;
                    }
                }
            }
            while (!breakLoop);

                return splitAddress;
        }


        public static string RemoveWhitespace( string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }
        #endregion
        private void buttonColourMain_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text !="Choose a Colour")
            {
                textBoxColourMain.Text = comboBoxColours.Text;
                buttonColourMain.BackColor = Color.FromName(comboBoxColours.Text);
                colourQueueTab("no");
            }
        }

        private void buttonColourColour_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourColour.Text = comboBoxColours.Text;
                buttonColourColour.BackColor = Color.FromName(comboBoxColours.Text);
                colourQueueTab("no");
            }
        }

        private void buttonColourTrue_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourTrue.Text = comboBoxColours.Text;
                buttonColourTrue.BackColor = Color.FromName(comboBoxColours.Text);
            }
        }
        private void buttonColourHalfway_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourHalfway.Text = comboBoxColours.Text;
                buttonColourHalfway.BackColor = Color.FromName(comboBoxColours.Text);
            }
        }

        private void buttonColouFalse_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourFalse.Text = comboBoxColours.Text;
                buttonColourFalse.BackColor = Color.FromName(comboBoxColours.Text);
            }
        }

        private void button1ColourMainText_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourMainText.Text = comboBoxColours.Text;
                button1ColourMainText.BackColor = Color.FromName(comboBoxColours.Text);
            }
        }

        private void buttonColourColourText_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourColourText.Text = comboBoxColours.Text;
                buttonColourColourText.BackColor = Color.FromName(comboBoxColours.Text);
            }
        }

        

        private void buttonListOrders_Click_1(object sender, EventArgs e)
        {
            int countLines = 0;
            int countPlants = 0;

            string name = "";
        Boolean notfirstTime = false;
        int count = 0;
        sortAuto("Plant");
        string orders = "";

            for (int i = 0; i < dataGridViewAuto.RowCount - 1; i++)
            {
                if (dataGridViewAuto.Rows[i].Cells[6].Value.ToString().Trim() == name)
                {
                    count = count + int.Parse(dataGridViewAuto.Rows[i].Cells[7].Value.ToString());
                    orders = orders + "," + dataGridViewAuto.Rows[i].Cells[7].Value.ToString();
                }
                else
                {
                    orders = orders + ")";
                    if (notfirstTime)
                    {
                        listBoxOrders.Items.Add(name + " - " + count.ToString() + "  " + orders);
                        countLines++;
                        countPlants = countPlants + count;
                    }
                    count = 0;
                    notfirstTime = true;
                    orders = " (";
                    name = dataGridViewAuto.Rows[i].Cells[6].Value.ToString().Trim();
                    count = int.Parse(dataGridViewAuto.Rows[i].Cells[7].Value.ToString());
                    orders = orders + count.ToString();
                }
            }
            orders = orders + ")";
            listBoxOrders.Items.Add(name + " - " + count.ToString() + "  " + orders);
            countLines++;
            countPlants = countPlants + count;
            labelOrderLines.Text = countLines.ToString();
            labelOrderPlants.Text = countPlants.ToString();

            //list by order
            sortAuto("ON");

        }

        private void button18_Click_2(object sender, EventArgs e)
        {
            //Passport list creation

            //sort by customer
            //sortAuto("Customer");
            
            createPassportList("visible","Main");

            
        }

        private void UsingQueues()
        {
            string[] message = { "Queues","These Tabs hold 4 different Print Queues", "", "Any queue can print any label type, but they are designed in the following way:","",
                "Add the currently visible plant entry to the visible queue by pressing 'ENTER' whilst","the cursor is in the Price or Quantity box","",
                "If you are adding to the 'Main' queue, the entry will also add to the 'Colour' queue if","certain rules are met.","","Each queue defaults to a particular label as specified on the defaults Tab,","although this can be overidden at any time",
            "The 'Address' and 'Passport' Tabs are loaded with limited information when you use 'AutoLabel'","and are designed to print box and passport labels for dispatch."};
            FormInformation form = new FormInformation( message);
            form.Show();

        }

        private void howToUseQueuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UsingQueues();
        }

        private void buttonColourAddress_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourAddress.Text = comboBoxColours.Text;
                buttonColourAddress.BackColor = Color.FromName(comboBoxColours.Text);
                colourQueueTab("no");
            }
        }

        private void buttonColourPassport_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourPassport.Text = comboBoxColours.Text;
                buttonColourPassport.BackColor = Color.FromName(comboBoxColours.Text);
                colourQueueTab("no");
            }
        }

        private void buttonColourAddressText_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourAddressText.Text = comboBoxColours.Text;
                buttonColourAddressText.BackColor = Color.FromName(comboBoxColours.Text);
                colourQueueTab("no");
            }
        }

        private void buttonColourPassportText_Click(object sender, EventArgs e)
        {
            if (comboBoxColours.Text != "Choose a Colour")
            {
                textBoxColourPassportText.Text = comboBoxColours.Text;
                buttonColourPassportText.BackColor = Color.FromName(comboBoxColours.Text);
                colourQueueTab("no");
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (textBoxProducePassportLabel.Text == "False") { textBoxProducePassportLabel.Text = "True"; }
            else { textBoxProducePassportLabel.Text = "False"; }
        }

        private void buttonDefaultsInformation_Click(object sender, EventArgs e)
        {
            string[] message = { "Defaults", "These are the index numbers for the defaults string:","",
                    "12. Id","",
                    "Folder Paths and Files","    0. Picture Folder Path","    1. File Folder Path","   11. Autolabel file name","",
                    "Default Labels",
                    "    2. Main Queue Label","    3. Colour Queue Label","   31. Address Label","   32. Passport Label","   18. Alternative Label",
                    "","Autolabel Defaults",
                    "   21. Address sort order","   22. Orders sort order","   23. Include Courier oredrs in Address labels","   24. Address Label Quantity",
                    "   25. Automatically produce Address labels","   26. Automatically produce Passport labels",
                    "    6. Autolabel - use stated quantities","    7. Autolabel - use modified (batch) quantities",
                    "    8. Autolabel - produce unlocked orders only","    9. Autolabel - produce all orders regardless of lock","   10. Automatically clean up addresses",
                    "",
                "Colours","   13. Main Queue background - (windows colour name)","   19. Main Queue Text - (windows colour name)",
                    "   14. Colour Queue background - (windows colour name)","   20. Colour Queue Text - (windows colour name)",
                    "   27. Address Queue background - (windows colour name)","   28. Address Queue Text - (windows colour name)",
                    "   29. Passport Queue background - (windows colour name)","   30. Passport Queue Text - (windows colour name)",
                    "","   15. True Button Colour - (windows colour name)","   16. Halfway Button Colour - (windows colour name)","   17. False Button Colour - (windows colour name)"
            };


            showMessageForm(message);
        }

        private void comboBoxAddressLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDefaultAddressLabel.Text = comboBoxAddressLabel.Text;
        }

        private void comboBoxPassportLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxDefaultPassportLabel.Text = comboBoxPassportLabel.Text;
        }

        private void comboBoxAddressSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxAddressSort.Text = comboBoxAddressSort.Text;
        }

        private void comboBoxOrdersSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxOrderSort.Text = comboBoxOrdersSort.Text;
        }

        private void buttonIncludeCourierOrders_Click(object sender, EventArgs e)
        {
            if (textBoxIncludeCourier.Text == "False") { textBoxIncludeCourier.Text = "True"; }
            else { textBoxIncludeCourier.Text = "False"; }
        }

        private void buttonChangeAddressProduce_Click(object sender, EventArgs e)
        {
            if (textBoxProduceAddressLabel.Text == "False") { textBoxProduceAddressLabel.Text = "True"; }
            else { textBoxProduceAddressLabel.Text = "False"; }
        }

        private void buttonAddressQtyPlus_Click(object sender, EventArgs e)
        {
            int Qty = int.Parse(textBoxAddressLabelQty.Text.ToString().Trim());
            Qty = Qty + 1;
            textBoxAddressLabelQty.Text = Qty.ToString();
        }

        private void buttonAddressQtyMinus_Click(object sender, EventArgs e)
        {
            int Qty = int.Parse(textBoxAddressLabelQty.Text.ToString().Trim());
            if (Qty > 1)
            {
                Qty = Qty - 1;
                textBoxAddressLabelQty.Text = Qty.ToString();
            }
        }

        

        private void dataGridViewAddressQ_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
                fillQueueUtilitiesTab();
        }

        private void dataGridViewPassportQ_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (tabControlMain.SelectedTab == tabPageQueueUtilities)
                fillQueueUtilitiesTab();
        }

        public void showMessageFormWithDispose(string[] message, int timer)
        {
            FormInformation form = new FormInformation( message);
            form.Show();
            form.Refresh();
            Thread.Sleep(timer);
            form.Dispose();
            
        }

        public void showMessageForm(string[] message)
        {
            FormInformation form = new FormInformation(message);
            form.Show();
        }


        private void showSplashScreen()
        {
            PerPixelAlphaForm splash = new PerPixelAlphaForm();
            Bitmap picture = new Bitmap(@"D:\LabelMaker\LabelMaker\PictureFiles\splash.png");
            splash.SelectBitmap(picture);
            splash.Show();
            Thread.Sleep(750);
            splash.Dispose();
            
        }
    }
}
    