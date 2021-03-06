﻿namespace Bechtel.iRING.SPRUtility
{
    partial class FrmSPRSynchronizationUtility
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSPRSynchronizationUtility));
            this.lblSelect = new System.Windows.Forms.Label();
            this.cboxCommodities = new System.Windows.Forms.ComboBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lbl2 = new System.Windows.Forms.Label();
            this.txtMdbName = new System.Windows.Forms.TextBox();
            this.lblStatusName = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.clistboxCommodities = new System.Windows.Forms.CheckedListBox();
            this.clistboxScopes = new System.Windows.Forms.CheckedListBox();
            this.lblscope = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSelect
            // 
            this.lblSelect.AutoSize = true;
            this.lblSelect.Location = new System.Drawing.Point(5, 51);
            this.lblSelect.Name = "lblSelect";
            this.lblSelect.Size = new System.Drawing.Size(117, 13);
            this.lblSelect.TabIndex = 0;
            this.lblSelect.Text = "Select the component :";
            // 
            // cboxCommodities
            // 
            this.cboxCommodities.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboxCommodities.FormattingEnabled = true;
            this.cboxCommodities.Location = new System.Drawing.Point(97, 317);
            this.cboxCommodities.Name = "cboxCommodities";
            this.cboxCommodities.Size = new System.Drawing.Size(239, 21);
            this.cboxCommodities.TabIndex = 1;
            this.cboxCommodities.Visible = false;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(306, 113);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(57, 23);
            this.btnGo.TabIndex = 2;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(243, 113);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(57, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(306, 85);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(57, 23);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(5, 86);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(101, 13);
            this.lbl2.TabIndex = 5;
            this.lbl2.Text = "Select the Mdb file :";
            // 
            // txtMdbName
            // 
            this.txtMdbName.Location = new System.Drawing.Point(124, 85);
            this.txtMdbName.Name = "txtMdbName";
            this.txtMdbName.ReadOnly = true;
            this.txtMdbName.Size = new System.Drawing.Size(176, 20);
            this.txtMdbName.TabIndex = 6;
            // 
            // lblStatusName
            // 
            this.lblStatusName.AutoSize = true;
            this.lblStatusName.Location = new System.Drawing.Point(5, 123);
            this.lblStatusName.Name = "lblStatusName";
            this.lblStatusName.Size = new System.Drawing.Size(46, 13);
            this.lblStatusName.TabIndex = 7;
            this.lblStatusName.Text = "Status : ";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(47, 123);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(41, 13);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Started";
            // 
            // clistboxCommodities
            // 
            this.clistboxCommodities.CheckOnClick = true;
            this.clistboxCommodities.FormattingEnabled = true;
            this.clistboxCommodities.Location = new System.Drawing.Point(124, 44);
            this.clistboxCommodities.Name = "clistboxCommodities";
            this.clistboxCommodities.Size = new System.Drawing.Size(239, 34);
            this.clistboxCommodities.TabIndex = 10;
            // 
            // clistboxScopes
            // 
            this.clistboxScopes.CheckOnClick = true;
            this.clistboxScopes.FormattingEnabled = true;
            this.clistboxScopes.Location = new System.Drawing.Point(124, 5);
            this.clistboxScopes.Name = "clistboxScopes";
            this.clistboxScopes.Size = new System.Drawing.Size(239, 34);
            this.clistboxScopes.TabIndex = 11;
            this.clistboxScopes.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.clistboxScopes_ItemCheck);
            this.clistboxScopes.SelectedIndexChanged += new System.EventHandler(this.clistboxScopes_SelectedIndexChanged);
            // 
            // lblscope
            // 
            this.lblscope.AutoSize = true;
            this.lblscope.Location = new System.Drawing.Point(5, 14);
            this.lblscope.Name = "lblscope";
            this.lblscope.Size = new System.Drawing.Size(93, 13);
            this.lblscope.TabIndex = 12;
            this.lblscope.Text = "Select the scope :";
            // 
            // FrmSPRSynchronizationUtility
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 141);
            this.Controls.Add(this.lblscope);
            this.Controls.Add(this.clistboxScopes);
            this.Controls.Add(this.clistboxCommodities);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblStatusName);
            this.Controls.Add(this.txtMdbName);
            this.Controls.Add(this.lbl2);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.cboxCommodities);
            this.Controls.Add(this.lblSelect);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmSPRSynchronizationUtility";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SPR Synchronization Utility";
            this.Load += new System.EventHandler(this.FrmSPRSynchronizationUtility_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSelect;
        private System.Windows.Forms.ComboBox cboxCommodities;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.TextBox txtMdbName;
        private System.Windows.Forms.Label lblStatusName;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.CheckedListBox clistboxCommodities;
        private System.Windows.Forms.CheckedListBox clistboxScopes;
        private System.Windows.Forms.Label lblscope;
    }
}

