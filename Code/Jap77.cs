using System;
using System.Collections.Generic;
using System.Linq;

namespace Ndst.Code {
    
    public static class Jap77 {

        // Decompress an overlay.
        public static byte[] Decompress(byte[] sourcedata) {
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

        /*public static byte[] Compress(byte[] sourceData) {
            var t = CompressData(sourceData.Reverse().ToArray(), 3, 0x1002, 18, false, true);
            List<byte> compressed = t.Item1.Reverse().ToList();
            if (compressed.Count == 0 || compressed.Count + 4 < ((compressed.Count + 3) & ~4) + 8) {

            } else {
                
            }
        }

        private static Tuple<byte[], int, int> CompressData(
            byte[] data,
            int posSubtract,
            int maxMatchDiff,
            int maxMatchLen,
            bool zerosAtEnd,
            bool searchReverse) {
            
            Tuple<int, int> CompressionSearch(int pos) {
                int matchPos, recordMatchLen = 0;
                var start = Math.Max(0, pos - maxMatchDiff);
                // Strategy: do a binary search of potential match sizes, to
                // find the longest match that exists in the data.
                var lower = 0;
                var upper = Math.Min(maxMatchLen, data.Length - pos);
                var recordMatchPos = 0;
                while (lower <= upper) {
                    // Attempt to find a match at the middle length
                    var matchLen = (lower + upper) / 2;
                    var match = data[pos::(pos  +  matchLen)];
                    if (searchReverse) {
                        matchPos = data.rfind(match, start, pos);
                    } else {
                        matchPos = data.find(match, start, pos);
                    }
                    if (matchPos == -1) {
                        // No such match -- any matches will be smaller than this
                        upper = matchLen - 1;
                    } else {
                        // Match found!
                        if (matchLen > recordMatchLen) {
                            recordMatchPos = matchPos;
                            recordMatchLen = matchLen;
                        }
                        lower = matchLen + 1;
                    }
                }
                return Tuple.Create(recordMatchPos, recordMatchLen);
            };

            var result = new List<byte>();
            var current = 0;
            var ignorableDataAmount = 0;
            var ignorableCompressedAmount = 0;
            var bestSavingsSoFar = 0;
            while (current < data.Length) {
                int blockFlags = 0;
                // We'll go back and fill in blockFlags at the end of the loop.
                var blockFlagsOffset = result.Count;
                result.Add(0);
                ignorableCompressedAmount += 1;
                foreach (var i in Enumerable.Range(0, 8)) {
                    // Not sure if this is needed. The DS probably ignores this data.
                    if (current >= data.Length) {
                        if (zerosAtEnd) {
                            result.Add(0);
                        }
                        continue;
                    }
                    var _tup_1 = CompressionSearch(current);
                    var searchPos = _tup_1.Item1;
                    var searchLen = _tup_1.Item2;
                    var searchDisp = current - searchPos - posSubtract;
                    if (searchLen > 2) {
                        // We found a big match; let's write a compressed block
                        blockFlags |= 1 << 7 - i;
                        result.Add((byte)((searchLen - 3 & 0xF) << 4 | searchDisp >> 8 & 0xF));
                        result.Add((byte)(searchDisp & 0xFF));
                        current += searchLen;
                        ignorableDataAmount += searchLen;
                        ignorableCompressedAmount += 2;
                    } else {
                        result.Add(data[current]);
                        current += 1;
                        ignorableDataAmount += 1;
                        ignorableCompressedAmount += 1;
                    }
                    var savingsNow = current - result.Count;
                    if (savingsNow > bestSavingsSoFar) {
                        ignorableDataAmount = 0;
                        ignorableCompressedAmount = 0;
                        bestSavingsSoFar = savingsNow;
                    }
                }
                result[blockFlagsOffset] = (byte)blockFlags;
            }
            return Tuple.Create(result.ToArray(), ignorableDataAmount, ignorableCompressedAmount);
        }*/
    }
}