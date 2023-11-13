using System.Windows.Forms;
using System;

namespace EasySequencer {
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
			this.components = new System.ComponentModel.Container();
			this.TimerUI = new System.Windows.Forms.Timer(this.components);
			this.TimerMonitor = new System.Windows.Forms.Timer(this.components);
			this.PicMonitor = new System.Windows.Forms.PictureBox();
			this.PicUI = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.PicMonitor)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PicUI)).BeginInit();
			this.SuspendLayout();
			// 
			// TimerUI
			// 
			this.TimerUI.Tick += new System.EventHandler(this.TimerUI_Tick);
			// 
			// TimerMonitor
			// 
			this.TimerMonitor.Tick += new System.EventHandler(this.TimerMonitor_Tick);
			// 
			// PicMonitor
			// 
			this.PicMonitor.BackColor = System.Drawing.Color.Transparent;
			this.PicMonitor.BackgroundImage = global::EasySequencer.Properties.Resources.Keyboard;
			this.PicMonitor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.PicMonitor.InitialImage = null;
			this.PicMonitor.Location = new System.Drawing.Point(12, 12);
			this.PicMonitor.Name = "picKeyboard";
			this.PicMonitor.Size = new System.Drawing.Size(489, 541);
			this.PicMonitor.TabIndex = 2;
			this.PicMonitor.TabStop = false;
			// 
			// PicUI
			// 
			this.PicUI.BackColor = System.Drawing.Color.Transparent;
			this.PicUI.BackgroundImage = global::EasySequencer.Properties.Resources.Monitor;
			this.PicUI.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.PicUI.InitialImage = null;
			this.PicUI.Location = new System.Drawing.Point(507, 12);
			this.PicUI.Name = "picMonitor";
			this.PicUI.Size = new System.Drawing.Size(740, 541);
			this.PicUI.TabIndex = 1;
			this.PicUI.TabStop = false;
			this.PicUI.DoubleClick += new System.EventHandler(this.PicMonitor_DoubleClick);
			this.PicUI.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PicMonitor_MouseDown);
			this.PicUI.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PicMonitor_MouseMove);
			this.PicUI.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PicMonitor_MouseUp);
			// 
			// Monitor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1237, 551);
			this.Controls.Add(this.PicMonitor);
			this.Controls.Add(this.PicUI);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Monitor";
			this.Text = "Monitor";
			this.Shown += new System.EventHandler(this.Monitor_Shown);
			((System.ComponentModel.ISupportInitialize)(this.PicMonitor)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PicUI)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox PicUI;
		private Timer TimerUI;
		private Timer TimerMonitor;
		private PictureBox PicMonitor;
	}
}