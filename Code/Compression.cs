namespace Ndst.Code {

    // Code compression.
    public static class Compression {

        // Decompress an overlay.
        public static byte[] DecompressOverlay(byte[] sourcedata) {
            uint DataVar1, DataVar2;
            //uint last 8-5 bytes
            DataVar1 = (uint)(sourcedata[sourcedata.Length - 8] | (sourcedata[sourcedata.Length - 7] << 8) | (sourcedata[sourcedata.Length - 6] << 16) | (sourcedata[sourcedata.Length - 5] << 24));
            //uint last 4 bytes
            DataVar2 = (uint)(sourcedata[sourcedata.Length - 4] | (sourcedata[sourcedata.Length - 3] << 8) | (sourcedata[sourcedata.Length - 2] << 16) | (sourcedata[sourcedata.Length - 1] << 24));

            byte[] memory = new byte[sourcedata.Length + DataVar2];
            sourcedata.CopyTo(memory, 0);

            uint r0, r1, r2, r3, r5, r6, r7, r12;
            bool N, V;
            r0 = (uint)sourcedata.Length;

            if (r0 == 0) {
                return null;
            }
            r1 = DataVar1;
            r2 = DataVar2;
            r2 = r0 + r2; //length + datavar2 -> decompressed length
            r3 = r0 - (r1 >> 0x18); //delete the latest 3 bits??
            r1 &= 0xFFFFFF; //save the latest 3 bits
            r1 = r0 - r1;
        a958:
            if (r3 <= r1) { //if r1 is 0 they will be equal
                goto a9B8; //return the memory buffer
            }
            r3 -= 1;
            r5 = memory[r3];
            r6 = 8;
        a968:
            SubS(out r6, r6, 1, out N, out V);
            if (N != V) {
                goto a958;
            }
            if ((r5 & 0x80) != 0) {
                goto a984;
            }
            r3 -= 1;
            r0 = memory[r3];
            r2 -= 1;
            memory[r2] = (byte)r0;
            goto a9AC;
        a984:
            r3 -= 1;
            r12 = memory[r3];
            r3 -= 1;
            r7 = memory[r3];
            r7 |= (r12 << 8);
            r7 &= 0xFFF;
            r7 += 2;
            r12 += 0x20;
        a99C:
            r0 = memory[r2 + r7];
            r2 -= 1;
            memory[r2] = (byte)r0;
            SubS(out r12, r12, 0x10, out N, out V);
            if (N == V) {
                goto a99C;
            }
        a9AC:
            r5 <<= 1;
            if (r3 > r1) {
                goto a968;
            }
        a9B8:
            return memory;
        }

        private static void SubS(out uint dest, uint v1, uint v2, out bool N, out bool V) {
            dest = v1 - v2;
            N = (dest & 2147483648) != 0;
            V = ((((v1 & 2147483648) != 0) && ((v2 & 2147483648) == 0) && ((dest & 2147483648) == 0)) || ((v1 & 2147483648) == 0) && ((v2 & 2147483648) != 0) && ((dest & 2147483648) != 0));
        }

    }

}