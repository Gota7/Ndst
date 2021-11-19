using System;
using System.IO;

namespace Ndst.Formats {

    // Format interface.
    public interface IFormat {
        bool IsType(byte[] data);
        void Read(BinaryReader r, byte[] rawData);
        void Write(BinaryWriter w);
        void Extract(string path);
        void Pack(string path);
        string GetFormat();
        bool IsOfFormat(string str);
        byte[] ContainedFile();
    }

    // Format tools.
    public static class FormatUtil {

        // Do conversion during extraction.
        public static IFormat DoExtractionConversion(BuildSystem b, BinaryReader r, long fileOff, string originalFilePath, byte[] file, bool parentWasLZ = false) {
            IFormat newData = null;
            foreach (var pFormat in Helper.FileFormats) {
                newData = (IFormat)Activator.CreateInstance(pFormat);
                if (newData.IsType(file)) {
                    if (newData as LZFile != null && parentWasLZ) {
                        continue;
                    }
                    r.BaseStream.Position = fileOff;
                    newData.Read(r, file);
                    byte[] containedFile = newData.ContainedFile();
                    b.AddPrebuiltEntry(originalFilePath, newData.GetFormat(), file, containedFile);
                    if (containedFile != null) {
                        using (MemoryStream src = new MemoryStream(containedFile)) {
                            BinaryReader br = new BinaryReader(src);
                            DoExtractionConversion(b, br, 0, originalFilePath, containedFile, newData as LZFile != null);
                        }
                    }
                    break;
                }
            }
            return newData;
        }

        // Convert an item.
        public static byte[] ConvertItem(string fileToConvert, string conversion) {
            foreach (var pFormat in Helper.FileFormats) {
                IFormat format = (IFormat)Activator.CreateInstance(pFormat);
                if (format.IsOfFormat(conversion)) {
                    format.Pack(fileToConvert);
                    using (MemoryStream o = new MemoryStream()) {
                        BinaryWriter w = new BinaryWriter(o);
                        format.Write(w);
                        return o.ToArray();
                    }
                }
            }
            return null;
        }

    }

}