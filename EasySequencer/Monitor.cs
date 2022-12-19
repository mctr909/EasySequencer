using System;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using EasySequencer.Properties;
using SMF;
using WaveoutDll;

namespace EasySequencer {
    public partial class Monitor : Form {
        DoubleBuffer mBuffer;
        Sender mSender;

        Point mMouseDownPos;
        bool mIsDrag;
        bool mIsMove;
        bool mIsParamChg;
        int mChannelNo;
        int mKnobNo;
        int mChangeValue;

        static readonly Font FONT_KNOB = new Font("ＭＳ ゴシック", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
        static readonly Pen COLOR_KNOB_BLACK = new Pen(Brushes.Black, 3) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        static readonly Pen COLOR_KNOB_GREEN = new Pen(Brushes.DarkOliveGreen, 3) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        static readonly Pen COLOR_KNOB_BLUE = new Pen(Brushes.Navy, 3) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        static readonly Brush COLOR_KNOB_TEXT = (new Pen(Color.FromArgb(255, 255, 255, 255), 1.0f)).Brush;

        const int FONT_WIDTH = 11;
        const int FONT_HEIGHT = 15;
        const int TAB_HEIGHT = 26;
        const int CHANNEL_HEIGHT = 32;
        const float KNOB_RADIUS = 11.0f;
        const double RMS_MIN = -36.0;
        const double RMS_MAX = 0.0;

        static readonly Bitmap[,] BMP_FONT = new Bitmap[16, 6];
        static readonly Rectangle RECT_ON_OFF = new Rectangle(527, 33, 28, 30);
        static readonly Rectangle RECT_METER_L = new Rectangle(563, 31, 144, 6);
        static readonly Rectangle RECT_METER_R = new Rectangle(563, 40, 144, 6);
        static readonly Size SIZE_METER_CELL = new Size(4, 6);
        static readonly Rectangle RECT_PRESET_NAME = new Rectangle(1025, 36, 219, 13);

        static readonly Rectangle[] RECT_KEYS = {
            new Rectangle(40, 44, 6, 10), // C
            new Rectangle(44, 29, 5, 16), // Db
            new Rectangle(47, 44, 6, 10), // D
            new Rectangle(51, 29, 5, 16), // Eb
            new Rectangle(54, 44, 6, 10), // E
            new Rectangle(61, 44, 6, 10), // F
            new Rectangle(65, 29, 5, 16), // Gb
            new Rectangle(68, 44, 6, 10), // G
            new Rectangle(72, 29, 5, 16), // Ab
            new Rectangle(75, 44, 6, 10), // A
            new Rectangle(79, 29, 5, 16), // Bb
            new Rectangle(82, 44, 6, 10)  // B
        };

        static readonly Point[] POS_KNOBS = {
            new Point(729, 43), // Vol.
            new Point(762, 43), // Exp.
            new Point(795, 43), // Pan
            new Point(831, 43), // Rev.
            new Point(864, 43), // Cho.
            new Point(897, 43), // Del.
            new Point(933, 43), // Fc
            new Point(966, 43), // Res.
            new Point(999, 43)  // Mod.
        };

        static readonly Point[] POS_KNOB_VALS = {
            new Point(720, 38), // Vol.
            new Point(753, 38), // Exp.
            new Point(786, 38), // Pan
            new Point(822, 38), // Rev.
            new Point(855, 38), // Cho.
            new Point(888, 38), // Del.
            new Point(924, 38), // Fc
            new Point(957, 38), // Res.
            new Point(990, 38)  // Mod.
        };

        static readonly PointF[] POS_KNOB_ROT = {
            new PointF(-0.604f,  0.797f), new PointF(-0.635f,  0.773f), new PointF(-0.665f,  0.747f), new PointF(-0.694f,  0.720f),
            new PointF(-0.721f,  0.693f), new PointF(-0.748f,  0.664f), new PointF(-0.774f,  0.634f), new PointF(-0.798f,  0.603f),
            new PointF(-0.821f,  0.571f), new PointF(-0.843f,  0.539f), new PointF(-0.863f,  0.505f), new PointF(-0.882f,  0.471f),
            new PointF(-0.900f,  0.436f), new PointF(-0.917f,  0.400f), new PointF(-0.932f,  0.364f), new PointF(-0.945f,  0.327f),
            new PointF(-0.957f,  0.290f), new PointF(-0.968f,  0.252f), new PointF(-0.977f,  0.214f), new PointF(-0.985f,  0.175f),
            new PointF(-0.991f,  0.137f), new PointF(-0.996f,  0.098f), new PointF(-0.999f,  0.058f), new PointF(-1.000f,  0.019f),
            new PointF(-1.000f, -0.020f), new PointF(-0.999f, -0.059f), new PointF(-0.996f, -0.099f), new PointF(-0.991f, -0.138f),
            new PointF(-0.985f, -0.176f), new PointF(-0.977f, -0.215f), new PointF(-0.968f, -0.253f), new PointF(-0.957f, -0.291f),
            new PointF(-0.945f, -0.328f), new PointF(-0.932f, -0.365f), new PointF(-0.917f, -0.401f), new PointF(-0.900f, -0.437f),
            new PointF(-0.882f, -0.472f), new PointF(-0.863f, -0.506f), new PointF(-0.843f, -0.540f), new PointF(-0.821f, -0.572f),
            new PointF(-0.798f, -0.604f), new PointF(-0.774f, -0.635f), new PointF(-0.748f, -0.665f), new PointF(-0.721f, -0.694f),
            new PointF(-0.694f, -0.721f), new PointF(-0.665f, -0.748f), new PointF(-0.635f, -0.774f), new PointF(-0.604f, -0.798f),
            new PointF(-0.572f, -0.821f), new PointF(-0.540f, -0.843f), new PointF(-0.506f, -0.863f), new PointF(-0.472f, -0.882f),
            new PointF(-0.437f, -0.900f), new PointF(-0.401f, -0.917f), new PointF(-0.365f, -0.932f), new PointF(-0.328f, -0.945f),
            new PointF(-0.291f, -0.957f), new PointF(-0.253f, -0.968f), new PointF(-0.215f, -0.977f), new PointF(-0.176f, -0.985f),
            new PointF(-0.138f, -0.991f), new PointF(-0.099f, -0.996f), new PointF(-0.059f, -0.999f), new PointF(-0.020f, -1.000f),
            new PointF( 0.019f, -1.000f), new PointF( 0.058f, -0.999f), new PointF( 0.098f, -0.996f), new PointF( 0.137f, -0.991f),
            new PointF( 0.175f, -0.985f), new PointF( 0.214f, -0.977f), new PointF( 0.252f, -0.968f), new PointF( 0.290f, -0.957f),
            new PointF( 0.327f, -0.945f), new PointF( 0.364f, -0.932f), new PointF( 0.400f, -0.917f), new PointF( 0.436f, -0.900f),
            new PointF( 0.471f, -0.882f), new PointF( 0.505f, -0.863f), new PointF( 0.539f, -0.843f), new PointF( 0.571f, -0.821f),
            new PointF( 0.603f, -0.798f), new PointF( 0.634f, -0.774f), new PointF( 0.664f, -0.748f), new PointF( 0.693f, -0.721f),
            new PointF( 0.720f, -0.694f), new PointF( 0.747f, -0.665f), new PointF( 0.773f, -0.635f), new PointF( 0.797f, -0.604f),
            new PointF( 0.820f, -0.572f), new PointF( 0.842f, -0.540f), new PointF( 0.862f, -0.506f), new PointF( 0.881f, -0.472f),
            new PointF( 0.899f, -0.437f), new PointF( 0.916f, -0.401f), new PointF( 0.931f, -0.365f), new PointF( 0.944f, -0.328f),
            new PointF( 0.956f, -0.291f), new PointF( 0.967f, -0.253f), new PointF( 0.976f, -0.215f), new PointF( 0.984f, -0.176f),
            new PointF( 0.990f, -0.138f), new PointF( 0.995f, -0.099f), new PointF( 0.998f, -0.059f), new PointF( 0.999f, -0.020f),
            new PointF( 0.999f,  0.019f), new PointF( 0.998f,  0.058f), new PointF( 0.995f,  0.098f), new PointF( 0.990f,  0.137f),
            new PointF( 0.984f,  0.175f), new PointF( 0.976f,  0.214f), new PointF( 0.967f,  0.252f), new PointF( 0.956f,  0.290f),
            new PointF( 0.944f,  0.327f), new PointF( 0.931f,  0.364f), new PointF( 0.916f,  0.400f), new PointF( 0.899f,  0.436f),
            new PointF( 0.881f,  0.471f), new PointF( 0.862f,  0.505f), new PointF( 0.842f,  0.539f), new PointF( 0.820f,  0.571f),
            new PointF( 0.797f,  0.603f), new PointF( 0.773f,  0.634f), new PointF( 0.747f,  0.664f), new PointF( 0.720f,  0.693f),
            new PointF( 0.693f,  0.720f), new PointF( 0.664f,  0.747f), new PointF( 0.634f,  0.773f), new PointF( 0.603f,  0.797f)
        };

        static readonly int OCT_WIDTH = (RECT_KEYS[0].Width + 1) * 7;

        public Monitor(Sender sender) {
            InitializeComponent();

            Width = Resources.Monitor.Width;
            Height = Resources.Monitor.Height;
            picMonitor.Width = Width;
            picMonitor.Height = Height;
            picMonitor.Image = new Bitmap(picMonitor.Width, picMonitor.Height);
            mBuffer = new DoubleBuffer(picMonitor, Resources.Monitor);
            mSender = sender;

            for (int by = 0; by < 6; by++) {
                for (int bx = 0; bx < 16; bx++) {
                    BMP_FONT[bx, by] = new Bitmap(FONT_WIDTH, FONT_HEIGHT);
                    var g = Graphics.FromImage(BMP_FONT[bx, by]);
                    g.DrawImage(Resources.font_5x7, new Rectangle(0, 0, FONT_WIDTH, FONT_HEIGHT),
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

        void Monitor_Load(object sender, EventArgs e) {
            Task.Run(() => {
                while (true) {
                    draw();
                    sendValue();
                    Thread.Sleep(20);
                }
            });
        }

        void picMonitor_DoubleClick(object sender, EventArgs e) {
            mMouseDownPos = picMonitor.PointToClient(Cursor.Position);
            var channel = (mMouseDownPos.Y - TAB_HEIGHT) / CHANNEL_HEIGHT;
            if (RECT_PRESET_NAME.X <= mMouseDownPos.X && channel < 16) {
                var fm = new InstList(mSender, channel);
                fm.ShowDialog();
            }
        }

        void picMonitor_MouseDown(object sender, MouseEventArgs e) {
            mMouseDownPos = picMonitor.PointToClient(Cursor.Position);
            var knobY = (mMouseDownPos.Y - TAB_HEIGHT) / CHANNEL_HEIGHT;
            var knobX = -1;
            for (int i = 0; i < POS_KNOBS.Length; i++) {
                var x = mMouseDownPos.X - POS_KNOBS[i].X - 1.5;
                var y = mMouseDownPos.Y - knobY * CHANNEL_HEIGHT - POS_KNOBS[i].Y;
                var r = Math.Sqrt(x * x + y * y);
                if (r <= KNOB_RADIUS + 5) {
                    knobX = i;
                }
            }

            mIsMove = mMouseDownPos.Y < TAB_HEIGHT;
            mIsDrag = 0 <= knobX && TAB_HEIGHT <= mMouseDownPos.Y;

            mChannelNo = knobY;
            if (TAB_HEIGHT <= mMouseDownPos.Y && RECT_ON_OFF.X <= mMouseDownPos.X && mMouseDownPos.X < RECT_ON_OFF.X + RECT_ON_OFF.Width) {
                if (e.Button == MouseButtons.Right) {
                    if (1 == mSender.Channel(knobY).enable) {
                        for (int i = 0; i < 16; ++i) {
                            if (knobY == i) {
                                mSender.MuteChannel(i, true);
                            } else {
                                mSender.MuteChannel(i, false);
                            }
                        }
                    } else {
                        for (int i = 0; i < 16; ++i) {
                            if (knobY == i) {
                                mSender.MuteChannel(i, false);
                            } else {
                                mSender.MuteChannel(i, true);
                            }
                        }
                    }
                } else {
                    mSender.MuteChannel(knobY, 1 == mSender.Channel(knobY).enable);
                }
            }
            mKnobNo = knobX;
        }

        void picMonitor_MouseUp(object sender, MouseEventArgs e) {
            mIsMove = false;
            mIsDrag = false;
        }

        void picMonitor_MouseMove(object sender, MouseEventArgs e) {
            if (mIsDrag) {
                var pos = picMonitor.PointToClient(Cursor.Position);
                var knobCenter = POS_KNOBS[mKnobNo];
                knobCenter.Y += mChannelNo * CHANNEL_HEIGHT;

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

                mIsParamChg = true;
            }
            if (mIsMove) {
                var pos = Cursor.Position;
                pos.X -= mMouseDownPos.X;
                pos.Y -= mMouseDownPos.Y;
                Location = pos;
            }
        }

        void draw() {
            var g = mBuffer.Graphics;

            for (var ch = 0; ch < Sender.CHANNEL_COUNT; ++ch) {
                var channel = mSender.Channel(ch);
                var y_ch = CHANNEL_HEIGHT * ch;

                /*** Keyboard ***/
                var transpose = (int)(channel.pitch * channel.bend_range / 8192.0 - 0.5);
                for (var n = 0; n < 128; ++n) {
                    var k = n + transpose;
                    if (k < 12 || 127 < k) {
                        continue;
                    }
                    var key = RECT_KEYS[k % 12];
                    var kx = key.X + OCT_WIDTH * (k / 12 - 1);
                    var ky = key.Y + y_ch;
                    var keyState = (E_KEY_STATE)Marshal.PtrToStructure<byte>(channel.p_keyboard + n);
                    switch (keyState) {
                        case E_KEY_STATE.PRESS:
                            g.FillRectangle(Brushes.Red, kx, ky, key.Width, key.Height);
                            break;
                        case E_KEY_STATE.HOLD:
                            g.FillRectangle(Brushes.Blue, kx, ky, key.Width, key.Height);
                            break;
                    }
                }

                /*** On/Off Button ***/
                if (1 == channel.enable) {
                    g.DrawImageUnscaled(Resources.track_on, RECT_ON_OFF.X, RECT_ON_OFF.Y + y_ch);
                }

                /*** RMS meter ***/
                var rmsL = Math.Sqrt(channel.rms_l) * 2;
                var rmsR = Math.Sqrt(channel.rms_r) * 2;
                if (rmsL < 0.0000001) {
                    rmsL = 0.0000001;
                }
                if (rmsR < 0.0000001) {
                    rmsR = 0.0000001;
                }
                rmsL = 20 * Math.Log10(rmsL);
                rmsR = 20 * Math.Log10(rmsR);
                rmsL = Math.Max(RMS_MIN, rmsL);
                rmsR = Math.Max(RMS_MIN, rmsR);
                rmsL = Math.Min(RMS_MAX, rmsL);
                rmsR = Math.Min(RMS_MAX, rmsR);
                var normL = 1.0 - (rmsL - RMS_MAX) / RMS_MIN;
                var normR = 1.0 - (rmsR - RMS_MAX) / RMS_MIN;
                var rmsLpx = (int)(normL * RECT_METER_L.Width + 1) / SIZE_METER_CELL.Width * SIZE_METER_CELL.Width;
                var rmsRpx = (int)(normR * RECT_METER_R.Width + 1) / SIZE_METER_CELL.Width * SIZE_METER_CELL.Width;
                g.DrawImageUnscaledAndClipped(Resources.Meter, new Rectangle(
                    RECT_METER_L.X, RECT_METER_L.Y + y_ch,
                    rmsLpx, SIZE_METER_CELL.Height
                ));
                g.DrawImageUnscaledAndClipped(Resources.Meter, new Rectangle(
                    RECT_METER_R.X, RECT_METER_R.Y + y_ch,
                    rmsRpx, SIZE_METER_CELL.Height
                ));

                /*** Vol. ***/
                drawKnob(g, COLOR_KNOB_BLACK, ch, 0, channel.vol);
                /*** Exp. ***/
                drawKnob(g, COLOR_KNOB_BLACK, ch, 1, channel.exp);
                /*** Pan  ***/
                drawKnob(g, COLOR_KNOB_BLACK, ch, 2, channel.pan, "R00;L00; C ");

                /*** Rev. ***/
                drawKnob(g, COLOR_KNOB_BLUE, ch, 3, channel.rev_send);
                /*** Cho. ***/
                drawKnob(g, COLOR_KNOB_BLUE, ch, 4, channel.cho_send);
                /*** Del. ***/
                drawKnob(g, COLOR_KNOB_BLUE, ch, 5, channel.del_send);

                /*** Fc ***/
                drawKnob(g, COLOR_KNOB_GREEN, ch, 6, channel.cutoff);
                /*** Res. ***/
                drawKnob(g, COLOR_KNOB_GREEN, ch, 7, channel.resonance);
                /*** Mod. ***/
                drawKnob(g, COLOR_KNOB_GREEN, ch, 8, channel.mod);

                /*** Preset name ***/
                var bName = Encoding.ASCII.GetBytes(channel.Name);
                for (int i = 0; i < bName.Length && i < 20; i++) {
                    var cx = bName[i] % 16;
                    var cy = (bName[i] - 0x20) / 16;
                    g.DrawImageUnscaled(BMP_FONT[cx, cy], new Rectangle(
                        RECT_PRESET_NAME.X + FONT_WIDTH * i,
                        RECT_PRESET_NAME.Y + y_ch,
                        FONT_WIDTH,
                        FONT_HEIGHT
                    ));
                }
            }

            mBuffer.Render();
        }

        void drawKnob(Graphics g, Pen color, int ch, int index, int value, string format = "000") {
            var y_ch = ch * CHANNEL_HEIGHT;
            var knobX = POS_KNOB_ROT[value].X * KNOB_RADIUS;
            var knobY = POS_KNOB_ROT[value].Y * KNOB_RADIUS;
            g.DrawLine(
                color,
                knobX * 0.25f + POS_KNOBS[index].X,
                knobY * 0.25f + POS_KNOBS[index].Y + y_ch,
                knobX + POS_KNOBS[index].X,
                knobY + POS_KNOBS[index].Y + y_ch
            );
            if ("000" != format) {
                value -= 64;
            }
            g.DrawString(
                value.ToString(format),
                FONT_KNOB, COLOR_KNOB_TEXT,
                POS_KNOB_VALS[index].X, POS_KNOB_VALS[index].Y + y_ch
            );
        }

        void sendValue() {
            if (!mIsParamChg) {
                return;
            }

            mIsParamChg = false;

            switch (mKnobNo) {
                case 0:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.VOLUME, mChangeValue));
                    break;
                case 1:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.EXPRESSION, mChangeValue));
                    break;
                case 2:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.PAN, mChangeValue));
                    break;
                case 3:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.REVERB, mChangeValue));
                    break;
                case 4:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.CHORUS, mChangeValue));
                    break;
                case 5:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.DELAY, mChangeValue));
                    break;
                case 6:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.CUTOFF, mChangeValue));
                    break;
                case 7:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.RESONANCE, mChangeValue));
                    break;
                case 8:
                    mSender.Send(new Event(mChannelNo, E_CONTROL.MODULATION, mChangeValue));
                    break;
            }
        }
    }
}
