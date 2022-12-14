using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Player {
    public class Player {
        private Sender mSender;
        private Event[] mEventList;
        private Stopwatch mSw;
        private Task mTask;

        private long mPrevious_mSec;
        private double mCurrentTick;
        private double mPreviousTick;
        private double mBeatTick;

        private int mMeasureDenomi;
        private int mMeasureNumer;

        public double BPM { get; private set; }

        public int Measure { get; private set; }

        public int Beat { get; private set; }

        public int Transpose { get; set; }

        public double Speed { get; set; }

        public int Seek {
            get { return (int)mCurrentTick; }
            set {
                Stop();
                if (value < 0) {
                    mCurrentTick = 0.0;
                } else if (MaxTick < value) {
                    mCurrentTick = MaxTick;
                } else {
                    mCurrentTick = value;
                }
                mPreviousTick = mCurrentTick;
                Play();
            }
        }

        public int MaxTick { get; private set; }

        public bool IsPlay { get; private set; }

        public Player(Sender sender) {
            mSender = sender;
            mEventList = null;
            BPM = 120;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            IsPlay = false;
            Speed = 1.0;
            mTask = new Task(mainProc);
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

            BPM = 120.0;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            mCurrentTick = 0.0;
        }

        public void Reset() {
            if (IsPlay) {
                Stop();
            }
            mPrevious_mSec = 0;
            mPreviousTick = 0.0;
            mCurrentTick = 0.0;
            Measure = 0;
            Beat = 0;
            mBeatTick = 0.0;
            mSw = new Stopwatch();
        }

        public void Play() {
            if (null == mEventList) {
                return;
            }
            if (null == mSw) {
                Reset();
            }
            countMesure();
            mSw.Start();
            IsPlay = true;
            mTask = new Task(mainProc);
            mTask.Start();
        }

        public void Stop() {
            if (null == mSw) {
                return;
            }
            IsPlay = false;
            mSw.Stop();
            while (!mTask.IsCompleted) {
                Thread.Sleep(10);
            }
            for (byte ch = 0; ch < 16; ++ch) {
                for (byte noteNo = 0; noteNo < 128; ++noteNo) {
                    mSender.Send(new Event(ch, E_STATUS.NOTE_OFF, noteNo));
                }
                mSender.Send(new Event(ch, E_CONTROL.ALL_RESET));
            }
        }

        void countMesure() {
            Measure = 0;
            Beat = 0;
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
                    ++Beat;
                    if (mMeasureNumer <= Beat) {
                        Beat -= mMeasureNumer;
                        ++Measure;
                    }
                }
                switch (ev.Type) {
                    case E_STATUS.META:
                        switch (ev.Meta.Type) {
                            case E_META.MEASURE:
                                var m = new Mesure(ev.Meta.Int);
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
                if (!IsPlay) {
                    return;
                }

                var ev = e;

                while (mCurrentTick < ev.Tick) {
                    if (!IsPlay || MaxTick <= mCurrentTick) {
                        return;
                    }
                    var current_mSec = mSw.ElapsedMilliseconds;
                    var deltaTime = current_mSec - mPrevious_mSec;
                    mCurrentTick += 0.096 * BPM * Speed * deltaTime / 6.0;
                    mBeatTick += mCurrentTick - mPreviousTick;
                    while (3840 / mMeasureDenomi <= mBeatTick) {
                        mBeatTick -= 3840 / mMeasureDenomi;
                        ++Beat;
                        if (mMeasureNumer <= Beat) {
                            Beat -= mMeasureNumer;
                            ++Measure;
                        }
                    }
                    mPrevious_mSec = current_mSec;
                    mPreviousTick = mCurrentTick;
                    Thread.Sleep(1);
                }

                switch (ev.Type) {
                case E_STATUS.NOTE_OFF: {
                        var chParam = mSender.Channel(ev.Channel);
                        if (0 == chParam.is_drum) {
                            if ((ev.Data[1] + Transpose) < 0 || 127 < (ev.Data[1] + Transpose)) {
                                continue;
                            } else {
                                ev = new Event(ev.Channel, E_STATUS.NOTE_OFF, ev.Data[1] + Transpose, ev.Data[2]);
                            }
                        } else {
                            ev = new Event(ev.Channel, E_STATUS.NOTE_OFF, ev.Data[1], ev.Data[2]);
                        }
                    }
                    break;
                case E_STATUS.NOTE_ON: {
                        var chParam = mSender.Channel(ev.Channel);
                        if (ev.Data[2] != 0) {
                            if (0.25 * 960 < (mCurrentTick - ev.Tick)) {
                                continue;
                            }
                            if (0 == chParam.enable) {
                                continue;
                            }
                        }
                        if (0 == chParam.is_drum) {
                            if ((ev.Data[1] + Transpose) < 0 || 127 < (ev.Data[1] + Transpose)) {
                                continue;
                            } else {
                                ev = new Event(ev.Channel, E_STATUS.NOTE_ON, ev.Data[1] + Transpose, ev.Data[2]);
                            }
                        } else {
                            ev = new Event(ev.Channel, E_STATUS.NOTE_ON, ev.Data[1], ev.Data[2]);
                        }
                    }
                    break;

                case E_STATUS.META:
                    switch (ev.Meta.Type) {
                    case E_META.TEMPO:
                        BPM = 60000000.0 / ev.Meta.Int;
                        break;
                    case E_META.MEASURE:
                        var m = new Mesure(ev.Meta.Int);
                        mMeasureNumer = m.numerator;
                        mMeasureDenomi = m.denominator;
                        break;
                    case E_META.KEY:
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
