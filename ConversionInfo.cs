using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ndst.Code;
using Ndst.Formats;
using Ndst.Graphics;
using Newtonsoft.Json;

namespace Ndst {

    // Conversion stage.
    public class ConversionStage {
        public string FromConversionFilePath;
        public string ConversionStep;
    }

    // Conversion information.
    public class ConversionInfo {
        string ConversionFolder;
        Dictionary<string, List<ConversionStage>> Files = new Dictionary<string, List<ConversionStage>>();
        public Dictionary<string, IFormat> CurrentFormats = new Dictionary<string, IFormat>();
        List<ExtractOptions> BulkConversions = new List<ExtractOptions>();
        public List<string> FilesToBulkConversions = new List<string>();

        public ConversionInfo(string conversionFolder) {
            ConversionFolder = conversionFolder;
        }

        // Add a file to convert.
        public void AddFileConversion(string filePath, string conversion, IFormat latestConversion) {
            if (!Files.ContainsKey(filePath)) {
                Files.Add(filePath, new List<ConversionStage>());
                CurrentFormats.Add(filePath, latestConversion);
            }
            Files[filePath].Add(new ConversionStage() { ConversionStep = conversion });
            CurrentFormats[filePath] = latestConversion;
        }

        // A bulk conversion.
        public void AddBulkConversion(string bulkConversionPath, string bulkConvertType) {
            ExtractOptions eo = null;
            switch (bulkConvertType) {
                case "Code":
                    eo = JsonConvert.DeserializeObject<CompilationOptions>(System.IO.File.ReadAllText(ConversionFolder + "/" + bulkConversionPath));
                    break;
                case "Graphic":
                    eo = JsonConvert.DeserializeObject<GraphicExtractOptions>(System.IO.File.ReadAllText(ConversionFolder + "/" + bulkConversionPath));
                    break;
            }
            if (eo != null) {
                BulkConversions.Add(eo);
                var filesInvolved = eo.GetFileList();
                FilesToBulkConversions.AddRange(filesInvolved);
                foreach (var f in filesInvolved) {
                    Files[f].Add(new ConversionStage() { FromConversionFilePath = bulkConversionPath });
                }
            }
        }

        // Write conversion info.
        public void WriteConversionInfo() {
            List<string> ret = new List<string>();
            foreach (var f in Files) {
                if (f.Value.Count < 1 || f.Value.Where(x => x.ConversionStep != null && x.ConversionStep.Equals("None")).Count() == f.Value.Count()) {
                    continue;
                }
                ret.Add(f.Key);
                f.Value.Reverse();
                foreach (var c in f.Value) {
                    if (c.ConversionStep != null && c.ConversionStep.Equals("None")) {
                        continue;
                    }
                    ret.Add("\t-" + (c.FromConversionFilePath == null ? (" " + c.ConversionStep) : ("* " + c.FromConversionFilePath)));
                }
                f.Value.Reverse();
            }
            System.IO.File.WriteAllLines(ConversionFolder + "/conversions.txt", ret);
        }

        // Write built files.
        public void WriteBuiltFiles(string folder) {
            foreach (var f in CurrentFormats) {
                if (f.Value != null && !FilesToBulkConversions.Contains(f.Key)) {
                    Directory.CreateDirectory(Path.GetDirectoryName(folder + "/" + f.Key));
                    f.Value.Extract(folder + "/" + f.Key);
                }
            }
            foreach (var b in BulkConversions) {
                b.ExtractFiles(folder, ConversionFolder, this);
            }
        }

    }

}