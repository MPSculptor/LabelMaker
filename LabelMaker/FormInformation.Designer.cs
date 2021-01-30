namespace LabelMaker
{
    partial class FormInformation
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBoxInformation = new System.Windows.Forms.RichTextBox();
            this.buttonHide = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // richTextBoxInformation
            // 
            this.richTextBoxInformation.BackColor = System.Drawing.SystemColors.Control;
            this.richTextBoxInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxInformation.Location = new System.Drawing.Point(12, 12);
            this.richTextBoxInformation.Name = "richTextBoxInformation";
            this.richTextBoxInformation.Size = new System.Drawing.Size(226, 226);
            this.richTextBoxInformation.TabIndex = 0;
            this.richTextBoxInformation.Text = "";
            // 
            // buttonHide
            // 
            this.buttonHide.Location = new System.Drawing.Point(402, 215);
            this.buttonHide.Name = "buttonHide";
            this.buttonHide.Size = new System.Drawing.Size(75, 23);
            this.buttonHide.TabIndex = 1;
            this.buttonHide.Text = "Hide";
            this.buttonHide.UseVisualStyleBackColor = true;
            this.buttonHide.Click += new System.EventHandler(this.buttonHide_Click);
            // 
            // FormInformation
            // 
            this.ClientSize = new System.Drawing.Size(489, 250);
            this.Controls.Add(this.buttonHide);
            this.Controls.Add(this.richTextBoxInformation);
            this.Name = "FormInformation";
            this.Load += new System.EventHandler(this.FormInformation_Load);
            this.ResumeLayout(false);

        }

        #endregion

        
        private System.Windows.Forms.RichTextBox richTextBoxInformation;
        private System.Windows.Forms.Button buttonHide;
    }
}