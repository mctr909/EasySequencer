using System.Drawing;
using System.Windows.Forms;
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

        private static readonly double[][] Knob = {
            new double[] { -0.604,  0.797 }, new double[] { -0.635,  0.773 },
            new double[] { -0.665,  0.747 }, new double[] { -0.694,  0.720 },
            new double[] { -0.721,  0.693 }, new double[] { -0.748,  0.664 },
            new double[] { -0.774,  0.634 }, new double[] { -0.798,  0.603 },
            new double[] { -0.821,  0.571 }, new double[] { -0.843,  0.539 },
            new double[] { -0.863,  0.505 }, new double[] { -0.882,  0.471 },
            new double[] { -0.900,  0.436 }, new double[] { -0.917,  0.400 },
            new double[] { -0.932,  0.364 }, new double[] { -0.945,  0.327 },
            new double[] { -0.957,  0.290 }, new double[] { -0.968,  0.252 },
            new double[] { -0.977,  0.214 }, new double[] { -0.985,  0.175 },
            new double[] { -0.991,  0.137 }, new double[] { -0.996,  0.098 },
            new double[] { -0.999,  0.058 }, new double[] { -1.000,  0.019 },
            new double[] { -1.000, -0.020 }, new double[] { -0.999, -0.059 },
            new double[] { -0.996, -0.099 }, new double[] { -0.991, -0.138 },
            new double[] { -0.985, -0.176 }, new double[] { -0.977, -0.215 },
            new double[] { -0.968, -0.253 }, new double[] { -0.957, -0.291 },
            new double[] { -0.945, -0.328 }, new double[] { -0.932, -0.365 },
            new double[] { -0.917, -0.401 }, new double[] { -0.900, -0.437 },
            new double[] { -0.882, -0.472 }, new double[] { -0.863, -0.506 },
            new double[] { -0.843, -0.540 }, new double[] { -0.821, -0.572 },
            new double[] { -0.798, -0.604 }, new double[] { -0.774, -0.635 },
            new double[] { -0.748, -0.665 }, new double[] { -0.721, -0.694 },
            new double[] { -0.694, -0.721 }, new double[] { -0.665, -0.748 },
            new double[] { -0.635, -0.774 }, new double[] { -0.604, -0.798 },
            new double[] { -0.572, -0.821 }, new double[] { -0.540, -0.843 },
            new double[] { -0.506, -0.863 }, new double[] { -0.472, -0.882 },
            new double[] { -0.437, -0.900 }, new double[] { -0.401, -0.917 },
            new double[] { -0.365, -0.932 }, new double[] { -0.328, -0.945 },
            new double[] { -0.291, -0.957 }, new double[] { -0.253, -0.968 },
            new double[] { -0.215, -0.977 }, new double[] { -0.176, -0.985 },
            new double[] { -0.138, -0.991 }, new double[] { -0.099, -0.996 },
            new double[] { -0.059, -0.999 }, new double[] { -0.020, -1.000 },
            new double[] {  0.019, -1.000 }, new double[] {  0.058, -0.999 },
            new double[] {  0.098, -0.996 }, new double[] {  0.137, -0.991 },
            new double[] {  0.175, -0.985 }, new double[] {  0.214, -0.977 },
            new double[] {  0.252, -0.968 }, new double[] {  0.290, -0.957 },
            new double[] {  0.327, -0.945 }, new double[] {  0.364, -0.932 },
            new double[] {  0.400, -0.917 }, new double[] {  0.436, -0.900 },
            new double[] {  0.471, -0.882 }, new double[] {  0.505, -0.863 },
            new double[] {  0.539, -0.843 }, new double[] {  0.571, -0.821 },
            new double[] {  0.603, -0.798 }, new double[] {  0.634, -0.774 },
            new double[] {  0.664, -0.748 }, new double[] {  0.693, -0.721 },
            new double[] {  0.720, -0.694 }, new double[] {  0.747, -0.665 },
            new double[] {  0.773, -0.635 }, new double[] {  0.797, -0.604 },
            new double[] {  0.820, -0.572 }, new double[] {  0.842, -0.540 },
            new double[] {  0.862, -0.506 }, new double[] {  0.881, -0.472 },
            new double[] {  0.899, -0.437 }, new double[] {  0.916, -0.401 },
            new double[] {  0.931, -0.365 }, new double[] {  0.944, -0.328 },
            new double[] {  0.956, -0.291 }, new double[] {  0.967, -0.253 },
            new double[] {  0.976, -0.215 }, new double[] {  0.984, -0.176 },
            new double[] {  0.990, -0.138 }, new double[] {  0.995, -0.099 },
            new double[] {  0.998, -0.059 }, new double[] {  0.999, -0.020 },
            new double[] {  0.999,  0.019 }, new double[] {  0.998,  0.058 },
            new double[] {  0.995,  0.098 }, new double[] {  0.990,  0.137 },
            new double[] {  0.984,  0.175 }, new double[] {  0.976,  0.214 },
            new double[] {  0.967,  0.252 }, new double[] {  0.956,  0.290 },
            new double[] {  0.944,  0.327 }, new double[] {  0.931,  0.364 },
            new double[] {  0.916,  0.400 }, new double[] {  0.899,  0.436 },
            new double[] {  0.881,  0.471 }, new double[] {  0.862,  0.505 },
            new double[] {  0.842,  0.539 }, new double[] {  0.820,  0.571 },
            new double[] {  0.797,  0.603 }, new double[] {  0.773,  0.634 },
            new double[] {  0.747,  0.664 }, new double[] {  0.720,  0.693 },
            new double[] {  0.693,  0.720 }, new double[] {  0.664,  0.747 },
            new double[] {  0.634,  0.773 }, new double[] {  0.603,  0.797 }
        };

        public Keyboard(PictureBox picKey, Player player) {
            mBuffer = new DoubleBuffer(picKey, (Image)picKey.BackgroundImage.Clone());
            mPlayer = player;

            Task.Run(() => {
                while (true) {
                    draw();
                    Thread.Sleep(20);
                }
            });
        }

        private void draw() {
            if (null == mPlayer) {
                return;
            }

            var whiteWidth = KeyboardPos[0].Width + 1;
            var g = mBuffer.Graphics;

            for (int ch = 0; ch < mPlayer.Channel.Length; ++ch) {
                var channel = mPlayer.Channel[ch];
                var y_ch = ChannelHeight * ch;
                var x_pitch = (int)(0.5 * whiteWidth * channel.Pitch * channel.BendRange / 8192.0);

                for (int k = 0; k < 127; ++k) {
                    if (KEY_STATUS.ON == channel.KeyBoard[k]) {
                        var x_oct = 7 * whiteWidth * (k / 12 - 1) + x_pitch;
                        var key = KeyboardPos[k % 12];
                        g.FillRectangle(Brushes.Red, key.X + x_oct, key.Y + y_ch, key.Width, key.Height);
                    }

                    if (KEY_STATUS.HOLD == channel.KeyBoard[k]) {
                        var x_oct = 7 * whiteWidth * (k / 12 - 1) + x_pitch;
                        var key = KeyboardPos[k % 12];
                        g.FillRectangle(Brushes.Blue, key.X + x_oct, key.Y + y_ch, key.Width, key.Height);
                    }
                }

                // Vol
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Vol][0]) + KnobPos[0].X,
                    (int)(KnobRadius * Knob[channel.Vol][1]) + KnobPos[0].Y + y_ch,
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
                    (int)(KnobRadius * Knob[channel.Exp][0]) + KnobPos[1].X,
                    (int)(KnobRadius * Knob[channel.Exp][1]) + KnobPos[1].Y + y_ch,
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
                    (int)(KnobRadius * Knob[channel.Pan][0]) + KnobPos[2].X,
                    (int)(KnobRadius * Knob[channel.Pan][1]) + KnobPos[2].Y + y_ch,
                    3, 3
                );
                var exp = channel.Pan - 64;
                if (0 == exp) {
                    g.DrawString(
                        " C ",
                        mFont, Brushes.Black,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                }
                else if (exp < 0) {
                    g.DrawString(
                        "L" + (-exp).ToString("00"),
                        mFont, Brushes.Black,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                }
                else {
                    g.DrawString(
                        "R" + exp.ToString("00"),
                        mFont, Brushes.Black,
                        KnobValPos[2].X, KnobValPos[2].Y + y_ch
                    );
                }

                // Rev
                g.FillRectangle(
                    Brushes.White,
                    (int)(KnobRadius * Knob[channel.Rev][0]) + KnobPos[3].X,
                    (int)(KnobRadius * Knob[channel.Rev][1]) + KnobPos[3].Y + y_ch,
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
                    (int)(KnobRadius * Knob[channel.Cho][0]) + KnobPos[4].X,
                    (int)(KnobRadius * Knob[channel.Cho][1]) + KnobPos[4].Y + y_ch,
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
                    (int)(KnobRadius * Knob[channel.Del][0]) + KnobPos[5].X,
                    (int)(KnobRadius * Knob[channel.Del][1]) + KnobPos[5].Y + y_ch,
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
                    (int)(KnobRadius * Knob[channel.Fc][0]) + KnobPos[6].X,
                    (int)(KnobRadius * Knob[channel.Fc][1]) + KnobPos[6].Y + y_ch,
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
                    (int)(KnobRadius * Knob[channel.Fq][0]) + KnobPos[7].X,
                    (int)(KnobRadius * Knob[channel.Fq][1]) + KnobPos[7].Y + y_ch,
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
