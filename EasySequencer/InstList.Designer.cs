﻿namespace EasySequencer {
    partial class InstList {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lstInst = new System.Windows.Forms.ListBox();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.btnCommit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstInst
            // 
            this.lstInst.ColumnWidth = 200;
            this.lstInst.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lstInst.FormattingEnabled = true;
            this.lstInst.HorizontalExtent = 1;
            this.lstInst.HorizontalScrollbar = true;
            this.lstInst.ItemHeight = 15;
            this.lstInst.Location = new System.Drawing.Point(5, 32);
            this.lstInst.Margin = new System.Windows.Forms.Padding(2);
            this.lstInst.MultiColumn = true;
            this.lstInst.Name = "lstInst";
            this.lstInst.ScrollAlwaysVisible = true;
            this.lstInst.Size = new System.Drawing.Size(531, 259);
            this.lstInst.TabIndex = 1;
            this.lstInst.SelectedIndexChanged += new System.EventHandler(this.lstInst_SelectedIndexChanged);
            // 
            // cmbCategory
            // 
            this.cmbCategory.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Location = new System.Drawing.Point(6, 6);
            this.cmbCategory.Margin = new System.Windows.Forms.Padding(2);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(176, 23);
            this.cmbCategory.TabIndex = 0;
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged);
            // 
            // btnCommit
            // 
            this.btnCommit.Font = new System.Drawing.Font("MS UI Gothic", 10.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnCommit.Location = new System.Drawing.Point(467, 4);
            this.btnCommit.Margin = new System.Windows.Forms.Padding(2);
            this.btnCommit.Name = "btnCommit";
            this.btnCommit.Size = new System.Drawing.Size(67, 24);
            this.btnCommit.TabIndex = 2;
            this.btnCommit.Text = "確定";
            this.btnCommit.UseVisualStyleBackColor = true;
            this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);
            // 
            // InstList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(546, 324);
            this.Controls.Add(this.btnCommit);
            this.Controls.Add(this.cmbCategory);
            this.Controls.Add(this.lstInst);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "InstList";
            this.Text = "InstList";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox lstInst;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Button btnCommit;
    }
}