using System;
using System.Collections.Generic;

namespace Ndst {

    // Ninja build system.
    public class NinjaBuildSystem {
        string RomPath;
        string PatchPath;
        string ConversionPath;
        Dictionary<string, string> Rules = new Dictionary<string, string>();
        Dictionary<string, List<string>> FileConversions = new Dictionary<string, List<string>>();
        List<Tuple<string, string, List<string>>> Conversions = new List<Tuple<string, string, List<string>>>();

        // New system.
        public NinjaBuildSystem(string romPath, string patchPath, string conversionPath = null) {
            RomPath = romPath;
            PatchPath = patchPath;
            ConversionPath = conversionPath;
        }

        // Add file.
        public void AddFile(string filePath) {
            FileConversions.Add(filePath, new List<string>());
        }

        // Add a single file conversion (like compression).
        public void AddFileConversion(string filePath, string conversion) {
            FileConversions[filePath].Add(conversion);
        }

        // Add a conversion that involves multiple files (like graphics).
        public void AddConversion(List<string> filesInvolved, string conversionType, string conversionFile) {
            foreach (var f in filesInvolved) {
                FileConversions[f].Add("Conversion");
            }
            Conversions.Add(new Tuple<string, string, List<string>>(conversionType, conversionFile, filesInvolved));
        }

        // Write a build system.
        public void WriteBuildSystem() {

            // Ninja.
            List<string> n = new List<string>();

            // Write rules.

            // Write conversions.

            // Write file conversions.
            foreach (var f in FileConversions) {

                // Write file conversions.
                bool isFromConversion = false;
                for (int i = 0; i < f.Value.Count; i++) {
                    string noSpacesPath = "build/" + f.Key.Replace(" ", "$ ");
                    string inPath = noSpacesPath + "." + (i - 1);
                    string outPath = noSpacesPath + "." + i;
                    string conversion = f.Value[i];
                    if (i == f.Value.Count - 1) {
                        // I really need to plan this out more...
                    } else if (i == 0) {

                    } else {

                    }
                    if (!conversion.Equals("Conversion")) {
                        n.Add("build " + inPath + ": " + outPath);
                    }

                }

                // Ndst needs to select the proper file if not from conversion.
                if (!isFromConversion) {
                    
                }
                
            }

            // Write build.
            System.IO.File.WriteAllLines("build.ninja", n);

        }

    }

}