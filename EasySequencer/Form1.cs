using System;
using System.Windows.Forms;
using System.IO;

using Player;
using MIDI;

namespace EasySequencer {
    unsafe public partial class Form1 : Form {
        private string mDlsFilePath;

        private bool mIsSeek = false;
        private bool mIsParamChg = false;
        private int mKnobX = 0;
        private int mKnobY = 0;
        private int mChangeValue = 0;

        private SMF mSMF;
        private Sender mMidiSender;
        private Player.Player mPlayer;
        private Keyboard mKeyboard;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            mDlsFilePath = "C:\\Users\\owner\\Desktop\\gm.dls";
            mMidiSender = new Sender(mDlsFilePath);
            mPlayer = new Player.Player(mMidiSender);
            mKeyboard = new Keyboard(picKey, mPlayer);

            setSize();

            timer1.Interval = 30;
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
                mSMF = new MIDI.SMF(filePath);
                mPlayer.SetEventList(mSMF.EventList, mSMF.Ticks);
                hsbSeek.Maximum = mPlayer.MaxTime;
                Text = Path.GetFileNameWithoutExtension(filePath);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void wavファイル出力ToolStripMenuItem_Click(Object sender, EventArgs e) {
            if(null == mSMF || null == mMidiSender || MIDI.Sender.IsFileOutput) {
                return;
            }

            saveFileDialog1.Filter = "wavファイル(*.wav)|*.wav";
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
                mPlayer.SeekTime = hsbSeek.Value;
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
            mPlayer.SeekTime = hsbSeek.Value;
            mIsSeek = false;
        }

        private void picKeyboard_MouseDown(Object sender, MouseEventArgs e) {
            var pos = picKey.PointToClient(Cursor.Position);
            var knobX = (pos.X - Keyboard.KnobValPos[0].X) / 24;
            var knobY = pos.Y / 40;

            if (0 <= knobY && knobY <= 15) {
                if (knobX == 8) {
                    if (e.Button == MouseButtons.Right) {
                        if (mPlayer.Channel[knobY].Enable) {
                            for (int i = 0; i < mPlayer.Channel.Length; ++i) {
                                if (knobY == i) {
                                    mPlayer.Channel[i].Enable = false;
                                }
                                else {
                                    mPlayer.Channel[i].Enable = true;
                                }
                            }
                        }
                        else {
                            for (int i = 0; i < mPlayer.Channel.Length; ++i) {
                                if (knobY == i) {
                                    mPlayer.Channel[i].Enable = true;
                                }
                                else {
                                    mPlayer.Channel[i].Enable = false;
                                }
                            }
                        }
                    }
                    else {
                        mPlayer.Channel[knobY].Enable = !mPlayer.Channel[knobY].Enable;
                    }
                }

                if (0 <= knobX && knobX <= 7) {
                    mIsParamChg = true;
                    mKnobX = knobX;
                    mKnobY = knobY;
                }
            }
        }

        private void picKeyboard_MouseUp(Object sender, MouseEventArgs e) {
            if (mIsParamChg) {
                mIsParamChg = false;
                mKnobX = 0;
                mKnobY = 0;
            }
        }

        private void picKeyboard_MouseMove(Object sender, MouseEventArgs e) {
            if (mIsParamChg) {
                var pos = picKey.PointToClient(Cursor.Position);
                var knobCenter = Keyboard.KnobPos[mKnobX];
                knobCenter.Y += mKnobY * 40;

                var sx = pos.X - knobCenter.X;
                var sy = pos.Y - knobCenter.Y;
                var th = 0.625 * Math.Atan2(sx, -sy) / Math.PI;
                if (th < -0.5) {
                    th = -0.5;
                }
                if (0.5 < th) {
                    th = 0.5;
                }

                mChangeValue = (int)((th + 0.5) * 127.0);
                if (mChangeValue < 0) {
                    mChangeValue = 0;
                }
                if (127 < mChangeValue) {
                    mChangeValue = 127;
                }
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e) {
            setSize();
        }

        private void setSize() {
            tabControl1.Width = Width - tabControl1.Location.X - 20;
            tabControl1.Height = Height - tabControl1.Location.Y - 48;
            pnlKeyboard.Width = tabControl1.Width - 16;
            pnlKeyboard.Height = tabControl1.Height - 38;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            lblPosition.Text = mPlayer.TimeText;
            lblTempo.Text = mPlayer.TempoText;

            if (!mIsSeek) {
                if (mPlayer.CurrentTime <= hsbSeek.Maximum) {
                    hsbSeek.Value = mPlayer.CurrentTime;
                }
                else {
                    hsbSeek.Value = 0;
                    mPlayer.SeekTime = hsbSeek.Value;
                }
            }

            if (mIsParamChg) {
                switch (mKnobX) {
                case 0:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.VOLUME,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;

                case 1:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.EXPRESSION,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;

                case 2:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.PAN,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;

                case 3:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.REVERB,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;

                case 4:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.CHORUS,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;

                case 5:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.DELAY,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;

                case 6:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.CUTOFF,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;

                case 7:
                    mMidiSender.Send(new MIDI.Message(
                        CTRL_TYPE.RESONANCE,
                        (byte)mKnobY,
                        (byte)mChangeValue
                    ));
                    break;
                }
            }
        }
    }
}
