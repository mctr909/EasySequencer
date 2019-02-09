using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

using MIDI;

namespace Player {
    public class Keyboard {
        private static readonly Font mFont = new Font("ＭＳ ゴシック", 9.0f, FontStyle.Regular, GraphicsUnit.Point);
        private DoubleBuffer mBuffer;
        private Player mPlayer;

        private static readonly int ChannelHeight = 40;
        private static readonly int KnobRadius = 7;

        private static readonly Rectangle[] KeyboardPos = {
            new Rectangle( 1, 20, 7, 11),    // C
            new Rectangle( 6,  0, 5, 21),    // Db
            new Rectangle( 9, 20, 7, 11),    // D
            new Rectangle(14,  0, 5, 21),    // Eb
            new Rectangle(17, 20, 7, 11),    // E
            new Rectangle(25, 20, 7, 11),    // F
            new Rectangle(30,  0, 5, 21),    // Gb
            new Rectangle(33, 20, 7, 11),    // G
            new Rectangle(38,  0, 5, 21),    // Ab
            new Rectangle(41, 20, 7, 11),    // A
            new Rectangle(46,  0, 5, 21),    // Bb
            new Rectangle(49, 20, 7, 11)     // B
        };

        public static readonly Point[] KnobPos = {
            new Point(611, 9),
            new Point(635, 9),
            new Point(659, 9),
            new Point(683, 9),
            new Point(707, 9),
            new Point(731, 9),
            new Point(755, 9),
            new Point(779, 9)
        };

        public static readonly Point[] KnobValPos = {
            new Point(602, 28),
            new Point(626, 28),
            new Point(650, 28),
            new Point(674, 28),
            new Point(698, 28),
            new Point(722, 28),
            new Point(746, 28),
            new Point(770, 28)
        };

        public static readonly PointF[] Knob = {
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

        public Keyboard(PictureBox picKey, Player player) {
            mBuffer = new DoubleBuffer(picKey, (Image)picKey.BackgroundImage.Clone());
            mPlayer = player;

            Task.Run(() => {
                while (true) {
                    draw();
                    Thread.Sleep(30);
                }
            });
        }

        private void draw() {
            var whiteWidth = KeyboardPos[0].Width + 1;
            var g = mBuffer.Graphics;

            for (var ch = 0; ch < mPlayer.Channel.Length; ++ch) {
                var channel = mPlayer.Channel[ch];
                var ofsKey = (int)(channel.Pitch * channel.BendRange / 8192.0 - 0.5);
                var y_ch = ChannelHeight * ch;

                for (var i = 0; i < 127; ++i) {
                    var k = i + ofsKey;
                    if (k < 0 || 127 < k) {
                        continue;
                    }
                    if (KEY_STATUS.ON == channel.KeyBoard[i]) {
                        var x_oct = 7 * whiteWidth * (k / 12 - 1);
                        var key = KeyboardPos[k % 12];
                        g.FillRectangle(Brushes.Red, key.X + x_oct, key.Y + y_ch, key.Width, key.Height);
                    }
                    if (KEY_STATUS.HOLD == channel.KeyBoard[i]) {
                        var x_oct = 7 * whiteWidth * (k / 12 - 1);
                        var key = KeyboardPos[k % 12];
                        g.FillRectangle(Brushes.Blue, key.X + x_oct, key.Y + y_ch, key.Width, key.Height);
                    }
                }

                // Vol
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Vol].X) + KnobPos[0].X,
                    (int)(KnobRadius * Knob[channel.Vol].Y) + KnobPos[0].Y + y_ch,
                    3, 3
                );
                g.DrawString(
                    channel.Vol.ToString("000"),
                    mFont, Brushes.Black,
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
                    mFont, Brushes.Black,
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
                        mFont, Brushes.Black,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                }
                else if (pan < 0) {
                    g.DrawString(
                        "L" + (-pan).ToString("00"),
                        mFont, Brushes.Black,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                }
                else {
                    g.DrawString(
                        "R" + pan.ToString("00"),
                        mFont, Brushes.Black,
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
                    mFont, Brushes.Black,
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
                    mFont, Brushes.Black,
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
                    mFont, Brushes.Black,
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
                    mFont, Brushes.Black,
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
                    mFont, Brushes.Black,
                    KnobValPos[7].X, KnobValPos[7].Y + y_ch
                );

                if (!channel.Enable) {
                    g.FillRectangle(Brushes.Red, 797, 4 + y_ch, 13, 18);
                }
            }

            mBuffer.Render();
        }
    }
}
