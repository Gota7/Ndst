using System.Collections.Generic;
using Gee.External.Capstone.Arm;

namespace Ndst.Code {

    // Code disassembler.
    public static class Disassmbler {

        // Disassembly a binary.
        public static List<string> DisassembleBin(byte[] bin, uint ramAddr) {
            List<string> ret = new List<string>();
            ArmDisassembleMode disassembleMode = ArmDisassembleMode.Arm;
            using (CapstoneArmDisassembler d = new CapstoneArmDisassembler(disassembleMode)) {
                d.EnableInstructionDetails = true;
                d.DisassembleSyntax = Gee.External.Capstone.DisassembleSyntax.Intel;
                foreach (var i in d.Disassemble(bin, ramAddr)) {
                    ret.Add(i.ToString());
                }
            }
            return ret;
        }

    }

}