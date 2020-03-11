using System.Windows.Forms;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using MIDI;
using EasySequencer;

namespace Player {
    public class Keyboard {
        private PictureBox mCtrl;
        private DoubleBuffer mBuffer;
        private Sender mSender;
        private Player mPlayer;

        private Point mMouseDownPos;
        private bool mIsDrag;
        private bool mIsParamChg;
        private int mChannelNo;
        private int mKnobNo;
        private int mChangeValue;

        private static readonly Font mKnobFont = new Font("ＭＳ ゴシック", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font mInstFont = new Font("Meiryo UI", 16.0f, FontStyle.Regular, GraphicsUnit.Point);

        private static readonly int ChannelHeight = 40;
        private static readonly int KnobRadius = 14;
        private static readonly int KnobWidth = 40;

        private static readonly Rectangle MuteButton = new Rectangle(925, 4, 13, 18);
        private static readonly Rectangle InstName = new Rectangle(944, 4, 238, 38);

        private static readonly Rectangle[] KeyboardPos = {
            new Rectangle( 1, 21, 7, 10),   // C
            new Rectangle( 6,  0, 5, 21),   // Db
            new Rectangle( 9, 21, 7, 10),   // D
            new Rectangle(14,  0, 5, 21),   // Eb
            new Rectangle(17, 21, 7, 10),   // E
            new Rectangle(25, 21, 7, 10),   // F
            new Rectangle(30,  0, 5, 21),   // Gb
            new Rectangle(33, 21, 7, 10),   // G
            new Rectangle(38,  0, 5, 21),   // Ab
            new Rectangle(41, 21, 7, 10),   // A
            new Rectangle(46,  0, 5, 21),   // Bb
            new Rectangle(49, 21, 7, 10)    // B
        };

        private static readonly Point[] KnobPos = {
            new Point(619, 19),  // Vol.
            new Point(659, 19),  // Exp.
            new Point(699, 19),  // Pan.
            new Point(739, 19),  // Rev.
            new Point(779, 19),  // Cho.
            new Point(819, 19),  // Del.
            new Point(859, 19),  // Fc
            new Point(899, 19)   // Q
        };

        private static readonly Point[] KnobValPos = {
            new Point(610, 15),  // Vol.
            new Point(650, 15),  // Exp.
            new Point(690, 15),  // Pan.
            new Point(730, 15),  // Rev.
            new Point(770, 15),  // Cho.
            new Point(810, 15),  // Del.
            new Point(850, 15),  // Fc
            new Point(890, 15)   // Q
        };

        private static readonly PointF[] Knob = {
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

        public Keyboard(PictureBox picKey, Sender sender, Player player) {
            mCtrl = picKey;
            mCtrl.Width = EasySequencer.Properties.Resources.keyboard.Width + 2;
            mCtrl.Height = EasySequencer.Properties.Resources.keyboard.Height + 2;

            mCtrl.DoubleClick += new EventHandler(picKeyboard_DoubleClick);
            mCtrl.MouseDown += new MouseEventHandler(picKeyboard_MouseDown);
            mCtrl.MouseMove += new MouseEventHandler(picKeyboard_MouseMove);
            mCtrl.MouseUp += new MouseEventHandler(picKeyboard_MouseUp);

            mBuffer = new DoubleBuffer(mCtrl, (Image)mCtrl.BackgroundImage.Clone());
            mSender = sender;
            mPlayer = player;

            Task.Run(() => {
                while (true) {
                    draw();
                    sendValue();
                    Thread.Sleep(25);
                }
            });
        }

        private void picKeyboard_DoubleClick(object sender, EventArgs e) {
            mMouseDownPos = mCtrl.PointToClient(Cursor.Position);
            var channel = mMouseDownPos.Y / ChannelHeight;
            if (InstName.X <= mMouseDownPos.X && channel < 16) {
                var fm = new InstList(mSender, channel);
                fm.ShowDialog();
            }
        }

        private void picKeyboard_MouseDown(object sender, MouseEventArgs e) {
            mMouseDownPos = mCtrl.PointToClient(Cursor.Position);
            var knobX = (mMouseDownPos.X - KnobValPos[0].X + KnobWidth / 4) / KnobWidth;
            var knobY = mMouseDownPos.Y / ChannelHeight;

            if (0 <= knobY && knobY <= 15) {
                mChannelNo = knobY;
                if (MuteButton.X <= mMouseDownPos.X && mMouseDownPos.X < MuteButton.X + MuteButton.Width) {
                    if (e.Button == MouseButtons.Right) {
                        if (mPlayer.Channel(knobY).Enable) {
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
                        mSender.MuteChannel(knobY, mPlayer.Channel(knobY).Enable);
                    }
                }
                if (0 <= knobX && knobX <= 7) {
                    mKnobNo = knobX;
                    mIsDrag = true;
                }
            }
        }

        private void picKeyboard_MouseUp(object sender, MouseEventArgs e) {
            mIsDrag = false;
        }

        private void picKeyboard_MouseMove(object sender, MouseEventArgs e) {
            if (mIsDrag) {
                var pos = mCtrl.PointToClient(Cursor.Position);
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
        }

        private void draw() {
            var whiteWidth = KeyboardPos[0].Width + 1;
            var g = mBuffer.Graphics;
            /** Keyboad **/
            for (var s = 0; s < Sender.SAMPLER_COUNT; ++s) {
                var smpl = mSender.Sampler(s);
                var channel = mPlayer.Channel(smpl.channelNum);
                var y_ch = ChannelHeight * smpl.channelNum;
                var transpose = (int)(channel.Pitch * channel.BendRange / 8192.0 - 0.5);
                var k = smpl.noteNum + transpose;
                if (k < 0 || 127 < k) {
                    continue;
                }
                int x_oct;
                Rectangle key;
                switch (smpl.state) {
                case E_KEY_STATE.PRESS:
                    x_oct = 7 * whiteWidth * (k / 12 - 1);
                    key = KeyboardPos[k % 12];
                    g.FillRectangle(Brushes.Red, key.X + x_oct, key.Y + y_ch, key.Width, key.Height);
                    break;
                case E_KEY_STATE.HOLD:
                    x_oct = 7 * whiteWidth * (k / 12 - 1);
                    key = KeyboardPos[k % 12];
                    g.FillRectangle(Brushes.Blue, key.X + x_oct, key.Y + y_ch, key.Width, key.Height);
                    break;
                }
            }
            /** Knob **/
            for (var ch = 0; ch < 16; ++ch) {
                var channel = mPlayer.Channel(ch);
                var y_ch = ChannelHeight * ch;
                // Vol
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Vol].X) + KnobPos[0].X,
                    (int)(KnobRadius * Knob[channel.Vol].Y) + KnobPos[0].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Vol.ToString("000"),
                    mKnobFont, Brushes.LightGray,
                    KnobValPos[0].X, KnobValPos[0].Y + y_ch
                );
                // Exp
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Exp].X) + KnobPos[1].X,
                    (int)(KnobRadius * Knob[channel.Exp].Y) + KnobPos[1].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Exp.ToString("000"),
                    mKnobFont, Brushes.LightGray,
                    KnobValPos[1].X, KnobValPos[1].Y + y_ch
                );
                // Pan
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Pan].X) + KnobPos[2].X,
                    (int)(KnobRadius * Knob[channel.Pan].Y) + KnobPos[2].Y + y_ch,
                    3, 3
                );
                var pan = channel.Pan - 64;
                if (0 == pan) {
                    g.DrawString(
                        " C ",
                        mKnobFont, Brushes.LightGray,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                } else if (pan < 0) {
                    g.DrawString(
                        "L" + (-pan).ToString("00"),
                        mKnobFont, Brushes.LightGray,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                } else {
                    g.DrawString(
                        "R" + pan.ToString("00"),
                        mKnobFont, Brushes.LightGray,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                }
                // Rev
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Rev].X) + KnobPos[3].X,
                    (int)(KnobRadius * Knob[channel.Rev].Y) + KnobPos[3].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Rev.ToString("000"),
                    mKnobFont, Brushes.LightGray,
                    KnobValPos[3].X, KnobValPos[3].Y + y_ch
                );
                // Cho
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Cho].X) + KnobPos[4].X,
                    (int)(KnobRadius * Knob[channel.Cho].Y) + KnobPos[4].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Cho.ToString("000"),
                    mKnobFont, Brushes.LightGray,
                    KnobValPos[4].X, KnobValPos[4].Y + y_ch
                );
                // Del
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Del].X) + KnobPos[5].X,
                    (int)(KnobRadius * Knob[channel.Del].Y) + KnobPos[5].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Del.ToString("000"),
                    mKnobFont, Brushes.LightGray,
                    KnobValPos[5].X, KnobValPos[5].Y + y_ch
                );
                // Fc
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Fc].X) + KnobPos[6].X,
                    (int)(KnobRadius * Knob[channel.Fc].Y) + KnobPos[6].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Fc.ToString("000"),
                    mKnobFont, Brushes.LightGray,
                    KnobValPos[6].X, KnobValPos[6].Y + y_ch
                );
                // Fq
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Fq].X) + KnobPos[7].X,
                    (int)(KnobRadius * Knob[channel.Fq].Y) + KnobPos[7].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Fq.ToString("000"),
                    mKnobFont, Brushes.LightGray,
                    KnobValPos[7].X, KnobValPos[7].Y + y_ch
                );
                // Mute Button
                if (!channel.Enable) {
                    g.FillRectangle(Brushes.Red, MuteButton.X, MuteButton.Y + y_ch, MuteButton.Width, MuteButton.Height);
                }
                // InstName
                g.DrawString(Marshal.PtrToStringAuto(channel.Name), mInstFont, Brushes.Black, InstName.X, InstName.Y + y_ch);
            }
            mBuffer.Render();
        }

        private void sendValue() {
            if (!mIsParamChg) {
                return;
            }

            mIsParamChg = false;

            switch (mKnobNo) {
            case 0:
                mSender.Send(new Event(
                    E_CTRL_TYPE.VOLUME,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            case 1:
                mSender.Send(new Event(
                    E_CTRL_TYPE.EXPRESSION,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            case 2:
                mSender.Send(new Event(
                    E_CTRL_TYPE.PAN,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            case 3:
                mSender.Send(new Event(
                    E_CTRL_TYPE.REVERB,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            case 4:
                mSender.Send(new Event(
                    E_CTRL_TYPE.CHORUS,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            case 5:
                mSender.Send(new Event(
                    E_CTRL_TYPE.DELAY,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            case 6:
                mSender.Send(new Event(
                    E_CTRL_TYPE.CUTOFF,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            case 7:
                mSender.Send(new Event(
                    E_CTRL_TYPE.RESONANCE,
                    (byte)mChannelNo,
                    (byte)mChangeValue
                ));
                break;
            }
        }
    }
}
