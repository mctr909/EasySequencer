using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Text;

using Player;
using EasySequencer.Properties;

namespace EasySequencer {
    public partial class Player : Form {
        const int FONT_WIDTH = 16;
        const int FONT_HEIGHT = 26;

        readonly Point POS_MEASURE = new Point(14, 20);
        readonly Point POS_BEAT = new Point(87, 20);
        readonly Point POS_TEMPO = new Point(128, 20);
        readonly Point POS_SPEED = new Point(212, 20);
        readonly Point POS_TRANSPOSE = new Point(284, 20);
        readonly Rectangle RECT_PREV = new Rectangle(10, 53, 35, 18);
        readonly Rectangle RECT_REW = new Rectangle(48, 53, 35, 18);
        readonly Rectangle RECT_STOP = new Rectangle(86, 53, 35, 18);
        readonly Rectangle RECT_PLAY = new Rectangle(124, 53, 35, 18);
        readonly Rectangle RECT_FF = new Rectangle(162, 53, 35, 18);
        readonly Rectangle RECT_SPEED_DOWN = new Rectangle(211, 53, 29, 18);
        readonly Rectangle RECT_SPEED_UP = new Rectangle(243, 53, 29, 18);
        readonly Rectangle RECT_KEY_DOWN = new Rectangle(284, 53, 29, 18);
        readonly Rectangle RECT_KEY_UP = new Rectangle(316, 53, 29, 18);
        readonly Rectangle RECT_SEEK = new Rectangle(12, 76, 330, 19);

        SMF mSMF;
        Sender mMidiSender;
        global::Player.Player mPlayer;
        Monitor mMonitor;
        Bitmap mBmp;
        Graphics mG;
        Bitmap mBmpActive;
        Graphics mGActive;
        string mDlsFilePath;
        bool mIsSeek = false;
        static readonly Bitmap[,] BMP_FONT = new Bitmap[16, 6];
        Point mMouseDownPos;
        bool mMouseDown = false;

        public Player() {
            InitializeComponent();
            for (int by = 0; by < 6; by++) {
                for (int bx = 0; bx < 16; bx++) {
                    BMP_FONT[bx, by] = new Bitmap(FONT_WIDTH, FONT_HEIGHT);
                    var g = Graphics.FromImage(BMP_FONT[bx, by]);
                    g.DrawImage(Resources.font_14seg, new Rectangle(0, 0, FONT_WIDTH, FONT_HEIGHT),
                        FONT_WIDTH * bx,
                        FONT_HEIGHT * by,
                        FONT_WIDTH,
                        FONT_HEIGHT,
                        GraphicsUnit.Pixel
                    );
                    g.Flush();
                    g.Dispose();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            //mDlsFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\gm.sf2";
            mDlsFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\gm.dls";

            mMidiSender = new Sender();
            if (!mMidiSender.Setup(mDlsFilePath)) {
                Close();
                return;
            }

            mPlayer = new global::Player.Player(mMidiSender);
            mMonitor = new Monitor(mMidiSender);
            mMonitor.Show();

            mBmp = new Bitmap(picPlayer.Width, picPlayer.Height);
            mG = Graphics.FromImage(mBmp);
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

            mPlayer.Reset();

            try {
                mSMF = new SMF(filePath);
                mPlayer.SetEventList(mSMF.EventList);
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

            mMidiSender.FileOut(mDlsFilePath, filePath, mSMF);
        }

        private void picPlayer_MouseDown(object sender, MouseEventArgs e) {
            mMouseDownPos = picPlayer.PointToClient(Cursor.Position);
            mMouseDown = true;
            if (RECT_SEEK.Contains(mMouseDownPos)) {
                mIsSeek = true;
            }
            if (RECT_FF.Contains(mMouseDownPos)) {
                mPlayer.Speed = 8.0;
            }
        }

        private void picPlayer_MouseUp(object sender, MouseEventArgs e) {
            mMouseDown = false;
            if (RECT_PREV.Contains(mMouseDownPos)) {
                if (mPlayer.IsPlay) {
                    mPlayer.Reset();
                    mPlayer.Play();
                } else {
                    mPlayer.Reset();
                }
            }
            if (RECT_STOP.Contains(mMouseDownPos) && mPlayer.IsPlay) {
                mPlayer.Stop();
            }
            if (RECT_PLAY.Contains(mMouseDownPos) && !mPlayer.IsPlay) {
                mPlayer.Play();
            }
            if (RECT_FF.Contains(mMouseDownPos)) {
                mPlayer.Speed = 1.0;
            }
            if (RECT_KEY_DOWN.Contains(mMouseDownPos)) {
                if (mPlayer.IsPlay) {
                    mPlayer.Stop();
                    mPlayer.Transpose--;
                    mPlayer.Play();
                } else {
                    mPlayer.Transpose--;
                }
            }
            if (RECT_KEY_UP.Contains(mMouseDownPos)) {
                if (mPlayer.IsPlay) {
                    mPlayer.Stop();
                    mPlayer.Transpose++;
                    mPlayer.Play();
                } else {
                    mPlayer.Transpose++;
                }
            }
            if (RECT_SPEED_DOWN.Contains(mMouseDownPos)) {
                if (0.25 < mPlayer.Speed) {
                    mPlayer.Speed -= 0.125;
                }
            }
            if (RECT_SPEED_UP.Contains(mMouseDownPos)) {
                if (mPlayer.Speed < 2.0) {
                    mPlayer.Speed += 0.125;
                }
            }
            if (mIsSeek) {
                var pos = picPlayer.PointToClient(Cursor.Position);
                pos.X -= RECT_SEEK.X;
                if (pos.X < 0) {
                    pos.X = 0;
                }
                if (RECT_SEEK.Width - 1 < pos.X) {
                    pos.X = RECT_SEEK.Width - 1;
                }
                mPlayer.Seek = mPlayer.MaxTick * pos.X / RECT_SEEK.Width;
            }
            mIsSeek = false;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            mG.Clear(Color.Transparent);
            draw7seg(POS_MEASURE, (mPlayer.Measure + 1).ToString().PadLeft(4, ' '));
            draw7seg(POS_BEAT, (mPlayer.Beat + 1).ToString().PadLeft(2, ' '));
            draw7seg(POS_TEMPO, 2 < mPlayer.Speed ? "----" :
                (mPlayer.BPM * mPlayer.Speed * 10).ToString("0").PadLeft(4, ' '));
            draw7seg(POS_SPEED, (mPlayer.Speed * 100).ToString("0").PadLeft(3, ' '));
            draw7seg(POS_TRANSPOSE, mPlayer.Transpose.ToString("+0;-0;0").PadLeft(3, ' '));

            if (mPlayer.IsPlay) {
                mG.DrawImageUnscaled(Resources.player_play, RECT_PLAY.X, RECT_PLAY.Y);
            }

            if (mMouseDown) {
                if (RECT_PREV.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_prev, RECT_PREV.X, RECT_PREV.Y);
                }
                if (RECT_REW.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_rew, RECT_REW.X, RECT_REW.Y);
                }
                if (RECT_STOP.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_stop, RECT_STOP.X, RECT_STOP.Y);
                }
                if (RECT_PLAY.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_play, RECT_PLAY.X, RECT_PLAY.Y);
                }
                if (RECT_FF.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_ff, RECT_FF.X, RECT_FF.Y);
                }
                if (RECT_SPEED_DOWN.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_down, RECT_SPEED_DOWN.X, RECT_SPEED_DOWN.Y);
                }
                if (RECT_SPEED_UP.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_up, RECT_SPEED_UP.X, RECT_SPEED_UP.Y);
                }
                if (RECT_KEY_DOWN.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_down, RECT_KEY_DOWN.X, RECT_KEY_DOWN.Y);
                }
                if (RECT_KEY_UP.Contains(mMouseDownPos)) {
                    mG.DrawImageUnscaled(Resources.player_up, RECT_KEY_UP.X, RECT_KEY_UP.Y);
                }
            }
            if (mIsSeek) {
                var pos = picPlayer.PointToClient(Cursor.Position);
                if (pos.X < RECT_SEEK.X) {
                    pos.X = RECT_SEEK.X;
                }
                if (RECT_SEEK.X + RECT_SEEK.Width - 1 < pos.X) {
                    pos.X = RECT_SEEK.X + RECT_SEEK.Width - 1;
                }
                mG.DrawImageUnscaled(Resources.player_seek, pos.X - Resources.player_seek.Width / 2, RECT_SEEK.Y);
            } else {
                var seek = 0;
                if (0 < mPlayer.MaxTick) {
                    seek = mPlayer.Seek * RECT_SEEK.Width / mPlayer.MaxTick;
                }
                mG.DrawImageUnscaled(Resources.player_seek, RECT_SEEK.X + seek - Resources.player_seek.Width / 2, RECT_SEEK.Y);
            }

            mGActive.Clear(Color.Transparent);
            mGActive.FillRectangle(Brushes.LightGreen, 0, 0,
                (float)mMidiSender.ActiveCount * mBmpActive.Width / Sender.SAMPLER_COUNT,
                mBmpActive.Height);

            picPlayer.Image = mBmp;
            picActive.Image = mBmpActive;
        }

        void draw7seg(Point pos, string value) {
            var bArr = Encoding.ASCII.GetBytes(value);
            for (int i = 0; i < bArr.Length; i++) {
                var cx = bArr[i] % 16;
                var cy = (bArr[i] - 0x20) / 16;
                mG.DrawImageUnscaled(BMP_FONT[cx, cy], new Rectangle(
                    pos.X + FONT_WIDTH * i, pos.Y,
                    FONT_WIDTH, FONT_HEIGHT
                ));
            }
        }
    }
}
