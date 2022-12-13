﻿using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using EasySequencer.Properties;
using Player;

namespace EasySequencer {
    public partial class Monitor : Form {
        DoubleBuffer mBuffer;
        Graphics mG;
        Sender mSender;

        Point mMouseDownPos;
        bool mIsDrag;
        bool mIsMove;
        bool mIsParamChg;
        int mChannelNo;
        int mKnobNo;
        int mChangeValue;

        static readonly Pen mKnobMark = new Pen(Brushes.LawnGreen, 3.5f) {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        static readonly Font mKnobFont = new Font("ＭＳ ゴシック", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
        static readonly Brush mKnobFontColor = (new Pen(Color.FromArgb(255, 255, 255, 255), 1.0f)).Brush;
        static readonly Font mInstFont = new Font("Meiryo UI", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
        static readonly StringFormat mInstFormat = new StringFormat() {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisCharacter,
        };

        static readonly int TabHeight = 26;
        static readonly int ChannelHeight = 32;
        static readonly float KnobRadius = 11.0f;

        static readonly Rectangle MuteButton = new Rectangle(496, 27, 28, 30);
        static readonly Rectangle InstName = new Rectangle(999, 36, 219, 13);

        static readonly Rectangle MeterPosL = new Rectangle(537, 31, 144, 6);
        static readonly Rectangle MeterPosR = new Rectangle(537, 40, 144, 6);
        static readonly Size MeterCell = new Size(4, 6);
        static readonly double MeterMin = -30.0;
        static readonly double MeterMax = 6.0;

        static readonly Rectangle[] KeyboardPos = {
            new Rectangle(40, 44, 5, 9),  // C
            new Rectangle(43, 31, 5, 14), // Db
            new Rectangle(46, 44, 5, 9),  // D
            new Rectangle(49, 31, 5, 14), // Eb
            new Rectangle(52, 44, 5, 9),  // E
            new Rectangle(58, 44, 5, 9),  // F
            new Rectangle(61, 31, 5, 14), // Gb
            new Rectangle(64, 44, 5, 9),  // G
            new Rectangle(67, 31, 5, 14), // Ab
            new Rectangle(70, 44, 5, 9),  // A
            new Rectangle(73, 31, 5, 14), // Bb
            new Rectangle(76, 44, 5, 9)   // B
        };

        static readonly int OctWidth = (KeyboardPos[0].Width + 1) * 7;

        static readonly Point[] KnobPos = {
            new Point(703, 43), // Vol.
            new Point(736, 43), // Exp.
            new Point(769, 43), // Pan.
            new Point(805, 43), // Rev.
            new Point(838, 43), // Cho.
            new Point(871, 43), // Del.
            new Point(907, 43), // Fc
            new Point(940, 43), // Q
            new Point(973, 43)  // Mod.
        };

        static readonly Point[] KnobValPos = {
            new Point(694, 38), // Vol.
            new Point(727, 38), // Exp.
            new Point(760, 38), // Pan.
            new Point(796, 38), // Rev.
            new Point(829, 38), // Cho.
            new Point(862, 38), // Del.
            new Point(898, 38), // Fc
            new Point(931, 38), // Q
            new Point(964, 38)  // Mod.
        };

        static readonly PointF[] Knob = {
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

        public Monitor(Sender sender) {
            InitializeComponent();

            Width = Resources.Monitor.Width;
            Height = Resources.Monitor.Height;
            picMonitor.Width = Width;
            picMonitor.Height = Height;

            picMonitor.DoubleClick += new EventHandler(picMonitor_DoubleClick);
            picMonitor.MouseDown += new MouseEventHandler(picMonitor_MouseDown);
            picMonitor.MouseMove += new MouseEventHandler(picMonitor_MouseMove);
            picMonitor.MouseUp += new MouseEventHandler(picMonitor_MouseUp);

            picMonitor.Image = new Bitmap(picMonitor.Width, picMonitor.Height);
            mBuffer = new DoubleBuffer(picMonitor, Resources.Monitor);
            mSender = sender;
        }

        void Monitor_Load(object sender, EventArgs e) {
            Task.Run(() => {
                while (true) {
                    draw();
                    sendValue();
                    Thread.Sleep(25);
                }
            });
        }

        void picMonitor_DoubleClick(object sender, EventArgs e) {
            mMouseDownPos = picMonitor.PointToClient(Cursor.Position);
            var channel = (mMouseDownPos.Y - TabHeight) / ChannelHeight;
            if (InstName.X <= mMouseDownPos.X && channel < 16) {
                var fm = new InstList(mSender, channel);
                fm.ShowDialog();
            }
        }

        void picMonitor_MouseDown(object sender, MouseEventArgs e) {
            mMouseDownPos = picMonitor.PointToClient(Cursor.Position);
            var knobY = (mMouseDownPos.Y - TabHeight) / ChannelHeight;
            var knobX = -1;
            for (int i = 0; i < KnobPos.Length; i++) {
                var x = mMouseDownPos.X - KnobPos[i].X - 1.5;
                var y = mMouseDownPos.Y - knobY * ChannelHeight - KnobPos[i].Y;
                var r = Math.Sqrt(x * x + y * y);
                if (r <= KnobRadius + 5) {
                    knobX = i;
                }
            }

            mIsMove = mMouseDownPos.Y < TabHeight;
            mIsDrag = 0 <= knobX && TabHeight <= mMouseDownPos.Y;

            mChannelNo = knobY;
            if (TabHeight <= mMouseDownPos.Y && MuteButton.X <= mMouseDownPos.X && mMouseDownPos.X < MuteButton.X + MuteButton.Width) {
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
                var knobCenter = KnobPos[mKnobNo];
                knobCenter.Y += mChannelNo * ChannelHeight;

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
            mG = mBuffer.Graphics;

            drawKeyboard();

            /** Meter & Knob **/
            for (var ch = 0; ch < Sender.CHANNEL_COUNT; ++ch) {
                var channel = mSender.Channel(ch);
                var y_ch = ChannelHeight * ch;

                // Mute Button
                if (0 == channel.enable) {
                    mG.FillRectangle(Brushes.Red, MuteButton.X, MuteButton.Y + y_ch, MuteButton.Width, MuteButton.Height);
                }

                // Peak meter
                var peakL = channel.peak_l;
                var peakR = channel.peak_r;
                if (peakL < 0.000001) {
                    peakL = 0.000001;
                }
                if (peakR < 0.000001) {
                    peakR = 0.000001;
                }
                peakL = 20 * Math.Log10(Math.Sqrt(peakL));
                peakR = 20 * Math.Log10(Math.Sqrt(peakR));
                peakL = Math.Max(MeterMin, peakL);
                peakR = Math.Max(MeterMin, peakR);
                peakL = Math.Min(MeterMax, peakL);
                peakR = Math.Min(MeterMax, peakR);
                var peakLnorm = 1.0 - (peakL - MeterMax) / MeterMin;
                var peakRnorm = 1.0 - (peakR - MeterMax) / MeterMin;
                var peakLpx = (int)(peakLnorm * MeterPosL.Width + 1) / MeterCell.Width * MeterCell.Width;
                var peakRpx = (int)(peakRnorm * MeterPosR.Width + 1) / MeterCell.Width * MeterCell.Width;
                mG.DrawImageUnscaledAndClipped(Resources.Meter, new Rectangle(
                    MeterPosL.X, MeterPosL.Y + y_ch,
                    peakLpx, MeterCell.Height
                ));
                mG.DrawImageUnscaledAndClipped(Resources.Meter, new Rectangle(
                    MeterPosR.X, MeterPosR.Y + y_ch,
                    peakRpx, MeterCell.Height
                ));

                // Vol
                drawKnob127(ch, 0, channel.vol);
                // Exp
                drawKnob127(ch, 1, channel.exp);
                // Pan
                var knobX = Knob[channel.pan].X * KnobRadius;
                var knobY = Knob[channel.pan].Y * KnobRadius;
                mG.DrawLine(
                    mKnobMark,
                    knobX * 0.5f + KnobPos[2].X,
                    knobY * 0.5f + KnobPos[2].Y + y_ch,
                    knobX + KnobPos[2].X,
                    knobY + KnobPos[2].Y + y_ch
                );
                var pan = channel.pan - 64;
                if (0 == pan) {
                    mG.DrawString(
                        " C ",
                        mKnobFont, mKnobFontColor,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                } else if (pan < 0) {
                    mG.DrawString(
                        "L" + (-pan).ToString("00"),
                        mKnobFont, mKnobFontColor,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                } else {
                    mG.DrawString(
                        "R" + pan.ToString("00"),
                        mKnobFont, mKnobFontColor,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                }

                // Rev
                drawKnob127(ch, 3, channel.rev_send);
                // Cho
                drawKnob127(ch, 4, channel.cho_send);
                // Del
                drawKnob127(ch, 5, channel.del_send);

                // Fc
                drawKnob127(ch, 6, channel.cutoff);
                // Fq
                drawKnob127(ch, 7, channel.resonance);
                // Mod.
                drawKnob127(ch, 8, channel.mod);

                mG.DrawString(channel.Name, mInstFont, Brushes.Black, InstName.X, InstName.Y + y_ch, mInstFormat);
            }

            mBuffer.Render();
        }

        void drawKeyboard() {
            for (var c = 0; c < Sender.CHANNEL_COUNT; c++) {
                var channel = mSender.Channel(c);
                var transpose = (int)(channel.pitch * channel.bend_range / 8192.0 - 0.5);
                var y_ch = ChannelHeight * c;
                for (var n = 0; n < 128; ++n) {
                    var k = n + transpose;
                    if (k < 0 || 127 < k) {
                        continue;
                    }
                    var key = KeyboardPos[k % 12];
                    var px = key.X + OctWidth * (k / 12 - 1);
                    var py = key.Y + y_ch;
                    var keyState = (E_KEY_STATE)Marshal.PtrToStructure<byte>(channel.p_keyboard + n);
                    switch (keyState) {
                        case E_KEY_STATE.PRESS:
                            mG.FillRectangle(Brushes.Red, px, py, key.Width, key.Height);
                            break;
                        case E_KEY_STATE.HOLD:
                            mG.FillRectangle(Brushes.Blue, px, py, key.Width, key.Height);
                            break;
                    }
                }
            }
        }

        void drawKnob127(int ch, int index, int value) {
            var y_ch = ch * ChannelHeight;
            var knobX = Knob[value].X * KnobRadius;
            var knobY = Knob[value].Y * KnobRadius;
            mG.DrawLine(
                mKnobMark,
                knobX * 0.5f + KnobPos[index].X,
                knobY * 0.5f + KnobPos[index].Y + y_ch,
                knobX + KnobPos[index].X,
                knobY + KnobPos[index].Y + y_ch
            );
            mG.DrawString(
                value.ToString("000"),
                mKnobFont, mKnobFontColor,
                KnobValPos[index].X, KnobValPos[index].Y + y_ch
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