using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using SMF;

namespace EasySequencer {
    public partial class PianoRoll : Form {
        enum E_SELECT {
            NONE,
            NOTE_SELECT,
            NOTE_SELECTED,
            NOTE_MOVE,
            NOTE_EXPAND,
            NOTE_WRITE
        }

        static readonly Dictionary<string, int> Snaps = new Dictionary<string, int> {
            { "4分",    960 },
            { "3連4分", 640 },
            { "8分",    480 },
            { "5連4分", 384 },
            { "3連8分", 320 },
            { "16分",   240 },
            { "5連8分", 192 },
            { "3連16分",160 },
            { "32分",   120 },
            { "5連16分", 96 },
            { "3連32分", 80 },
            { "64分",    60 },
            { "5連32分", 48 },
            { "3連64分", 40 },
            { "5連64分", 24 }
        };

        static readonly int[] TimeScales = {
            480, 960, 960 * 2, 960 * 4
        };

        static readonly int[] ToneHeights = {
            24, 20, 18, 15, 12, 10, 9, 8, 7
        };

        const int MeasureTabHeight = 17;

        Bitmap mBmpRoll;
        Graphics mgRoll;

        Font mOctNameFont = new Font("Segoe UI", 9.0f, FontStyle.Bold);
        Font mMeasureFont = new Font("Segoe UI", 11.0f, FontStyle.Bold);

        int mTimeScaleIdx = 1;
        int mTimeScale = TimeScales[1];
        int mQuarterNoteWidth = 96;
        int mNoteHeightIdx = 5;
        int mNoteHeight = ToneHeights[5];
        int mDispNoteCount = 0;

        Cursor mMouseCursor = Cursors.Default;
        int mMouseDownTick;
        int mMouseTick;
        int mMouseNote;
        int mSnappedTick;
        int mCursorX;
        int mTickBegin;
        int mTickEnd;
        int mNoteBegin;
        int mNoteEnd;

        E_SELECT mSelectState = E_SELECT.NONE;
        bool mPressCtrl = false;
        Keys mPressKey = Keys.None;

        EventEditor mEditor = new EventEditor();

        public PianoRoll(Event[] eventList) {
            InitializeComponent();
            MouseWheel += new MouseEventHandler(PianoRoll_MouseWheel);

            setLayout();
            tsmEditMode_Click(tsmEditModeNote);
            tsbWriteSelect_Click(tsbSelect);
            tsmTick_Click(tsmTick240);

            mEditor.LoadEvent(eventList);

            timer1.Interval = 50;
            timer1.Enabled = true;
            timer1.Start();
            vScroll.Value = vScroll.Minimum < 80 ? 80 : vScroll.Minimum;
        }

        private void PianoRoll_SizeChanged(object sender, EventArgs e) {
            setLayout();
            timer1_Tick();
        }

        void setLayout() {
            if (Height < toolStrip1.Bottom + hScroll.Height + 40) {
                Height = toolStrip1.Bottom + hScroll.Height + 40;
            }

            pnlRoll.Left = 2;
            pnlRoll.Top = toolStrip1.Bottom;
            pnlRoll.Width = Width - 20;
            pnlRoll.Height = Height - toolStrip1.Bottom - 41;

            vScroll.Top = 0;
            vScroll.Left = pnlRoll.Width - vScroll.Width;
            vScroll.Height = pnlRoll.Height - hScroll.Height;
            vScroll.SmallChange = 1;
            vScroll.LargeChange = 1;

            hScroll.Left = 0;
            hScroll.Top = pnlRoll.Height - hScroll.Height;
            hScroll.Width = pnlRoll.Width - vScroll.Width;
            hScroll.SmallChange = EventEditor.TickSnap;
            hScroll.LargeChange = EventEditor.TickSnap;

            picRoll.Left = 0;
            picRoll.Top = 0;
            picRoll.Width = vScroll.Left;
            picRoll.Height = hScroll.Top;
            if (picRoll.Width == 0) {
                picRoll.Width = 1;
            }
            if (picRoll.Height == 0) {
                picRoll.Height = 1;
            }

            if (null != picRoll.Image) {
                picRoll.Image.Dispose();
                picRoll.Image = null;
            }
            if (null != mgRoll) {
                mgRoll.Dispose();
                mgRoll = null;
            }
            mBmpRoll = new Bitmap(picRoll.Width, picRoll.Height);
            picRoll.Image = mBmpRoll;
            mgRoll = Graphics.FromImage(picRoll.Image);
            setDispNoteCount();
        }

        void setDispNoteCount() {
            mDispNoteCount = (picRoll.Height - MeasureTabHeight) / mNoteHeight;
            if (128 < mDispNoteCount) {
                mDispNoteCount = 128;
            }
            vScroll.Maximum = 129;
            vScroll.Minimum = mDispNoteCount;
            int fontSize = 7;
            while (fontSize <= 24) {
                var newFont = new Font(mOctNameFont.Name, fontSize, FontStyle.Bold);
                var fsize = mgRoll.MeasureString("C-1", newFont).Height * 0.75;
                if (mNoteHeight <= fsize) {
                    break;
                }
                mOctNameFont = newFont;
                fontSize++;
            }
        }

        int getScrollTick(int offsetPixel = 0) {
            return (hScroll.Value / EventEditor.TickSnap * EventEditor.TickSnap) + offsetPixel * mTimeScale / mQuarterNoteWidth;
        }

        private void PianoRoll_Leave(object sender, EventArgs e) {
            mPressKey = Keys.None;
            mPressCtrl = false;
        }

        private void PianoRoll_KeyDown(object sender, KeyEventArgs e) {
            mPressKey = e.KeyCode;
            switch (e.KeyCode) {
            case Keys.ControlKey:
                mPressCtrl = true;
                break;
            case Keys.Right:
                tsmScrollNext_Click();
                break;
            case Keys.Left:
                tsmScrollPrev_Click();
                break;
            case Keys.Up:
                tsmScrollUp_Click();
                break;
            case Keys.Down:
                tsmScrollDown_Click();
                break;
            case Keys.A:
                if (mPressCtrl) {
                }
                break;
            case Keys.C:
                if (mPressCtrl) {
                    mEditor.Copy(mTickBegin);
                }
                break;
            case Keys.X:
                if (mPressCtrl) {
                    mEditor.Copy(mTickBegin);
                    mEditor.Delete();
                }
                break;
            case Keys.V:
                if (mPressCtrl) {
                    mEditor.Paste(mSnappedTick);
                }
                break;
            }
        }

        private void PianoRoll_KeyUp(object sender, KeyEventArgs e) {
            mPressKey = Keys.None;
            switch (e.KeyCode) {
            case Keys.ControlKey:
                mPressCtrl = false;
                break;
            case Keys.Delete:
                mEditor.ClearClipBoard();
                mEditor.Delete();
                break;
            case Keys.Escape:
                mEditor.ClearClipBoard();
                mSelectState = E_SELECT.NONE;
                break;
            case Keys.F1:
                tsbWriteSelect_Click(tsbWrite);
                break;
            case Keys.F2:
                tsbWriteSelect_Click(tsbSelect);
                break;
            case Keys.F3:
                tsbWriteSelect_Click(tsbMultiSelect);
                break;
            }
        }

        private void PianoRoll_MouseWheel(object sender, MouseEventArgs e) {
            switch (mPressKey) {
            case Keys.None:
                if (0 < Math.Sign(e.Delta)) {
                    tsmScrollUp_Click();
                } else {
                    tsmScrollDown_Click();
                }
                break;
            case Keys.Alt:
            case Keys.Menu:
                if (0 < Math.Sign(e.Delta)) {
                    tsmToneZoom_Click();
                } else {
                    tsmToneZoomout_Click();
                }
                break;
            case Keys.ControlKey:
                if (0 < Math.Sign(e.Delta)) {
                    tsmScrollPrev_Click();
                } else {
                    tsmScrollNext_Click();
                }
                break;
            case Keys.ShiftKey:
                if (0 < Math.Sign(e.Delta)) {
                    tsmTimeZoom_Click();
                } else {
                    tsmTimeZoomout_Click();
                }
                break;
            default:
                break;
            }
        }

        #region Menu bar event
        private void tsmScrollNext_Click(object sender = null, EventArgs e = null) {
            var begin = getScrollTick();
            var measureList = mEditor.GetMeasureList(begin, begin + 3840);
            if (0 == measureList.Count) {
                return;
            }
            var measure = measureList[0];
            var unitTick = 3840 * measure.Nume / measure.Deno;
            unitTick -= (begin - measure.EventTick) % unitTick;
            if (hScroll.Value + unitTick < hScroll.Maximum) {
                hScroll.Value += unitTick;
            }
        }

        private void tsmScrollPrev_Click(object sender = null, EventArgs e = null) {
            var begin = getScrollTick();
            var measureList = mEditor.GetMeasureList(begin - 3840, begin);
            if (0 == measureList.Count) {
                return;
            }
            var measure = measureList[measureList.Count - 1];
            var unitTick = 3840 * measure.Nume / measure.Deno;
            var mod = (begin - measure.EventTick) % unitTick;
            if (0 != mod) {
                unitTick = mod;
            }
            if (hScroll.Minimum <= hScroll.Value - unitTick) {
                hScroll.Value -= unitTick;
            }
        }

        private void tsmScrollUp_Click(object sender = null, EventArgs e = null) {
            if (vScroll.Minimum <= vScroll.Value - 1) {
                vScroll.Value--;
            }
        }

        private void tsmScrollDown_Click(object sender = null, EventArgs e = null) {
            if (vScroll.Value < vScroll.Maximum) {
                vScroll.Value++;
            }
        }

        private void tsmTimeZoom_Click(object sender = null, EventArgs e = null) {
            if (0 < mTimeScaleIdx) {
                mTimeScaleIdx--;
                mTimeScale = TimeScales[mTimeScaleIdx];
            }
            if (0 == mTimeScaleIdx) {
                tsmTimeZoom.Enabled = false;
            }
            tsmTimeZoomout.Enabled = true;
        }

        private void tsmTimeZoomout_Click(object sender = null, EventArgs e = null) {
            if (mTimeScaleIdx < TimeScales.Length - 1) {
                mTimeScaleIdx++;
                mTimeScale = TimeScales[mTimeScaleIdx];
            }
            if (mTimeScaleIdx == TimeScales.Length - 1) {
                tsmTimeZoomout.Enabled = false;
            }
            tsmTimeZoom.Enabled = true;
            checkDiv();
        }

        private void tsmToneZoom_Click(object sender = null, EventArgs e = null) {
            if (0 < mNoteHeightIdx) {
                mNoteHeightIdx--;
                mNoteHeight = ToneHeights[mNoteHeightIdx];
                setDispNoteCount();
            }
            if (0 == mNoteHeightIdx) {
                tsmToneZoom.Enabled = false;
            }
            tsmToneZoomout.Enabled = true;
        }

        private void tsmToneZoomout_Click(object sender = null, EventArgs e = null) {
            if (mNoteHeightIdx < ToneHeights.Length - 1) {
                mNoteHeightIdx++;
                mNoteHeight = ToneHeights[mNoteHeightIdx];
                setDispNoteCount();
            }
            if (mNoteHeightIdx == ToneHeights.Length - 1) {
                tsmToneZoomout.Enabled = false;
            }
            tsmToneZoom.Enabled = true;
        }

        private void tsbWriteSelect_Click(object sender, EventArgs e = null) {
            tsbWrite.Checked = false;
            tsbWrite.Image = Properties.Resources.write_disable;
            tsbSelect.Checked = false;
            tsbSelect.Image = Properties.Resources.select_disable;
            tsbMultiSelect.Checked = false;
            tsbMultiSelect.Image = Properties.Resources.select_multi_disable;

            var obj = (ToolStripButton)sender;
            obj.Checked = true;

            switch (obj.Name) {
            case "tsbWrite":
                tsbWrite.Image = Properties.Resources.write;
                break;
            case "tsbSelect":
                tsbSelect.Image = Properties.Resources.select;
                break;
            case "tsbMultiSelect":
                tsbMultiSelect.Image = Properties.Resources.select_multi;
                break;
            }

            mEditor.ClearSelected();
            mSelectState = E_SELECT.NONE;
        }

        private void tsmEditMode_Click(object sender, EventArgs e = null) {
            tsmEditModeNote.Checked = false;
            tsmEditModeAccent.Checked = false;
            tsmEditModeVol.Checked = false;
            tsmEditModeExp.Checked = false;
            tsmEditModePan.Checked = false;
            tsmEditModePitch.Checked = false;
            tsmEditModeInst.Checked = false;
            tsmEditModePreset.Checked = false;
            tsmEditModeFc.Checked = false;
            tsmEditModeFq.Checked = false;
            tsmEditModeAttack.Checked = false;
            tsmEditModeRelease.Checked = false;
            tsmEditModeVib.Checked = false;
            tsmEditModeVibDep.Checked = false;
            tsmEditModeVibRate.Checked = false;
            tsmEditModeVibDelay.Checked = false;
            tsmEditModeRev.Checked = false;
            tsmEditModeDel.Checked = false;
            tsmEditModeDelDep.Checked = false;
            tsmEditModeDelTime.Checked = false;
            tsmEditModeCho.Checked = false;
            tsmEditModeTempo.Checked = false;
            tsmEditModeMeasure.Checked = false;

            var obj = (ToolStripMenuItem)sender;
            obj.Checked = true;
            tsdEditMode.Image = obj.Image;
            tsdEditMode.ToolTipText = string.Format("入力種別({0})", obj.Text);

            tsmEditModeInst.Checked
                = tsmEditModePreset.Checked
                | tsmEditModeFc.Checked
                | tsmEditModeFq.Checked
                | tsmEditModeAttack.Checked
                | tsmEditModeRelease.Checked;
            tsmEditModeVib.Checked
                = tsmEditModeVibDep.Checked
                | tsmEditModeVibRate.Checked
                | tsmEditModeVibDelay.Checked;
            tsmEditModeDel.Checked
                = tsmEditModeDelDep.Checked
                | tsmEditModeDelTime.Checked;

            mSelectState = E_SELECT.NONE;
        }

        private void tsmTick_Click(object sender, EventArgs e = null) {
            tsmTick480.Checked = false;
            tsmTick240.Checked = false;
            tsmTick120.Checked = false;
            tsmTick060.Checked = false;

            tsmTick320.Checked = false;
            tsmTick160.Checked = false;
            tsmTick080.Checked = false;
            tsmTick040.Checked = false;

            tsmTick192.Checked = false;
            tsmTick096.Checked = false;
            tsmTick048.Checked = false;
            tsmTick024.Checked = false;

            var obj = (ToolStripMenuItem)sender;
            obj.Checked = true;
            tsdTimeDiv.Image = obj.Image;
            tsdTimeDiv.ToolTipText = string.Format("入力単位({0})", obj.Text);

            EventEditor.TickSnap = Snaps[obj.Text];
            hScroll.LargeChange = EventEditor.TickSnap;
            hScroll.SmallChange = EventEditor.TickSnap;

            checkDiv();
        }

        void checkDiv() {
            if (0 == EventEditor.TickSnap % 24) {
                mQuarterNoteWidth = 120;
            }
            if (0 == EventEditor.TickSnap % 40) {
                mQuarterNoteWidth = 120;
            }
            if (0 == EventEditor.TickSnap % 60) {
                mQuarterNoteWidth = 96;
            }
            var divX = EventEditor.TickSnap * mQuarterNoteWidth / mTimeScale;
            if (divX < 4) {
                if (0 == EventEditor.TickSnap % 24) {
                    if (tsmTick024.Checked) {
                        tsmTick024.Checked = false;
                        tsmTick_Click(tsmTick048);
                        return;
                    }
                    if (tsmTick048.Checked) {
                        tsmTick048.Checked = false;
                        tsmTick_Click(tsmTick096);
                        return;
                    }
                }
                if (0 == EventEditor.TickSnap % 40) {
                    if (tsmTick040.Checked) {
                        tsmTick040.Checked = false;
                        tsmTick_Click(tsmTick080);
                        return;
                    }
                    if (tsmTick080.Checked) {
                        tsmTick080.Checked = false;
                        tsmTick_Click(tsmTick160);
                        return;
                    }
                }
                if (0 == EventEditor.TickSnap % 60) {
                    if (tsmTick060.Checked) {
                        tsmTick060.Checked = false;
                        tsmTick_Click(tsmTick120);
                        return;
                    }
                    if (tsmTick120.Checked) {
                        tsmTick120.Checked = false;
                        tsmTick_Click(tsmTick240);
                        return;
                    }
                }
            }
        }
        #endregion

        #region picRoll event
        private void picRoll_MouseDown(object sender, MouseEventArgs e) {
            switch (e.Button) {
            case MouseButtons.Right:
                mEditor.ClearClipBoard();
                mEditor.Delete();
                break;
            case MouseButtons.Left:
                mMouseDownTick = mSnappedTick;
                mTickBegin = mSnappedTick;
                mTickEnd = mSnappedTick;
                if (picRoll.PointToClient(Cursor.Position).Y < MeasureTabHeight) {
                    return;
                }
                if (tsmEditModeNote.Checked) {
                    mNoteBegin = mMouseNote;
                    mNoteEnd = mNoteBegin;
                    mEditor.GripNote(mMouseTick, mMouseNote, tsbMultiSelect.Checked);
                    if (mEditor.CopyMovingNote()) {
                        mSelectState = E_SELECT.NOTE_MOVE;
                        break;
                    }
                    if (mEditor.CopyExpandingNote()) {
                        mSelectState = E_SELECT.NOTE_EXPAND;
                        break;
                    }
                    if (tsbSelect.Checked || tsbMultiSelect.Checked) {
                        mSelectState = E_SELECT.NOTE_SELECT;
                    }
                    if (tsbWrite.Checked) {
                        mSelectState = E_SELECT.NOTE_WRITE;
                    }
                    mEditor.Paste(mSnappedTick);
                }
                break;
            }
        }

        private void picRoll_MouseUp(object sender, MouseEventArgs e) {
            switch (e.Button) {
            case MouseButtons.Right:
                break;
            case MouseButtons.Left:
                switch (mSelectState) {
                case E_SELECT.NOTE_MOVE:
                    mEditor.PasteMovedNote(mSnappedTick, mMouseNote);
                    mSelectState = E_SELECT.NONE;
                    break;
                case E_SELECT.NOTE_EXPAND:
                    mEditor.PasteExpandedNote(mSnappedTick);
                    mSelectState = E_SELECT.NONE;
                    break;
                case E_SELECT.NOTE_SELECT:
                case E_SELECT.NOTE_WRITE:
                    if (mTickEnd < mTickBegin) {
                        var tmp = mTickEnd;
                        mTickEnd = mTickBegin;
                        if (tsbWrite.Checked) {
                            mTickEnd += EventEditor.TickSnap;
                        }
                        mTickBegin = tmp;
                    } else {
                        mTickEnd += EventEditor.TickSnap;
                    }
                    if (mNoteEnd < mNoteBegin) {
                        var tmp = mNoteEnd;
                        mNoteEnd = mNoteBegin;
                        mNoteBegin = tmp;
                    }
                    if (E_SELECT.NOTE_SELECT == mSelectState) {
                        if (!mPressCtrl) {
                            mEditor.ClearSelected();
                        }
                        mEditor.SelectNote(mTickBegin, mNoteBegin, mTickEnd, mNoteEnd, tsbMultiSelect.Checked);
                        if (mEditor.Selected) {
                            mSelectState = E_SELECT.NOTE_SELECTED;
                        } else {
                            mSelectState = E_SELECT.NONE;
                        }
                    }
                    if (E_SELECT.NOTE_WRITE == mSelectState) {
                        mEditor.AddNote(mNoteBegin, 127, mTickBegin, mTickEnd);
                        mSelectState = E_SELECT.NONE;
                    }
                    break;
                default:
                    mSelectState = E_SELECT.NONE;
                    break;
                }
                hScroll.Maximum = mEditor.MaxTick + 960 * 4;
                break;
            }
        }

        private void picRoll_MouseMove(object sender, MouseEventArgs e) {
            var pos = picRoll.PointToClient(Cursor.Position);
            if (picRoll.Width <= pos.X) {
                pos.X = picRoll.Width - 1;
                if (hScroll.Maximum <= hScroll.Value + EventEditor.TickSnap) {
                    hScroll.Maximum += EventEditor.TickSnap;
                }
                hScroll.Value += EventEditor.TickSnap;
            }
            if (pos.X < 0) {
                pos.X = 0;
                if (0 <= hScroll.Value - EventEditor.TickSnap) {
                    hScroll.Value -= EventEditor.TickSnap;
                }
            }
            switch (mSelectState) {
            case E_SELECT.NOTE_SELECT:
            case E_SELECT.NOTE_WRITE:
            case E_SELECT.NOTE_MOVE:
            case E_SELECT.NOTE_EXPAND:
                if (picRoll.Height <= pos.Y) {
                    pos.Y = picRoll.Height - 1;
                    if (vScroll.Value < vScroll.Maximum) {
                        vScroll.Value++;
                    }
                }
                if (pos.Y < MeasureTabHeight) {
                    pos.Y = MeasureTabHeight;
                    if (vScroll.Minimum <= vScroll.Value - 1) {
                        vScroll.Value--;
                    }
                }
                break;
            }

            var snapDivX = EventEditor.TickSnap * mQuarterNoteWidth / mTimeScale;
            mCursorX = pos.X / snapDivX * snapDivX;
            mSnappedTick = getScrollTick(mCursorX);
            mMouseTick = getScrollTick(pos.X);
            var ofsY = MeasureTabHeight + mBmpRoll.Height % mNoteHeight;
            var snappedY = (pos.Y - ofsY) / mNoteHeight * mNoteHeight;
            var note = 128 + vScroll.Minimum - vScroll.Value - snappedY / mNoteHeight;
            if (note < 0) {
                note = 0;
            } else if (127 < note) {
                note = 127;
            }
            mMouseNote = note;
            switch (mSelectState) {
            case E_SELECT.NOTE_SELECT:
            case E_SELECT.NOTE_WRITE:
            case E_SELECT.NOTE_MOVE:
                if (mMouseDownTick <= mSnappedTick) {
                    mTickEnd = mSnappedTick;
                }
                if (mSnappedTick <= mMouseDownTick) {
                    mTickBegin = mSnappedTick;
                }
                mNoteEnd = mMouseNote;
                if (mNoteEnd < 0) {
                    mNoteEnd = 0;
                }
                break;
            case E_SELECT.NOTE_EXPAND:
                mSnappedTick += EventEditor.TickSnap;
                break;
            }
            if (E_SELECT.NOTE_MOVE == mSelectState || tsbWrite.Checked) {
                mNoteBegin = mNoteEnd;
            }
        }
        #endregion

        #region Drawing methods
        private void timer1_Tick(object sender = null, EventArgs e = null) {
            if (E_SELECT.NONE == mSelectState) {
                mEditor.SelectNote(mSnappedTick, mMouseNote, tsbMultiSelect.Checked);
            }

            var measureList = mEditor.GetMeasureList(mSnappedTick, mSnappedTick + 960 * 4);
            SCALE.SetKey(measureList[0].Key);

            if (mEditor.Selected) {
                var code = mEditor.SelectedChordName(mSnappedTick);
                tslKey.Text = SCALE.KeyName;
                tslDegree.Text = code[0];
                tslChord.Text = code[1] + code[2];
            } else {
                var code = mEditor.CurrentChordName(mSnappedTick);
                tslKey.Text = SCALE.KeyName;
                tslDegree.Text = code[0];
                tslChord.Text = code[1] + code[2];
            }

            var scrollTickBegin = getScrollTick();
            var scrollTickEnd = getScrollTick(mBmpRoll.Width);
            mgRoll.Clear(Colors.Roll);
            drawRoll();
            drawMeasure(scrollTickBegin, scrollTickEnd);
            drawNote(scrollTickBegin, scrollTickEnd);
            drawClipBoardNote(scrollTickBegin, scrollTickEnd);
            drawEditingNote(scrollTickBegin);
            mgRoll.DrawLine(Pens.Red, mCursorX, 0, mCursorX, mBmpRoll.Height);
            picRoll.Image = picRoll.Image;
        }

        void drawRoll() {
            var ofsY = MeasureTabHeight + mBmpRoll.Height % mNoteHeight;
            var highestNote = 128 - vScroll.Value + mDispNoteCount;
            for (int i = 0, noteNumber = highestNote; i < mDispNoteCount; i++, noteNumber--) {
                var py = mNoteHeight * i + ofsY;
                if (127 < noteNumber) {
                    continue;
                }
                switch (noteNumber % 12) {
                case 0: {
                    var name = "C" + (noteNumber / 12 - 1).ToString();
                    var fsize = mgRoll.MeasureString(name, mOctNameFont).Height - 3;
                    mgRoll.DrawString(name, mOctNameFont, Colors.Text, 0, py + mNoteHeight - fsize);
                }
                break;
                case 1:
                case 3:
                case 6:
                case 8:
                case 10:
                    mgRoll.FillRectangle(Colors.BlackKey.Brush, 0, py + 1, mBmpRoll.Width, mNoteHeight);
                    break;
                case 4:
                    mgRoll.DrawLine(Colors.BlackKey, 0, py, mBmpRoll.Width, py);
                    break;
                case 11:
                    mgRoll.DrawLine(Colors.Border, 0, py, mBmpRoll.Width, py);
                    break;
                default:
                    break;
                }
            }
        }

        void drawMeasure(int begin, int end) {
            var measureList = mEditor.GetMeasureList(begin, end);
            mgRoll.FillRectangle(Colors.MeasureTab, 0, 0, mBmpRoll.Width, MeasureTabHeight);
            mgRoll.DrawRectangle(Colors.Border, 0, 0, mBmpRoll.Width, MeasureTabHeight);
            foreach (var ev in measureList) {
                if (ev.DispTick < begin || end <= ev.DispTick) {
                    continue;
                }
                var x = (ev.DispTick - begin) * mQuarterNoteWidth / mTimeScale;
                if (ev.IsBar) {
                    mgRoll.DrawLine(Colors.Border, x, 0, x, mBmpRoll.Height);
                    mgRoll.DrawString(ev.Bar.ToString(), mMeasureFont, Colors.Text, x, -2);
                } else {
                    mgRoll.DrawLine(Colors.Beat, x, MeasureTabHeight + 1, x, mBmpRoll.Height);
                }
            }
        }

        void drawNote(int begin, int end) {
            var ofsY = MeasureTabHeight + mBmpRoll.Height % mNoteHeight;
            var mouseCursor = Cursors.Default;
            foreach (var ev in mEditor.Events) {
                if (ev.IsHide || ev.Type != E_STATUS.NOTE_ON) {
                    continue;
                }
                if (ev.End < begin || end <= ev.Begin) {
                    continue;
                }
                if (ev.TrackId == EventEditor.EditTrack || ev.Selected) {
                    continue;
                }
                var tone = 128 + vScroll.Minimum - vScroll.Value - ev.NoteNumber;
                var x1 = (ev.Begin - begin) * mQuarterNoteWidth / mTimeScale;
                var x2 = (ev.End - begin) * mQuarterNoteWidth / mTimeScale;
                var y1 = mNoteHeight * tone + ofsY;
                var y2 = mNoteHeight * (tone + 1) + ofsY;
                if (y1 < MeasureTabHeight) {
                    continue;
                }
                Colors.DrawOtherNote(mgRoll, x1, y1, x2, y2);
            }
            foreach (var ev in mEditor.Events) {
                if (ev.IsHide || ev.Type != E_STATUS.NOTE_ON) {
                    continue;
                }
                if (ev.End < begin || end <= ev.Begin) {
                    continue;
                }
                if (!(ev.TrackId == EventEditor.EditTrack || ev.Selected)) {
                    continue;
                }
                var tone = 128 + vScroll.Minimum - vScroll.Value - ev.NoteNumber;
                var x1 = (ev.Begin - begin) * mQuarterNoteWidth / mTimeScale;
                var x2 = (ev.End - begin) * mQuarterNoteWidth / mTimeScale;
                var y1 = mNoteHeight * tone + ofsY;
                var y2 = mNoteHeight * (tone + 1) + ofsY;
                if (y1 < MeasureTabHeight) {
                    continue;
                }
                if (ev.Selected) {
                    Colors.DrawSelectedNote(mgRoll, x1, y1, x2, y2);
                } else {
                    Colors.DrawNote(mgRoll, x1, y1, x2, y2);
                }
                var centerTick = ev.Begin + (ev.End - ev.Begin) * 0.75;
                if (ev.Begin <= mMouseTick && mMouseTick < centerTick && mMouseNote == ev.NoteNumber) {
                    mouseCursor = Cursors.Hand;
                }
                if (centerTick <= mMouseTick && mMouseTick < ev.End && mMouseNote == ev.NoteNumber) {
                    mouseCursor = Cursors.VSplit;
                }
            }
            if (mSelectState != E_SELECT.NOTE_WRITE && mouseCursor != mMouseCursor) {
                mMouseCursor = mouseCursor;
                Cursor = mMouseCursor;
            }
        }

        void drawClipBoardNote(int begin, int end) {
            var ofsY = MeasureTabHeight + mBmpRoll.Height % mNoteHeight;
            var expandTick = mEditor.GetExpandTick(mSnappedTick);
            var expandingNoteEnd = mCursorX;
            foreach (var ev in mEditor.ClipBoardEvents) {
                if (ev.Type != E_STATUS.NOTE_ON) {
                    continue;
                }
                var note = ev.NoteNumber;
                if (mSelectState == E_SELECT.NOTE_MOVE) {
                    note += mMouseNote;
                }
                var noteEnd = ev.End;
                var ofsTick = mSnappedTick;
                if (mSelectState == E_SELECT.NOTE_EXPAND) {
                    var noteLen = ev.End - ev.Begin;
                    ofsTick = 0;
                    if (EventEditor.TickSnap <= noteLen + expandTick) {
                        noteEnd += expandTick;
                    } else {
                        if (noteLen < EventEditor.TickSnap) {
                            noteEnd = ev.Begin + noteLen;
                        } else {
                            noteEnd = ev.Begin + EventEditor.TickSnap;
                        }
                    }
                }
                var noteBegin = ofsTick + ev.Begin;
                noteEnd += ofsTick;
                if (noteEnd < begin || end <= noteBegin) {
                    continue;
                }
                var tone = 128 + vScroll.Minimum - vScroll.Value - note;
                var x1 = (noteBegin - begin) * mQuarterNoteWidth / mTimeScale;
                var x2 = (noteEnd - begin) * mQuarterNoteWidth / mTimeScale;
                var y1 = mNoteHeight * tone + ofsY;
                var y2 = mNoteHeight * (tone + 1) + ofsY;
                if (y1 < MeasureTabHeight) {
                    continue;
                }
                if (mEditor.IsExpandingNote(ev)) {
                    expandingNoteEnd = x2;
                }
                Colors.DrawClipBoardNote(mgRoll, x1, y1, x2, y2);
            }
            mCursorX = expandingNoteEnd;
        }

        void drawEditingNote(int begin) {
            if (E_SELECT.NOTE_WRITE != mSelectState && E_SELECT.NOTE_SELECT != mSelectState) {
                return;
            }
            var ofsY = MeasureTabHeight + mBmpRoll.Height % mNoteHeight;
            var x1 = (mTickBegin - begin) * mQuarterNoteWidth / mTimeScale;
            var x2 = (mTickEnd - begin) * mQuarterNoteWidth / mTimeScale;
            if (x1 <= x2) {
                x2 += EventEditor.TickSnap * mQuarterNoteWidth / mTimeScale;
            } else {
                if (tsbWrite.Checked) {
                    x1 += EventEditor.TickSnap * mQuarterNoteWidth / mTimeScale;
                }
            }
            if (x2 < x1) {
                var tmp = x2;
                x2 = x1;
                x1 = tmp;
            }
            var y1 = (128 + vScroll.Minimum - vScroll.Value - mNoteBegin) * mNoteHeight + ofsY;
            var y2 = (128 + vScroll.Minimum - vScroll.Value - mNoteEnd) * mNoteHeight + ofsY;
            if (y1 <= y2) {
                y2 += mNoteHeight;
            } else {
                y1 += mNoteHeight;
            }
            if (y2 < y1) {
                var tmp = y2;
                y2 = y1;
                y1 = tmp;
            }
            mCursorX = x2;
            if (mSelectState == E_SELECT.NOTE_SELECT) {
                mgRoll.FillRectangle(Colors.SelectArea, x1, y1, x2 - x1, y2 - y1);
                mgRoll.DrawRectangle(Colors.SelectBorder, x1, y1, x2 - x1, y2 - y1);
            }
            if (mSelectState == E_SELECT.NOTE_WRITE) {
                Colors.DrawNote(mgRoll, x1, y1, x2, y2);
            }
        }
        #endregion
    }
}