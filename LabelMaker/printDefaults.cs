using System;
using System.Drawing.Printing;

namespace LabelMaker
{

    static class canIusePrinter
    {
        public static string[] printerList;
        public static Boolean[] inUseOrNot;

        static int _getsetprinterList;
        public static int getsetprinterList
        {
            set { _getsetprinterList = value; }
            get { return _getsetprinterList; }
        }

        static int _getsetinUseOrNot;
        public static int getsetinUseOrNot
        {
            set { _getsetinUseOrNot = value; }
            get { return _getsetinUseOrNot; }
        }

        public static void getPrinterList()
        {
            int counter = 0;
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                counter++;
            }
            canIusePrinter.printerList = new string[counter];
            canIusePrinter.inUseOrNot = new Boolean[counter];
            counter = 0;
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                canIusePrinter.printerList[counter] = printer.ToString();
                canIusePrinter.inUseOrNot[counter] = false;
                counter++;
            }
        }

        public static void alterPrinterStatus(string printerName, Boolean whichWay)
        {
            for (int i = 0; i < canIusePrinter.printerList.Length; i++)
            {
                if (printerName.Trim() == canIusePrinter.printerList[i].Trim())
                {
                    canIusePrinter.inUseOrNot[i] = whichWay;
                }
            }
        }

        public static int getPrinterIndex(string printerName)
        {
            int index = 0;
            for (int i = 0; i < canIusePrinter.printerList.Length; i++)
            {
                if (printerName.Trim() == canIusePrinter.printerList[i].Trim())
                {
                    index =i;
                }
            }
            return index;
        }
    }

    public class printDefaults : Object
    {

        // Holds different variables to pass to the Print Methods so that it can all be sent as one object
        public string[] labelData { get; set; }
        public string whichQueue { get; set; }
        public int howManyLines { get; set; }
        public string[] defaultsString { get; set; }
        public string[] printerDetails { get; set; }
        public PaperSource paperSource { get; set; }
        public string[,] wholeQueue { get; set; }
        public int labelCount { get; set; }
        public int printerListIndex { get; set; }

        public printDefaults() { }

        public printDefaults(string[] hlabelData, string hwhichQueue, int hhowManyLines, string[] hdefaultsString, string[] hprinterDetails, PaperSource hpaperSource, string[,] hwholeQueue, int hlabelCount,int hprinterListIndex)
        {
            // h stands for header just for readability
            labelData = hlabelData;
            whichQueue = hwhichQueue;
            howManyLines = hhowManyLines;
            defaultsString = hdefaultsString;
            printerDetails = hprinterDetails;
            paperSource = hpaperSource;
            wholeQueue = hwholeQueue;
            labelCount = hlabelCount;
            printerListIndex = hprinterListIndex;

        }
    }

    public class quickPrintDefaults : Object
    {

        // Holds different variables to pass to quickPrint so that it can all be sent as one object and it can print as a thread (this allows it to wait for an empty printer in the background)
        public string[] queueString { get; set; }
        public string[] defaultsString { get; set; }
        public string[] labelString { get; set; }
        public string[] headerString  {get; set;}
        

        public quickPrintDefaults() { }

        public quickPrintDefaults(string[] hqueueString, string[] hdefaultsString, string[] hlabelString, string[] hheaderString)
        {
            // h stands for header just for readability
            queueString = hqueueString;
            defaultsString = hdefaultsString;
            labelString = hlabelString;
            headerString = hheaderString;

        }
    }
}
