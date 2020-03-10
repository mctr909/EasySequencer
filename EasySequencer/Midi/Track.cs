using System.IO;
using System.Collections.Generic;

namespace MIDI {
    public class Track {
        public readonly ushort No;
        public List<Event> Events { get; private set; }

        public Track(ushort no) {
            No = no;
            Events = new List<Event>();
        }

        public Track(BinaryReader br, ushort no) {
            No = no;
            Events = new List<Event>();

            Util.ReadUI32(br);
            int size = (int)Util.ReadUI32(br);

            var ms = new MemoryStream(br.ReadBytes(size), false);
            uint time = 0;
            byte currentStatus = 0;
            while (ms.Position < ms.Length) {
                var delta = Util.ReadDelta(ms);
                time += delta;
                Events.Add(new Event(ms, time, ref currentStatus));
            }
        }

        public void Write(Stream str) {
            var temp = new MemoryStream();
            Util.WriteUI32(temp, 0x4D54726B);
            Util.WriteUI32(temp, 0);
            uint currentTime = 0;
            foreach (var ev in Events) {
                Util.WriteDelta(temp, ev.Time - currentTime);
                ev.WriteMessage(temp);
                currentTime = ev.Time;
            }
            temp.Seek(4, SeekOrigin.Begin);
            Util.WriteUI32(temp, (uint)(temp.Length - 8));
            temp.WriteTo(str);
        }

        public List<Event> Copy(uint beginTime, uint endTime, byte lowNote, byte highNote) {
            return getTargetList(beginTime, endTime, lowNote, highNote);
        }

        public List<Event> Cut(uint beginTime, uint endTime, byte lowNote, byte highNote) {
            var notTarget = new List<Event>();
            var target = getTargetList(beginTime, endTime, lowNote, highNote, notTarget);
            Events.Clear();
            Events.AddRange(notTarget);
            return target;
        }

        public List<Event> Past(List<Event> eventList) {
            var overlapList = getOverlapList(eventList);
            if (0 == overlapList.Count) {
                Events.AddRange(eventList);
                Events.Sort(Event.Compare);
            }
            return overlapList;
        }

        private List<Event> getOverlapList(List<Event> eventList) {
            var overlapList = new List<Event>();
            for (int c = 0; c < eventList.Count; ++c) {
                var checkEv = eventList[c];
                var checkTime = checkEv.Time;
                if (E_EVENT_TYPE.NOTE_ON != checkEv.Type && E_EVENT_TYPE.NOTE_OFF != checkEv.Type) {
                    continue;
                }

                for (int i = 0; i < Events.Count; ++i) {
                    var curEv = Events[i];
                    var curTime = curEv.Time;
                    uint beginTime = 0xFFFFFFFF;
                    uint endTime = 0;
                    switch (curEv.Type) {
                    case E_EVENT_TYPE.NOTE_ON:
                        if (checkEv.NoteNo != curEv.NoteNo) {
                            break;
                        }
                        beginTime = curTime;
                        for (int j = i; j < Events.Count; ++j) {
                            var nxEv = Events[j];
                            var nxTime = nxEv.Time;
                            if (E_EVENT_TYPE.NOTE_OFF == nxEv.Type && curEv.NoteNo == nxEv.NoteNo) {
                                endTime = nxTime;
                                break;
                            }
                        }
                        break;
                    case E_EVENT_TYPE.NOTE_OFF:
                        if (checkEv.NoteNo != curEv.NoteNo) {
                            break;
                        }
                        endTime = curTime;
                        for (int j = i; 0 <= j; --j) {
                            var bkEv = Events[j];
                            var bkTime = Events[j].Time;
                            if (E_EVENT_TYPE.NOTE_ON == bkEv.Type && curEv.NoteNo == bkEv.NoteNo) {
                                beginTime = bkTime;
                                break;
                            }
                        }
                        break;
                    }
                    if (E_EVENT_TYPE.NOTE_ON == checkEv.Type) {
                        if (beginTime <= checkTime && checkTime < endTime) {
                            overlapList.Add(checkEv);
                            break;
                        }
                    } else {
                        if (beginTime < checkTime && checkTime <= endTime) {
                            overlapList.Add(checkEv);
                            break;
                        }
                    }
                }
            }
            return overlapList;
        }

        private List<Event> getTargetList(uint beginTime, uint endTime, byte lowNote, byte highNote, List<Event> notTargetList = null) {
            var targetList = new List<Event>();
            for (int i = 0; i < Events.Count; ++i) {
                var checkEv = Events[i];
                var checkTime = checkEv.Time;
                var isTarget = false;
                if (beginTime <= checkTime && checkTime <= endTime) {
                    switch (checkEv.Type) {
                    case E_EVENT_TYPE.NOTE_ON:
                        if (checkEv.NoteNo < lowNote || highNote < checkEv.NoteNo) {
                            break;
                        }
                        for (int j = i; j < Events.Count; ++j) {
                            var nxEv = Events[j];
                            var nxTime = nxEv.Time;
                            if (E_EVENT_TYPE.NOTE_OFF == nxEv.Type && checkEv.NoteNo == nxEv.NoteNo) {
                                if (beginTime <= nxTime && nxTime <= endTime) {
                                    isTarget = true;
                                    break;
                                }
                            }
                        }
                        break;
                    case E_EVENT_TYPE.NOTE_OFF:
                        if (checkEv.NoteNo < lowNote || highNote < checkEv.NoteNo) {
                            break;
                        }
                        for (int j = i; 0 <= j; --j) {
                            var bkEv = Events[j];
                            var bkTime = bkEv.Time;
                            if (E_EVENT_TYPE.NOTE_ON == bkEv.Type && checkEv.NoteNo == bkEv.NoteNo) {
                                if (beginTime <= bkTime && bkTime <= endTime) {
                                    isTarget = true;
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        isTarget = true;
                        break;
                    }
                }
                if (isTarget) {
                    targetList.Add(checkEv);
                } else {
                    if (null != notTargetList) {
                        notTargetList.Add(checkEv);
                    }
                }
            }
            return targetList;
        }
    }
}
