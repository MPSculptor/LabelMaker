using System;
using System.Drawing.Printing;

namespace LabelMaker
{
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

        public printDefaults() { }

        public printDefaults(string[] hlabelData, string hwhichQueue, int hhowManyLines, string[] hdefaultsString, string[] hprinterDetails, PaperSource hpaperSource, string[,] hwholeQueue, int hlabelCount)
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

        }
    }
}
