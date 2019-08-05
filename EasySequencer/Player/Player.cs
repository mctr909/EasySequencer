using System.Threading;
using System.Threading.Tasks;

using MIDI;
using WaveOut;

namespace Player {
    unsafe public class Player {
        private Sender mSender;
        private Event[] mEventList;
        private Task mTask;

        private int mTicksPerBeat;
        private double mBPM;
        private double mCurrentTick;
        private double mTick;
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
            mTask = null;
            mTicksPerBeat = 960;
            mBPM = 120;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            IsPlay = false;
            SoloChannel = -1;
            Speed = 1.0;
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

            IsPlay = true;
            mMeasure = 0;
            mBeat = 0;
            mTick = 0.0;
            mTask = Task.Factory.StartNew(() => MainProc());
        }

        public void Stop() {
            if (null == mTask) {
                return;
            }

            IsPlay = false;
            while (!mTask.IsCompleted) {
                Task.Delay(100);
            }
            mTask = null;

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

        private void MainProc() {
            long current_mSec = 0;
            long previous_mSec = 0;
            double previousTick = 0.0;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            foreach (var ev in mEventList) {
                if (!IsPlay) {
                    break;
                }

                long eventTick = 960 * ev.Time / mTicksPerBeat;
                while (mCurrentTick < eventTick) {
                    if (!IsPlay) {
                        break;
                    }

                    current_mSec = sw.ElapsedMilliseconds;
                    mCurrentTick += 0.96 * mBPM * Speed * (current_mSec - previous_mSec) / 60.0;
                    var deltaTime = mCurrentTick - previousTick;
                    mTick += deltaTime;
                    if (3840 / mMeasureDenomi <= mTick) {
                        mTick -= 3840 / mMeasureDenomi;
                        ++mBeat;
                        if (mMeasureNumer <= mBeat) {
                            mBeat -= mMeasureNumer;
                            ++mMeasure;
                        }
                    }
                    previous_mSec = current_mSec;
                    previousTick = mCurrentTick;
                }

                var msg = ev.Message;
                var type = msg.Type;

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
                        }
                        else {
                            msg = new Message(msg.Status, (byte)(msg.V1 + Transpose), msg.V2);
                        }
                    }
                }

                mSender.Send(msg);
            }

            IsPlay = false;
        }
    }
}
