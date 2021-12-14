using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Ndst.Formats;
using System.Text;

namespace Ndst {

    // Extension method helper.
    public static class Helper {
        static Dictionary<string, long> Offsets = new Dictionary<string, long>();
        public static List<Type> FileFormats = new List<Type>() {
            typeof(LZFile),
            typeof(Narc),
            typeof(Kcl),
            typeof(Enpg),
            typeof(GenericFile)
        };

        // Read a 24-bit integer.
        public static Int24 ReadInt24(this BinaryReader r) {
            Int24 ret = new Int24();
            ret.Read(r);
            return ret;
        }

        // Read multiple signed bytes.
        public static sbyte[] ReadSBytes(this BinaryReader r, int num) {
            sbyte[] ret = new sbyte[num];
            for (int i = 0; i < num; i++) {
                ret[i] = r.ReadSByte();
            }
            return ret;
        }

        // Read multiple ints.
        public static short[] ReadInt16s(this BinaryReader r, int num) {
            short[] ret = new short[num];
            for (int i = 0; i < num; i++) {
                ret[i] = r.ReadInt16();
            }
            return ret;
        }

        // Read multiple ints.
        public static int[] ReadInt32s(this BinaryReader r, int num) {
            int[] ret = new int[num];
            for (int i = 0; i < num; i++) {
                ret[i] = r.ReadInt32();
            }
            return ret;
        }

        // Read multiple ints.
        public static uint[] ReadUInt32s(this BinaryReader r, int num) {
            uint[] ret = new uint[num];
            for (int i = 0; i < num; i++) {
                ret[i] = r.ReadUInt32();
            }
            return ret;
        }

        // Read multiple floats.
        public static float[] ReadSingles(this BinaryReader r, int num) {
            float[] ret = new float[num];
            for (int i = 0; i < num; i++) {
                ret[i] = r.ReadSingle();
            }
            return ret;
        }

        // Read a null terminated string.
        public static string ReadNullTerminated(this BinaryReader r) {
            string ret = "";
            char c = r.ReadChar();
            while (c != 0) {
                ret += c;
                c = r.ReadChar();
            }
            return ret;
        }

        // Read a fixed length string.
        public static string ReadFixedLen(this BinaryReader r, uint len) {
            string ret = "";
            for (uint i = 0; i < len; i++) {
                char c = r.ReadChar();
                if (c != 0) {
                    ret += c;
                }
            }
            return ret;
        }

        public static string ReadFixedLenWide(this BinaryReader r, uint len) {
            List<byte> ret = new List<byte>();
            for (uint i = 0; i < len * 2; i++) {
                byte b1 = r.ReadByte();
                byte b2 = r.ReadByte();
                if (b1 == 0 && b2 == 0) {
                    break;
                } else {
                    ret.Add(b1);
                    ret.Add(b2);
                }
            }
            return Encoding.Unicode.GetString(ret.ToArray());
        }

        public static Fix4x12i ReadFix4x12i(this BinaryReader r) {
            return new Fix4x12i() { Val = r.ReadInt16() };
        }

        public static Fix20x12i ReadFix20x12i(this BinaryReader r) {
            return new Fix20x12i() { Val = r.ReadInt32() };
        }

        // Write multiple ints.
        public static void Write(this BinaryWriter w, short[] data) {
            foreach (var f in data) {
                w.Write(f);
            }
        }

        // Write multiple ints.
        public static void Write(this BinaryWriter w, int[] data) {
            foreach (var f in data) {
                w.Write(f);
            }
        }

        // Write multiple ints.
        public static void Write(this BinaryWriter w, uint[] data) {
            foreach (var f in data) {
                w.Write(f);
            }
        }

        // Write multiple floats.
        public static void Write(this BinaryWriter w, float[] data) {
            foreach (var f in data) {
                w.Write(f);
            }
        }

        // Write a fixed length string.
        public static void WriteFixedLen(this BinaryWriter w, string str, uint len) {
            uint numToWrite = Math.Min((uint)str.Length, len);
            for (uint i = 0; i < numToWrite; i++) {
                w.Write(str[(int)i]);
            }
            w.Write0s(len - numToWrite);
        }

        public static void Write(this BinaryWriter w, Fix4x12i val) {
            w.Write(val.Val);
        }

        public static void Write(this BinaryWriter w, Fix20x12i val) {
            w.Write(val.Val);
        }

        // Save an offset.
        public static void SaveOffset(this BinaryWriter w, string name) {
            Offsets.Add(name, w.BaseStream.Position);
            w.Write0s(4);
        }

        // Write an offset.
        public static void WriteOffset(this BinaryWriter w, string name, long offsetBase = 0, uint valOverride = uint.MaxValue) {
            long bak = w.BaseStream.Position;
            w.BaseStream.Position = Offsets[name];
            Offsets.Remove(name);
            if (valOverride == uint.MaxValue) {
                w.Write((uint)(bak - offsetBase));
            } else {
                w.Write(valOverride);
            }
            w.BaseStream.Position = bak;
        }

        // Write data.
        public static void Write(this BinaryWriter w, IFormat data) {
            data.Write(w);
        }

        // Write 0s.
        public static void Write0s(this BinaryWriter w, uint num) {
            w.Write(new byte[num]);
        }

        // Align writer.
        public static void Align(this BinaryWriter w, uint alignment, long baseOff = 0) {
            while ((w.BaseStream.Position - baseOff) % alignment != 0) {
                w.Write((byte)0);
            }
        }

        // Read a string as a number.
        public static long ReadStringNumber(string str) {
            long val = 0;
            if (str.StartsWith("0b")) {
                str = str.Substring(2);
                val = Convert.ToInt64(str, 2);
            } else if (str.StartsWith("0x")) {
                str = str.Substring(2);
                val = Convert.ToInt64(str, 16);
            } else {
                val = Convert.ToInt64(str, 10);
            }
            return val;
        }    

        // Get the MD5 sum.
        public static byte[] Md5Sum(byte[] data) {
            byte[] hash;
            using (var md5 = System.Security.Cryptography.MD5.Create()) {
                md5.TransformFinalBlock(data, 0, data.Length);
                hash = md5.Hash;
            }
            return hash;
        }

        // Bytes array match.
        public static bool BytesMatch(byte[] a, byte[] b) {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }
            return true;
        }

        // File exists.
        public static bool ROMFileExists(string path, string srcFolder, string patchFolder) {
            return System.IO.File.Exists(srcFolder + "/" + path) || System.IO.File.Exists(patchFolder + "/" + path);
        }

        // If to use patch.
        public static bool ROMUsePatch(string path, string patchFolder) {
            return System.IO.File.Exists(patchFolder + "/" + path);
        }

        // Read an extracted ROM file.
        public static byte[] ReadROMFile(string path, string srcFolder, string patchFolder) {
            bool UsePatch() {
                return System.IO.File.Exists(patchFolder + "/" + path);
            }
            if (UsePatch()) {
                return System.IO.File.ReadAllBytes(patchFolder + "/" + path);
            } else {
                return System.IO.File.ReadAllBytes(srcFolder + "/" + path);
            }
        }

        // Read an extracted ROM file lines.
        public static string[] ReadROMLines(string path, string srcFolder, string patchFolder) {
            bool UsePatch() {
                return System.IO.File.Exists(patchFolder + "/" + path);
            }
            if (UsePatch()) {
                return System.IO.File.ReadAllLines(patchFolder + "/" + path);
            } else {
                return System.IO.File.ReadAllLines(srcFolder + "/" + path);
            }
        }

        // Read extracted text.
        public static string ReadROMText(string path, string srcFolder, string patchFolder) {
            bool UsePatch() {
                return System.IO.File.Exists(patchFolder + "/" + path);
            }
            if (UsePatch()) {
                return System.IO.File.ReadAllText(patchFolder + "/" + path);
            } else {
                return System.IO.File.ReadAllText(srcFolder + "/" + path);
            }
        }

        // Write a ROM file.
        public static void WriteROMFile(string path, string patchFolder, byte[] file) {
            // TODO: CREATE DIR!!!
        }

        // Get a reader.
        public static BinaryReader GetReader(string filePath) {
            FileStream s = new FileStream(filePath, FileMode.OpenOrCreate);
            return new BinaryReader(s);
        }

        // Get a reader.
        public static BinaryReader GetReader(byte[] bin) {
            MemoryStream s = new MemoryStream(bin);
            return new BinaryReader(s);
        }

        // Get a writer.
        public static BinaryWriter GetWriter(string filePath) {
            FileStream s = new FileStream(filePath, FileMode.OpenOrCreate);
            s.SetLength(0);
            return new BinaryWriter(s);
        }

        // Get bytes.
        public delegate void WriteFunction(BinaryWriter w);
        public static byte[] GetBytes(WriteFunction func) {
            using (MemoryStream o = new MemoryStream()) {
                using (BinaryWriter w = new BinaryWriter(o)) {
                    func(w);
                }
                return o.ToArray();
            }
        }
        
    }

    public sealed class HexStringJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(uint).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue($"0x{value:x}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var str = (string)reader.Value;
            if (str == null)
                throw new JsonSerializationException();
            long val = Helper.ReadStringNumber(str);
            var ret = Convert.ChangeType(val, objectType);
            return ret;
        }
        
    }

}