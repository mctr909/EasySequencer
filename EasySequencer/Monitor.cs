using System;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using EasySequencer.Properties;
using SMF;
using SynthDll;

namespace EasySequencer {
    public partial class Monitor : Form {
        Graphics mG;

        Point mMouseDownPos;
        bool mIsDrag;
        bool mIsParamChg;
        int mTrackNum;
        int mKnobNum;
        int mChangeValue;
        string[] mPresetNames = new string[DISP_TRACKS];
        Bitmap[] mBmpPresetNames = new Bitmap[DISP_TRACKS];

        static readonly Pen COLOR_KNOB_BLACK = new Pen(Brushes.Black, 3) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        static readonly Pen COLOR_KNOB_BLUE = new Pen(Color.FromArgb(0, 0, 191), 3) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        static readonly Pen COLOR_KNOB_GREEN = new Pen(Color.FromArgb(0, 127, 0), 3) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };

        const int FONT_WIDTH = 11;
        const int FONT_HEIGHT = 15;
        const int DISP_TRACKS = 16;
        const int TAB_HEIGHT = 26;
        const int TRACK_HEIGHT = 32;
        const float KNOB_RADIUS = 11.0f;
        const double METER_MIN = -36.0;
        const double METER_MAX = 0.0;

        static readonly Bitmap[,] BMP_FONT = new Bitmap[16, 6];
        static readonly Rectangle RECT_ON_OFF = new Rectangle(527, 33, 28, 30);
        static readonly Rectangle RECT_RMS_L = new Rectangle(563, 32, 144, 6);
        static readonly Rectangle RECT_RMS_R = new Rectangle(563, 46, 144, 6);
        static readonly Rectangle RECT_PEAK_L = new Rectangle(563, 39, 144, 2);
        static readonly Rectangle RECT_PEAK_R = new Rectangle(563, 43, 144, 2);
        static readonly Size SIZE_RMS_CELL = new Size(4, 6);
        static readonly Size SIZE_PEAK_CELL = new Size(4, 2);
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

        public Monitor() {
            InitializeComponent();

            Width = Resources.Monitor.Width + 16;
            Height = Resources.Monitor.Height + 39;
            picMonitor.Width = Width;
            picMonitor.Height = Height;
            picMonitor.Image = new Bitmap(picMonitor.Width, picMonitor.Height);
            mG = Graphics.FromImage(picMonitor.Image);
            mG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

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

        private void Monitor_Shown(object sender, EventArgs e) {
            timer1.Enabled = true;
            timer1.Interval = 33;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e) {
            draw();
            sendValue();
        }

        void picMonitor_DoubleClick(object sender, EventArgs e) {
            mMouseDownPos = picMonitor.PointToClient(Cursor.Position);
            var channel = (mMouseDownPos.Y - TAB_HEIGHT) / TRACK_HEIGHT;
            if (RECT_PRESET_NAME.X <= mMouseDownPos.X && channel < 16) {
                var fm = new InstList(channel);
                fm.ShowDialog();
            }
        }

        void picMonitor_MouseDown(object sender, MouseEventArgs e) {
            mMouseDownPos = picMonitor.PointToClient(Cursor.Position);
            var knobY = (mMouseDownPos.Y - TAB_HEIGHT) / TRACK_HEIGHT;
            var knobX = -1;
            for (int i = 0; i < POS_KNOBS.Length; i++) {
                var x = mMouseDownPos.X - POS_KNOBS[i].X - 1.5;
                var y = mMouseDownPos.Y - knobY * TRACK_HEIGHT - POS_KNOBS[i].Y;
                var r = Math.Sqrt(x * x + y * y);
                if (r <= KNOB_RADIUS + 5) {
                    knobX = i;
                }
            }

            mIsDrag = 0 <= knobX && TAB_HEIGHT <= mMouseDownPos.Y;

            if (TAB_HEIGHT <= mMouseDownPos.Y && RECT_ON_OFF.X <= mMouseDownPos.X && mMouseDownPos.X < RECT_ON_OFF.X + RECT_ON_OFF.Width) {
                if (e.Button == MouseButtons.Right) {
                    if (1 == Synth.GetChannel(knobY).enable) {
                        for (int i = 0, chNum = 0; i < DISP_TRACKS; ++i, ++chNum) {
                            if (knobY == i) {
                                Synth.MuteChannel(chNum, true);
                            } else {
                                Synth.MuteChannel(chNum, false);
                            }
                        }
                    } else {
                        for (int i = 0, chNum = 0; i < DISP_TRACKS; ++i, ++chNum) {
                            if (knobY == i) {
                                Synth.MuteChannel(chNum, false);
                            } else {
                                Synth.MuteChannel(chNum, true);
                            }
                        }
                    }
                } else {
                    Synth.MuteChannel(knobY, 1 == Synth.GetChannel(knobY).enable);
                }
            }

            mKnobNum = knobX;
            mTrackNum = knobY;
        }

        void picMonitor_MouseUp(object sender, MouseEventArgs e) {
            mIsDrag = false;
        }

        void picMonitor_MouseMove(object sender, MouseEventArgs e) {
            if (mIsDrag) {
                var pos = picMonitor.PointToClient(Cursor.Position);
                var knobCenter = POS_KNOBS[mKnobNum];
                knobCenter.Y += mTrackNum * TRACK_HEIGHT;

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
        }

        void setPresetName(CHANNEL_PARAM param, int chNum) {
            var bmp = mBmpPresetNames[chNum];
            if (null != bmp) {
                bmp.Dispose();
            }
            bmp = new Bitmap(FONT_WIDTH * 20, FONT_HEIGHT);
            var g = Graphics.FromImage(bmp);
            var bName = Encoding.ASCII.GetBytes(param.Name);
            g.Clear(Color.Transparent);
            for (int i = 0; i < bName.Length && i < 20; i++) {
                var cx = bName[i] % 16;
                var cy = (bName[i] - 0x20) / 16;
                g.DrawImageUnscaled(BMP_FONT[cx, cy], new Rectangle(
                    FONT_WIDTH * i, 0,
                    FONT_WIDTH, FONT_HEIGHT
                ));
            }
            g.Dispose();
            mBmpPresetNames[chNum] = bmp;
            mPresetNames[chNum] = param.Name;
        }

        void draw() {
            mG.Clear(Color.Transparent);

            for (int track = 0, chNum = 0; track < DISP_TRACKS; ++track, ++chNum) {
                var param = Synth.GetChannel(chNum);
                var track_y = TRACK_HEIGHT * track;

                /*** Keyboard ***/
                var transpose = (int)(param.pitch * param.bend_range / 8192.0 - 0.5);
                for (var n = 0; n < 128; ++n) {
                    var k = n + transpose;
                    if (k < 12 || 127 < k) {
                        continue;
                    }
                    var key = RECT_KEYS[k % 12];
                    var kx = key.X + OCT_WIDTH * (k / 12 - 1);
                    var ky = key.Y + track_y;
                    var keyState = (E_KEY_STATE)Marshal.PtrToStructure<byte>(param.p_keyboard + n);
                    switch (keyState) {
                    case E_KEY_STATE.PRESS:
                        mG.FillRectangle(Brushes.Red, kx, ky, key.Width, key.Height);
                        break;
                    case E_KEY_STATE.HOLD:
                        mG.FillRectangle(Brushes.Blue, kx, ky, key.Width, key.Height);
                        break;
                    }
                }

                /*** On/Off Button ***/
                if (1 == param.enable) {
                    mG.DrawImageUnscaled(Resources.track_on, RECT_ON_OFF.X, RECT_ON_OFF.Y + track_y);
                }

                /*** Meter ***/
                var rmsL = Math.Sqrt(param.rms_l) * 2;
                var rmsR = Math.Sqrt(param.rms_r) * 2;
                var peakL = param.peak_l;
                var peakR = param.peak_r;
                rmsL = 20 * Math.Log10(Math.Max(rmsL, 0.0000001));
                rmsR = 20 * Math.Log10(Math.Max(rmsR, 0.0000001));
                peakL = 20 * Math.Log10(Math.Max(peakL, 0.0000001));
                peakR = 20 * Math.Log10(Math.Max(peakR, 0.0000001));
                var nrmsL = 1.0 - (rmsL - METER_MAX) / METER_MIN;
                var nrmsR = 1.0 - (rmsR - METER_MAX) / METER_MIN;
                var npeakL = 1.0 - (peakL - METER_MAX) / METER_MIN;
                var npeakR = 1.0 - (peakR - METER_MAX) / METER_MIN;
                var rmsLpx = (int)(nrmsL * RECT_RMS_L.Width + 1) / SIZE_RMS_CELL.Width * SIZE_RMS_CELL.Width;
                var rmsRpx = (int)(nrmsR * RECT_RMS_R.Width + 1) / SIZE_RMS_CELL.Width * SIZE_RMS_CELL.Width;
                var peakLpx = (int)(npeakL * RECT_PEAK_L.Width + 1) / SIZE_PEAK_CELL.Width * SIZE_PEAK_CELL.Width;
                var peakRpx = (int)(npeakR * RECT_PEAK_R.Width + 1) / SIZE_PEAK_CELL.Width * SIZE_PEAK_CELL.Width;
                rmsLpx = Math.Min(rmsLpx, RECT_RMS_L.Width - 1);
                rmsRpx = Math.Min(rmsRpx, RECT_RMS_R.Width - 1);
                peakLpx = Math.Min(peakLpx, RECT_PEAK_L.Width - 1);
                peakRpx = Math.Min(peakRpx, RECT_PEAK_R.Width - 1);
                mG.DrawImageUnscaledAndClipped(Resources.Meter, new Rectangle(
                    RECT_RMS_L.X, RECT_RMS_L.Y + track_y,
                    rmsLpx, SIZE_RMS_CELL.Height
                ));
                mG.DrawImageUnscaledAndClipped(Resources.Meter, new Rectangle(
                    RECT_RMS_R.X, RECT_RMS_R.Y + track_y,
                    rmsRpx, SIZE_RMS_CELL.Height
                ));
                mG.DrawImageUnscaledAndClipped(Resources.Meter_narrow, new Rectangle(
                    RECT_PEAK_L.X, RECT_PEAK_L.Y + track_y,
                    peakLpx, SIZE_PEAK_CELL.Height
                ));
                mG.DrawImageUnscaledAndClipped(Resources.Meter_narrow, new Rectangle(
                    RECT_PEAK_R.X, RECT_PEAK_R.Y + track_y,
                    peakRpx, SIZE_PEAK_CELL.Height
                ));

                /*** Vol. ***/
                drawKnob(COLOR_KNOB_BLACK, track, 0, param.vol);
                /*** Exp. ***/
                drawKnob(COLOR_KNOB_BLACK, track, 1, param.exp);
                /*** Pan  ***/
                drawKnob(COLOR_KNOB_BLACK, track, 2, param.pan);

                /*** Rev. ***/
                drawKnob(COLOR_KNOB_BLUE, track, 3, param.rev_send);
                /*** Cho. ***/
                drawKnob(COLOR_KNOB_BLUE, track, 4, param.cho_send);
                /*** Del. ***/
                drawKnob(COLOR_KNOB_BLUE, track, 5, param.del_send);

                /*** Fc ***/
                drawKnob(COLOR_KNOB_GREEN, track, 6, param.cutoff);
                /*** Res. ***/
                drawKnob(COLOR_KNOB_GREEN, track, 7, param.resonance);
                /*** Mod. ***/
                drawKnob(COLOR_KNOB_GREEN, track, 8, param.mod);

                /*** Preset name ***/
                if (mPresetNames[chNum] != param.Name) {
                    setPresetName(param, chNum);
                }
                if (null != mBmpPresetNames[chNum]) {
                    mG.DrawImageUnscaled(mBmpPresetNames[chNum],
                        RECT_PRESET_NAME.X,
                        RECT_PRESET_NAME.Y + track_y
                    );
                }
            }

            picMonitor.Image = picMonitor.Image;
        }

        void drawKnob(Pen color, int track, int index, int value) {
            var track_y = track * TRACK_HEIGHT;
            var knobX = POS_KNOB_ROT[value].X * KNOB_RADIUS;
            var knobY = POS_KNOB_ROT[value].Y * KNOB_RADIUS;
            mG.DrawLine(
                color,
                knobX * 0.1f + POS_KNOBS[index].X,
                knobY * 0.1f + POS_KNOBS[index].Y + track_y,
                knobX + POS_KNOBS[index].X,
                knobY + POS_KNOBS[index].Y + track_y
            );
        }

        void sendValue() {
            if (!mIsParamChg) {
                return;
            }

            mIsParamChg = false;

            var chNum = mTrackNum % 16;
            var port = (byte)(mTrackNum / 16);

            switch (mKnobNum) {
            case 0:
                Synth.Send(port, new Event(chNum, E_CONTROL.VOLUME, mChangeValue));
                break;
            case 1:
                Synth.Send(port, new Event(chNum, E_CONTROL.EXPRESSION, mChangeValue));
                break;
            case 2:
                Synth.Send(port, new Event(chNum, E_CONTROL.PAN, mChangeValue));
                break;
            case 3:
                Synth.Send(port, new Event(chNum, E_CONTROL.REVERB, mChangeValue));
                break;
            case 4:
                Synth.Send(port, new Event(chNum, E_CONTROL.CHORUS, mChangeValue));
                break;
            case 5:
                Synth.Send(port, new Event(chNum, E_CONTROL.DELAY, mChangeValue));
                break;
            case 6:
                Synth.Send(port, new Event(chNum, E_CONTROL.CUTOFF, mChangeValue));
                break;
            case 7:
                Synth.Send(port, new Event(chNum, E_CONTROL.RESONANCE, mChangeValue));
                break;
            case 8:
                Synth.Send(port, new Event(chNum, E_CONTROL.MODULATION, mChangeValue));
                break;
            }
        }
    }
}
