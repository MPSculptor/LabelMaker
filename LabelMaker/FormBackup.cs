using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Deployment.Application;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabelMaker
{
    public partial class FormBackup : Form
    {
        public FormBackup(string conString, string backupDir)
        {
            InitializeComponent();

            int gap = 20;
            int scale = 66;
            Point point = new Point(100, 100);
            this.Width = (Screen.GetWorkingArea(point).Width - 50)*scale/100;
            this.Height = (Screen.GetWorkingArea(point).Height - 50)*scale/100;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Backup and Restore Utility";
            labelConString.Text = conString;
            labelBackup.Text = backupDir;
            int widthToUse = this.ClientSize.Width;
            int heightToUse = this.ClientSize.Height;
            groupBoxProperties.Top = gap;
            groupBoxProperties.Left = gap;
            groupBoxProperties.Width = widthToUse - (gap * 2);

            groupBoxTables.Width = ((widthToUse - (gap * 2))/2);
            groupBoxTables.Top = groupBoxProperties.Bottom + gap;
            groupBoxTables.Left = gap;
            groupBoxTables.Height = heightToUse - groupBoxTables.Top - gap;

            groupBoxActions.Left = groupBoxTables.Right + gap;
            groupBoxActions.Top = groupBoxTables.Top;
            groupBoxActions.Height = groupBoxTables.Height;
            groupBoxActions.Width = widthToUse - gap - groupBoxActions.Left;

            flowLayoutPanelTables.Controls.Clear();

            groupBoxTables.Controls.Add(buttonSelect);
            groupBoxTables.Controls.Add(buttonClear);
            buttonSelect.Top = gap*3;
            buttonSelect.Left = gap;
            buttonClear.Left = buttonSelect.Left;
            buttonClear.Top = buttonSelect.Bottom + gap;
            buttonList.Left = buttonSelect.Left;
            buttonList.Top = buttonClear.Bottom + gap + gap;

            
            List<string> TableNames = GetDatabaseTabels(conString);

            //int count = TableNames.Count;
            
            flowLayoutPanelTables.Height = groupBoxTables.Height - (gap*3);
            flowLayoutPanelTables.Left = buttonSelect.Right+gap;
            flowLayoutPanelTables.Top = gap*2;
            flowLayoutPanelTables.Width = groupBoxTables.Width - flowLayoutPanelTables.Left - -gap - gap;
            flowLayoutPanelTables.AutoScroll = true;
            flowLayoutPanelTables.AutoSize = false;
            flowLayoutPanelTables.FlowDirection = FlowDirection.TopDown;

            

            int counter = 0;

            CheckBox[] checkBoxTables = new CheckBox[TableNames.Count];
            foreach (string s in TableNames)
            {
                checkBoxTables[counter] = new CheckBox();
                Font checkFont = new Font("Calibri", 12);
                checkBoxTables[counter].Text = s;
                checkBoxTables[counter].Font = checkFont;
                checkBoxTables[counter].AutoSize = true;
                //checkBoxTables[counter].Width = 300;
                checkBoxTables[counter].Name = s;
                checkBoxTables[counter].Checked = true;
                flowLayoutPanelTables.Controls.Add(checkBoxTables[counter]);
                counter++;
            }
                        
            try { labelDeploy.Text = ApplicationDeployment.CurrentDeployment.DataDirectory; }
            catch { labelDeploy.Text  = "Not a Deployed version so no Data Directory"; }
            


        }

        public List<string> GetDatabaseTabels(string connectionString)
        {
            List<string> TableNames = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                DataTable schema = connection.GetSchema("Tables");

                foreach (DataRow row in schema.Rows)
                {
                    TableNames.Add(row[2].ToString());
                }
            }

            return TableNames;       

        }

        private void FormBackup_Load(object sender, EventArgs e)
        {

        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            foreach(CheckBox cb in flowLayoutPanelTables.Controls)
            {
                cb.Checked = true;
            }
                
            }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            foreach (CheckBox cb in flowLayoutPanelTables.Controls)
            {
                cb.Checked = false;
            }
        }

        private void buttonList_Click(object sender, EventArgs e)
        {
            List<string> TableNames = new List<string>();
            
            foreach (CheckBox cb in flowLayoutPanelTables.Controls)
            {
                if (cb.Checked == true)
                {
                    TableNames.Add(cb.Text);
                }
            }
            int count = TableNames.Count;
            int offset = 4;
            string[] AllNames = new string[count + offset];
            int counter = offset;
            foreach (string s in TableNames)
            {
                AllNames[counter] = s;
                counter++;
            }
            AllNames[0] = "Selected Database Tables";
            AllNames[1] = "Here is a list of the Tables selected from your Database";
            AllNames[2] = "";
            AllNames[3] = "";

            FormInformation Tables = new FormInformation(AllNames);
            Tables.Show();
        }

        private void buttonChooseRestore_Click(object sender, EventArgs e)
        {
            
            string filePlace = labelBackup.Text ;

            openFileDialog1.InitialDirectory = filePlace;
            //openFileDialog1.Filter = "CSV (Comma Delimited) (*.csv)|*.csv";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) { labelRestoreFile.Text = openFileDialog1.FileName; }
        }

        private void buttonDoAction_Click(object sender, EventArgs e)
        {
            List<string> ActionAndTables = returnActionAndTables();

            string[] AllNames = new string[ActionAndTables.Count+1];
            int counter = 1;
            AllNames[0] = "Selected Backup and Restore Actions";
            foreach (string s in ActionAndTables)
            {
                AllNames[counter] = s;
                counter++;
            }

            FormInformation Tables = new FormInformation(AllNames);
            Tables.Show();
        }

        private List<string> returnActionAndTables()
        { 
            String Action = "";
            if (radioButtonBackup.Checked == true) { Action = "Backup"; }
            if (radioButtonRestoreAll.Checked == true) { Action = "RestoreAll"; }
            if (radioButtonRestoreTablesOver.Checked == true) { Action = "SelectedOverwrite"; }
            if (radioButtonRestoreTablesAdd.Checked == true) { Action = "SelectedAdditive"; }

            List<string> TableNames = new List<string>();
            if (Action != "")
            {
                TableNames.Add(Action);
                TableNames.Add(labelConString.Text);
                TableNames.Add(labelBackup.Text);
                TableNames.Add(labelRestoreFile.Text);

                foreach (CheckBox cb in flowLayoutPanelTables.Controls)
                {
                    if (cb.Checked == true)
                    {
                        TableNames.Add(cb.Text);
                    }
                }

            }
            else
            {
                TableNames.Add("No Action Selected");
            }

            return TableNames;
        }
    }
    }

