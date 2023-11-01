using System.Collections.Generic;
using SMF;

namespace EasySequencer {
    struct DrawEvent {
        public bool Selected;
        public bool IsHide;
        public int Channel;
        public int TrackId;
        public int Begin;
        public int End;
        public Meta Meta;
        public int[] Data;

        public E_STATUS Type {
            get { return (E_STATUS)((Data[0] < 0xF0) ? (Data[0] & 0xF0) : Data[0]); }
        }
        public int NoteNumber {
            get { return Data[1]; }
            set { Data[1] = value; }
        }
        public E_CONTROL CtrlType {
            get { return (E_CONTROL)Data[1]; }
        }
        public int Velocity {
            get { return Data[2]; }
            set { Data[2] = value; }
        }
        public int Value {
            get { return Data[2]; }
            set { Data[2] = value; }
        }

        public DrawEvent(int track, int channel, int begin, int end, int noteNum, int velocity) {
            Channel = channel;
            TrackId = track;
            Begin = begin;
            End = end;
            Selected = false;
            Data = new int[] {
                (byte)E_STATUS.NOTE_ON | (byte)(channel & 0xF),
                noteNum,
                velocity
            };
            Meta = null;
            IsHide = false;
        }
        public DrawEvent(int track, int channel, int tick, E_CONTROL type, int value) {
            Channel = channel;
            TrackId = track;
            Begin = tick;
            End = -1;
            Selected = false;
            Data = new int[] {
                (byte)E_STATUS.CONTROL | (byte)(channel & 0xF),
                (byte)type,
                (byte)value
            };
            Meta = null;
            IsHide = false;
        }
        public DrawEvent(Event ev) {
            Channel = ev.Channel;
            TrackId = ev.Track;
            Begin = ev.Tick;
            End = -1;
            Selected = false;
            Data = new int[ev.Data.Length];
            for (int i = 0; i < ev.Data.Length; i++) {
                Data[i] = ev.Data[i];
            }
            if (ev.Data[0] == (byte)E_STATUS.META) {
                Meta = new Meta(ev.Data);
            } else {
                Meta = null;
            }
            IsHide = false;
        }
        public DrawEvent(DrawEvent ev) {
            Channel = ev.Channel;
            TrackId = ev.TrackId;
            Begin = ev.Begin;
            End = ev.End;
            Selected = false;
            Data = new int[ev.Data.Length];
            for (int i = 0; i < ev.Data.Length; i++) {
                Data[i] = ev.Data[i];
            }
            if (ev.Data[0] == (byte)E_STATUS.META) {
                Meta = new Meta(ev.Meta.Data);
            } else {
                Meta = null;
            }
            IsHide = false;
        }
    }

    struct DrawMeasure {
        public bool IsBar;
        public int DispTick;
        public int EventTick;
        public int Bar;
        public int Beat;
        public int Nume;
        public int Deno;
        public E_KEY Key;
    }

    class EventEditor {
        public static int EditTrack = 5;
        public static int EditChannel = 0;
        public static int TickSnap = 240;

        public DrawEvent[] Events { get { return mEvents.ToArray(); } }
        public DrawEvent[] ClipBoardEvents { get { return mClipBoardEvents.ToArray(); } }

        public int MaxTick {
            get {
                var maxTick = 0;
                foreach (var ev in mEvents) {
                    if (maxTick < ev.Begin) {
                        maxTick = ev.Begin;
                    }
                    if (maxTick < ev.End) {
                        maxTick = ev.End;
                    }
                }
                return maxTick;
            }
        }

        public bool Selected { get; private set; } = false;

        List<DrawEvent> mEvents = new List<DrawEvent>();
        List<DrawEvent> mClipBoardEvents = new List<DrawEvent>();
        DrawEvent mMovingNote = new DrawEvent() {
            Data = new int[1]
        };
        DrawEvent mExpandingNote = new DrawEvent() {
            Data = new int[1]
        };

        void addNote(DrawEvent evNote) {
            var reservedNote = new DrawEvent();
            reservedNote.Begin = int.MaxValue;
            foreach (var ev in mEvents) {
                if (ev.TrackId != evNote.TrackId || E_STATUS.NOTE_ON != ev.Type || evNote.NoteNumber != ev.Data[1]) {
                    continue;
                }
                if (ev.Begin <= evNote.End && evNote.Begin < ev.End) {
                    if (ev.Begin < reservedNote.Begin) {
                        reservedNote = ev;
                    }
                }
            }
            if (int.MaxValue == reservedNote.Begin) {
                mEvents.Add(evNote);
            } else {
                if (evNote.Begin < reservedNote.Begin) {
                    mEvents.Add(new DrawEvent(
                        evNote.TrackId, evNote.Channel,
                        evNote.Begin, reservedNote.Begin,
                        evNote.NoteNumber, evNote.Velocity
                    ));
                }
                if (reservedNote.Begin < evNote.Begin) {
                    for (int i = 0; i < mEvents.Count; i++) {
                        var ev = mEvents[i];
                        if (ev.Begin == reservedNote.Begin && ev.NoteNumber == reservedNote.NoteNumber) {
                            ev.End = evNote.Begin;
                            mEvents[i] = ev;
                            break;
                        }
                    }
                    mEvents.Add(evNote);
                }
            }
        }

        void copy(int tickOffset, int noteOffset) {
            mClipBoardEvents.Clear();
            for (int i = 0; i < mEvents.Count; i++) {
                var ev = mEvents[i];
                if (ev.Selected) {
                    ev.Begin -= tickOffset;
                    if (ev.Type == E_STATUS.NOTE_ON) {
                        ev.NoteNumber -= noteOffset;
                        ev.End -= tickOffset;
                    }
                    mClipBoardEvents.Add(ev);
                }
            }
        }

        void paste(int tickOffset, int noteOffset, int diffTick) {
            // controls
            for (int i = 0; i < mClipBoardEvents.Count; i++) {
                var ev = mClipBoardEvents[i];
                if (ev.Type == E_STATUS.NOTE_ON) {
                    continue;
                }
                ev.Begin += tickOffset;
                mEvents.Add(new DrawEvent(ev));
            }
            // notes
            for (int i = 0; i < mClipBoardEvents.Count; i++) {
                var ev = mClipBoardEvents[i];
                if (ev.Type != E_STATUS.NOTE_ON) {
                    continue;
                }
                var noteEnd = ev.End;
                var noteLen = ev.End - ev.Begin;
                if (TickSnap <= noteLen + diffTick) {
                    noteEnd += diffTick;
                } else {
                    if (noteLen < TickSnap) {
                        noteEnd = ev.Begin + noteLen;
                    } else {
                        noteEnd = ev.Begin + TickSnap;
                    }
                }
                ev.NoteNumber += noteOffset;
                ev.Begin += tickOffset;
                ev.End = noteEnd + tickOffset;
                addNote(new DrawEvent(ev));
            }
        }

        public int GetExpandTick(int cursorTick) {
            return cursorTick - mExpandingNote.End;
        }

        public bool IsExpandingNote(DrawEvent ev) {
            return mExpandingNote.Type == E_STATUS.NOTE_ON &&
                ev.Channel == mExpandingNote.Channel &&
                ev.NoteNumber == mExpandingNote.NoteNumber &&
                ev.Begin == mExpandingNote.Begin &&
                ev.End == mExpandingNote.End;
        }

        public string[] SelectedChordName(int currentTick) {
            var notes = new List<int>();
            foreach (var ev in mEvents) {
                if (!ev.Selected || ev.Type != E_STATUS.NOTE_ON) {
                    continue;
                }
                notes.Add(ev.NoteNumber);
            }
            if (notes.Count < 2) {
                return CurrentChordName(currentTick);
            } else {
                return ChordHelper.GetName(notes.ToArray());
            }
        }
        public string[] CurrentChordName(int currentTick) {
            var notes = new List<int>();
            foreach (var ev in mEvents) {
                if (ev.Type != E_STATUS.NOTE_ON) {
                    continue;
                }
                if (ev.Begin <= currentTick && currentTick < ev.End) {
                    notes.Add(ev.NoteNumber);
                }
            }
            return ChordHelper.GetName(notes.ToArray());
        }

        public void AddNote(int note, int velocity, int tickBegin, int tickEnd) {
            addNote(new DrawEvent(EditTrack, EditChannel, tickBegin, tickEnd, note, velocity));
        }

        public void SelectNote(int tickBegin, int noteBegin, int tickEnd, int noteEnd, bool allTrack = false) {
            for (int i = 0; i < mEvents.Count; i++) {
                var ev = mEvents[i];
                if (ev.IsHide || (!allTrack && EditTrack != ev.TrackId)) {
                    continue;
                }
                if (E_STATUS.NOTE_ON == ev.Type) {
                    if (noteBegin <= ev.NoteNumber && ev.NoteNumber <= noteEnd
                        && tickBegin <= ev.Begin && ev.Begin < tickEnd) {
                        ev.Selected = true;
                        Selected = true;
                    }
                } else {
                    if (tickBegin <= ev.Begin && ev.Begin < tickEnd) {
                        ev.Selected = true;
                        Selected = true;
                    }
                }
                mEvents[i] = ev;
            }
        }
        public void SelectNote(int tick, int note, bool allTrack = false) {
            Selected = false;
            for (int i = 0; i < mEvents.Count; i++) {
                var ev = mEvents[i];
                if (ev.IsHide || (!allTrack && EditTrack != ev.TrackId)) {
                    continue;
                }
                ev.Selected = false;
                if (E_STATUS.NOTE_ON == ev.Type
                    && note == ev.NoteNumber
                    && ev.Begin <= tick && tick < ev.End) {
                    ev.Selected = true;
                    Selected = true;
                }
                mEvents[i] = ev;
            }
        }
        public void GripNote(int tick, int note, bool allTrack = false) {
            mMovingNote = new DrawEvent() {
                Data = new int[1]
            };
            mExpandingNote = new DrawEvent() {
                Data = new int[1]
            };
            foreach (var ev in mEvents) {
                if (ev.IsHide || (!allTrack && EditTrack != ev.TrackId)) {
                    continue;
                }
                if (E_STATUS.NOTE_ON != ev.Type || note != ev.NoteNumber) {
                    continue;
                }
                var centerTick = ev.Begin + (ev.End - ev.Begin) * 0.75;
                if (0 == mMovingNote.Data[0] && ev.Begin <= tick && tick < centerTick) {
                    mMovingNote = ev;
                }
                if (0 == mExpandingNote.Data[0] && centerTick <= tick && tick < ev.End) {
                    mExpandingNote = ev;
                }
            }
        }

        public void Copy(int tickOffset, int noteOffset = 0) {
            copy(tickOffset, noteOffset);
        }
        public bool CopyMovingNote() {
            if (mMovingNote.Type == E_STATUS.NOTE_ON) {
                copy(mMovingNote.Begin, mMovingNote.NoteNumber);
                Delete();
                return true;
            } else {
                return false;
            }
        }
        public bool CopyExpandingNote() {
            if (mExpandingNote.Type == E_STATUS.NOTE_ON) {
                copy(0, 0);
                Delete();
                return true;
            } else {
                return false;
            }
        }

        public void Paste(int tickOffset) {
            paste(tickOffset, 0, 0);
        }
        public void PasteMovedNote(int tickOffset, int noteOffset) {
            if (mMovingNote.Type == E_STATUS.NOTE_ON) {
                paste(tickOffset, noteOffset, 0);
                mClipBoardEvents.Clear();
            }
        }
        public void PasteExpandedNote(int tickOffset) {
            if (mExpandingNote.Type == E_STATUS.NOTE_ON) {
                paste(0, 0, tickOffset - mExpandingNote.End);
                mClipBoardEvents.Clear();
            }
        }

        public void Delete() {
            var tmp = new List<DrawEvent>();
            foreach (var ev in mEvents) {
                if (!ev.Selected) {
                    tmp.Add(ev);
                }
            }
            mEvents.Clear();
            mEvents.AddRange(tmp);
        }

        public void ClearSelected() {
            for (int i = 0; i < mEvents.Count; i++) {
                var ev = mEvents[i];
                ev.Selected = false;
                mEvents[i] = ev;
            }
            Selected = false;
        }

        public void ClearClipBoard() {
            mClipBoardEvents.Clear();
        }

        public void LoadSMF(string path) {
            mEvents.Clear();
            if (!System.IO.File.Exists(path)) {
                return;
            }
            var smf = new SMF.File(path);
            LoadEvent(smf.EventList);
        }

        public void LoadEvent(Event[] eventList) {
            mEvents.Clear();
            var noteList = new List<DrawEvent>();
            for (int idxEv = 0; idxEv < eventList.Length; idxEv++) {
                var ev = eventList[idxEv];
                switch (ev.Type) {
                case E_STATUS.NOTE_OFF:
                    for (int idxNote = 0; idxNote < noteList.Count; idxNote++) {
                        var note = noteList[idxNote];
                        if (note.TrackId == ev.Track && note.NoteNumber == ev.Data[1]) {
                            note.End = ev.Tick;
                            mEvents.Add(note);
                            noteList.RemoveAt(idxNote);
                            idxNote--;
                        }
                    }
                    break;
                case E_STATUS.NOTE_ON:
                    noteList.Add(new DrawEvent(ev));
                    break;
                default:
                    mEvents.Add(new DrawEvent(ev));
                    break;
                }
            }
        }

        public List<DrawMeasure> GetMeasureList(int tickBegin, int tickEnd) {
            var measureDeno = 4;
            var measureNume = 4;
            var measureTick = 0;
            var measureNum = 1;
            var measureInterval = 3840;
            var beatInterval = 960;
            var eventTick = 0;
            var key = E_KEY.C_MAJOR;
            var measureList = new List<DrawMeasure>();
            foreach (var ev in mEvents) {
                if (E_STATUS.META != ev.Type) {
                    continue;
                }
                if (E_META.KEY == ev.Meta.Type) {
                    key = (E_KEY)ev.Meta.UInt;
                    continue;
                }
                if (E_META.MEASURE != ev.Meta.Type) {
                    continue;
                }
                for (; measureTick < ev.Begin; measureTick += measureInterval) {
                    for (int beat = 1, beatTick = 0; beatTick < measureInterval; beat++, beatTick += beatInterval) {
                        var tick = measureTick + beatTick;
                        if (tickBegin <= tick && tick < tickEnd) {
                            var measure = new DrawMeasure();
                            measure.IsBar = beatTick == 0;
                            measure.DispTick = tick;
                            measure.EventTick = eventTick;
                            measure.Bar = measureNum;
                            measure.Beat = beat;
                            measure.Deno = measureDeno;
                            measure.Nume = measureNume;
                            measure.Key = key;
                            measureList.Add(measure);
                        }
                    }
                    measureNum++;
                }
                var m = new Mesure(ev.Meta.UInt);
                measureDeno = m.denominator;
                measureNume = m.numerator;
                measureInterval = 3840 * measureNume / measureDeno;
                beatInterval = 3840 / measureDeno;
                eventTick = ev.Begin;
            }
            for (; measureTick <= tickEnd; measureTick += measureInterval) {
                for (int beat = 1, beatTick = 0; beatTick < measureInterval; beat++, beatTick += beatInterval) {
                    var tick = measureTick + beatTick;
                    if (tickBegin <= tick && tick < tickEnd) {
                        var measure = new DrawMeasure();
                        measure.IsBar = beatTick == 0;
                        measure.DispTick = tick;
                        measure.EventTick = eventTick;
                        measure.Bar = measureNum;
                        measure.Beat = beat;
                        measure.Deno = measureDeno;
                        measure.Nume = measureNume;
                        measure.Key = key;
                        measureList.Add(measure);
                    }
                }
                measureNum++;
            }
            return measureList;
        }
    }
}
