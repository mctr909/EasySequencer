using System.Threading;
using System.Threading.Tasks;

using MIDI;

namespace Player {
    unsafe public class Player {
        private Sender mSender;
        private Event[] mEventList;
        private Task mTask;

        private int mTicks;
        private double mBPM;
        private double mCurrentTime;

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
                int ib = (int)(mCurrentTime / 960);
                int measure = ib / 4;
                int beat = ib % 4 + 1;
                int tick = (int)(mCurrentTime - 960 * ib);
                return string.Format(
                    "{0}:{1}:{2}",
                    measure.ToString("0000"),
                    beat.ToString("00"),
                    tick.ToString("000")
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
            mTicks = 960;
            IsPlay = false;
            SoloChannel = -1;
            Speed = 1.0;
        }

        public void SetEventList(Event[] eventList, int ticks) {
            mEventList = eventList;
            mTicks = ticks;
            MaxTime = 0;

            foreach (var ev in eventList) {
                if (EVENT_TYPE.NOTE_OFF == ev.Message.Type || EVENT_TYPE.NOTE_ON == ev.Message.Type) {
                    var time = 1000 * ev.Time / ticks;
                    if (MaxTime < time) {
                        MaxTime = (int)time;
                    }
                }
            }

            mBPM = 120.0;
            mCurrentTime = 0.0;
        }

        public void Play() {
            if (null == mEventList) {
                return;
            }

            IsPlay = true;
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

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            foreach (var ev in mEventList) {
                if (!IsPlay) {
                    break;
                }

                long eventTime = 1000 * ev.Time / mTicks;
                while (mCurrentTime < eventTime) {
                    if (!IsPlay) {
                        break;
                    }

                    current_mSec = sw.ElapsedMilliseconds;
                    mCurrentTime += mBPM * Speed * (current_mSec - previous_mSec) / 60.0;
                    previous_mSec = current_mSec;
                    Thread.Sleep(10);
                }

                var msg = ev.Message;
                var type = msg.Type;

                if (EVENT_TYPE.META == type) {
                    if (META_TYPE.TEMPO == msg.Meta.Type) {
                        mBPM = msg.Meta.BPM;
                    }
                }

                if (EVENT_TYPE.NOTE_ON == type && msg.V2 != 0) {
                    if (0.25 * mTicks < (mCurrentTime - eventTime)) {
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
