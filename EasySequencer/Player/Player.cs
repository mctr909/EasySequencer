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
        private double mCurrentTime;
        private double mBPM;
        private int mMeasureDenomi;
        private int mMeasureNumer;
        private int mMeasures;
        private int mBeat;
        private double mTicks;

        public Channel[] Channel {
            get { return mSender.Channel; }
        }

        public int SoloChannel { get; set; }

        public int Transpose { get; set; }

        public double Speed { get; set; }

        public int SeekTime {
            set {
                Stop();
                if (value < 0) {
                    mCurrentTime = 0.0;
                }
                else if (MaxTime < value) {
                    mCurrentTime = MaxTime;
                }
                else {
                    mCurrentTime = value;
                }
                Play();
            }
        }

        public int MaxTime { get; private set; }

        public int CurrentTime {
            get { return (int)mCurrentTime; }
        }

        public bool IsPlay { get; private set; }

        public string TimeText {
            get {
                return string.Format(
                    "{0}:{1}:{2}",
                    (mMeasures + 1).ToString("0000"),
                    (mBeat + 1).ToString("00"),
                    ((int)mTicks).ToString("0000")
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
            MaxTime = 0;

            foreach (var ev in eventList) {
                if (EVENT_TYPE.NOTE_OFF == ev.Message.Type || EVENT_TYPE.NOTE_ON == ev.Message.Type) {
                    var time = 1000 * ev.Time / ticksPerBeat;
                    if (MaxTime < time) {
                        MaxTime = (int)time;
                    }
                }
            }

            mBPM = 120.0;
            mMeasureDenomi = 4;
            mMeasureNumer = 4;
            mCurrentTime = 0.0;
        }

        public void Play() {
            if (null == mEventList) {
                return;
            }

            IsPlay = true;
            mMeasures = 0;
            mBeat = 0;
            mTicks = 0.0;
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
            double previous_time = 0.0;

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            foreach (var ev in mEventList) {
                if (!IsPlay) {
                    break;
                }

                long eventTime = 1000 * ev.Time / mTicksPerBeat;
                while (mCurrentTime < eventTime) {
                    if (!IsPlay) {
                        break;
                    }

                    current_mSec = sw.ElapsedMilliseconds;
                    mCurrentTime += mBPM * Speed * (current_mSec - previous_mSec) / 60.0;
                    var deltaTime = mCurrentTime - previous_time;
                    mTicks += deltaTime;
                    if (4000.0 / mMeasureDenomi <= mTicks) {
                        mTicks -= 4000.0 / mMeasureDenomi;
                        ++mBeat;
                        if (mMeasureNumer <= mBeat) {
                            mBeat -= mMeasureNumer;
                            ++mMeasures;
                        }
                    }
                    previous_time = mCurrentTime;
                    previous_mSec = current_mSec;
                    Thread.Sleep(1);
                }

                var msg = ev.Message;
                var type = msg.Type;

                if (EVENT_TYPE.META == type) {
                    if (META_TYPE.TEMPO == msg.Meta.Type) {
                        mBPM = msg.Meta.BPM;
                    }
                    if (META_TYPE.MEASURE == msg.Meta.Type) {
                        mMeasureNumer = msg.Meta.Data[2];
                        mMeasureDenomi = (int)System.Math.Pow(2.0, msg.Meta.Data[3]);
                    }
                }

                if (EVENT_TYPE.NOTE_ON == type && msg.V2 != 0) {
                    if (0.25 * mTicksPerBeat < (mCurrentTime - eventTime)) {
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
