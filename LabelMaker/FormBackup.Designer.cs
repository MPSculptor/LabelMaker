namespace LabelMaker
{
    partial class FormBackup
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
            this.groupBoxTables = new System.Windows.Forms.GroupBox();
            this.buttonList = new System.Windows.Forms.Button();
            this.flowLayoutPanelTables = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonClear = new System.Windows.Forms.Button();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.buttonChooseRestore = new System.Windows.Forms.Button();
            this.buttonDoAction = new System.Windows.Forms.Button();
            this.radioButtonRestoreTablesAdd = new System.Windows.Forms.RadioButton();
            this.radioButtonRestoreTablesOver = new System.Windows.Forms.RadioButton();
            this.radioButtonBackup = new System.Windows.Forms.RadioButton();
            this.groupBoxProperties = new System.Windows.Forms.GroupBox();
            this.labelRestoreFile = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.labelBackup = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.labelDeploy = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labelConString = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBoxTables.SuspendLayout();
            this.groupBoxActions.SuspendLayout();
            this.groupBoxProperties.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxTables
            // 
            this.groupBoxTables.Controls.Add(this.buttonList);
            this.groupBoxTables.Controls.Add(this.flowLayoutPanelTables);
            this.groupBoxTables.Controls.Add(this.buttonClear);
            this.groupBoxTables.Controls.Add(this.buttonSelect);
            this.groupBoxTables.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxTables.Location = new System.Drawing.Point(13, 136);
            this.groupBoxTables.Name = "groupBoxTables";
            this.groupBoxTables.Size = new System.Drawing.Size(417, 302);
            this.groupBoxTables.TabIndex = 0;
            this.groupBoxTables.TabStop = false;
            this.groupBoxTables.Text = "Tables";
            // 
            // buttonList
            // 
            this.buttonList.Location = new System.Drawing.Point(276, 146);
            this.buttonList.Name = "buttonList";
            this.buttonList.Size = new System.Drawing.Size(116, 32);
            this.buttonList.TabIndex = 3;
            this.buttonList.Text = "List Tables";
            this.buttonList.UseVisualStyleBackColor = true;
            this.buttonList.Click += new System.EventHandler(this.buttonList_Click);
            // 
            // flowLayoutPanelTables
            // 
            this.flowLayoutPanelTables.Location = new System.Drawing.Point(17, 29);
            this.flowLayoutPanelTables.Name = "flowLayoutPanelTables";
            this.flowLayoutPanelTables.Size = new System.Drawing.Size(200, 100);
            this.flowLayoutPanelTables.TabIndex = 2;
            // 
            // buttonClear
            // 
            this.buttonClear.Location = new System.Drawing.Point(276, 75);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(116, 32);
            this.buttonClear.TabIndex = 1;
            this.buttonClear.Text = "Clear All";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // buttonSelect
            // 
            this.buttonSelect.Location = new System.Drawing.Point(276, 37);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(116, 32);
            this.buttonSelect.TabIndex = 0;
            this.buttonSelect.Text = "Select All";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Controls.Add(this.buttonChooseRestore);
            this.groupBoxActions.Controls.Add(this.buttonDoAction);
            this.groupBoxActions.Controls.Add(this.radioButtonRestoreTablesAdd);
            this.groupBoxActions.Controls.Add(this.radioButtonRestoreTablesOver);
            this.groupBoxActions.Controls.Add(this.radioButtonBackup);
            this.groupBoxActions.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxActions.Location = new System.Drawing.Point(448, 148);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Size = new System.Drawing.Size(340, 498);
            this.groupBoxActions.TabIndex = 1;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "Actions";
            // 
            // buttonChooseRestore
            // 
            this.buttonChooseRestore.Location = new System.Drawing.Point(182, 248);
            this.buttonChooseRestore.Name = "buttonChooseRestore";
            this.buttonChooseRestore.Size = new System.Drawing.Size(118, 89);
            this.buttonChooseRestore.TabIndex = 5;
            this.buttonChooseRestore.Text = "Choose Restore Folder";
            this.buttonChooseRestore.UseVisualStyleBackColor = true;
            this.buttonChooseRestore.Click += new System.EventHandler(this.buttonChooseRestore_Click);
            // 
            // buttonDoAction
            // 
            this.buttonDoAction.Location = new System.Drawing.Point(20, 248);
            this.buttonDoAction.Name = "buttonDoAction";
            this.buttonDoAction.Size = new System.Drawing.Size(131, 89);
            this.buttonDoAction.TabIndex = 4;
            this.buttonDoAction.Text = "Perform Backup or Restore";
            this.buttonDoAction.UseVisualStyleBackColor = true;
            this.buttonDoAction.Click += new System.EventHandler(this.buttonDoAction_Click);
            // 
            // radioButtonRestoreTablesAdd
            // 
            this.radioButtonRestoreTablesAdd.AutoSize = true;
            this.radioButtonRestoreTablesAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonRestoreTablesAdd.Location = new System.Drawing.Point(20, 183);
            this.radioButtonRestoreTablesAdd.Name = "radioButtonRestoreTablesAdd";
            this.radioButtonRestoreTablesAdd.Size = new System.Drawing.Size(315, 28);
            this.radioButtonRestoreTablesAdd.TabIndex = 2;
            this.radioButtonRestoreTablesAdd.Text = "Restore Selected Tables (additive)";
            this.radioButtonRestoreTablesAdd.UseVisualStyleBackColor = true;
            this.radioButtonRestoreTablesAdd.CheckedChanged += new System.EventHandler(this.radioButtonRestoreTablesAdd_CheckedChanged);
            // 
            // radioButtonRestoreTablesOver
            // 
            this.radioButtonRestoreTablesOver.AutoSize = true;
            this.radioButtonRestoreTablesOver.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonRestoreTablesOver.Location = new System.Drawing.Point(20, 149);
            this.radioButtonRestoreTablesOver.Name = "radioButtonRestoreTablesOver";
            this.radioButtonRestoreTablesOver.Size = new System.Drawing.Size(327, 28);
            this.radioButtonRestoreTablesOver.TabIndex = 1;
            this.radioButtonRestoreTablesOver.Text = "Restore Selected Tables (overwrite)";
            this.radioButtonRestoreTablesOver.UseVisualStyleBackColor = true;
            this.radioButtonRestoreTablesOver.CheckedChanged += new System.EventHandler(this.radioButtonRestoreTablesOver_CheckedChanged);
            // 
            // radioButtonBackup
            // 
            this.radioButtonBackup.AutoSize = true;
            this.radioButtonBackup.Checked = true;
            this.radioButtonBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButtonBackup.Location = new System.Drawing.Point(20, 39);
            this.radioButtonBackup.Name = "radioButtonBackup";
            this.radioButtonBackup.Size = new System.Drawing.Size(174, 28);
            this.radioButtonBackup.TabIndex = 0;
            this.radioButtonBackup.TabStop = true;
            this.radioButtonBackup.Text = "Backup Database";
            this.radioButtonBackup.UseVisualStyleBackColor = true;
            // 
            // groupBoxProperties
            // 
            this.groupBoxProperties.BackColor = System.Drawing.Color.White;
            this.groupBoxProperties.Controls.Add(this.labelRestoreFile);
            this.groupBoxProperties.Controls.Add(this.label5);
            this.groupBoxProperties.Controls.Add(this.labelBackup);
            this.groupBoxProperties.Controls.Add(this.label3);
            this.groupBoxProperties.Controls.Add(this.labelDeploy);
            this.groupBoxProperties.Controls.Add(this.label4);
            this.groupBoxProperties.Controls.Add(this.labelConString);
            this.groupBoxProperties.Controls.Add(this.label1);
            this.groupBoxProperties.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBoxProperties.Location = new System.Drawing.Point(13, 13);
            this.groupBoxProperties.Name = "groupBoxProperties";
            this.groupBoxProperties.Size = new System.Drawing.Size(775, 117);
            this.groupBoxProperties.TabIndex = 2;
            this.groupBoxProperties.TabStop = false;
            this.groupBoxProperties.Text = "Properties";
            // 
            // labelRestoreFile
            // 
            this.labelRestoreFile.AutoSize = true;
            this.labelRestoreFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelRestoreFile.Location = new System.Drawing.Point(158, 92);
            this.labelRestoreFile.Name = "labelRestoreFile";
            this.labelRestoreFile.Size = new System.Drawing.Size(93, 16);
            this.labelRestoreFile.TabIndex = 7;
            this.labelRestoreFile.Text = "none selected";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(34, 92);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 16);
            this.label5.TabIndex = 6;
            this.label5.Text = "Restore File : ";
            // 
            // labelBackup
            // 
            this.labelBackup.AutoSize = true;
            this.labelBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelBackup.Location = new System.Drawing.Point(158, 71);
            this.labelBackup.Name = "labelBackup";
            this.labelBackup.Size = new System.Drawing.Size(45, 16);
            this.labelBackup.TabIndex = 5;
            this.labelBackup.Text = "label2";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(34, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Backup Directory : ";
            // 
            // labelDeploy
            // 
            this.labelDeploy.AutoSize = true;
            this.labelDeploy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDeploy.Location = new System.Drawing.Point(158, 51);
            this.labelDeploy.Name = "labelDeploy";
            this.labelDeploy.Size = new System.Drawing.Size(0, 16);
            this.labelDeploy.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(34, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Data Directory : ";
            // 
            // labelConString
            // 
            this.labelConString.AutoSize = true;
            this.labelConString.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelConString.Location = new System.Drawing.Point(158, 30);
            this.labelConString.Name = "labelConString";
            this.labelConString.Size = new System.Drawing.Size(45, 16);
            this.labelConString.TabIndex = 1;
            this.labelConString.Text = "label2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "ConnectionString : ";
            // 
            // FormBackup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 658);
            this.Controls.Add(this.groupBoxProperties);
            this.Controls.Add(this.groupBoxActions);
            this.Controls.Add(this.groupBoxTables);
            this.Name = "FormBackup";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.FormBackup_Load);
            this.groupBoxTables.ResumeLayout(false);
            this.groupBoxActions.ResumeLayout(false);
            this.groupBoxActions.PerformLayout();
            this.groupBoxProperties.ResumeLayout(false);
            this.groupBoxProperties.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxTables;
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.GroupBox groupBoxProperties;
        private System.Windows.Forms.Label labelConString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelDeploy;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelTables;
        private System.Windows.Forms.Button buttonList;
        private System.Windows.Forms.Label labelBackup;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radioButtonRestoreTablesAdd;
        private System.Windows.Forms.RadioButton radioButtonRestoreTablesOver;
        private System.Windows.Forms.RadioButton radioButtonBackup;
        private System.Windows.Forms.Button buttonChooseRestore;
        private System.Windows.Forms.Button buttonDoAction;
        private System.Windows.Forms.Label labelRestoreFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
    }
}