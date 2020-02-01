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
            this.lblTempoPercent = new System.Windows.Forms.Label();
            this.trkSpeed = new System.Windows.Forms.TrackBar();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pnlKeyboard = new System.Windows.Forms.Panel();
            this.picKey = new System.Windows.Forms.PictureBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numKey)).BeginInit();
            this.pnlPlayer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.pnlKeyboard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picKey)).BeginInit();
            this.SuspendLayout();
            // 
            // lblPosition
            // 
            this.lblPosition.AutoSize = true;
            this.lblPosition.Font = new System.Drawing.Font("Meiryo UI", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblPosition.Location = new System.Drawing.Point(259, 5);
            this.lblPosition.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Size = new System.Drawing.Size(259, 47);
            this.lblPosition.TabIndex = 4;
            this.lblPosition.Text = "0001:01:000";
            // 
            // lblTempo
            // 
            this.lblTempo.AutoSize = true;
            this.lblTempo.Font = new System.Drawing.Font("Meiryo UI", 13.875F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTempo.Location = new System.Drawing.Point(840, 5);
            this.lblTempo.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblTempo.Name = "lblTempo";
            this.lblTempo.Size = new System.Drawing.Size(148, 47);
            this.lblTempo.TabIndex = 7;
            this.lblTempo.Text = "132.00";
            // 
            // hsbSeek
            // 
            this.hsbSeek.Location = new System.Drawing.Point(6, 56);
            this.hsbSeek.Name = "hsbSeek";
            this.hsbSeek.Size = new System.Drawing.Size(1086, 46);
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
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(13, 4, 0, 4);
            this.menuStrip1.Size = new System.Drawing.Size(2321, 44);
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
            this.ファイルFToolStripMenuItem.Size = new System.Drawing.Size(121, 36);
            this.ファイルFToolStripMenuItem.Text = "ファイル(F)";
            // 
            // 新規作成ToolStripMenuItem
            // 
            this.新規作成ToolStripMenuItem.Name = "新規作成ToolStripMenuItem";
            this.新規作成ToolStripMenuItem.Size = new System.Drawing.Size(336, 38);
            this.新規作成ToolStripMenuItem.Text = "新規作成(N)";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(333, 6);
            // 
            // 開くOToolStripMenuItem
            // 
            this.開くOToolStripMenuItem.Name = "開くOToolStripMenuItem";
            this.開くOToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.開くOToolStripMenuItem.Size = new System.Drawing.Size(336, 38);
            this.開くOToolStripMenuItem.Text = "開く(O)";
            this.開くOToolStripMenuItem.Click += new System.EventHandler(this.開くOToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(333, 6);
            // 
            // 上書き保存SToolStripMenuItem
            // 
            this.上書き保存SToolStripMenuItem.Name = "上書き保存SToolStripMenuItem";
            this.上書き保存SToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.上書き保存SToolStripMenuItem.Size = new System.Drawing.Size(336, 38);
            this.上書き保存SToolStripMenuItem.Text = "上書き保存(S)";
            // 
            // 名前を付けて保存ToolStripMenuItem
            // 
            this.名前を付けて保存ToolStripMenuItem.Name = "名前を付けて保存ToolStripMenuItem";
            this.名前を付けて保存ToolStripMenuItem.Size = new System.Drawing.Size(336, 38);
            this.名前を付けて保存ToolStripMenuItem.Text = "名前を付けて保存(A)";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(333, 6);
            // 
            // wavファイル出力ToolStripMenuItem
            // 
            this.wavファイル出力ToolStripMenuItem.Name = "wavファイル出力ToolStripMenuItem";
            this.wavファイル出力ToolStripMenuItem.Size = new System.Drawing.Size(336, 38);
            this.wavファイル出力ToolStripMenuItem.Text = "wavファイル出力(W)";
            this.wavファイル出力ToolStripMenuItem.Click += new System.EventHandler(this.wavファイル出力ToolStripMenuItem_Click);
            // 
            // 編集EToolStripMenuItem
            // 
            this.編集EToolStripMenuItem.Name = "編集EToolStripMenuItem";
            this.編集EToolStripMenuItem.Size = new System.Drawing.Size(101, 36);
            this.編集EToolStripMenuItem.Text = "編集(E)";
            // 
            // btnPalyStop
            // 
            this.btnPalyStop.Location = new System.Drawing.Point(7, 6);
            this.btnPalyStop.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.btnPalyStop.Name = "btnPalyStop";
            this.btnPalyStop.Size = new System.Drawing.Size(119, 46);
            this.btnPalyStop.TabIndex = 27;
            this.btnPalyStop.Text = "再生";
            this.btnPalyStop.UseVisualStyleBackColor = true;
            this.btnPalyStop.Click += new System.EventHandler(this.btnPalyStop_Click);
            // 
            // numKey
            // 
            this.numKey.Font = new System.Drawing.Font("Meiryo UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.numKey.Location = new System.Drawing.Point(139, 8);
            this.numKey.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
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
            this.numKey.Size = new System.Drawing.Size(115, 38);
            this.numKey.TabIndex = 28;
            this.numKey.ValueChanged += new System.EventHandler(this.numKey_ValueChanged);
            // 
            // pnlPlayer
            // 
            this.pnlPlayer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPlayer.Controls.Add(this.lblTempoPercent);
            this.pnlPlayer.Controls.Add(this.lblPosition);
            this.pnlPlayer.Controls.Add(this.lblTempo);
            this.pnlPlayer.Controls.Add(this.hsbSeek);
            this.pnlPlayer.Controls.Add(this.numKey);
            this.pnlPlayer.Controls.Add(this.btnPalyStop);
            this.pnlPlayer.Controls.Add(this.trkSpeed);
            this.pnlPlayer.Location = new System.Drawing.Point(26, 54);
            this.pnlPlayer.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.pnlPlayer.Name = "pnlPlayer";
            this.pnlPlayer.Size = new System.Drawing.Size(1107, 104);
            this.pnlPlayer.TabIndex = 35;
            // 
            // lblTempoPercent
            // 
            this.lblTempoPercent.AutoSize = true;
            this.lblTempoPercent.Font = new System.Drawing.Font("Meiryo UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblTempoPercent.Location = new System.Drawing.Point(987, 12);
            this.lblTempoPercent.Margin = new System.Windows.Forms.Padding(9, 0, 9, 0);
            this.lblTempoPercent.Name = "lblTempoPercent";
            this.lblTempoPercent.Size = new System.Drawing.Size(105, 38);
            this.lblTempoPercent.TabIndex = 30;
            this.lblTempoPercent.Text = "400%";
            // 
            // trkSpeed
            // 
            this.trkSpeed.LargeChange = 10;
            this.trkSpeed.Location = new System.Drawing.Point(514, 5);
            this.trkSpeed.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.trkSpeed.Maximum = 200;
            this.trkSpeed.Minimum = 25;
            this.trkSpeed.Name = "trkSpeed";
            this.trkSpeed.Size = new System.Drawing.Size(310, 90);
            this.trkSpeed.TabIndex = 29;
            this.trkSpeed.TickFrequency = 25;
            this.trkSpeed.Value = 100;
            this.trkSpeed.Scroll += new System.EventHandler(this.trkSpeed_Scroll);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Location = new System.Drawing.Point(26, 172);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(2290, 1372);
            this.tabControl1.TabIndex = 36;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.pnlKeyboard);
            this.tabPage2.Font = new System.Drawing.Font("MS UI Gothic", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.tabPage2.Location = new System.Drawing.Point(8, 39);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.tabPage2.Size = new System.Drawing.Size(2274, 1325);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "演奏画面";
            // 
            // pnlKeyboard
            // 
            this.pnlKeyboard.AutoScroll = true;
            this.pnlKeyboard.Controls.Add(this.picKey);
            this.pnlKeyboard.Location = new System.Drawing.Point(9, 14);
            this.pnlKeyboard.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.pnlKeyboard.Name = "pnlKeyboard";
            this.pnlKeyboard.Size = new System.Drawing.Size(2216, 1294);
            this.pnlKeyboard.TabIndex = 0;
            // 
            // picKey
            // 
            this.picKey.BackColor = System.Drawing.Color.Transparent;
            this.picKey.BackgroundImage = global::EasySequencer.Properties.Resources.keyboard;
            this.picKey.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picKey.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picKey.InitialImage = null;
            this.picKey.Location = new System.Drawing.Point(0, 0);
            this.picKey.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.picKey.Name = "picKey";
            this.picKey.Size = new System.Drawing.Size(2203, 1282);
            this.picKey.TabIndex = 0;
            this.picKey.TabStop = false;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Location = new System.Drawing.Point(8, 39);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.tabPage1.Size = new System.Drawing.Size(2274, 1325);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "入力画面";
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2321, 1554);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.pnlPlayer);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numKey)).EndInit();
            this.pnlPlayer.ResumeLayout(false);
            this.pnlPlayer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trkSpeed)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.pnlKeyboard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picKey)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox picKey;
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
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Panel pnlKeyboard;
		private System.Windows.Forms.TrackBar trkSpeed;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem wavファイル出力ToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblTempoPercent;
    }
}

