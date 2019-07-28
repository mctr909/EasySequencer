using System.IO;

namespace MIDI {
    class Util {
        public static ushort ReadUI16(BinaryReader br) {
            return (ushort)((br.ReadByte() << 8) | br.ReadByte());
        }

        public static uint ReadUI32(BinaryReader br) {
            return (uint)((br.ReadByte() << 24) | (br.ReadByte() << 16) | (br.ReadByte() << 8) | br.ReadByte());
        }

        public static uint ReadDelta(MemoryStream ms) {
            var temp = (uint)ms.ReadByte();
            var retVal = temp & 0x7F;

            while (0x7F < temp) {
                temp = (uint)ms.ReadByte();
                retVal <<= 7;
                retVal |= temp & 0x7F;
            }

            return retVal;
        }

        public static byte[] ReadBytes(MemoryStream ms) {
            var arr = new byte[ReadDelta(ms)];
            ms.Read(arr, 0, arr.Length);
            return arr;
        }

        public static void WriteUI16(MemoryStream ms, ushort value) {
            ms.WriteByte((byte)(value >> 8));
            ms.WriteByte((byte)(value & 0xFF));
        }

        public static void WriteUI32(MemoryStream ms, uint value) {
            ms.WriteByte((byte)((value >> 24) & 0xFF));
            ms.WriteByte((byte)((value >> 16) & 0xFF));
            ms.WriteByte((byte)((value >> 8) & 0xFF));
            ms.WriteByte((byte)(value & 0xFF));
        }

        public static void WriteDelta(MemoryStream ms, uint value) {
            if (0 < (value >> 21)) {
                ms.WriteByte((byte)(0x80 | ((value >> 21) & 0x7F)));
                ms.WriteByte((byte)(0x80 | ((value >> 14) & 0x7F)));
                ms.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                ms.WriteByte((byte)(value & 0x7F));
                return;
            }

            if (0 < (value >> 14)) {
                ms.WriteByte((byte)(0x80 | ((value >> 14) & 0x7F)));
                ms.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                ms.WriteByte((byte)(value & 0x7F));
                return;
            }

            if (0 < (value >> 7)) {
                ms.WriteByte((byte)(0x80 | ((value >> 7) & 0x7F)));
                ms.WriteByte((byte)(value & 0x7F));
                return;
            }

            ms.WriteByte((byte)value);
            return;
        }
    }
}
