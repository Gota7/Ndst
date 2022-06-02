using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Ndst.Code {

    // Arm9 patch info.
    public class Arm9 {
        public List<BinSection> Sections = new List<BinSection>();

        // Load the ARM9 info given a ROM and patch path.
        public void Load(string romPath, string patchPath) {
            
            // Load the ARM9.
            ROM rom = JsonConvert.DeserializeObject<ROM>(Helper.ReadROMText("__ROM__/header.json", romPath, patchPath));
            var r = Helper.GetReader(Helper.ReadROMFile("__ROM__/arm9.bin", romPath, patchPath));

            // Get code settings offset.
            long codeSettingsOff = -1;
            for (int i = 0; i < r.BaseStream.Length; i += 4) {
                uint a = r.ReadUInt32();
                uint b = r.ReadUInt32();
                r.BaseStream.Position -= 4;
                if (a == 0xDEC00621 && b == 0x2106C0DE) {
                    codeSettingsOff = i - 0x1C;
                }
            }

            // Get compressed.
            r.BaseStream.Position = codeSettingsOff + 0x14;
            bool isCompressed = r.ReadUInt32() > 0;
            if (isCompressed) {
                // TODO!!!
            }

            // Load sections.
            Sections = new List<BinSection>();
            r.BaseStream.Position = codeSettingsOff;
            uint copyTableBegin = r.ReadUInt32() - rom.Arm9LoadAddress;
            uint copyTableEnd = r.ReadUInt32() - rom.Arm9LoadAddress;
            uint dataBegin = r.ReadUInt32() - rom.Arm9LoadAddress;
            r.BaseStream.Position = copyTableBegin;
            for (int i = 0; i < (copyTableEnd - copyTableBegin) / 0xC; i++) {
                uint start = r.ReadUInt32();
                uint size = r.ReadUInt32();
                uint bssSize = r.ReadUInt32();
                AddSection(r, start, (int)size, (int)bssSize, dataBegin);
                dataBegin += size;
            }

        }

        public void AddSection(BinaryReader r, uint ramAddress, int ramLen, int bssSize, uint dataBegin) {
            long bak = r.BaseStream.Position;
            r.BaseStream.Position = dataBegin;
            Sections.Add(new BinSection(r.ReadBytes(ramLen), ramAddress, bssSize));
            r.BaseStream.Position = bak;
        }

    }

}