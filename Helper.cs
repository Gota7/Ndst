using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Ndst {

    // Extension method helper.
    public static class Helper {
        static Dictionary<string, long> Offsets = new Dictionary<string, long>();

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

        // Write a fixed length string.
        public static void WriteFixedLen(this BinaryWriter w, string str, uint len) {
            uint numToWrite = Math.Min((uint)str.Length, len);
            for (uint i = 0; i < numToWrite; i++) {
                w.Write(str[(int)i]);
            }
            w.Write0s(len - numToWrite);
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
        
    }

    public sealed class HexStringJsonConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return typeof(uint).Equals(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            writer.WriteValue($"0x{value:x}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var str = reader.ReadAsString();
            if (str == null || !str.StartsWith("0x"))
                throw new JsonSerializationException();
            return Convert.ToUInt32(str);
        }
        
    }

}