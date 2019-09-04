using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using MIDI;
using WaveOut;

namespace Player {
    unsafe public class Player {
        private Sender mSender;
        private Event[] mEventList;
        private Stopwatch mSw;
        private Task mTask;
        private Event mCurEvent = new Event();

        private int mTicksPerBeat;
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
                if (E_EVENT_TYPE.META == ev.Type) {
                    if (E_META_TYPE.MEASURE == ev.Meta.Type) {
                        measureNumer = ev.Meta.MeasureNumer;
                        measureDenomi = ev.Meta.MeasureDenomi;
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
            mTask = new Task(MainProc);
        }

        public void SetEventList(Event[] eventList, int ticksPerBeat) {
            mEventList = eventList;
            mTicksPerBeat = ticksPerBeat;
            MaxTick = 0;

            foreach (var ev in eventList) {
                if (E_EVENT_TYPE.NOTE_OFF == ev.Type || E_EVENT_TYPE.NOTE_ON == ev.Type) {
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
                    mSender.Send(Event.NoteOff(ch, noteNo));
                    Task.Delay(10);
                }
                mSender.Send(new Event(E_CTRL_TYPE.ALL_RESET, ch, 0));
            }
        }

        private void MainProc() {
            foreach (Event e in mEventList) {
                if (!IsPlay) {
                    return;
                }

                var ev = e;
                var eventTick = 960 * ev.Time / mTicksPerBeat;

                while (mCurrentTick < eventTick) {
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
                }

                switch (ev.Type) {
                case E_EVENT_TYPE.NOTE_ON:
                case E_EVENT_TYPE.NOTE_OFF:
                    if (ev.Velocity != 0) {
                        if (0.25 * mTicksPerBeat < (mCurrentTick - eventTick)) {
                            continue;
                        }
                        if (!mSender.Channel[ev.Channel].Enable || (0 <= SoloChannel && SoloChannel != ev.Channel)) {
                            continue;
                        }
                    }
                    if (0x00 == mSender.Channel[ev.Channel].InstId.isDrum) {
                        if ((ev.NoteNo + Transpose) < 0 || 127 < (ev.NoteNo + Transpose)) {
                            continue;
                        } else {
                            ev = new Event(ev.Status, (byte)(ev.NoteNo + Transpose), ev.Velocity);
                        }
                    }
                    break;
                case E_EVENT_TYPE.META:
                    switch (ev.Meta.Type) {
                    case E_META_TYPE.TEMPO:
                        mBPM = ev.Meta.Tempo;
                        break;
                    case E_META_TYPE.MEASURE:
                        mMeasureNumer = ev.Meta.MeasureNumer;
                        mMeasureDenomi = ev.Meta.MeasureDenomi;
                        break;
                    case E_META_TYPE.KEY:
                        mKey = ev.Meta.Key;
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
