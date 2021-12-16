using System.Collections.Generic;
using System.IO;
using Ndst.Formats;
using Ndst.Graphics;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace Ndst {

    // DS banner. See https://problemkaputt.de/gbatek.htm#dscartridgeicontitle
    public class Banner : IFormat {
        [JsonIgnore]
        public Screen Icon;
        public Dictionary<string, string> Title;
        public bool HasChineseTitle;
        public bool HasKoreanTitle;
        public bool IsDSIAnimated; // Currently not supported, I'll get to it eventually.
        [JsonIgnore]
        public Screen AnimatedFrames;

        // Banner languages.
        public static readonly List<string> BANNER_LANGUAGES = new List<string>() {
            "Japanese",
            "English",
            "French",
            "German",
            "Italian",
            "Spanish",
            "Chinese",
            "Korean",
            "Unused_1",
            "Unused_2",
            "Unused_3",
            "Unused_4",
            "Unused_5",
            "Unused_6",
            "Unused_7",
        };

        // Is type.
        public bool IsType(byte[] data) {
            return false;
        }
        
        // Read banner.
        public void Read(BinaryReader r, byte[] rawData) {

            // Get version.
            ushort version = r.ReadUInt16();
            if (!ROM.BANNER_LENGTHS.ContainsKey(version)) {
                version = 1;
            }
            HasChineseTitle = version >= 2;
            HasKoreanTitle = version >= 3;
            IsDSIAnimated = version >= 0x103;
            
            // CRC info.
            r.ReadBytes(4 * 2); // CRCs.
            r.ReadBytes(0x16); // Reserved.

            // 0x200 size graphic.
            Graphic graphic = new Graphic();
            graphic.Read(r, 4, 4, true, true);

            // 0x20 size palette.
            Palette palette = new Palette();
            palette.Read(r, 0x20);
            graphic.Palette = palette;
            Icon = Screen.GenerateDefault(graphic);

            // Read all the titles.
            int numTitles = 6;
            if (HasChineseTitle) numTitles = 7;
            if (HasKoreanTitle) numTitles = 15;
            Title = new Dictionary<string, string>();
            for (int i = 0; i < numTitles; i++) {
                Title.Add(BANNER_LANGUAGES[i], r.ReadFixedLenWide(0x80));
            }
            for (int i = numTitles; i < 15; i++) {
                if (i == 15) break;
                Title.Add(BANNER_LANGUAGES[i], "");
            }

            // TODO: FRAME ANIMATIONS!!!

        }

        // Write a banner.
        public void Write(BinaryWriter w) {

        }

        public byte[] RawData() {
            return null;
        }

        public void Extract(string path) {

            // Get folder.
            string folder = path.Split('.')[0];
            Directory.CreateDirectory(folder);
            System.IO.File.WriteAllText(folder + "/banner.json", JsonConvert.SerializeObject(this, Formatting.Indented));
            Icon.ToImage().SaveAsPng(folder + "/icon.png");

        }

        public void Pack(string path) {

            // Get folder.
            string folder = path.Split('.')[0];

        }

        public string GetFormat() {
            return "Banner";
        }

        public bool IsOfFormat(string str) {
            return str.Equals("Banner");
        }

        public byte[] ContainedFile() {
            return null;
        }

        public string GetPathExtension() => "";

    }

}