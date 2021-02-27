using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using SMF;

namespace Player {
    public class Player {
        private Sender mSender;
        private Event[] mEventList;
        private Stopwatch mSw;
        private Task mTask;

        private long mPrevious_mSec;
        private double mCurrentTick;
        private double mPreviousTick;
        private double mTick;
        private double mBPM;
        private E_KEY mKey;

        private int mBeat;
        private int mMeasure;
        private int mMeasureDenomi;
        private int mMeasureNumer;

        public CHANNEL_PARAM Channel(int num) {
            return mSender.Channel(num);
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
            var mesure = new Mesure() {
                denominator = 4,
                numerator = 4
            };

            foreach (var ev in mEventList) {
                if (tick <= currentTick) {
                    break;
                }
                while (currentTick < ev.Tick) {
                    currentTick++;
                    ticks++;
                    if (3840 / mesure.denominator <= ticks) {
                        ticks -= 3840 / mesure.denominator;
                        ++beat;
                        if (mesure.numerator <= beat) {
                            beat -= mesure.numerator;
                            ++measures;
                        }
                    }
                }
                if (E_STATUS.META == ev.Type) {
                    if (E_META.MEASURE == ev.Meta.Type) {
                        mesure = new Mesure(ev.Meta.Int);
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
            mBPM = 120;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            IsPlay = false;
            SoloChannel = -1;
            Speed = 1.0;
            mTask = new Task(MainProc);
        }

        public void SetEventList(Event[] eventList) {
            mEventList = eventList;
            MaxTick = 0;

            foreach (var ev in eventList) {
                if (E_STATUS.NOTE_OFF == ev.Type || E_STATUS.NOTE_ON == ev.Type) {
                    if (MaxTick < ev.Tick) {
                        MaxTick = ev.Tick;
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
            mSw = new Stopwatch();
            mSw.Start();
            IsPlay = true;
            mTask = new Task(MainProc);
            mTask.Start();
        }

        public void Stop() {
            IsPlay = false;
            while (!mTask.IsCompleted) {
                Thread.Sleep(100);
            }

            for (byte ch = 0; ch < 16; ++ch) {
                for (byte noteNo = 0; noteNo < 128; ++noteNo) {
                    mSender.Send(new Event(ch, E_STATUS.NOTE_OFF, noteNo));
                    Task.Delay(10);
                }
                mSender.Send(new Event(ch, E_CONTROL.ALL_RESET));
            }
        }

        private void MainProc() {
            foreach (Event e in mEventList) {
                if (!IsPlay) {
                    return;
                }

                var ev = e;

                while (mCurrentTick < ev.Tick) {
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
                    Thread.Sleep(1);
                }


                switch (ev.Type) {
                case E_STATUS.NOTE_OFF: {
                    var chParam = mSender.Channel(ev.Channel);
                    if (0 == chParam.InstId.isDrum) {
                        if ((ev.Data[1] + Transpose) < 0 || 127 < (ev.Data[1] + Transpose)) {
                            continue;
                        } else {
                            ev = new Event(ev.Channel, E_STATUS.NOTE_OFF, ev.Data[1] + Transpose, ev.Data[2]);
                        }
                    } else {
                        continue;
                    }
                }
                break;
                case E_STATUS.NOTE_ON: {
                    var chParam = mSender.Channel(ev.Channel);
                    if (ev.Data[2] != 0) {
                        if (0.25 * 960 < (mCurrentTick - ev.Tick)) {
                            continue;
                        }
                        if (!chParam.Enable || (0 <= SoloChannel && SoloChannel != ev.Channel)) {
                            continue;
                        }
                    }
                    if (0 == chParam.InstId.isDrum) {
                        if ((ev.Data[1] + Transpose) < 0 || 127 < (ev.Data[1] + Transpose)) {
                            continue;
                        } else {
                            ev = new Event(ev.Channel, E_STATUS.NOTE_ON, ev.Data[1] + Transpose, ev.Data[2]);
                        }
                    }
                }
                break;
                case E_STATUS.META:
                    switch (ev.Meta.Type) {
                    case E_META.TEMPO:
                        mBPM = 60000000.0 / ev.Meta.Int;
                        break;
                    case E_META.MEASURE:
                        var m = new Mesure(ev.Meta.Int);
                        mMeasureNumer = m.numerator;
                        mMeasureDenomi = m.denominator;
                        break;
                    case E_META.KEY:
                        mKey = (E_KEY)ev.Meta.Int;
                        break;
                    }
                    break;
                }
                //
                mSender.Send(ev);
            }
        }
    }
}
