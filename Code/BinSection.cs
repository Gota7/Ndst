namespace Ndst.Code {

    // A binary section.
    public class BinSection {
        public byte[] Data;
        public uint RamAddress;
        public int BssSize;

        public BinSection(byte[] data, uint ramAddress, int bssSize) {
            Data = data;
            RamAddress = ramAddress;
            BssSize = bssSize;
        }

        public bool HasAddress(uint address) {
            return address >= RamAddress && address < RamAddress + Data.Length;
        }

        public uint ReadFromAddress(uint address) {
            uint off = address - RamAddress;
            return (uint)(Data[off] |
                Data[off + 1] << 8 |
                Data[off + 2] << 16 |
                Data[off + 3] << 24);
        }

        public void WriteToAddress(uint address, uint val) {
            uint off = address - RamAddress;
            Data[off] = (byte)val;
            Data[off + 1] = (byte)(val >> 8);
            Data[off + 2] = (byte)(val >> 16);
            Data[off + 3] = (byte)(val >> 24);
        }

    }

}