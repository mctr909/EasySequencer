﻿namespace EasySequencer {
    partial class Monitor {
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
            this.picMonitor = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // picMonitor
            // 
            this.picMonitor.BackColor = System.Drawing.Color.Transparent;
            this.picMonitor.BackgroundImage = global::EasySequencer.Properties.Resources.Monitor;
            this.picMonitor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picMonitor.InitialImage = null;
            this.picMonitor.Location = new System.Drawing.Point(0, 0);
            this.picMonitor.Name = "picMonitor";
            this.picMonitor.Size = new System.Drawing.Size(1227, 541);
            this.picMonitor.TabIndex = 1;
            this.picMonitor.TabStop = false;
            // 
            // Monitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1237, 551);
            this.Controls.Add(this.picMonitor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Monitor";
            this.Text = "Monitor";
            this.Load += new System.EventHandler(this.Monitor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picMonitor)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picMonitor;
    }
}