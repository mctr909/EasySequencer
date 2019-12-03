using System;
using System.Windows.Forms;
using System.IO;

using Player;
using MIDI;
using WaveOut;

namespace EasySequencer {
    unsafe public partial class Form1 : Form {
        private SMF mSMF;
        private Sender mMidiSender;
        private Player.Player mPlayer;
        private Keyboard mKeyboard;

        private string mDlsFilePath;
        private bool mIsSeek = false;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            mDlsFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\gm.dls";
            mMidiSender = new Sender(mDlsFilePath);
            mPlayer = new Player.Player(mMidiSender);
            mKeyboard = new Keyboard(picKey, mMidiSender, mPlayer);

            setSize();

            timer1.Interval = 20;
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
                mSMF = new SMF(filePath);
                mPlayer.SetEventList(mSMF.EventList, mSMF.Ticks);
                hsbSeek.Maximum = mPlayer.MaxTick;
                Text = Path.GetFileNameWithoutExtension(filePath);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void wavファイル出力ToolStripMenuItem_Click(Object sender, EventArgs e) {
            if(null == mSMF || null == mMidiSender || Sender.IsFileOutput) {
                return;
            }

            saveFileDialog1.Filter = "wavファイル(*.wav)|*.wav";
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(Text);
            saveFileDialog1.ShowDialog();
            var filePath = saveFileDialog1.FileName;

            var fm = new StatusWindow();
            var ev = mSMF.EventList;
            fm.TimeMax = (int)(ev[ev.Length - 1].Time / mSMF.Ticks);
            fixed (int* p = &mMidiSender.OutputTime) {
                fm.Time = p;
            }
            mMidiSender.FileOut(filePath, ev, mSMF.Ticks);
            fm.Show();
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
            mPlayer.Transpose = (int)numericUpDown1.Value;
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
            hsbSeek.Width = lblPosition.Right;
            pnlPlayer.Width = lblPosition.Right + 8;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            lblPosition.Text = mPlayer.PositionText;
            lblTempo.Text = mPlayer.TempoText;
            lblTempoPercent.Text = trkSpeed.Value + "%";

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
