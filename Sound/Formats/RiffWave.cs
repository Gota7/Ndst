using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ndst.Sound.Encoding;
using Ndst.Sound.Riff;

namespace Ndst.Sound.Formats {

    /// <summary>
    /// A standard RIFF wave.
    /// </summary>
    public class RiffWave : SoundFile {

        /// <summary>
        /// Get the supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public override Type[] SupportedEncodings() => new Type[] { typeof(PCM16), typeof(PCM8), typeof(ImaAdpcm) };

        /// <summary>
        /// Name.
        /// </summary>
        /// <returns>The name.</returns>
        public override string Name() => "WAV";

        /// <summary>
        /// Extensions.
        /// </summary>
        /// <returns>The extensions.</returns>
        public override string[] Extensions() => new string[] { "WAV" };

        /// <summary>
        /// Description.
        /// </summary>
        /// <returns>The description.</returns>
        public override string Description() => "A standard PCM wav file.";

        /// <summary>
        /// If the file supports tracks.
        /// </summary>
        /// <returns>No, it doesn't.</returns>
        public override bool SupportsTracks() => false;

        /// <summary>
        /// The preferred encoding.
        /// </summary>
        /// <returns>Preferred encoding for the file.</returns>
        public override Type PreferredEncoding() => null;

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public RiffWave() {}

        /// <summary>
        /// Read the RIFF.
        /// </summary>
        /// <param name="r">The reader.</param>
        public override void Read(BinaryReader r) {

            //Init.
            using (RiffReader rr = new RiffReader(r.BaseStream)) {

                //Format.
                rr.OpenChunk(rr.Chunks.Where(x => x.Magic.Equals("fmt ")).FirstOrDefault());
                ushort wavDataFormat = rr.ReadUInt16();
                bool isImaAdpcm = wavDataFormat == 0x11;
                if (wavDataFormat != 1 && !isImaAdpcm) { throw new Exception("Unexpected standard WAV data format."); }
                int numChannels = rr.ReadUInt16();
                SampleRate = rr.ReadUInt32();
                rr.ReadUInt32(); //Byte rate.
                ushort blockSize = rr.ReadUInt16(); //Block size.
                ushort bitsPerSample = rr.ReadUInt16();
                LoopStart = 0;
                LoopEnd = 0;
                Loops = false;

                // PCM format.
                if (!isImaAdpcm) {
                    if (bitsPerSample != 8 && bitsPerSample != 16) { throw new Exception("This tool only accepts 8-bit or 16-bit WAV files."); }

                    //Sample.
                    var smpl = rr.Chunks.Where(x => x.Magic.Equals("smpl")).FirstOrDefault();
                    if (smpl != null) {
                        rr.OpenChunk(smpl);
                        rr.ReadUInt32s(7);
                        Loops = rr.ReadUInt32() > 0;
                        if (Loops) {
                            rr.ReadUInt32s(3);
                            LoopStart = r.ReadUInt32(); //(uint)(r.ReadUInt32() / (bitsPerSample / 8));
                            LoopEnd = r.ReadUInt32(); //(uint)(r.ReadUInt32() / (bitsPerSample / 8));
                        }
                    }

                    //Data.
                    rr.OpenChunk(rr.Chunks.Where(x => x.Magic.Equals("data")).FirstOrDefault());
                    uint dataSize = rr.Chunks.Where(x => x.Magic.Equals("data")).FirstOrDefault().Size;
                    uint numBlocks = (uint)(dataSize / numChannels / (bitsPerSample / 8));
                    r.BaseStream.Position = rr.BaseStream.Position;
                    Audio.Read(r, bitsPerSample == 16 ? typeof(PCM16) : typeof(PCM8), numChannels, numBlocks, (uint)bitsPerSample / 8, 1, (uint)bitsPerSample / 8, 1, 0);
                    Audio.ChangeBlockSize(-1);
                }

                // IMA-ADPCM format.
                else {

                    // Sanity check.
                    if (bitsPerSample != 4) { throw new Exception("This tool only accepts 4-bit IMA-ADPCM WAV files."); }

                    // More info.
                    r.ReadUInt16(); // Number of extra bytes (2).
                    r.ReadUInt16(); // TODO: FIGURE OUT WHAT THIS MEANS!!!

                    //Sample.
                    var smpl = rr.Chunks.Where(x => x.Magic.Equals("smpl")).FirstOrDefault();
                    if (smpl != null) {
                        rr.OpenChunk(smpl);
                        rr.ReadUInt32s(7);
                        Loops = rr.ReadUInt32() > 0;
                        if (Loops) {
                            rr.ReadUInt32s(3);
                            LoopStart = r.ReadUInt32(); //(uint)(r.ReadUInt32() / (bitsPerSample / 8));
                            LoopEnd = r.ReadUInt32(); //(uint)(r.ReadUInt32() / (bitsPerSample / 8));
                        }
                    }

                    // Fact chunk.
                    rr.OpenChunk(rr.Chunks.Where(x => x.Magic.Equals("fact")).FirstOrDefault());
                    uint numSamples = rr.ReadUInt32();

                    // Get data.
                    throw new NotImplementedException();

                }

            }

        }

        /// <summary>
        /// Write the RIFF.
        /// </summary>
        /// <param name="w">The writer.</param>
        public override void Write(BinaryWriter w) {

            //Init.
            using (RiffWriter rw = new RiffWriter(w.BaseStream)) {

                //Init file.
                rw.InitFile("WAVE");

                //Format block.
                rw.StartChunk("fmt ");
                rw.Write((ushort)1);
                rw.Write((ushort)Audio.Channels.Count);
                rw.Write(SampleRate);
                uint bitsPerSample = Audio.EncodingType.Equals(typeof(PCM16)) ? 16u : 8u;
                rw.Write((uint)(SampleRate * Audio.Channels.Count * (bitsPerSample / 8)));
                rw.Write((ushort)(bitsPerSample / 8 * Audio.Channels.Count));
                rw.Write((ushort)bitsPerSample);
                rw.EndChunk();

                //Sample block.
                if (Loops) {
                    rw.StartChunk("smpl");
                    rw.Write(new uint[2]);
                    rw.Write((uint)(1d / SampleRate * 1000000000));
                    rw.Write((uint)60);
                    rw.Write(new uint[3]);
                    rw.Write((uint)1);
                    rw.Write(new uint[3]);
                    rw.Write(LoopStart/* * bitsPerSample / 8*/);
                    rw.Write(LoopEnd/* * bitsPerSample / 8*/);
                    rw.Write((ulong)0);
                    rw.EndChunk();
                }

                //Data block.
                Audio.ChangeBlockSize((int)bitsPerSample / 8);
                rw.StartChunk("data");
                w.BaseStream.Position = rw.BaseStream.Position;
                Audio.Write(w);
                rw.BaseStream.Position = w.BaseStream.Position;
                while (rw.BaseStream.Position % 2 != 0) {
                    rw.Write((byte)0);
                }
                rw.EndChunk();

                //Close file.
                rw.CloseFile();
                Audio.ChangeBlockSize(-1);

            }

        }

        // Read an IMA-ADPCM block.
        private List<ImaAdpcm> ReadImaAdpcmBlock(RiffReader rr, ushort blockSize, int numChannels) {
            List<ImaAdpcm> channels = new List<ImaAdpcm>();
            List<List<byte>> channelDatas = new List<List<byte>>();
            for (int i = 0; i < numChannels; i++) {
                ImaAdpcm c = new ImaAdpcm();
                c.Sample = rr.ReadUInt16();
                c.Index = rr.ReadUInt16();
                channels.Add(c);
                channelDatas.Add(new List<byte>());
            }
            int numSampleSessions = (blockSize - (4 * numChannels)) / numChannels / 4; // First get number of actual data bytes, get how many bytes per channel, then divide by 4 to get how many sessions.
            for (int i = 0; i < numSampleSessions; i++) {
                for (int j = 0; j < numChannels; j++) {
                    channelDatas[j].AddRange(rr.ReadBytes(4));
                }
            }
            for (int i = 0; i < numChannels; i++) {
                channels[i].Data = channelDatas[i].ToArray();
            }
            return channels;
        }
        
        // Write an IMA-ADPCM block.
        private void WriteImaAdpcmBlock(RiffWriter rw, List<ImaAdpcm> channels) {
            foreach (var c in channels) {
                rw.Write(c.Sample);
                rw.Write(c.Index);
            }
            int numSampleSessions = channels[0].Data.Length / 4;
            bool spaceSessions = channels[0].Data.Length % 4 != 0;
            int currSampleSession = 0;
            foreach (var c in channels) {
                rw.Write(c.Data[currSampleSession * 4 + 0]);
                rw.Write(c.Data[currSampleSession * 4 + 1]);
                rw.Write(c.Data[currSampleSession * 4 + 2]);
                rw.Write(c.Data[currSampleSession * 4 + 3]);
                if (c == channels.Last()) currSampleSession++;
            }
            if (spaceSessions) { // Padding.
                int byteOff = currSampleSession * 4;
                int bytesRem = channels[0].Data.Length - byteOff;
                foreach (var c in channels) {
                    for (int i = 0; i < bytesRem; i++) {
                        rw.Write(c.Data[bytesRem + i]);
                    }
                    rw.Write(new byte[4 - bytesRem]);
                }
            }
        }

    }

}