using System;
using System.Collections.Generic;
using System.Linq;

namespace Ndst {

    class Program {

        static void Main(string[] args) {
            if (args.Length < 3) {
                PrintUsage();
            } else {
                if (args[0].Equals("-e")) {
                    ROM r = new ROM(args[1]);
                    r.Extract(args[2]);
                } else if (args[0].Equals("-p")) {
                    ROM r = new ROM(args[1]);
                    r.Save(args[2]);
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
                } else {
                    PrintUsage();
                }
            }
        }

        static void PrintUsage() {
            Console.WriteLine("NDS Toolkit");
            Console.WriteLine("  c2021 Gota7\n");
            Console.WriteLine("\tUsage:");
            Console.WriteLine("\t\tNdst MODE input1 (input2) output\n");
            Console.WriteLine("\tModes:");
            Console.WriteLine("\t\t-e Extract a ROM (input1) to a folder (output).");
            Console.WriteLine("\t\t-p Pack a ROM folder (input1) and patch folder (input2) to a ROM (output).");
        }

    }

}
