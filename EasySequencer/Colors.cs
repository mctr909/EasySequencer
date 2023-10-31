using System.Drawing.Drawing2D;
using System.Drawing;
using System;

namespace EasySequencer {
    static class Colors {
        public static readonly Color CRoll = Color.FromArgb(211, 211, 211);
        static readonly Color CMeasureBorder = Color.FromArgb(47, 47, 47);
        static readonly Color CMeasureArea = Color.FromArgb(191, 191, 191);
        static readonly Color CBeatBorder = Color.FromArgb(191, 191, 191);
        static readonly Color CSelectBorder = Color.FromArgb(63, 167, 167);
        static readonly Color CSelectArea = Color.FromArgb(23, 255, 255, 127);
        static readonly Color CBlackKey = Color.FromArgb(127, 127, 127);
        static readonly Color CSolidNote = Color.FromArgb(63, 211, 63);
        static readonly Color CSelectedNote = Color.FromArgb(235, 71, 71);
        static readonly Color COtherNote = Color.FromArgb(235, 235, 235);

        public static readonly Pen OctBorder = new Pen(Color.FromArgb(0, 0, 0));
        public static readonly Pen KeyBorder = new Pen(CBlackKey);
        public static readonly Pen MeasureBorder = new Pen(CMeasureBorder);
        public static readonly Pen BeatBorder = new Pen(CBeatBorder);
        public static readonly Pen SelectBorder = new Pen(CSelectBorder)
        {
            Width = 1.0f,
            DashStyle = DashStyle.Dot
        };
        public static readonly Brush OctText = Brushes.White;
        public static readonly Brush MeasureText = Brushes.Black;
        public static readonly Brush BlackKey = new Pen(CBlackKey).Brush;
        public static readonly Brush MeasureArea = new Pen(CMeasureArea).Brush;
        public static readonly Brush SelectArea = new Pen(CSelectArea).Brush;

        static readonly Color CSolidNoteH = ToLight(CSolidNote);
        static readonly Color CSolidNoteL = ToDark(CSolidNote);
        static readonly Color CSolidNoteThin = FromHSV(GetHue(CSolidNote), 1, 0.8);
        static readonly Brush SolidNote = new Pen(CSolidNote).Brush;
        static readonly Pen SolidNoteH = new Pen(CSolidNoteH);
        static readonly Pen SolidNoteL = new Pen(CSolidNoteL);
        static readonly Pen SolidNoteThin = new Pen(CSolidNoteThin);
        static readonly Brush ClipBoardNote = new Pen(Color.Black) {
            Color = Color.FromArgb(111, CSolidNote.R, CSolidNote.G, CSolidNote.B),
            Width = 1.0f
        }.Brush;
        static readonly Pen ClipBoardNoteH = new Pen(Color.Black) {
            Color = Color.FromArgb(111, CSolidNoteH.R, CSolidNoteH.G, CSolidNoteH.B),
            Width = 1.0f
        };
        static readonly Pen ClipBoardNoteL = new Pen(Color.Black) {
            Color = Color.FromArgb(111, CSolidNoteL.R, CSolidNoteL.G, CSolidNoteL.B),
            Width = 1.0f
        };
        static readonly Pen ClipBoardNoteThin = new Pen(Color.Black) {
            Color = Color.FromArgb(111, CSolidNoteThin.R, CSolidNoteThin.G, CSolidNoteThin.B),
            Width = 1.0f
        };
        static readonly Brush SelectedNote = new Pen(CSelectedNote).Brush;
        static readonly Pen SelectedNoteH = new Pen(ToLight(CSelectedNote));
        static readonly Pen SelectedNoteL = new Pen(ToDark(CSelectedNote));
        static readonly Pen SelectedNoteThin = new Pen(FromHSV(GetHue(CSelectedNote), 1, 0.8));
        static readonly Brush OtherNote = new Pen(COtherNote).Brush;
        static readonly Pen OtherNoteH = new Pen(ToLight(COtherNote));
        static readonly Pen OtherNoteL = new Pen(ToDark(COtherNote));

        public static Color ToDark(Color c) {
            return FromHSV(GetHue(c), 0.75, 0.5, c.A);
        }
        public static Color ToLight(Color c) {
            return FromHSV(GetHue(c), 0.75, 1.0, c.A);
        }
        public static double GetHue(Color c) {
            var x = c.R * 2 / 3.0 - c.G / 3.0 - c.B / 3.0;
            var y = c.G / Math.Sqrt(3) - c.B / Math.Sqrt(3);
            if (x * x + y * y < 1) {
                return -1;
            } else {
                var h = Math.Atan2(y, x);
                if (h < 0.0) {
                    return h + 2 * Math.PI;
                } else {
                    return h;
                }
            }
        }
        public static Color FromHSV(double h, double s, double v, int a = 255) {
            double r;
            double g;
            double b;
            if (h < 0) {
                r = 1.0;
                g = 1.0;
                b = 1.0;
            } else {
                r = 0.5 + 0.5 * Math.Cos(h);
                g = 0.5 + 0.5 * Math.Cos(h - 2 * Math.PI / 3);
                b = 0.5 + 0.5 * Math.Cos(h + 2 * Math.PI / 3);
            }
            r *= s; g *= s; b *= s;
            r += 1.0 - s; g += 1.0 - s; b += 1.0 - s;
            r *= v * 255; g *= v * 255; b *= v * 255;
            return Color.FromArgb(a, (int)r, (int)g, (int)b);
        }

        public static void DrawNote(Graphics g, int x1, int y1, int x2, int y2) {
            x1++;
            y1++;
            var w = x2 - x1;
            if (2 <= w) {
                g.FillRectangle(SolidNote, x1, y1, w, y2 - y1 + 1);
                g.DrawLine(SolidNoteL, x1, y2, x2, y2);
                g.DrawLine(SolidNoteH, x1, y1, x2, y1);
                g.DrawLine(SolidNoteL, x2, y2, x2, y1);
                g.DrawLine(SolidNoteH, x1, y1, x1, y2 - 1);
            } else {
                g.DrawLine(SolidNoteThin, x1, y2, x2, y2);
                g.DrawLine(SolidNoteThin, x1, y1, x2, y1);
                g.DrawLine(SolidNoteThin, x2, y2, x2, y1);
                g.DrawLine(SolidNoteThin, x1, y1, x1, y2 - 1);
            }
        }

        public static void DrawClipBoardNote(Graphics g, int x1, int y1, int x2, int y2) {
            x1++;
            y1++;
            var w = x2 - x1;
            if (2 <= w) {
                g.FillRectangle(ClipBoardNote, x1, y1, w, y2 - y1 + 1);
                g.DrawLine(ClipBoardNoteL, x1, y2, x2, y2);
                g.DrawLine(ClipBoardNoteH, x1, y1, x2, y1);
                g.DrawLine(ClipBoardNoteL, x2, y2, x2, y1);
                g.DrawLine(ClipBoardNoteH, x1, y1, x1, y2 - 1);
            } else {
                g.DrawLine(ClipBoardNoteThin, x1, y2, x2, y2);
                g.DrawLine(ClipBoardNoteThin, x1, y1, x2, y1);
                g.DrawLine(ClipBoardNoteThin, x2, y2, x2, y1);
                g.DrawLine(ClipBoardNoteThin, x1, y1, x1, y2 - 1);
            }
        }

        public static void DrawSelectedNote(Graphics g, int x1, int y1, int x2, int y2) {
            x1++;
            y1++;
            var w = x2 - x1;
            if (2 <= w) {
                g.FillRectangle(SelectedNote, x1, y1, w, y2 - y1 + 1);
                g.DrawLine(SelectedNoteL, x1, y2, x2, y2);
                g.DrawLine(SelectedNoteH, x1, y1, x2, y1);
                g.DrawLine(SelectedNoteL, x2, y2, x2, y1);
                g.DrawLine(SelectedNoteH, x1, y1, x1, y2 - 1);
            } else {
                g.DrawLine(SelectedNoteThin, x1, y2, x2, y2);
                g.DrawLine(SelectedNoteThin, x1, y1, x2, y1);
                g.DrawLine(SelectedNoteThin, x2, y2, x2, y1);
                g.DrawLine(SelectedNoteThin, x1, y1, x1, y2 - 1);
            }
        }

        public static void DrawOtherNote(Graphics g, int x1, int y1, int x2, int y2) {
            x1++;
            y1++;
            var w = x2 - x1;
            if (2 <= w) {
                g.FillRectangle(OtherNote, x1, y1, w, y2 - y1 + 1);
                g.DrawLine(OtherNoteL, x1, y2, x2, y2);
                g.DrawLine(OtherNoteH, x1, y1, x2, y1);
                g.DrawLine(OtherNoteL, x2, y2, x2, y1);
                g.DrawLine(OtherNoteH, x1, y1, x1, y2 - 1);
            } else {
                g.DrawLine(OtherNoteL, x1, y2, x2, y2);
                g.DrawLine(OtherNoteL, x1, y1, x2, y1);
                g.DrawLine(OtherNoteL, x2, y2, x2, y1);
                g.DrawLine(OtherNoteL, x1, y1, x1, y2 - 1);
            }
        }
    }
}