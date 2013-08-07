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
using System.Threading.Tasks;

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
            //syncUtility = new SPRSynchronizationUtility(logFile);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            SaveLoggingFile();
            this.Close();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                btnGo.Enabled = false;
                Application.DoEvents();

                if (clistboxScopes.CheckedItems.Count == 0)
                {
                    MessageBox.Show("Please select the sccope.");
                    return;
                }

                if (string.IsNullOrEmpty(txtMdbName.Text))
                {
                    MessageBox.Show("Please select the Mdb File.");
                    return;
                }

                var lstitems = (from string item in clistboxCommodities.CheckedItems select item).ToList<string>();

                //  string selectedObject = ((org.iringtools.library.DataObject)(cboxCommodities.Items[cboxCommodities.SelectedIndex])).objectName.ToString();
                // if (!string.IsNullOrEmpty(selectedObject))
                if (clistboxCommodities.CheckedItems.Count > 0)
                {
                    lblStatus.Text = "Processing.... please wait";
                    Application.DoEvents();

                    syncUtility.MDBSynchronization(lstitems);
                    //Task task = new Task(new Action(() => ProcessSync(lstitems)));
                    //task.Start();
                    //task.Wait();

                    lblStatus.Text = "Completed";
                    Application.DoEvents();
                }
                else
                {
                    MessageBox.Show("Please select the component.");
                    return;
                }

                DialogResult dialogResult = MessageBox.Show("Mdb file updated for the selected components. Do you want to continue?", "Message", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dialogResult == DialogResult.No)
                {
                    this.Close();
                    SaveLoggingFile();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Exception Raised";
                Application.DoEvents();

                logFile.WriteLine(ex.Message + ex.StackTrace);
                SaveLoggingFile();
                MessageBox.Show("Exception occured please check logFile: " + ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGo.Enabled = true;
                Application.DoEvents();
            }
        }

        private void ProcessSync(List<string> lstitems)
        {
            syncUtility.MDBSynchronization(lstitems);
        }


        private void FrmSPRSynchronizationUtility_Load(object sender, EventArgs e)
        {
            //Load all the scopes based on no. of configuration files present in app data folder.
            string appDataPath = @".\App_Data\";
            string _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin"));

            string _dataPath = Path.Combine(_baseDirectory, appDataPath);
            DirectoryInfo appDataDir = new DirectoryInfo(_dataPath);
            string filterFilePattern = String.Format("Configuration.*.xml");
            FileInfo[] filterFiles = appDataDir.GetFiles(filterFilePattern);

            foreach (FileInfo file in filterFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file.Name);
                string scopeName = fileName.Substring(fileName.IndexOf('.') + 1);
                clistboxScopes.Items.Add(scopeName); 
            }

            /*
            // Based on the Scope selected, Laod all the objects from the corresponding dictionary.
            syncUtility = new SPRSynchronizationUtility(logFile);

            //Loading objects on which we have to operate.
            List<org.iringtools.library.DataObject> objects = syncUtility.GetObjects();

            clistboxCommodities.Items.Clear();
            foreach (org.iringtools.library.DataObject obj in objects)
            {
                clistboxCommodities.Items.Add(obj.objectName); 
            }
            for (int i = 0; i < clistboxCommodities.Items.Count; i++)
            {
                clistboxCommodities.SetItemChecked(i, true);
            }
             * */
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
            if (logFile.BaseStream != null)
            {
                logFile.WriteLine("Log file saved successfully!");
                logFile.WriteLine("End Time:" + DateTime.Now);
                logFile.WriteLine();
                logFile.Close();
            }
        }


        private void clistboxScopes_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int ix = 0; ix < clistboxScopes.Items.Count; ++ix)
            {
                if (ix != e.Index)
                {
                    clistboxScopes.SetItemChecked(ix, false);
                }
            }
        }

        private void clistboxScopes_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (object scopeChecked in clistboxScopes.CheckedItems)
            {
                // Based on the Scope selected, do all the settings.
                syncUtility = new SPRSynchronizationUtility(logFile,Convert.ToString(scopeChecked));

                // Load all the objects from the corresponding dictionary.
                List<org.iringtools.library.DataObject> objects = syncUtility.GetObjects();

                clistboxCommodities.Items.Clear();
                foreach (org.iringtools.library.DataObject obj in objects)
                {
                    clistboxCommodities.Items.Add(obj.objectName);
                }
                for (int i = 0; i < clistboxCommodities.Items.Count; i++)
                {
                    clistboxCommodities.SetItemChecked(i, true);
                }
            }
            if (clistboxScopes.CheckedItems.Count == 0)
            {
                clistboxCommodities.Items.Clear();
            }
        }
    }
}
