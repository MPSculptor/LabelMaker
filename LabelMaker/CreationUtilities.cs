using System;
using System.IO;
using System.Drawing;

namespace CreationUtilities
{
    //Handles all of the text manipulation routines

    public class TextOperations
    {

        public static string[] SplitText(string toSplit, int pieces)
        {
            string decisionText;
            //string partialText = "";
            int finalCount = 0;
            int finalPieces = pieces;
            int splits;
            string[] splitString = new string[2];

            //remove leading and trailiing spaces and asign
            decisionText = toSplit.Trim();

            //test that there are enough gaps to split into
            int testSpaces = TextOperations.CountGaps(decisionText);
            if ((testSpaces + 1) < pieces)
            {
                finalPieces = testSpaces + 1;
            }
            splits = finalPieces;
            string[] returnText = new string[(finalPieces)];


            for (int count = 0; count < (finalPieces - 1); count++)
            {

                int textLength = decisionText.Length;
                int textPiece = (textLength / splits);
                int textPositionStart = textPiece;


                char testCharPlus;
                char testCharMinus;
                bool state = false;
                bool testPlus = false;
                bool testMinus = false;

                testSpaces = TextOperations.CountGaps(decisionText);
                if ((testSpaces + 1) == splits)
                {
                    textPositionStart = 1;
                    //break;
                }

                //set after test for gaps
                int textPositionPlus = textPositionStart;
                int textPositionMinus = textPositionStart;
                int splitPosition = textPositionStart;
                int splitPositionPlus = splitPosition;
                int splitPositionMinus = splitPosition;


                while (state == false)
                {
                    testCharPlus = decisionText[textPositionPlus];
                    testCharMinus = decisionText[textPositionMinus];

                    //test upstring of position
                    if (testCharPlus == ' ')
                    {
                        splitPositionPlus = textPositionPlus;
                        state = true;
                        testPlus = true;
                    }
                    //test downstring of position
                    if (testCharMinus == ' ')
                    {
                        splitPositionMinus = textPositionMinus;
                        state = true;
                        testMinus = true;
                    }
                    textPositionPlus++;
                    if (textPositionPlus > (decisionText.Length - 1))
                    {
                        textPositionPlus = decisionText.Length;
                    }
                    textPositionMinus--;
                    if (textPositionMinus < 0)
                    {
                        textPositionMinus = 0;
                    }

                }

                if (testPlus == true)
                {
                    splitPosition = splitPositionPlus;
                }
                if (testMinus == true)
                {
                    splitPosition = splitPositionMinus;
                }

                //split Text in two
                splitString = TextOperations.SplitTextAtPoint(decisionText, splitPosition);
                //set return text segment
                returnText[count] = splitString[0].Trim();

                //trim of segment ready to go again
                decisionText = splitString[1].Trim();

                //set index for final segment assignment
                finalCount = count + 1;

                //reduce segements as we go
                splits--;
            }

            //set last segment
            returnText[finalCount] = decisionText;


            return returnText;
        }

        public static int CountGaps(string gapText)
        {

            int gaps = 0;
            bool space = false;

            for (int count = 0; count < gapText.Length; count++)
            {

                if (gapText[count] == ' ')
                {
                    if (space == false)
                    {
                        gaps++;
                        space = true;
                    }
                }
                else
                {
                    space = false;
                }
            }

            return gaps;

        }

        public static string[] SplitTextAtPoint(string toSplit, int splitPosition)
        {

            string[] returnText = new string[2];

            returnText[0] = toSplit.Substring(0, splitPosition);
            returnText[1] = toSplit.Substring(splitPosition);

            return returnText;
        }

    }

    //  **THIS IS A TEMPORARY CLASS **
    // Its purpose is to collect data for one label from a couple of temporary text files
    // replace by database reader in time



    public class dataReader
    {
        public static string[] readQueue( string[] sentData, string[] sentName, string[] moreData)
        {
            string[] queueData = new string[25];

            queueData[0] = sentName[0];
            queueData[1] = "2";
            queueData[2] = "£5.50";
            queueData[3] = sentData[9];
            queueData[4] = "Derek Wellington";
            queueData[5] = sentData[11];
            queueData[6] = sentData[8];
            queueData[7] = sentData[6];
            queueData[8] = moreData[0];
            queueData[9] = "Arial";
            queueData[10] = "4210688";
            queueData[11] = "1";
            queueData[12] = "0";
            queueData[13] = "4227072";
            queueData[14] = "16777183";
            queueData[15] = sentData[19];
            queueData[16] = sentName[1]; 
            queueData[17] = sentName[2];
            queueData[18] = sentName[3];
            queueData[19] = moreData[1];
            queueData[20] = sentData[12];
            queueData[21] = sentData[13];
            queueData[22] = sentData[14];
            queueData[23] = sentData[15];
            queueData[24] = "#9527";

            return queueData;
        }
        public static string[] readFile(string fileName)
        {
            try
            {
                string path = fileName;
                // Open the file to read from.
                string readInString = File.ReadAllText(path);
                //LabelMaker.Form1.Controls.richTextBox1.Text = outputString;

                //find out divisions and set string array to right size
                int pipeCount = 0;
                for (int i = 0; i < readInString.Length; i++)
                {
                    if (readInString[i] == '|')
                    {
                        pipeCount++;
                    }
                }
                string[] outputString = new string[pipeCount];

                //Add text from between each pipe char.to subsequent string lines

                int counter = 0;
                string addString = "";

                for (int i = 0; i < readInString.Length; i++)
                {
                    if (readInString[i] != '|')
                    {
                        addString = addString + readInString[i];
                    }
                    else
                    {
                        outputString[counter] = addString;
                        addString = "";
                        counter++;
                        //System.out.println(counter + " - " + outputString[counter]);
                    }
                }

                return outputString;

            }
            catch (IOException)
            {

                string[] outputString = new string[1];
                return outputString;
            }


        }

    }

    /// @author Martin
    /// This Routine handles the creation of a label
    /// which is printed with Graphics onto a panel Container
    /// </summary>

    
}
