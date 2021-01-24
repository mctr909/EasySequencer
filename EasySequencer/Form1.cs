using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

using Player;
using SMF;

namespace EasySequencer {
    public partial class Form1 : Form {
        private SMF.SMF mSMF;
        private Sender mMidiSender;
        private Player.Player mPlayer;
        private Keyboard mKeyboard;
        private Bitmap mBmpActive;
        private Graphics mGActive;
        private string mDlsFilePath;
        private bool mIsSeek = false;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            //mDlsFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\AnyConv.com__Equinox_Grand_Pianos.sf2";
            //mDlsFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\gm.sf2";
            mDlsFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\test.dls";
            mMidiSender = new Sender(mDlsFilePath);
            mPlayer = new Player.Player(mMidiSender);
            mKeyboard = new Keyboard(picKeyBack, mMidiSender, mPlayer);

            setSize();

            mBmpActive = new Bitmap(picActive.Width, picActive.Height);
            mGActive = Graphics.FromImage(mBmpActive);

            timer1.Interval = 25;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void 開くOToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.Filter = "MIDIファイル(*.mid)|*.mid";
            openFileDialog1.ShowDialog();
            var filePath = openFileDialog1.FileName;
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) {
                return;
            }

            if (mPlayer.IsPlay) {
                mPlayer.Stop();
                btnPalyStop.Text = "再生";
            }

            try {
                mSMF = new SMF.SMF(filePath);
                mPlayer.SetEventList(mSMF.EventList, mSMF.Ticks);
                hsbSeek.Maximum = mPlayer.MaxTick;
                Text = Path.GetFileNameWithoutExtension(filePath);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void wavファイル出力ToolStripMenuItem_Click(Object sender, EventArgs e) {
            if (null == mSMF || null == mMidiSender || Sender.IsFileOutput) {
                return;
            }

            saveFileDialog1.Filter = "wavファイル(*.wav)|*.wav";
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(Text);
            saveFileDialog1.ShowDialog();
            var filePath = saveFileDialog1.FileName;

            mMidiSender.FileOut(filePath, mSMF);
        }

        private void btnPalyStop_Click(object sender, EventArgs e) {
            if (mPlayer.IsPlay) {
                mPlayer.Stop();
            }
            else {
                mPlayer.Play();
            }

            btnPalyStop.Text = mPlayer.IsPlay ? "停止" : "再生";
        }

        private void hsbSeek_MouseLeave(object sender, EventArgs e) {
            if (mIsSeek) {
                mIsSeek = false;
                mPlayer.Seek = hsbSeek.Value;
                btnPalyStop.Text = "停止";
            }
        }

        private void hsbSeek_Scroll(object sender, ScrollEventArgs e) {
            mIsSeek = true;
        }

        private void trkSpeed_Scroll(Object sender, EventArgs e) {
            mPlayer.Speed = trkSpeed.Value / 100.0;
        }

        private void numKey_ValueChanged(Object sender, EventArgs e) {
            mIsSeek = true;
            mPlayer.Transpose = (int)numKey.Value;
            mPlayer.Seek = hsbSeek.Value;
            mIsSeek = false;
        }

        private void Form1_SizeChanged(object sender, EventArgs e) {
            setSize();
        }

        private void setSize() {
            tabControl1.Width = Width - tabControl1.Location.X - 20;
            tabControl1.Height = Height - tabControl1.Location.Y - 48;
            pnlKeyboard.Width = tabControl1.Width - 16;
            pnlKeyboard.Height = tabControl1.Height - 38;

            btnPalyStop.Top = 2;
            btnPalyStop.Left = 2;
            numKey.Top = 2;
            numKey.Left = btnPalyStop.Right + 4;
            lblPosition.Top = 0;
            lblPosition.Left = numKey.Right + 4;
            trkSpeed.Width = 150;
            trkSpeed.Top = 0;
            trkSpeed.Left = lblPosition.Right;
            lblTempo.Width = 260;
            lblTempo.Top = 0;
            lblTempo.Left = trkSpeed.Right;
            lblTempoPercent.Width = 100;
            lblTempoPercent.Top = 3;
            lblTempoPercent.Left = lblTempo.Right;
            picActive.Width = 150;
            picActive.Height = 24;
            picActive.Top = 2;
            picActive.Left = lblTempoPercent.Right;
            hsbSeek.Width = picActive.Right - btnPalyStop.Left + 1;
            hsbSeek.Height = 21;
            hsbSeek.Top = numKey.Bottom + 2;
            hsbSeek.Left = 0;
            pnlPlayer.Width = hsbSeek.Width + 4;

            Width = Properties.Resources.Keyboard.Width + 50;
            Height = Properties.Resources.Keyboard.Height + pnlPlayer.Height + 122;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            lblPosition.Text = mPlayer.PositionText;
            lblTempo.Text = mPlayer.TempoText;
            lblTempoPercent.Text = trkSpeed.Value + "%";

            mGActive.Clear(Color.Transparent);
            mGActive.FillRectangle(Brushes.LightGreen, 0, 0,
                (float)Sender.ActiveCount * mBmpActive.Width / Sender.SAMPLER_COUNT,
                mBmpActive.Height);

            picActive.Image = mBmpActive;

            if (!mIsSeek) {
                if (mPlayer.CurrentTick <= hsbSeek.Maximum) {
                    hsbSeek.Value = mPlayer.CurrentTick;
                }
                else {
                    hsbSeek.Value = 0;
                    mPlayer.Seek = hsbSeek.Value;
                }
            }
        }
    }
}
