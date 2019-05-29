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
using System.Drawing.Printing;

namespace LabelMaker
{
    public partial class formLabel : Form
    {
        public formLabel(string[] queueData, string[] labelData, string[] defaultsString)
        {
            InitializeComponent();

            //Set up the label size and shape
            int labelWidth = int.Parse(labelData[0]);
            int labelHeight = int.Parse(labelData[1]);
            string widthString = labelWidth.ToString();
            string heightString = labelHeight.ToString();
            int finalWidth = 1;
            int finalHeight = 1;

            
            string orientation = "portrait";
            if (labelWidth > labelHeight)
            {
                orientation = "landscape";
            }

            switch (orientation)
            {
                case "portrait":
                    int Ysizep = this.ClientRectangle.Height - 20;
                    int Xsizep = Ysizep / labelHeight * labelWidth;
                    finalHeight = Ysizep;
                    finalWidth = Xsizep;
                    int plusWidth = this.Width - this.ClientRectangle.Width;
                    this.Width = Xsizep + 20 + plusWidth;
                    break;

                case "landscape":
                    int Xsizel = this.ClientRectangle.Width - 20;
                    int Ysizel = Xsizel / labelWidth * labelHeight;
                    finalHeight = Ysizel;
                    finalWidth = Xsizel;
                    int plusHeight = this.Height - this.ClientRectangle.Height;
                    this.Height = Ysizel + 20 + plusHeight; 
                    break;
            }
            this.Text = queueData[0] + " as " + labelData[2] + " ( " + orientation + " - " + heightString + " , " + widthString + " )";

            whereToNow whereTo = new whereToNow(queueData, labelData, defaultsString, finalWidth, finalHeight, "screen");
            whereTo.BackColor = Color.White;

            whereTo.Width = finalWidth;
            whereTo.Height = finalHeight;

            whereTo.Location = new Point(10, 10);
            return;
        }



        private void formLabel_Load(object sender, EventArgs e)
        {

        }
    }



    public class whereToNow : Panel
    {
        public whereToNow(string[] queueData, string[] labelData, string[] defaultsString, int sentWidth, int sentHeight, string printORscreen)
        {
            int contentWidth = sentWidth;
            int contentHeight = sentHeight;
            Panel cp = this;

            if (printORscreen == "print")
            {
                PrintDocument pd = new PrintDocument();

                pd.PrintPage += (sender, args) => DrawImage(queueData, labelData, defaultsString, sentWidth, sentHeight, sender, args);
                pd.Print();
            } 

            this.Paint += (sender2, e2) => whereToNow_Paint(sender2, e2, queueData, labelData, defaultsString, contentWidth, contentHeight, cp, printORscreen);
         
        }

        private void whereToNow_Paint(object sender, PaintEventArgs e, string[] queueData, string[] labelData, string[] defaultsString, int contentWidth, int contentHeight, Panel cp, string printORscreen)
        {
            if (printORscreen == "screen")
            {
                Graphics formGraphics;
                formGraphics = CreateGraphics();
                CreateLabel(queueData, labelData, defaultsString, contentWidth, contentHeight, formGraphics);
                formGraphics.Dispose();
            }
        }

        #region ***PRINTING***


        private void DrawImage(string[] queueData, string[] labelData, string[] defaultsString, int sentWidth, int sentHeight, object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            MessageBox.Show("Got to DrawImage");
            CreateLabel(queueData, labelData, defaultsString, sentWidth, sentHeight, e.Graphics);
            }

        public void PrintImage(string[] queueData, string[] labelData, string[] defaultsString, int sentWidth, int sentHeight)
        {

            
        }


        #endregion

        public void CreateLabel(string[] queueData, string[] labelData, string[] defaultsString, int contentWidth, int contentHeight, Graphics formGraphics)
        {

            //DISCOVER LABEL PARAMETERS
            //find name and split into fields
            string labelName = labelData[2];
            this.Text = this.Text + "  " + labelName;

            // fields = how many fields on the label
            int fields = 1;
            //datainputs = how many data items identify a field
            int dataInputs = 1;

            for (int i = 3; i < labelData.Length; i++)
            {
                if (labelData[i].Equals(labelName))
                {
                    fields++;
                }
                else
                {
                    if (fields == 1)
                    {
                        dataInputs++;
                    }
                }
            }

            //MAIN ROUTINE FOR ADDING LABEL FIELDS ONE BY ONE

            //iterate through fields
            for (int i = 0; i < fields; i++)
            {

                int jump = dataInputs;
                int start = 2 + (jump * i);

                //Collect queue data into variables
                string type = (labelData[start + 2]);
                ////Console.WriteLine(i + " - " + type);
                float xSize = float.Parse(labelData[start + 3]);
                float ySize = float.Parse(labelData[start + 4]);
                float xPos = float.Parse(labelData[start + 5]);
                float yPos = float.Parse(labelData[start + 6]);
                bool? isProfile = bool.Parse(labelData[start + 7]); //whether data from profile or special
                bool? isFontVariable = bool.Parse(labelData[start + 8]);
                bool? areLinesReduceable = bool.Parse(labelData[start + 9]);
                int listboxNo = int.Parse(labelData[start + 10]);
                string fixedValueString = (labelData[start + 11]); //such as border width etc
                int noLines = int.Parse(labelData[start + 12]);
                string justify = (labelData[start + 13]);
                bool? isFontColourProfile = bool.Parse(labelData[start + 14]);

                float fontSize = float.Parse(labelData[start + 16]);
                bool? isFontBold = bool.Parse(labelData[start + 17]);
                bool? isFontItalic = bool.Parse(labelData[start + 18]);
                string sentColour = (labelData[start + 19]);

                string profileTextColour = (queueData[10]);
                string profileBorderColour = (queueData[13]);
                string profileBackgroundColour = (queueData[14]);

                string fontName = "Arial"; // Backstop value
                if (isFontColourProfile.Value)
                {
                    //MessageBox.Show(labelData[start + 7] + " , " + labelData[start + 15]);
                    fontName = (labelData[start + 15]); // As label data
                    profileTextColour = ("0");
                }
                else
                {
                    fontName = queueData[9].Trim(); // As profile
                }
                

                //set justification as an integer
                int justifyInt = 0;
                switch (justify.Trim())
                {
                    case "left":
                        justifyInt = 0;
                        break;
                    case "right":
                        justifyInt = 2;
                        break;
                    case "center":
                        justifyInt = 1;
                        break;
                }

                //set content size and position
                int lines = 1; //set lines as 1 and then increase if necessary later
                float yPosd = 0;
                float xPosd = 0;
                float xSized = contentWidth * xSize / 100;
                float ySized = contentHeight * ySize / 100 / (lines);
                if (xPos == 0)
                {
                    //set as centred
                    xPosd = (contentWidth / 2) * (100 - xSize) / 100;
                }
                else
                {
                    xPosd = contentWidth * xPos / 100;
                }

                if (yPos == 0)
                {
                    yPosd = (contentHeight / 2) * (100 - ySize) / 100;
                }
                else
                {
                    yPosd = contentHeight * yPos / 100;
                }

                switch (type)
                {
                    case "text":

                        //load with custom text from label
                        string textToSend = labelData[start + 11];
                        if (isProfile.Value)
                        {
                            //if text comes from queue entry, load this instead
                            textToSend = queueData[listboxNo];
                        }

                        //Work out colour
                        Color colourFont = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(sentColour));
                        if (!isFontColourProfile.Value)
                        {
                            colourFont = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(profileTextColour));
                        }

                        string[] textToSendArray = new string[1];
                        textToSendArray[0] = textToSend;
                        if (noLines > 1)
                        {
                            //handle multi-line text
                            string[] labelReturned = new string[noLines];
                            labelReturned = CreationUtilities.TextOperations.SplitText(textToSend, noLines);
                            paintText(formGraphics, labelReturned, labelReturned.Length, xPosd, yPosd, xSized, ySized, justifyInt, fontName, fontSize, colourFont, isFontVariable.Value, areLinesReduceable.Value, isFontBold.Value, isFontItalic.Value, textToSend);
                        }
                        else
                        {
                            //send single line text
                            paintText(formGraphics, textToSendArray, 1, xPosd, yPosd, xSized, ySized, justifyInt, fontName, fontSize, colourFont, isFontVariable.Value, areLinesReduceable.Value, isFontBold.Value, isFontItalic.Value, textToSend);
                        }

                        break;

                    case "border":

                        Color borderColour = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(profileBorderColour));
                        float borderWidth = float.Parse(fixedValueString);
                        borderWidth = (contentWidth * borderWidth / 100);
                        paintBorder(formGraphics, xPosd, yPosd, xSized, ySized, borderWidth, borderColour);

                        break;

                    case "colourbox":

                        Color colourBoxColour = System.Drawing.ColorTranslator.FromHtml(CreationUtilities.TextOperations.getHexColour(profileBackgroundColour));
                        paintColourbox(formGraphics, xPosd, yPosd, xSized, ySized, colourBoxColour);

                        break;

                    case "image":

                        string pictureString = "";
                        if (isProfile.Value)
                        {
                            pictureString = (defaultsString[0] + queueData[listboxNo]);
                        }
                        else
                        {
                            pictureString = (defaultsString[1] + fixedValueString);
                        }
                        PaintImage(formGraphics, xPosd, yPosd, xSized, ySized, pictureString);

                        break;
                    default:
                        break;
                }
            }
        }

        

        public virtual void paintText(Graphics formGraphics, string[] textArray, int lines, float xPosd, float yPosd, float xSized, float ySized, int justify, string fontName, float fontSize, Color colourFont, bool isFontVariable, bool areLinesReduceable, bool isFontBold, bool isFontItalic, string textToSend)
        {
            float fontNewSize = 10;

            string fontForm = "";
            if (isFontBold)
            {
                fontForm = fontForm + "BOLD";
            }
            if (isFontItalic)
            {
                fontForm = fontForm + "ITALIC";
            }


            if (!isFontVariable)
            {
                fontNewSize = fontSize;
            }

            Pen p = new Pen(colourFont);
            Brush b = new SolidBrush(colourFont);

            float ffontSize = fontSize;
            FontFamily fFont = new FontFamily(fontName);
            FontStyle ffontStyle = new FontStyle();

            switch (fontForm)
            {
                case "BOLD":
                    ffontStyle  =  FontStyle.Bold;
                    break;
                case "ITALIC":
                    ffontStyle = FontStyle.Italic;
                    break;
                case "BOLDITALIC":
                    ffontStyle = FontStyle.Bold | FontStyle.Italic;
                    break;
                default:
                    ffontStyle = FontStyle.Regular;
                    break;
            }
            Font fontSet = new Font(fFont, ffontSize, ffontStyle);


            //GO FOR PRINTING
            float yOriginalSized = ySized;
            ySized = ySized / lines;

            double[] factorsToUse = new double[textArray.Length];

            //Find out the correct font size to use

            bool? haveLinesReduced = new bool?(false);
            int realNoLines = textArray.Length;

            if (areLinesReduceable && lines > 1)
            {
                //Console.WriteLine("***REDUCING LINES ROUTINE ***");
                haveLinesReduced = true;
                double bestFactor = 0;
                int bestLines = 1;
                for (int f = lines; f > 0; f--)
                {
                    string[] LabelReturnedText = new string[f];
                    LabelReturnedText = CreationUtilities.TextOperations.SplitText(textToSend, f);
                    
                    for (int i = 0; i < f; i++)
                    {
                        Font fontToSend = fontSet;

                        double? factorToUseD;
                        factorToUseD = sizeGraphicText(formGraphics, LabelReturnedText[i], fontSet, xSized, (yOriginalSized / f));

                        factorsToUse[i] = factorToUseD.Value;
                    }
                    double returnedFactor = 0;
                    if (isFontVariable)
                    {
                        double averageFactor = 0;
                        //Console.WriteLine("Font IS Variable. Lines - " + LabelReturnedText.Length);
                        for (int x = 0; x < (LabelReturnedText.Length); x++)
                        {
                            averageFactor = averageFactor + factorsToUse[x];
                            //Console.WriteLine("Average Factor = " + averageFactor);
                        }
                        returnedFactor = averageFactor / (LabelReturnedText.Length);

                    }
                    else
                    {
                        //Console.WriteLine("Font NOT Variable");
                        double averageFactor = 99999999;
                        for (int x = 0; x < (LabelReturnedText.Length); x++)
                        {
                            if (averageFactor > factorsToUse[x])
                            {
                                averageFactor = factorsToUse[x];
                            }
                        }
                        returnedFactor = averageFactor;
                    }

                    if (returnedFactor > bestFactor)
                    {
                        bestFactor = returnedFactor;
                        bestLines = f;
                    }
                }

                string[] labelReturned = new string[bestLines];
                labelReturned = CreationUtilities.TextOperations.SplitText(textToSend, bestLines);

                for (int i = 0; i < (labelReturned.Length); i++)
                {
                    Font fontToSend = fontSet;

                    double? factorToUseD;
                    factorToUseD = sizeGraphicText(formGraphics, labelReturned[i], fontSet, xSized, (yOriginalSized / (labelReturned.Length)));

                    factorsToUse[i] = factorToUseD.Value;
                }
                realNoLines = labelReturned.Length;

                ySized = yOriginalSized / realNoLines;

                for (int i = 0; i < (labelReturned.Length); i++)
                {
                    textArray[i] = labelReturned[i];
                }


            }
            else
            {
                for (int i = 0; i < (textArray.Length); i++)
                {
                    Font fontToSend = fontSet;

                    double? factorToUseD;
                    factorToUseD = sizeGraphicText(formGraphics, textArray[i], fontSet, xSized, ySized);

                    factorsToUse[i] = factorToUseD.Value;
                }
            }

            double? factorToUse;
            
            //Set all fonts to the same if required
            if (!isFontVariable)
            {
                double smallestFactor = (double)99999;
                for (int i = 0; i < (textArray.Length); i++)
                {
                    if (factorsToUse[i] < smallestFactor)
                    {
                        smallestFactor = factorsToUse[i];
                    }
                }
                for (int i = 0; i < (textArray.Length); i++)
                {
                    factorsToUse[i] = smallestFactor;
                }
            }


            for (int i = 0; i < (realNoLines); i++)
            {
                factorToUse = factorsToUse[i];
                // create new font using sized data 
                float useThisFontSize = fontSize * (float)factorToUse;
                Font useThisFont = new Font(fontName, useThisFontSize, ffontStyle);

                //find size and find positions
                Size proposedSize = new Size(int.MaxValue, int.MaxValue);

                Size textSize = TextRenderer.MeasureText(textArray[i], useThisFont);
                
                float newWidth = textSize.Width; 
                float newHeight = textSize.Height;

                //Set horizontal position
                float xxPosd = 0;
                
                switch (justify)
                {
                    case 0:
                        xxPosd = xPosd;
                        break;
                    case 1:
                        xxPosd = xPosd + ((xSized - newWidth) / 2);
                        break;
                    case 2:
                        xxPosd = xPosd + xSized - newWidth;
                        break;
                }
                
                //Centre Height
                float yySized = ySized;
                float yyPosd = (yPosd +  (i * yySized));
                yyPosd = (yyPosd + ((yySized - newHeight) / 2));

                //Draw the Text on the Object
                Point P = new Point((int)xxPosd, (int)yyPosd);
                TextRenderer.DrawText(formGraphics, textArray[i], useThisFont, P,colourFont);//, xxPosd, yyPosd);
            
                useThisFont.Dispose();
            }
            p.Dispose();
            b.Dispose();
            }

        public virtual double? sizeGraphicText(Graphics formGraphics, String textSent, Font fontSent, float xSized, float ySized)
        {
            //PRODUCES A SCALE FACTOR TO MULTIPLY THE DEFAULT FONT.

            formGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            StringFormat format = StringFormat.GenericTypographic;
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

            // Set up string.
            string measureString = textSent;
            Font firstFont = fontSent;

            // Set maximum layout size.
            SizeF layoutSize = new SizeF(10 * xSized, 10 * ySized);

            // Measure string.
            SizeF stringSize = new SizeF();
                        
            stringSize = TextRenderer.MeasureText(measureString , firstFont);

            //Get sizes
            float width = stringSize.Width;
            float height = stringSize.Height;

            //compare to sizes wanted
            double factorToUse = 0;
            double xFactor = ((double)xSized / (double)width);
            double yFactor = ((double)ySized / (double)height);

            //int newFontSize = 0;
            if (xFactor < yFactor)
            {
                factorToUse = xFactor;
            }
            else
            {
                factorToUse = yFactor;
            }
            
            if (width == 0) { factorToUse = 0.01; }
            if (height == 0 ) { factorToUse = 0.01; }

            return factorToUse;
        }

        public virtual void paintBorder(Graphics formGraphics, float xPosd, float yPosd, float xSized, float ySized, float borderWidth, Color definedColour)
        {
            Pen p = new Pen(definedColour);
            p.Width = borderWidth;
            Rectangle r = new Rectangle((int)xPosd, (int)yPosd, (int)xSized, (int)ySized);
            formGraphics.DrawRectangle(p, r);
            p.Dispose();
        }

        public virtual void paintColourbox(Graphics formGraphics, float xPosd, float yPosd, float xSized, float ySized, Color definedColour)
        {
            SolidBrush myBrush = new SolidBrush(definedColour);
            formGraphics.FillRectangle(myBrush, new Rectangle((int)xPosd, (int)yPosd, (int)xSized, (int)ySized));
            myBrush.Dispose();
        }

        public virtual void PaintImage(Graphics formGraphics, float xPosd, float yPosd, float xSized, float ySized, string imageFile)
        {
            try
            {
                Image imageToDraw = Image.FromFile(imageFile);
                formGraphics.DrawImage(imageToDraw, xPosd, yPosd, xSized, ySized );
                imageToDraw.Dispose();
            }
            catch (IOException)
            {
            }


        }

    }
 }
