using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Ndst {

    // Utilities for file conversions that "depend" on other conversions.
    public class BuildSystem {
        public Dictionary<string, List<BuildSystemEntry>> Entries = new Dictionary<string, List<BuildSystemEntry>>();
        public delegate byte[] ConvertFile(string fileToConvert, string format);

        // Load build system.
        public void Load(string srcPath, string patchPath) {
            Entries = new Dictionary<string, List<BuildSystemEntry>>();
            string currEntry = null;
            foreach (var l in Helper.ReadROMLines("__ROM__/build.txt", srcPath, patchPath)) {
                string s = l.Replace("\t", "");
                if (s.StartsWith("-")) {
                    s = s.Substring(1);
                    Entries[currEntry].Add(new BuildSystemEntry() { SrcPath = currEntry, Conversion = s });
                } else {
                    currEntry = s;
                    Entries.Add(s, new List<BuildSystemEntry>());
                }
            }
        }

        // Add an entry.
        public void AddEntry(string pathToConvert, string conversion) {
            if (conversion.Equals("")) return;
            if (!Entries.ContainsKey(pathToConvert)) {
                Entries.Add(pathToConvert, new List<BuildSystemEntry>());
            }
            Entries[pathToConvert].Add(new BuildSystemEntry() {
                SrcPath = pathToConvert,
                Conversion = conversion
            });
        }

        // Add a prebuild entry.
        public void AddPrebuiltEntry(string pathToConvert, string conversion, byte[] preconverted, byte[] fileToConvert) {
            
            // Add entries.
            if (conversion.Equals("")) return;
            if (!Entries.ContainsKey(pathToConvert)) {
                Entries.Add(pathToConvert, new List<BuildSystemEntry>());
            }
            Entries[pathToConvert].Add(new BuildSystemEntry() {
                SrcPath = pathToConvert,
                Conversion = conversion,
                Preconverted = preconverted,
                ToConvert = fileToConvert
            });

        }

        // Write build system.
        public void Write(string srcPath, string patchPath, bool isOriginalExtraction = true) {
            string destPath = isOriginalExtraction ? (srcPath + "/__ROM__/build.txt") : (patchPath + "/__ROM__/build.txt");
            List<string> ret = new List<string>();
            foreach (var p in Entries) {
                ret.Add(p.Key);
                foreach (var i in p.Value) {
                    ret.Add("\t-" + i.Conversion);
                }
            }
            System.IO.File.WriteAllLines(destPath, ret);
        }

        // Write prebuilt items.
        public void WritePrebuiltItems(string srcPath) {

            // Each entry.
            foreach (var e in Entries) {
                foreach (var i in e.Value) {
                    
                    // Make sure valid.
                    if (i.Preconverted == null || i.ToConvert == null) {
                        continue;
                    }
                    
                    // Get paths.
                    int stage = e.Value.IndexOf(i);
                    string md5Path = srcPath + "/__ROM__/build/" + i.SrcPath + "." + stage + ".MD5";
                    string preconvertedPath = srcPath + "/__ROM__/build/" + i.SrcPath + "." + stage;

                    // Write data.
                    Directory.CreateDirectory(Path.GetDirectoryName(md5Path));
                    System.IO.File.WriteAllBytes(md5Path, Helper.Md5Sum(i.ToConvert));
                    System.IO.File.WriteAllBytes(preconvertedPath, i.Preconverted);

                }
            }

        }

        // Build items.
        public void BuildItems(string srcPath, string patchPath, ConvertFile conversionFunc) {

            // Run for each file.
            foreach (var file in Entries.Keys) {
                
                // Items.
                var items = Entries[file];

                // Build items.
                int convNum = 0;
                for (int i = items.Count - 1; i >= 0; i--) {

                    // Check if it needs to be built.
                    if (StageNeedsBuilt(items[i], i, srcPath, patchPath, i == items.Count - 1)) {
                        StageBuild(items[i], i, srcPath, patchPath, i == items.Count - 1, conversionFunc);
                    }

                    // Increase conversion number.
                    convNum++;

                }

            }

        }

        // If a stage needs to be built.
        private static bool StageNeedsBuilt(BuildSystemEntry e, int stage, string srcPath, string patchPath, bool first) {
            
            // Must build if file doesn't exist.
            if (!Helper.ROMFileExists("__ROM__/build/" + e.SrcPath + "." + stage, srcPath, patchPath)) {
                return true;
            }

            // Must build if MD5 doesn't exist.
            string md5Path = "__ROM__/build/" + e.SrcPath + "." + stage + ".MD5";
            if (!Helper.ROMFileExists(md5Path, srcPath, patchPath)) {
                return true;
            }

            // Must build if MD5 of source file doesn't match.
            string toConvertPath = first ? e.SrcPath : ("__ROM__/build/" + e.SrcPath + "." + (stage + 1));
            if (!Helper.BytesMatch(Helper.Md5Sum(Helper.ReadROMFile(toConvertPath, srcPath, patchPath)), Helper.ReadROMFile(md5Path, srcPath, patchPath))) {
                return true;
            }

            // Don't build.
            return false;
            
        }

        // Build a stage.
        private static void StageBuild(BuildSystemEntry e, int stage, string srcPath, string patchPath, bool first, ConvertFile conversionFunc) {

            // Get file to build.
            string toConvertPath = first ? e.SrcPath : ("__ROM__/build/" + e.SrcPath + "." + (stage + 1));
            bool isPatch = Helper.ROMUsePatch(e.SrcPath, patchPath);

            // Build the file.
            byte[] md5 = Helper.Md5Sum(Helper.ReadROMFile(toConvertPath, srcPath, patchPath));
            byte[] converted = conversionFunc((isPatch ? (patchPath + "/") : (srcPath + "/")) + toConvertPath, e.Conversion);

            // Get paths.
            string md5Path = (isPatch ? patchPath : srcPath) + "/__ROM__/build/" + e.SrcPath + "." + stage + ".MD5";
            string buildPath = (isPatch ? patchPath : srcPath) + "/__ROM__/build/" + e.SrcPath + "." + stage;

            // Write converted files.
            Directory.CreateDirectory(Path.GetDirectoryName(md5Path));
            System.IO.File.WriteAllBytes(md5Path, md5);
            System.IO.File.WriteAllBytes(buildPath, converted);

        }

    }

    // Build system entry.
    public class BuildSystemEntry {
        public string SrcPath;
        public string Conversion; // Conversion information.
        [JsonIgnore]
        public byte[] Preconverted;
        [JsonIgnore]
        public byte[] ToConvert;
    }
    
}