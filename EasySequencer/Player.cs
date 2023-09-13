using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using EasySequencer.Properties;
using SMF;
using SynthDll;

namespace Player {
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

        SMF.File mSMF;
        Stopwatch mSw;
        Task mTask;
        Event[] mEventList;

        bool mIsPlay = false;
        bool mIsSeek = false;
        int mTranspose = 0;
        double mSpeed = 1.0;

        long mPrevious_mSec;
        double mCurrentTick;
        double mPreviousTick;
        double mBeatTick;
        double mBPM;
        int mMeasureDenomi;
        int mMeasureNumer;
        int mMeasure;
        int mBeat;
        int mMaxTick;

        EasySequencer.Monitor mMonitor;
        Graphics mG;
        string mDlsFilePath;
        static readonly Bitmap[,] BMP_FONT = new Bitmap[16, 6];
        Point mMouseDownPos;
        bool mMouseDown = false;

        int Seek {
            get { return (int)mCurrentTick; }
            set {
                var isPlay = mIsPlay;
                if (isPlay) {
                    stop();
                }
                if (value < 0) {
                    mCurrentTick = 0.0;
                } else if (mMaxTick < value) {
                    mCurrentTick = mMaxTick;
                } else {
                    mCurrentTick = value;
                }
                mPreviousTick = mCurrentTick;
                if (isPlay) {
                    play();
                } else {
                    countMesure();
                }
            }
        }

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

        private void Player_Shown(object sender, EventArgs e) {
            mDlsFilePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\gm.dls";

            if (!Synth.Setup(mDlsFilePath, 44100)) {
                Close();
                return;
            }

            reset();

            picPlayer.Image = new Bitmap(picPlayer.Width, picPlayer.Height);
            mG = Graphics.FromImage(picPlayer.Image);

            timer1.Interval = 50;
            timer1.Enabled = true;
            timer1.Start();
        }

        private void 開くOToolStripMenuItem_Click(object sender, EventArgs e) {
            openFileDialog1.Filter = "MIDIファイル(*.mid)|*.mid";
            openFileDialog1.ShowDialog();
            var filePath = openFileDialog1.FileName;
            if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath)) {
                return;
            }

            reset();

            try {
                mSMF = new SMF.File(filePath);
                setEventList(mSMF.EventList);
                Text = Path.GetFileNameWithoutExtension(filePath);
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void wavファイル出力ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (null == mSMF) {
                return;
            }

            saveFileDialog1.Filter = "wavファイル(*.wav)|*.wav";
            saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(Text);
            saveFileDialog1.ShowDialog();
            var filePath = saveFileDialog1.FileName;

            Synth.FileOut(mDlsFilePath, filePath, mSMF);
        }

        private void ピアノロールPToolStripMenuItem_Click(object sender, EventArgs e) {

        }

        private void トラック編集TToolStripMenuItem_Click(object sender, EventArgs e) {

        }

        private void 演奏モニタMToolStripMenuItem_Click(object sender, EventArgs e) {
            if (null == mMonitor || mMonitor.IsDisposed) {
                mMonitor = new EasySequencer.Monitor();
                mMonitor.Show();
            } else {
                mMonitor.Close();
            }
        }

        private void picPlayer_MouseDown(object sender, MouseEventArgs e) {
            mMouseDownPos = picPlayer.PointToClient(Cursor.Position);
            mMouseDown = true;
            if (RECT_SEEK.Contains(mMouseDownPos)) {
                mIsSeek = true;
            }
            if (RECT_FF.Contains(mMouseDownPos)) {
                mSpeed = 8.0;
            }
        }

        private void picPlayer_MouseUp(object sender, MouseEventArgs e) {
            mMouseDown = false;
            if (RECT_PREV.Contains(mMouseDownPos)) {
                if (mIsPlay) {
                    reset();
                    play();
                } else {
                    reset();
                }
            }
            if (RECT_STOP.Contains(mMouseDownPos) && mIsPlay) {
                stop();
            }
            if (RECT_PLAY.Contains(mMouseDownPos) && !mIsPlay) {
                play();
            }
            if (RECT_FF.Contains(mMouseDownPos)) {
                mSpeed = 1.0;
            }
            if (RECT_KEY_DOWN.Contains(mMouseDownPos)) {
                if (mIsPlay) {
                    stop();
                    mTranspose--;
                    play();
                } else {
                    mTranspose--;
                }
            }
            if (RECT_KEY_UP.Contains(mMouseDownPos)) {
                if (mIsPlay) {
                    stop();
                    mTranspose++;
                    play();
                } else {
                    mTranspose++;
                }
            }
            if (RECT_SPEED_DOWN.Contains(mMouseDownPos)) {
                if (0.25 < mSpeed) {
                    mSpeed -= 0.125;
                }
            }
            if (RECT_SPEED_UP.Contains(mMouseDownPos)) {
                if (mSpeed < 2.0) {
                    mSpeed += 0.125;
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
                Seek = mMaxTick * pos.X / RECT_SEEK.Width;
            }
            mIsSeek = false;
        }

        private void timer1_Tick(object sender, EventArgs e) {
            mG.Clear(Color.Transparent);

            draw7seg(POS_MEASURE, (mMeasure + 1).ToString().PadLeft(4, ' '));
            draw7seg(POS_BEAT, (mBeat + 1).ToString().PadLeft(2, ' '));
            draw7seg(POS_TEMPO, 2 < mSpeed ? "----" :
                (mBPM * mSpeed * 10).ToString("0").PadLeft(4, ' '));
            draw7seg(POS_SPEED, (mSpeed * 100).ToString("0").PadLeft(3, ' '));
            draw7seg(POS_TRANSPOSE, mTranspose.ToString("+0;-0;0").PadLeft(3, ' '));

            if (mIsPlay) {
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
                if (!mIsPlay) {
                    var seekPos = picPlayer.PointToClient(Cursor.Position);
                    seekPos.X -= RECT_SEEK.X;
                    if (seekPos.X < 0) {
                        seekPos.X = 0;
                    }
                    if (RECT_SEEK.Width - 1 < seekPos.X) {
                        seekPos.X = RECT_SEEK.Width - 1;
                    }
                    Seek = mMaxTick * seekPos.X / RECT_SEEK.Width;
                }
            } else {
                var seek = 0;
                if (0 < mMaxTick) {
                    seek = Seek * RECT_SEEK.Width / mMaxTick;
                }
                mG.DrawImageUnscaled(Resources.player_seek, RECT_SEEK.X + seek - Resources.player_seek.Width / 2, RECT_SEEK.Y);
            }

            picPlayer.Image = picPlayer.Image;
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

        void reset() {
            if (mIsPlay) {
                stop();
            }
            mBPM = 120;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            mPrevious_mSec = 0;
            mPreviousTick = 0.0;
            mCurrentTick = 0.0;
            mMeasure = 0;
            mBeat = 0;
            mBeatTick = 0.0;
            mSw = new Stopwatch();
        }

        void play() {
            if (null == mEventList) {
                return;
            }
            countMesure();
            mSw.Start();
            mIsPlay = true;
            mTask = new Task(mainProc);
            mTask.Start();
        }

        void stop() {
            if (null == mSw || null == mTask) {
                return;
            }
            mIsPlay = false;
            mSw.Stop();
            while (!mTask.IsCompleted) {
                Thread.Sleep(10);
            }
            for (int track_num = 0; track_num < Synth.TRACK_COUNT; ++track_num) {
                var port = (byte)(track_num / 16);
                var chNum = track_num % 16;
                for (byte noteNo = 0; noteNo < 128; ++noteNo) {
                    Synth.Send(port, new Event(chNum, E_STATUS.NOTE_OFF, noteNo));
                }
                Synth.Send(port, new Event(chNum, E_CONTROL.ALL_RESET));
            }
        }

        void setEventList(Event[] eventList) {
            mEventList = eventList;
            mMaxTick = 0;
            foreach (var ev in eventList) {
                if (E_STATUS.NOTE_OFF == ev.Type || E_STATUS.NOTE_ON == ev.Type) {
                    if (mMaxTick < ev.Tick) {
                        mMaxTick = ev.Tick;
                    }
                }
            }
            mBPM = 120.0;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            mCurrentTick = 0.0;
        }

        void countMesure() {
            if (null == mEventList) {
                return;
            }
            mMeasure = 0;
            mBeat = 0;
            mBeatTick = 0;
            int curTick = 0;
            int preTick = 0;
            foreach (Event ev in mEventList) {
                curTick += ev.Tick - preTick;
                mBeatTick += ev.Tick - preTick;
                if (mCurrentTick <= curTick) {
                    return;
                }
                preTick = ev.Tick;
                while (3840 / mMeasureDenomi <= mBeatTick) {
                    mBeatTick -= 3840 / mMeasureDenomi;
                    ++mBeat;
                    if (mMeasureNumer <= mBeat) {
                        mBeat -= mMeasureNumer;
                        ++mMeasure;
                    }
                }
                switch (ev.Type) {
                case E_STATUS.META:
                    switch (ev.Meta.Type) {
                    case E_META.MEASURE:
                        var m = new Mesure(ev.Meta.UInt);
                        mMeasureNumer = m.numerator;
                        mMeasureDenomi = m.denominator;
                        break;
                    case E_META.KEY:
                        break;
                    }
                    break;
                }
            }
        }

        void mainProc() {
            foreach (Event e in mEventList) {
                if (!mIsPlay) {
                    return;
                }

                var port = mSMF.Tracks[e.Track].Port;
                var ev = e;

                while (mCurrentTick < ev.Tick) {
                    if (!mIsPlay || mMaxTick <= mCurrentTick) {
                        return;
                    }
                    var current_mSec = mSw.ElapsedMilliseconds;
                    var deltaTime = current_mSec - mPrevious_mSec;
                    mCurrentTick += 0.096 * mBPM * mSpeed * deltaTime / 6.0;
                    mBeatTick += mCurrentTick - mPreviousTick;
                    while (3840 / mMeasureDenomi <= mBeatTick) {
                        mBeatTick -= 3840 / mMeasureDenomi;
                        ++mBeat;
                        if (mMeasureNumer <= mBeat) {
                            mBeat -= mMeasureNumer;
                            ++mMeasure;
                        }
                    }
                    mPrevious_mSec = current_mSec;
                    mPreviousTick = mCurrentTick;
                    Thread.Sleep(1);
                }

                switch (ev.Type) {
                case E_STATUS.NOTE_OFF: {
                    var param = Synth.GetChannel(ev.Channel);
                    if (0 == param.is_drum) {
                        if ((ev.Data[1] + mTranspose) < 0 || 127 < (ev.Data[1] + mTranspose)) {
                            continue;
                        } else {
                            ev = new Event(ev.Channel, E_STATUS.NOTE_OFF, ev.Data[1] + mTranspose, ev.Data[2]);
                        }
                    } else {
                        ev = new Event(ev.Channel, E_STATUS.NOTE_OFF, ev.Data[1], ev.Data[2]);
                    }
                }
                break;
                case E_STATUS.NOTE_ON: {
                    var param = Synth.GetChannel(ev.Channel);
                    if (ev.Data[2] != 0) {
                        if (0.25 * 960 < (mCurrentTick - ev.Tick)) {
                            continue;
                        }
                        if (0 == param.enable) {
                            continue;
                        }
                    }
                    if (0 == param.is_drum) {
                        if ((ev.Data[1] + mTranspose) < 0 || 127 < (ev.Data[1] + mTranspose)) {
                            continue;
                        } else {
                            ev = new Event(ev.Channel, E_STATUS.NOTE_ON, ev.Data[1] + mTranspose, ev.Data[2]);
                        }
                    } else {
                        ev = new Event(ev.Channel, E_STATUS.NOTE_ON, ev.Data[1], ev.Data[2]);
                    }
                }
                break;

                case E_STATUS.META:
                    switch (ev.Meta.Type) {
                    case E_META.TEMPO:
                        mBPM = 60000000.0 / ev.Meta.UInt;
                        break;
                    case E_META.MEASURE:
                        var m = new Mesure(ev.Meta.UInt);
                        mMeasureNumer = m.numerator;
                        mMeasureDenomi = m.denominator;
                        break;
                    case E_META.KEY:
                        break;
                    }
                    break;
                }

                /*** send message ***/
                Synth.Send(port, ev);
            }
        }
    }
}
