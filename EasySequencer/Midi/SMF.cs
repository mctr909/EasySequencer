using System;
using System.IO;
using System.Collections.Generic;

namespace MIDI {
    public class SMF {
        private struct Header {
            public readonly FORMAT Format;
            public ushort Tracks;
            public readonly ushort Ticks;

            public Header(FORMAT format, ushort tracks, ushort ticks) {
                Format = format;
                Tracks = tracks;
                Ticks = ticks;
            }

            public Header(BinaryReader br) {
                Util.ReadUI32(br);
                Util.ReadUI32(br);
                Format = (FORMAT)Util.ReadUI16(br);
                Tracks = Util.ReadUI16(br);
                Ticks = Util.ReadUI16(br);

                if (!Enum.IsDefined(typeof(FORMAT), Format)) {
                    Format = FORMAT.INVALID;
                }
            }

            public void Write(MemoryStream ms) {
                Util.WriteUI32(ms, 0x4D546864);
                Util.WriteUI32(ms, 6);
                Util.WriteUI16(ms, (ushort)Format);
                Util.WriteUI16(ms, Tracks);
                Util.WriteUI16(ms, Ticks);
            }
        }

        private string mPath;
        private Header mHead;
        private Dictionary<int, Track> mTracks;

        public int Ticks { get { return mHead.Ticks; } }

        public Event[] EventList {
            get {
                var hash = new HashSet<Event>();
                foreach (var tr in mTracks) {
                    foreach (var ev in tr.Value.Events) {
                        hash.Add(ev);
                    }
                }

                var evList = new Event[hash.Count];
                hash.CopyTo(evList);
                Array.Sort(evList, Event.Compare);

                return evList;
            }
        }

        public SMF(FORMAT format = FORMAT.FORMAT1, ushort ticks = 960) {
            mHead = new Header(format, 0, ticks);
            mTracks = new Dictionary<int, Track>();
        }

        public SMF(string filePath) {
            var fs = new FileStream(filePath, FileMode.Open);
            var br = new BinaryReader(fs);

            mPath = filePath;
            mHead = new Header(br);
            mTracks = new Dictionary<int, Track>();

            for (ushort i = 0; i < mHead.Tracks; ++i) {
                mTracks.Add(i, new Track(br, i));
            }

            br.Close();
        }
    }
}
