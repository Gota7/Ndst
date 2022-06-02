using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ndst.Formats;
using Ndst.Graphics;
//using Ndst.Sound.Formats;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace Ndst {

    class Program {

        static void Main(string[] args) {
            //ROM r2 = new ROM("Shining.nds", "Convert");
            //r2.Extract("Shining");
            //NinjaBuildSystem.GenerateBuildSystem("Shining", "Patch", "Convert", "Test.nds");
            ROM r3 = new ROM("NSMBDecompressed.nds", "ConvertNSMBDecompressed");
            r3.Extract("NSMBDecompressed");
            return;
            if (args.Length < 3) {
                PrintUsage();
            } else {
                if (args[0].Equals("-e")) {
                    string conversionFolder = "";
                    string outputPath = args[2];
                    if (args.Length >= 4) {
                        conversionFolder = args[2];
                        outputPath = args[3];
                    }
                    ROM r = new ROM(args[1], conversionFolder);
                    r.Extract(outputPath);
                } else if (args[0].Equals("-p")) {
                    ROM r = new ROM(args[1]);
                    r.Save(args[2]);
                } else if (args[0].Equals("-n")) {
                    if (args.Length < 4) {
                        PrintUsage();
                        return;
                    }
                    string conversionFolder = "";
                    string outputPath = args[3];
                    if (args.Length >= 5) {
                        conversionFolder = args[3];
                        outputPath = args[4];
                    }
                    NinjaBuildSystem.GenerateBuildSystem(args[1], args[2], conversionFolder, outputPath);
                } else if (args[0].Equals("-t")) {
                    string conversion = args[1];
                    string inFile = args[2];
                    string outFile = args[3];
                    foreach (var f in Helper.FileFormats) {
                        IFormat i = (IFormat)Activator.CreateInstance(f);
                        if (i.IsOfFormat(conversion)) {
                            i.Pack(inFile);
                            using (FileStream s = new FileStream(outFile, FileMode.OpenOrCreate)) {
                                s.SetLength(0);
                                using (BinaryWriter w = new BinaryWriter(s)) {
                                    i.Write(w);
                                }
                            }
                            return;
                        }
                    }
                    throw new Exception("Invalid file conversion!");
                } else if (args[0].Equals("-c")) {
                    System.IO.File.Copy(args[1], args[2], true);
                } else {
                    PrintUsage();
                }
            }
        }

        static void PrintUsage() {
            Console.WriteLine("NDS Toolkit");
            Console.WriteLine("  c2021 Gota7\n");
            Console.WriteLine("\tUsage:");
            Console.WriteLine("\t\tNdst MODE input1 (input2) (input3) output\n");
            Console.WriteLine("\tModes:");
            Console.WriteLine("\t\t-e Extract a ROM (input1) with optional conversion folder (input2) to a folder (output).");
            Console.WriteLine("\t\t-n Generate a ninja build system for ROM folder (input1), patch folder (input2), and optional conversion folder (input3) to build ROM (output).");
            Console.WriteLine("\t\t-p Pack a ROM folder (input1) to a ROM (output).");
            Console.WriteLine("\t\t-t Use file conversion method (input1) to convert file (input2) and save as (output).");
            Console.WriteLine("\t\t-c Copy file (input1) to (output).");
        }

    }

}
