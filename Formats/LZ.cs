using System;
using System.IO;

namespace Ndst.Formats {

    // An LZ compressed file.
    public class LZFile : IFormat {
        public bool HasHeader;
        public byte[] CompressedData;

        public bool IsType(byte[] data) {
            if (data.Length > 4) {
                if (data[0] == 'L' && data[1] == 'Z' && data[2] == '7' && data[3] == '7') {
                    HasHeader = true;
                    return true;
                } else if (data[0] == 0x10) {
                    HasHeader = false;
                    byte[] testData = new byte[data.Length];
                    Array.Copy(data, testData, testData.Length);
                    try {
                        LZ77.Decompress(ref testData, false);
                        return true;
                    } catch {}
                }
            }
            return false;
        }

        public void Read(BinaryReader r, byte[] rawData) {
            CompressedData = rawData;
        }

        public void Write(BinaryWriter w) {
            w.Write(CompressedData);
        }

        public void Extract(string path) {

            // Decompress data.
            byte[] testData = new byte[CompressedData.Length];
            Array.Copy(CompressedData, testData, testData.Length);
            LZ77.Decompress(ref testData, HasHeader);
            System.IO.File.WriteAllBytes(path, testData);

        }

        public void Pack(string path) {

            // Compress data.
            byte[] data = System.IO.File.ReadAllBytes(path);
            LZ77.LZ77_Compress(ref data, HasHeader);
            CompressedData = data;

        }

        public string GetFormat() {
            return HasHeader ? "LZH" : "LZ";
        }

        public bool IsOfFormat(string str) {
            if (str.Equals("LZ")) {
                HasHeader = false;
                return true;
            } else if (str.Equals("LZH")) {
                HasHeader = true;
                return true;
            }
            return false;
        }

        public byte[] ContainedFile() {
            byte[] testData = new byte[CompressedData.Length];
            Array.Copy(CompressedData, testData, testData.Length);
            LZ77.Decompress(ref testData, HasHeader);
            return testData;
        }

    }

}