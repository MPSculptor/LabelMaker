using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelMaker
{
    public class printDefaults : Object
    {

        // Holds different variables to pass to the Print Methods
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
            // h stands for header
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
