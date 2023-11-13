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
			this.TimerKeyboard = new System.Windows.Forms.Timer(this.components);
			this.TimerMeter = new System.Windows.Forms.Timer(this.components);
			this.PicMeter = new System.Windows.Forms.PictureBox();
			this.PicKeyboard = new System.Windows.Forms.PictureBox();
			this.PicUI = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.PicMeter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PicKeyboard)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PicUI)).BeginInit();
			this.SuspendLayout();
			// 
			// TimerUI
			// 
			this.TimerUI.Tick += new System.EventHandler(this.TimerUI_Tick);
			// 
			// TimerKeyboard
			// 
			this.TimerKeyboard.Tick += new System.EventHandler(this.TimerKeyboard_Tick);
			// 
			// TimerMeter
			// 
			this.TimerMeter.Tick += new System.EventHandler(this.TimerMeter_Tick);
			// 
			// PicMeter
			// 
			this.PicMeter.BackColor = System.Drawing.Color.Transparent;
			this.PicMeter.BackgroundImage = global::EasySequencer.Properties.Resources.Meter;
			this.PicMeter.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.PicMeter.InitialImage = null;
			this.PicMeter.Location = new System.Drawing.Point(507, 12);
			this.PicMeter.Name = "PicMeter";
			this.PicMeter.Size = new System.Drawing.Size(173, 541);
			this.PicMeter.TabIndex = 3;
			this.PicMeter.TabStop = false;
			// 
			// PicKeyboard
			// 
			this.PicKeyboard.BackColor = System.Drawing.Color.Transparent;
			this.PicKeyboard.BackgroundImage = global::EasySequencer.Properties.Resources.Keyboard;
			this.PicKeyboard.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.PicKeyboard.InitialImage = null;
			this.PicKeyboard.Location = new System.Drawing.Point(12, 12);
			this.PicKeyboard.Name = "PicMonitor";
			this.PicKeyboard.Size = new System.Drawing.Size(489, 541);
			this.PicKeyboard.TabIndex = 2;
			this.PicKeyboard.TabStop = false;
			// 
			// PicUI
			// 
			this.PicUI.BackColor = System.Drawing.Color.Transparent;
			this.PicUI.BackgroundImage = global::EasySequencer.Properties.Resources.Monitor;
			this.PicUI.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.PicUI.InitialImage = null;
			this.PicUI.Location = new System.Drawing.Point(686, 12);
			this.PicUI.Name = "PicUI";
			this.PicUI.Size = new System.Drawing.Size(581, 541);
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
			this.Controls.Add(this.PicMeter);
			this.Controls.Add(this.PicKeyboard);
			this.Controls.Add(this.PicUI);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Monitor";
			this.Text = "Monitor";
			this.Shown += new System.EventHandler(this.Monitor_Shown);
			((System.ComponentModel.ISupportInitialize)(this.PicMeter)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PicKeyboard)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PicUI)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox PicUI;
		private Timer TimerUI;
		private Timer TimerKeyboard;
		private Timer TimerMeter;
		private PictureBox PicKeyboard;
		private PictureBox PicMeter;
	}
}