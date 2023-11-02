using System.Drawing.Drawing2D;
using System.Drawing;
using System;

namespace EasySequencer {
    static class Colors {
        public static readonly Color Roll = SystemColors.ButtonHighlight;
        public static readonly Brush Text = new Pen(SystemColors.ControlText).Brush;
        public static readonly Brush MeasureTab = new Pen(SystemColors.Control).Brush;
        public static readonly Pen Measure = new Pen(SystemColors.ControlText);
        public static readonly Pen Beat = new Pen(Color.FromArgb(191, 191, 191));
        public static readonly Pen KeyBorder = new Pen(SystemColors.ButtonShadow);
        public static readonly Pen OctBorder = new Pen(SystemColors.ControlText);
        public static readonly Brush SelectArea = new Pen(Alpha(SystemColors.Highlight)).Brush;
        public static readonly Pen SelectBorder = new Pen(SystemColors.HighlightText) { DashStyle = DashStyle.Dot };

        static readonly Color CNote = Color.FromArgb(0, 255, 0);
        static readonly Color CSelectedNote = Color.FromArgb(255, 0, 0);
        static readonly Color COtherNote = Color.FromArgb(0, 255, 255);

        static Color Dark(Color c) { return FromHSV(GetHue(c), 0.6, 0.5, c.A); }
        static Color Solid(Color c) { return FromHSV(GetHue(c), 1.0, 0.85, c.A); }
        static Color Light(Color c) { return FromHSV(GetHue(c), 0.6, 0.9, c.A); }
        static Color Thin(Color c) { return FromHSV(GetHue(c), 0.6, 0.6, c.A); }
        static Color Alpha(Color c) { return Color.FromArgb(115, c.R, c.G, c.B); }

        static readonly Brush Note = new Pen(Solid(CNote)).Brush;
        static readonly Pen NoteH = new Pen(Light(CNote));
        static readonly Pen NoteL = new Pen(Dark(CNote));
        static readonly Pen NoteThin = new Pen(Thin(CNote));
        static readonly Brush SelectedNote = new Pen(Solid(CSelectedNote)).Brush;
        static readonly Pen SelectedNoteH = new Pen(Light(CSelectedNote));
        static readonly Pen SelectedNoteL = new Pen(Dark(CSelectedNote));
        static readonly Pen SelectedNoteThin = new Pen(Thin(CSelectedNote));
        static readonly Brush OtherNote = new Pen(Solid(COtherNote)).Brush;
        static readonly Pen OtherNoteH = new Pen(Light(COtherNote));
        static readonly Pen OtherNoteL = new Pen(Dark(COtherNote));
        static readonly Brush ClipBoardNote = new Pen(Alpha(Solid(CNote))).Brush;
        static readonly Pen ClipBoardNoteH = new Pen(Alpha(NoteH.Color));
        static readonly Pen ClipBoardNoteL = new Pen(Alpha(NoteL.Color));
        static readonly Pen ClipBoardNoteThin = new Pen(Alpha(NoteThin.Color));

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
                g.FillRectangle(Note, x1, y1, w, y2 - y1 + 1);
                g.DrawLine(NoteL, x1, y2, x2, y2);
                g.DrawLine(NoteH, x1, y1, x2, y1);
                g.DrawLine(NoteL, x2, y2, x2, y1);
                g.DrawLine(NoteH, x1, y1, x1, y2 - 1);
            } else {
                g.DrawLine(NoteThin, x1, y2, x2, y2);
                g.DrawLine(NoteThin, x1, y1, x2, y1);
                g.DrawLine(NoteThin, x2, y2, x2, y1);
                g.DrawLine(NoteThin, x1, y1, x1, y2 - 1);
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
    }
}