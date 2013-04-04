using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Bechtel.iRING.SPRUtility;
using System.IO;

namespace Bechtel.iRING.SPRUtility
{
    public partial class FrmSPRSynchronizationUtility : Form
    {
        SPRSynchronizationUtility syncUtility = null;
        StreamWriter logFile = null;

        public FrmSPRSynchronizationUtility()
        {
            InitializeComponent();

            if (!File.Exists("Log.txt"))
                logFile = new StreamWriter("Log.txt");
            else
                logFile = File.AppendText("Log.txt");

            logFile.WriteLine("Start Time:" + DateTime.Now);
            syncUtility = new SPRSynchronizationUtility(logFile);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
            SaveLoggingFile();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMdbName.Text))
                {
                    MessageBox.Show("Please select the Mdb File.");
                    return;
                }

                string selectedObject = ((org.iringtools.library.DataObject)(cboxCommodities.Items[cboxCommodities.SelectedIndex])).objectName.ToString();

                if (!string.IsNullOrEmpty(selectedObject))
                {
                    lblStatus.Text = "Processing.... please wait";
                    Application.DoEvents();

                    syncUtility.MDBSynchronization(selectedObject);

                    lblStatus.Text = "Completed";
                    Application.DoEvents();
                }
                else
                {
                    MessageBox.Show("Please select the component.");
                    return;
                }

                DialogResult dialogResult = MessageBox.Show("Mdb file updated for component : '" + selectedObject + "'. Do you want to continue?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dialogResult == DialogResult.No)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Exception Raised";
                Application.DoEvents();

                logFile.WriteLine(ex.Message + ex.StackTrace);
                MessageBox.Show("Exception occured please check logFile: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SaveLoggingFile();
            }
        }

        private void FrmSPRSynchronizationUtility_Load(object sender, EventArgs e)
        {
            List<org.iringtools.library.DataObject> objects = syncUtility.GetObjects();
            List<string> items = new List<string>();
            cboxCommodities.Items.Clear();
            cboxCommodities.Items.Add(string.Empty);
            cboxCommodities.DataSource = objects;

            cboxCommodities.DisplayMember = "objectName";
            cboxCommodities.ValueMember = "objectName";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string InitialbasePath = Directory.GetCurrentDirectory();
            string CurrentbasePath = string.Empty;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CurrentbasePath = Directory.GetCurrentDirectory();
                string fileToOpen = openFileDialog.FileName;
                string fileExtension = fileToOpen.Substring(fileToOpen.LastIndexOf('.') + 1);
                if (fileExtension.ToUpper() != "MDB2")
                {
                    MessageBox.Show("Please select the file with Mdb2 extension");
                    return;
                }
                txtMdbName.Text = fileToOpen;
                syncUtility.UpdateMdbFile(fileToOpen);
            }

            if (InitialbasePath != CurrentbasePath)                 // In XP both path would be different.
                Directory.SetCurrentDirectory(InitialbasePath);
        }

        private void SaveLoggingFile()
        {
            logFile.WriteLine("Log file saved successfully!");
            logFile.WriteLine("End Time:" + DateTime.Now);
            logFile.WriteLine();
            logFile.Close();
        }
    }
}
