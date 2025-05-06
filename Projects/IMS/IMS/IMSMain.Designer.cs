namespace IMS
{
    partial class IMSMain
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
            this.components = new System.ComponentModel.Container();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.lblHeader = new System.Windows.Forms.Label();
            this.txtEmail = new System.Windows.Forms.TextBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.grdACTRecords = new System.Windows.Forms.DataGridView();
            this.pnlRecord = new System.Windows.Forms.Panel();
            this.bsGridView = new System.Windows.Forms.BindingSource(this.components);
            this.ContactID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Contact = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DBA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CompanyName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Email = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AffiliateReferral = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Processor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rtbValue = new System.Windows.Forms.RichTextBox();
            this.lblContactID = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grdACTRecords)).BeginInit();
            this.pnlRecord.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSubmit
            // 
            this.btnSubmit.BackColor = System.Drawing.Color.Transparent;
            this.btnSubmit.Location = new System.Drawing.Point(377, 488);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 0;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Visible = false;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.Location = new System.Drawing.Point(25, 9);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(437, 17);
            this.lblHeader.TabIndex = 1;
            this.lblHeader.Text = "Enter the Email for the application you want to submit and click on the Load butt" +
                "on.";
            this.lblHeader.UseCompatibleTextRendering = true;
            // 
            // txtEmail
            // 
            this.txtEmail.Location = new System.Drawing.Point(483, 9);
            this.txtEmail.Name = "txtEmail";
            this.txtEmail.Size = new System.Drawing.Size(100, 20);
            this.txtEmail.TabIndex = 2;
            // 
            // btnLoad
            // 
            this.btnLoad.BackColor = System.Drawing.Color.Transparent;
            this.btnLoad.Location = new System.Drawing.Point(603, 7);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(75, 23);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Search";
            this.btnLoad.UseVisualStyleBackColor = false;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // grdACTRecords
            // 
            this.grdACTRecords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdACTRecords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ContactID,
            this.Contact,
            this.DBA,
            this.CompanyName,
            this.Email,
            this.AffiliateReferral,
            this.Processor});
            this.grdACTRecords.Location = new System.Drawing.Point(25, 35);
            this.grdACTRecords.Name = "grdACTRecords";
            this.grdACTRecords.Size = new System.Drawing.Size(765, 165);
            this.grdACTRecords.TabIndex = 4;
            this.grdACTRecords.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdACTRecords_CellDoubleClick);
            this.grdACTRecords.Click += new System.EventHandler(this.grdACTRecords_Click);
            // 
            // pnlRecord
            // 
            this.pnlRecord.Controls.Add(this.lblContactID);
            this.pnlRecord.Controls.Add(this.rtbValue);
            this.pnlRecord.Location = new System.Drawing.Point(25, 207);
            this.pnlRecord.Name = "pnlRecord";
            this.pnlRecord.Size = new System.Drawing.Size(765, 275);
            this.pnlRecord.TabIndex = 5;
            // 
            // ContactID
            // 
            this.ContactID.DataPropertyName = "ContactID";
            this.ContactID.HeaderText = "ContactID";
            this.ContactID.Name = "ContactID";
            this.ContactID.ReadOnly = true;
            // 
            // Contact
            // 
            this.Contact.HeaderText = "Contact";
            this.Contact.Name = "Contact";
            // 
            // DBA
            // 
            this.DBA.DataPropertyName = "DBA";
            this.DBA.HeaderText = "DBA";
            this.DBA.Name = "DBA";
            this.DBA.ReadOnly = true;
            // 
            // CompanyName
            // 
            this.CompanyName.DataPropertyName = "CompanyName";
            this.CompanyName.HeaderText = "Company Name";
            this.CompanyName.Name = "CompanyName";
            this.CompanyName.ReadOnly = true;
            // 
            // Email
            // 
            this.Email.DataPropertyName = "Email";
            this.Email.HeaderText = "Email";
            this.Email.Name = "Email";
            this.Email.ReadOnly = true;
            // 
            // AffiliateReferral
            // 
            this.AffiliateReferral.HeaderText = "Referred By";
            this.AffiliateReferral.Name = "AffiliateReferral";
            // 
            // Processor
            // 
            this.Processor.DataPropertyName = "Processor";
            this.Processor.HeaderText = "Processor";
            this.Processor.Name = "Processor";
            // 
            // rtbValue
            // 
            this.rtbValue.Location = new System.Drawing.Point(31, 43);
            this.rtbValue.Name = "rtbValue";
            this.rtbValue.Size = new System.Drawing.Size(704, 229);
            this.rtbValue.TabIndex = 1;
            this.rtbValue.Text = "";
            // 
            // lblContactID
            // 
            this.lblContactID.AutoSize = true;
            this.lblContactID.Location = new System.Drawing.Point(358, 27);
            this.lblContactID.Name = "lblContactID";
            this.lblContactID.Size = new System.Drawing.Size(0, 13);
            this.lblContactID.TabIndex = 2;
            // 
            // IMSMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(816, 523);
            this.Controls.Add(this.pnlRecord);
            this.Controls.Add(this.grdACTRecords);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.txtEmail);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.btnSubmit);
            this.MaximizeBox = false;
            this.Name = "IMSMain";
            this.Text = "E-Commerce Exchange - Submit Application To IMS";
            this.Load += new System.EventHandler(this.Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grdACTRecords)).EndInit();
            this.pnlRecord.ResumeLayout(false);
            this.pnlRecord.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bsGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.DataGridView grdACTRecords;
        private System.Windows.Forms.Panel pnlRecord;
        private System.Windows.Forms.BindingSource bsGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn ContactID;
        private System.Windows.Forms.DataGridViewTextBoxColumn Contact;
        private System.Windows.Forms.DataGridViewTextBoxColumn DBA;
        private System.Windows.Forms.DataGridViewTextBoxColumn CompanyName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Email;
        private System.Windows.Forms.DataGridViewTextBoxColumn AffiliateReferral;
        private System.Windows.Forms.DataGridViewTextBoxColumn Processor;
        private System.Windows.Forms.RichTextBox rtbValue;
        private System.Windows.Forms.Label lblContactID;
    }
}

