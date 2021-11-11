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
                    ROM r = new ROM(args[1], args[2]);
                    r.Save(args[3]);
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
