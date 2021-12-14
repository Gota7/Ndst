using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ndst {

    /// <summary>
    /// Signed 24-bit integer.
    /// </summary>
    public struct Int24 {

        /// <summary>
        /// Max value.
        /// </summary>
        public const int MaxValue = 8388607;

        /// <summary>
        /// Min value.
        /// </summary>
        public const int MinValue = -8388608;

        /// <summary>
        /// Data.
        /// </summary>
        private byte[] Data;

        /// <summary>
        /// Check for null.
        /// </summary>
        private void NullCheck() {
            if (Data == null) {
                Data = new byte[3];
            }
        }

        /// <summary>
        /// Get this as an int.
        /// </summary>
        /// <returns>This as an int.</returns>
        private int GetInt() {
            NullCheck();
            int ret = 0;
            ret |= Data[2];
            ret |= (Data[1] << 8);
            ret |= ((Data[0] & 0x7F) << 16);
            if ((Data[0] & 0x80) > 0) { ret = MinValue + ret; }
            return ret;
        }

        /// <summary>
        /// Convert an into to an Int24.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>Value as an Int24.</returns>
        private static Int24 FromInt(int val) {
            Int24 ret = new Int24();
            ret.NullCheck();
            if (val > MaxValue) { val = MaxValue; }
            if (val < MinValue) { val = MinValue; }
            uint un = (uint)val;
            if (val < 0) { un = (uint)(val - MinValue); }
            ret.Data[2] = (byte)((un >> 0) & 0xFF);
            ret.Data[1] = (byte)((un >> 8) & 0xFF);
            ret.Data[0] = (byte)((un >> 16) & 0x7F);
            if (val < 0) { ret.Data[0] |= 0x80; }
            return ret;
        }

        /// <summary>
        /// Get this as an int.
        /// </summary>
        /// <param name="val">Value.</param>

        public static implicit operator int(Int24 val) => val.GetInt();

        /// <summary>
        /// Convert from an int.
        /// </summary>
        /// <param name="val">Value.</param>
        public static explicit operator Int24(int val) => Int24.FromInt(val);

        /// <summary>
        /// Convert from a uint.
        /// </summary>
        /// <param name="val">Value.</param>
        public static explicit operator Int24(uint val) => Int24.FromInt((int)val);

        /// <summary>
        /// Convert from a float.
        /// </summary>
        /// <param name="val">Value.</param>
        public static explicit operator Int24(float val) => Int24.FromInt((int)val);

        /// <summary>
        /// Read the data.
        /// </summary>
        /// <param name="r">The reader.</param>
        public void Read(BinaryReader r) {
            NullCheck();
            Data[2] = r.ReadByte();
            Data[1] = r.ReadByte();
            Data[0] = r.ReadByte();
        }

        /// <summary>
        /// Write the data.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(BinaryWriter w) {
            NullCheck();
            w.Write(Data[2]);
            w.Write(Data[1]);
            w.Write(Data[0]);
        }

    }

}