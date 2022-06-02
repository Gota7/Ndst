using System.Collections.Generic;

namespace Ndst.Code {

    public class CompilationOptions : ExtractOptions {
        public string BinaryPath;
        public bool ReplaceInsteadOfPatch;
        public string RelativeASMFolderPath;

        public override void ExtractFiles(string destRomFolder, string conversionFolder, ConversionInfo conversionInfo) {
            byte[] bin = conversionInfo.CurrentFormats[BinaryPath].RawData();
            System.IO.File.WriteAllBytes(destRomFolder + "/" + BinaryPath, bin);
        }

        public override List<string> GetFileList() {
            List<string> ret = new List<string>();
            ret.Add(BinaryPath);
            return ret;
        }

        public override void PackFiles() {
            throw new System.NotImplementedException();
        }

    }

}