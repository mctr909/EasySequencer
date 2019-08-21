using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MIDI;
using WaveOut;

namespace Player {
    unsafe public class Player {
        private Sender mSender;
        private Event[] mEventList;
        private System.Windows.Forms.Timer mTimer;
        private Stopwatch mSw;
        private Queue<Event> mEventQue;
        private Event mCurEvent = new Event(0, 0, 0, new Message(EVENT_TYPE.SYS_EX, 0));

        private int mTicksPerBeat;
        private long mPrevious_mSec;
        private double mCurrentTick;
        private double mPreviousTick;
        private double mTick;
        private double mBPM;

        private int mBeat;
        private int mMeasure;
        private int mMeasureDenomi;
        private int mMeasureNumer;

        public Channel[] Channel {
            get { return mSender.Channel; }
        }

        public int SoloChannel { get; set; }

        public int Transpose { get; set; }

        public double Speed { get; set; }

        public int Seek {
            set {
                Stop();
                if (value < 0) {
                    mCurrentTick = 0.0;
                }
                else if (MaxTick < value) {
                    mCurrentTick = MaxTick;
                }
                else {
                    mCurrentTick = value;
                }
                Play();
            }
        }

        public int MaxTick { get; private set; }

        public int CurrentTick {
            get { return (int)mCurrentTick; }
        }

        public bool IsPlay { get; private set; }

        public string GetPositionText(int tick) {
            int ticks = 0;
            int beat = 0;
            int measures = 0;
            int currentTick = 0;
            int measureNumer = 4;
            int measureDenomi = 4;

            foreach (var ev in mEventList) {
                if (tick <= currentTick) {
                    break;
                }
                long eventTick = 960 * ev.Time / mTicksPerBeat;
                while (currentTick < eventTick) {
                    currentTick++;
                    ticks++;
                    if (3840 / measureDenomi <= ticks) {
                        ticks -= 3840 / measureDenomi;
                        ++beat;
                        if (measureNumer <= beat) {
                            beat -= measureNumer;
                            ++measures;
                        }
                    }
                }
                var msg = ev.Message;
                var type = msg.Type;
                if (EVENT_TYPE.META == type) {
                    if (META_TYPE.MEASURE == msg.Meta.Type) {
                        measureNumer = msg.Meta.MeasureNumer;
                        measureDenomi = msg.Meta.MeasureDenomi;
                    }
                }
            }
            return string.Format(
                "{0}:{1}:{2}",
                (measures + 1).ToString("0000"),
                (beat + 1).ToString("00"),
                ticks.ToString("0000")
            );
        }

        public string PositionText {
            get {
                return string.Format(
                    "{0}:{1}:{2}",
                    (mMeasure + 1).ToString("0000"),
                    (mBeat + 1).ToString("00"),
                    ((int)mTick).ToString("0000")
                );
            }
        }

        public string TempoText {
            get { return (mBPM * Speed).ToString("000.00"); }
        }

        public Player(Sender sender) {
            mSender = sender;
            mEventList = null;
            mTicksPerBeat = 960;
            mBPM = 120;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            IsPlay = false;
            SoloChannel = -1;
            Speed = 1.0;
            mEventQue = new Queue<Event>();
            mTimer = new System.Windows.Forms.Timer();
            mTimer.Tick += new System.EventHandler(MainProc);
            mTimer.Interval = 1;
            mTimer.Enabled = true;
        }

        public void SetEventList(Event[] eventList, int ticksPerBeat) {
            mEventList = eventList;
            mTicksPerBeat = ticksPerBeat;
            MaxTick = 0;

            foreach (var ev in eventList) {
                if (EVENT_TYPE.NOTE_OFF == ev.Message.Type || EVENT_TYPE.NOTE_ON == ev.Message.Type) {
                    var time = 960 * ev.Time / ticksPerBeat;
                    if (MaxTick < time) {
                        MaxTick = (int)time;
                    }
                }
            }

            mBPM = 120.0;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            mCurrentTick = 0.0;
        }

        public void Play() {
            if (null == mEventList) {
                return;
            }
            mPrevious_mSec = 0;
            mPreviousTick = 0.0;
            mMeasure = 0;
            mBeat = 0;
            mTick = 0.0;
            mEventQue.Clear();
            foreach (var ev in mEventList) {
                mEventQue.Enqueue(ev);
            }
            mCurEvent = mEventQue.Dequeue();
            mSw = new Stopwatch();
            mSw.Start();
            if (null != mTimer) {
                mTimer.Dispose();
            }
            IsPlay = true;
            mTimer.Start();
        }

        public void Stop() {
            IsPlay = false;
            mTimer.Stop();

            for (byte ch = 0; ch < 16; ++ch) {
                for (byte noteNo = 0; noteNo < 128; ++noteNo) {
                    mSender.Send(new Message(EVENT_TYPE.NOTE_OFF, ch, noteNo));
                    Task.Delay(10);
                }
                {
                    Message msg = new Message(CTRL_TYPE.ALL_RESET, ch);
                    mSender.Send(msg);
                }
            }
        }

        private void MainProc(object sender, System.EventArgs e) {
            if (!IsPlay) {
                return;
            }

            var current_mSec = mSw.ElapsedMilliseconds;
            var deltaTime = current_mSec - mPrevious_mSec;
            mCurrentTick += 0.96 * mBPM * Speed * deltaTime / 60.0;
            mTick += mCurrentTick - mPreviousTick;
            if (3840 / mMeasureDenomi <= mTick) {
                mTick -= 3840 / mMeasureDenomi;
                ++mBeat;
                if (mMeasureNumer <= mBeat) {
                    mBeat -= mMeasureNumer;
                    ++mMeasure;
                }
            }
            mPrevious_mSec = current_mSec;
            mPreviousTick = mCurrentTick;

            var eventTick = 960 * mCurEvent.Time / mTicksPerBeat;
            while (eventTick <= mCurrentTick) {
                if (!IsPlay) {
                    return;
                }

                var msg = mCurEvent.Message;
                var type = msg.Type;

                mCurEvent = mEventQue.Dequeue();
                eventTick = 960 * mCurEvent.Time / mTicksPerBeat;

                if (EVENT_TYPE.META == type) {
                    if (META_TYPE.TEMPO == msg.Meta.Type) {
                        mBPM = msg.Meta.Tempo;
                    }
                    if (META_TYPE.MEASURE == msg.Meta.Type) {
                        mMeasureNumer = msg.Meta.MeasureNumer;
                        mMeasureDenomi = msg.Meta.MeasureDenomi;
                    }
                }

                if (EVENT_TYPE.NOTE_ON == type && msg.V2 != 0) {
                    if (0.25 * mTicksPerBeat < (mCurrentTick - eventTick)) {
                        continue;
                    }

                    if (!mSender.Channel[msg.Channel].Enable || (0 <= SoloChannel && SoloChannel != msg.Channel)) {
                        continue;
                    }
                }

                if (EVENT_TYPE.NOTE_OFF == type || EVENT_TYPE.NOTE_ON == type) {
                    if (0x00 == mSender.Channel[msg.Channel].InstId.isDrum) {
                        if ((msg.V1 + Transpose) < 0 || 127 < (msg.V1 + Transpose)) {
                            continue;
                        } else {
                            msg = new Message(msg.Status, (byte)(msg.V1 + Transpose), msg.V2);
                        }
                    }
                }

                //
                mSender.Send(msg);
            }
        }
    }
}
