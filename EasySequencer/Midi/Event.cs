using System;

namespace MIDI {
    public struct Event {
        public readonly uint Time;
        public readonly ushort Track;
        public readonly ushort Index;
        public readonly Message Message;

        public static readonly Comparison<Event> Compare = new Comparison<Event>((a, b) => {
            var dTime = (long)a.Time - b.Time;
            if (0 == dTime) {
                var dComp = (long)((a.Track << 16) | a.Index) - ((b.Track << 16) | b.Index);
                return 0 == dComp ? 0 : (0 < dComp ? 1 : -1);
            } else {
                return 0 < dTime ? 1 : -1;
            }
        });

        public Event(uint time, ushort track, ushort index, Message message) {
            Time = time;
            Track = track;
            Index = index;
            Message = message;
        }
    }
}
