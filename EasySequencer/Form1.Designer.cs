namespace EasySequencer
{
	partial class Form1
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lblPosition = new System.Windows.Forms.Label();
            this.lblTempo = new System.Windows.Forms.Label();
            this.hsbSeek = new System.Windows.Forms.HScrollBar();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.ファイルFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.新規作成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.開くOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.上書き保存SToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.名前を付けて保存ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.wavファイル出力ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.編集EToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnPalyStop = new System.Windows.Forms.Button();
            this.numKey = new System.Windows.Forms.NumericUpDown();
            this.pnlPlayer = new System.Windows.Forms.Panel();
            this.picActive = new System.Windows.Forms.PictureBox();
            this.lblTempoPercent = new System.Windows.Forms.Label();
            this.trkSpeed = new System.Windows.Forms.TrackBar();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.picKeyBack = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numKey)).BeginInit();
            this.pnlPlayer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picActive)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picKeyBack)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Font = new System.Drawing.Font("Meiryo UI", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblPosition.Location = new System.Drawing.Point(120, 2);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(134, 24);
            this.lblPosition.TabIndex = 4;
            this.lblPosition.Text = "0001:01:000";
            // 
            // lblTempo
            // 
            this.lblTempo.AutoSize = true;
            this.lblTempo.Font = new System.Drawing.Font("Meiryo UI", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTempo.Location = new System.Drawing.Point(429, 2);
            this.lblTempo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTempo.Name = "lblTempo";
            this.lblTempo.Size = new System.Drawing.Size(77, 24);
            this.lblTempo.TabIndex = 7;
            this.lblTempo.Text = "132.00";
            // 
            // hsbSeek
            // 
            this.hsbSeek.Location = new System.Drawing.Point(3, 28);
            this.hsbSeek.Name = "hsbSeek";
            this.hsbSeek.Size = new System.Drawing.Size(608, 46);
            this.hsbSeek.TabIndex = 8;
            this.hsbSeek.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hsbSeek_Scroll);
            this.hsbSeek.MouseLeave += new System.EventHandler(this.hsbSeek_MouseLeave);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルFToolStripMenuItem,
            this.編集EToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1052, 24);
            this.menuStrip1.TabIndex = 26;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // ファイルFToolStripMenuItem
            // 
            this.ファイルFToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新規作成ToolStripMenuItem,
            this.toolStripSeparator1,
            this.開くOToolStripMenuItem,
            this.toolStripSeparator2,
            this.上書き保存SToolStripMenuItem,
            this.名前を付けて保存ToolStripMenuItem,
            this.toolStripSeparator3,
            this.wavファイル出力ToolStripMenuItem});
            this.ファイルFToolStripMenuItem.Name = "ファイルFToolStripMenuItem";
            this.ファイルFToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.ファイルFToolStripMenuItem.Text = "ファイル(F)";
            // 
            // 新規作成ToolStripMenuItem
            // 
            this.新規作成ToolStripMenuItem.Name = "新規作成ToolStripMenuItem";
            this.新規作成ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.新規作成ToolStripMenuItem.Text = "新規作成(N)";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
            // 
            // 開くOToolStripMenuItem
            // 
            this.開くOToolStripMenuItem.Name = "開くOToolStripMenuItem";
            this.開くOToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.開くOToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.開くOToolStripMenuItem.Text = "開く(O)";
            this.開くOToolStripMenuItem.Click += new System.EventHandler(this.開くOToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
            // 
            // 上書き保存SToolStripMenuItem
            // 
            this.上書き保存SToolStripMenuItem.Name = "上書き保存SToolStripMenuItem";
            this.上書き保存SToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.上書き保存SToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.上書き保存SToolStripMenuItem.Text = "上書き保存(S)";
            // 
            // 名前を付けて保存ToolStripMenuItem
            // 
            this.名前を付けて保存ToolStripMenuItem.Name = "名前を付けて保存ToolStripMenuItem";
            this.名前を付けて保存ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.名前を付けて保存ToolStripMenuItem.Text = "名前を付けて保存(A)";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(181, 6);
            // 
            // wavファイル出力ToolStripMenuItem
            // 
            this.wavファイル出力ToolStripMenuItem.Name = "wavファイル出力ToolStripMenuItem";
            this.wavファイル出力ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.wavファイル出力ToolStripMenuItem.Text = "wavファイル出力(W)";
            this.wavファイル出力ToolStripMenuItem.Click += new System.EventHandler(this.wavファイル出力ToolStripMenuItem_Click);
            // 
            // 編集EToolStripMenuItem
            // 
            this.編集EToolStripMenuItem.Name = "編集EToolStripMenuItem";
            this.編集EToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.編集EToolStripMenuItem.Text = "編集(E)";
            // 
            // btnPalyStop
            // 
            this.btnPalyStop.Location = new System.Drawing.Point(3, 3);
            this.btnPalyStop.Name = "btnPalyStop";
            this.btnPalyStop.Size = new System.Drawing.Size(55, 23);
            this.btnPalyStop.TabIndex = 27;
            this.btnPalyStop.Text = "再生";
            this.btnPalyStop.UseVisualStyleBackColor = true;
            this.btnPalyStop.Click += new System.EventHandler(this.btnPalyStop_Click);
            // 
            // numKey
            // 
            this.numKey.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numKey.Location = new System.Drawing.Point(64, 4);
            this.numKey.Maximum = new decimal(new int[] {
            24,
            0,
            0,
            0});
            this.numKey.Minimum = new decimal(new int[] {
            24,
            0,
            0,
            -2147483648});
            this.numKey.Name = "numKey";
            this.numKey.Size = new System.Drawing.Size(53, 23);
            this.numKey.TabIndex = 28;
            this.numKey.ValueChanged += new System.EventHandler(this.numKey_ValueChanged);
            // 
            // pnlPlayer
            // 
            this.pnlPlayer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPlayer.Controls.Add(this.picActive);
            this.pnlPlayer.Controls.Add(this.lblTempoPercent);
            this.pnlPlayer.Controls.Add(this.lblPosition);
            this.pnlPlayer.Controls.Add(this.lblTempo);
            this.pnlPlayer.Controls.Add(this.hsbSeek);
            this.pnlPlayer.Controls.Add(this.numKey);
            this.pnlPlayer.Controls.Add(this.btnPalyStop);
            this.pnlPlayer.Controls.Add(this.trkSpeed);
            this.pnlPlayer.Location = new System.Drawing.Point(0, 27);
            this.pnlPlayer.Name = "pnlPlayer";
            this.pnlPlayer.Size = new System.Drawing.Size(613, 53);
            this.pnlPlayer.TabIndex = 35;
            // 
            // picActive
            // 
            this.picActive.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picActive.Location = new System.Drawing.Point(551, 7);
            this.picActive.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.picActive.Name = "picActive";
            this.picActive.Size = new System.Drawing.Size(48, 18);
            this.picActive.TabIndex = 31;
            this.picActive.TabStop = false;
            // 
            // lblTempoPercent
            // 
            this.lblTempoPercent.AutoSize = true;
            this.lblTempoPercent.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTempoPercent.Location = new System.Drawing.Point(497, 6);
            this.lblTempoPercent.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTempoPercent.Name = "lblTempoPercent";
            this.lblTempoPercent.Size = new System.Drawing.Size(51, 19);
            this.lblTempoPercent.TabIndex = 30;
            this.lblTempoPercent.Text = "400%";
            // 
            // trkSpeed
            // 
            this.trkSpeed.LargeChange = 10;
            this.trkSpeed.Location = new System.Drawing.Point(237, 2);
            this.trkSpeed.Maximum = 200;
            this.trkSpeed.Minimum = 25;
            this.trkSpeed.Name = "trkSpeed";
            this.trkSpeed.Size = new System.Drawing.Size(185, 45);
            this.trkSpeed.TabIndex = 29;
            this.trkSpeed.TickFrequency = 25;
            this.trkSpeed.Value = 100;
            this.trkSpeed.Scroll += new System.EventHandler(this.trkSpeed_Scroll);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // picKeyBack
            // 
            this.picKeyBack.BackColor = System.Drawing.Color.Transparent;
            this.picKeyBack.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("picKeyBack.BackgroundImage")));
            this.picKeyBack.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picKeyBack.InitialImage = null;
            this.picKeyBack.Location = new System.Drawing.Point(0, 86);
            this.picKeyBack.Name = "picKeyBack";
            this.picKeyBack.Size = new System.Drawing.Size(1002, 642);
            this.picKeyBack.TabIndex = 0;
            this.picKeyBack.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1052, 490);
            this.Controls.Add(this.picKeyBack);
            this.Controls.Add(this.pnlPlayer);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numKey)).EndInit();
            this.pnlPlayer.ResumeLayout(false);
            this.pnlPlayer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picActive)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picKeyBack)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblPosition;
		private System.Windows.Forms.Label lblTempo;
		private System.Windows.Forms.HScrollBar hsbSeek;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem ファイルFToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 新規作成ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 開くOToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 上書き保存SToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 名前を付けて保存ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 編集EToolStripMenuItem;
		private System.Windows.Forms.Button btnPalyStop;
		private System.Windows.Forms.NumericUpDown numKey;
		private System.Windows.Forms.Panel pnlPlayer;
		private System.Windows.Forms.TrackBar trkSpeed;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem wavファイル出力ToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblTempoPercent;
        private System.Windows.Forms.PictureBox picActive;
        private System.Windows.Forms.PictureBox picKeyBack;
    }
}

