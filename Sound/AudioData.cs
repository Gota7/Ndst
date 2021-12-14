using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ndst.Sound.Encoding;

namespace Ndst.Sound {
    
    /// <summary>
    /// Audio data.
    /// </summary>
    public class AudioData {

        /// <summary>
        /// Encoding type.
        /// </summary>
        public Type EncodingType {
            get {
                if (m_EncodingType != null) {
                    return m_EncodingType;
                } else if (Channels.Count() > 0) {
                    return Channels[0][0].GetType();
                } else {
                    return null;
                }
            }
            set {
                m_EncodingType = value;
            }
        }
        private Type m_EncodingType;

        /// <summary>
        /// Block size. A size of -1 means that data is not blocked and only takes up one block.
        /// </summary>
        public int BlockSize { get; private set; } = -1;

        /// <summary>
        /// Blocks samples. A size of -1 means that data is not blocked and only takes up one block.
        /// </summary>
        public int BlockSamples { get; private set; } = -1;

        /// <summary>
        /// Last block padding size.
        /// </summary>
        public int LastBlockPaddingSize;

        /// <summary>
        /// Number of samples.
        /// </summary>
        public int NumSamples => Channels.Count == 0 ? 0 : Channels[0].Select(x => x.SampleCount()).Sum();

        /// <summary>
        /// Total size of the audio data.
        /// </summary>
        public int DataSize => Channels.Count == 0 ? 0 : (Channels[0].Select(x => x.DataSize()).Sum() + LastBlockPaddingSize * (Channels.Count - 1));

        /// <summary>
        /// Get the number of blocks.
        /// </summary>
        public int NumBlocks => Channels.Count == 0 ? 0 : Channels[0].Count;

        /// <summary>
        /// Size of the last block.
        /// </summary>
        public int LastBlockSize => Channels.Count == 0 ? 0 : Channels[0].Last().DataSize();

        /// <summary>
        /// Samples in the last block.
        /// </summary>
        public int LastBlockSamples => Channels.Count == 0 ? 0 : Channels[0].Last().SampleCount();

        /// <summary>
        /// Get the size of the last block with padding.
        /// </summary>
        public int LastBlockWithPaddingSize => LastBlockSize + LastBlockPaddingSize;

        /// <summary>
        /// Channels. I only recommend using these to set and get properties.
        /// </summary>
        public List<List<IAudioEncoding>> Channels = new List<List<IAudioEncoding>>();

        /// <summary>
        /// Create a duplicate of the audio data.
        /// </summary>
        /// <returns>Duplicate audio data.</returns>
        public AudioData Duplicate() {
            AudioData a = new AudioData();
            a.EncodingType = EncodingType;
            a.BlockSize = BlockSize;
            a.BlockSamples = BlockSamples;
            a.LastBlockPaddingSize = LastBlockPaddingSize;
            a.Channels = new List<List<IAudioEncoding>>();
            for (int i = 0; i < Channels.Count; i++) {
                List<IAudioEncoding> chan = new List<IAudioEncoding>();
                for (int j = 0; j < Channels[i].Count; j++) {
                    chan.Add(Channels[i][j].Duplicate());
                }
                a.Channels.Add(chan);
            }
            return a;
        }

        /// <summary>
        /// Convert audio data to another encoding.
        /// </summary>
        /// <param name="targetEncoding">Target encoding.</param>
        /// <param name="targetBlockSize">Target block size.</param>
        /// <param name="loopStart">Loop start position.</param>
        /// <param name="loopEnd">Loop end position.</param>
        public void Convert(Type targetEncoding, int targetBlockSize, int loopStart = -1, int loopEnd = -1) {

            //Encoding type.
            EncodingType = targetEncoding;

            //For each channel.
            for (int i = 0; i < Channels.Count; i++) {

                //Decoding data.
                object decodingData = null;

                //Get samples.
                List<float> samples = new List<float>();
                foreach (var b in Channels[i]) {
                    samples.AddRange(b.ToFloatPCM(decodingData));
                }
                var s = samples.ToArray();

                //Clear blocks.
                Channels[i].Clear();

                //Temporary block.
                IAudioEncoding tmp = (IAudioEncoding)Activator.CreateInstance(targetEncoding);

                //No block size.
                if (targetBlockSize == -1) {
                    BlockSize = BlockSamples = -1;
                    tmp.FromFloatPCM(s, null, loopStart, loopEnd);
                    Channels[i].Add(tmp);
                    continue;
                }

                //Encoding data.
                object encodingData = null;

                //Get samples per block.
                BlockSize = targetBlockSize;
                int samplesPerBlock = tmp.SamplesFromBlockSize(targetBlockSize);
                BlockSamples = samplesPerBlock;
                int numBlocks = samples.Count / samplesPerBlock;
                if (samples.Count % samplesPerBlock != 0) { numBlocks++; }
                for (int j = 0; j < numBlocks; j++) {
                    int numSamples = (j == numBlocks - 1) ? (samples.Count % samplesPerBlock) : samplesPerBlock;
                    if (numSamples == 0) { numSamples = samplesPerBlock; }
                    int lS = -1, lE = -1;
                    if (loopStart != -1) {
                        if (loopStart >= samplesPerBlock * j && loopStart < samplesPerBlock * j + numSamples) {
                            lS = loopStart - samplesPerBlock * j;
                        }
                    }
                    if (loopEnd != -1) {
                        if (loopEnd >= samplesPerBlock * j && loopEnd < samplesPerBlock * j + numSamples) {
                            lE = loopEnd - samplesPerBlock * j;
                        }
                    }
                    IAudioEncoding block = (IAudioEncoding)Activator.CreateInstance(targetEncoding);
                    block.FromFloatPCM(s.SubArray(j * samplesPerBlock, numSamples), encodingData, lS, lE);
                    Channels[i].Add(block);
                }

            }

        }

        /// <summary>
        /// Change the block size.
        /// </summary>
        /// <param name="targetBlockSize">New block size to change to.</param>
        public void ChangeBlockSize(int targetBlockSize) {
            if (BlockSize == targetBlockSize) { return; }
            var tmp = (IAudioEncoding)Activator.CreateInstance(EncodingType);
            BlockSize = targetBlockSize;
            for (int i = 0; i < Channels.Count; i++) {
                Channels[i] = tmp.ChangeBlockSize(Channels[i], targetBlockSize);
            }
            if (targetBlockSize == -1) { BlockSamples = -1; } else { BlockSamples = Channels.Count == 0 ? 0 : Channels[0][0].SampleCount(); }
        }

        /// <summary>
        /// Read unblocked data.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="encodingType">Encoding type.</param>
        /// <param name="numChannels">Number of channels to read.</param>
        /// <param name="dataSize">Data size of each channel.</param>
        /// <param name="numSamples">Number of samples to read.</param>
        /// <param name="dataPadding">Padding between one channel's data and the next.</param>
        public void Read(BinaryReader r, Type encodingType, int numChannels, int dataSize, int numSamples, int dataPadding) {

            //Set properties.
            EncodingType = encodingType;
            BlockSize = -1;
            BlockSamples = -1;
            LastBlockPaddingSize = dataPadding;

            //Read channel data.
            Channels.Clear();
            for (int i = 0; i < numChannels; i++) {
                Channels.Add(new List<IAudioEncoding>());
                IAudioEncoding data = (IAudioEncoding)Activator.CreateInstance(EncodingType);
                data.ReadRaw(r, (uint)numSamples, (uint)dataSize);
                Channels.Last().Add(data);
                r.ReadBytes(dataPadding);
            }

        }

        /// <summary>
        /// Read blocked data.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <param name="encodingType">Encoding type.</param>
        /// <param name="numChannels">Number of channels to read.</param>
        /// <param name="numBlocks">Number of blocks to read.</param>
        /// <param name="blockSize">Size of each block.</param>
        /// <param name="blockSamples">Samples in each block.</param>
        /// <param name="lastBlockSize">Last block size.</param>
        /// <param name="lastBlockSamples">Last block samples.</param>
        /// <param name="lastBlockPaddingSize">Size of the padding following the last block.</param>
        public void Read(BinaryReader r, Type encodingType, int numChannels, uint numBlocks, uint blockSize, uint blockSamples, uint lastBlockSize, uint lastBlockSamples, uint lastBlockPaddingSize) {
            
            //Set properties.
            EncodingType = encodingType;
            BlockSize = (int)blockSize;
            BlockSamples = (int)blockSamples;
            LastBlockPaddingSize = (int)lastBlockPaddingSize;

            //Init channels.
            Channels.Clear();
            for (int i = 0; i < numChannels; i++) {
                Channels.Add(new List<IAudioEncoding>());
            }

            //Read standard blocks.
            for (uint i = 0; i < numBlocks - 1; i++) {
                for (int j = 0; j < numChannels; j++) {
                    IAudioEncoding data = (IAudioEncoding)Activator.CreateInstance(EncodingType);
                    data.ReadRaw(r, blockSamples, blockSize);
                    Channels[j].Add(data);
                }
            }

            //Read last block.
            for (int i = 0; i < numChannels; i++) {
                IAudioEncoding data = (IAudioEncoding)Activator.CreateInstance(EncodingType);
                data.ReadRaw(r, lastBlockSamples, lastBlockSize);
                Channels[i].Add(data);
                r.ReadBytes(LastBlockPaddingSize);
            }

        }

        /// <summary>
        /// Write audio data.
        /// </summary>
        /// <param name="w">The writer.</param>
        public void Write(BinaryWriter w) {

            //Write standard blocks.
            for (int i = 0; i < NumBlocks - 1; i++) {
                for (int j = 0; j < Channels.Count; j++) {
                    Channels[j][i].WriteRaw(w);
                }
            }
            
            //Write last block.
            for (int j = 0; j < Channels.Count; j++) {
                Channels[j].Last().WriteRaw(w);
                w.Write(new byte[LastBlockPaddingSize]);
            }

        }

        // Ok, we don't need Dsp-Adpcm SEEK stuff as no DS game uses it?
        
        /*

        /// <summary>
        /// Get seek information.
        /// </summary>
        /// <returns>Seek information.</returns>
        public byte[] GetSeek() {
            if (EncodingType != typeof(DspAdpcm)) { return null; }
            using (MemoryStream o = new MemoryStream()) {
                using (BinaryWriter w = new BinaryWriter(o)) {
                    for (int i = 0; i < NumBlocks; i++) {
                        for (int j = 0; j < Channels.Count; j++) {
                            w.Write((Channels[j][i] as DspAdpcm).Context.yn1);
                            w.Write((Channels[j][i] as DspAdpcm).Context.yn2);
                        }
                    }
                    return o.ToArray();
                }
            }
        }

        /// <summary>
        /// Set seek info.
        /// </summary>
        /// <param name="seekInfo">Seek information.</param>
        public void SetSeek(byte[] seekInfo) {
            if (EncodingType != typeof(DspAdpcm)) { return; }
            using (MemoryStream src = new MemoryStream(seekInfo)) {
                using (BinaryReader r = new BinaryReader(src)) {
                    for (int i = 0; i < NumBlocks; i++) {
                        for (int j = 0; j < Channels.Count; j++) {
                            (Channels[j][i] as DspAdpcm).Context.yn1 = r.ReadInt16();
                            (Channels[j][i] as DspAdpcm).Context.yn2 = r.ReadInt16();
                        }
                    }
                }
            }
        }

        */

        /// <summary>
        /// Trim the audio data to contain a certain number of samples.
        /// </summary>
        /// <param name="totalSamples">Number of samples to keep.</param>
        public void Trim(int totalSamples) {

            //Trim samples.
            int samplesToTrim = NumSamples - totalSamples;
            for (int i = 0; i < Channels.Count; i++) {
                int toTrim = samplesToTrim;
                while (toTrim > 0) {
                    int cutSamples = Math.Min(Channels[i].Last().SampleCount(), toTrim);
                    Channels[i].Last().Trim(cutSamples);
                    toTrim -= cutSamples;
                    if (Channels[i].Last().SampleCount() == 0) {
                        Channels[i].Remove(Channels[i].Last());
                    }
                }
            }

        }

        /// <summary>
        /// Mix the audio data to mono. This also converts the format to PCM32Float. This does not support muting everything.
        /// </summary>
        /// <param name="mutes">Channels to mute.</param>
        public void MixToMono(bool[] mutes = null) {

            //Test.
            if (Channels.Count < 2 && mutes == null) { return; }

            //Get mutes.
            if (mutes == null) {
                mutes = new bool[Channels.Count];
            }

            //Test.
            if (Channels.Count == 0 || mutes.Where(x => x == false).Count() == 0) { return; }

            //Divisor to make audio not loud.
            double divisor = 1 / Math.Sqrt(mutes.Where(x => x == false).Count());

            //Convert data.
            Convert(typeof(PCM32Float), BlockSamples);

            //Start mixing.
            List<IAudioEncoding> newData = new List<IAudioEncoding>();
            for (int i = 0; i < NumBlocks; i++) {
                List<float> samples = new List<float>();
                IAudioEncoding block = new PCM32Float();
                for (int j = 0; j < Channels[0][i].SampleCount(); j++) {
                    double sample = 0;
                    for (int k = 0; k < Channels.Count; k++) {
                        if (!mutes[k]) {
                            sample += Channels[k][i].ToFloatPCM()[j];
                        }
                    }
                    sample /= divisor;
                    if (sample > 1) { sample = 1; }
                    if (sample < -1) { sample = -1; }
                    samples.Add((float)sample);
                }
                block.FromFloatPCM(samples.ToArray());
                newData.Add(block);
            }

            //Set data.
            Channels.Clear();
            Channels.Add(newData);

        }

        /// <summary>
        /// Mix the audio data to stereo. This also converts the format to PCM32Float.
        /// </summary>
        /// <param name="mutes">Channels to mute.</param>
        /// <param name="isRightChannel">If a channel is to be on the right.</param>
        /// <param name="isBoth">If channel is both.</param>
        public void MixToStereo(bool[] mutes = null, bool[] isRightChannel = null, bool[] isBoth = null) {

            //Test.
            if (Channels.Count < 3 && mutes == null) { return; }

            //Get data.
            if (mutes == null) {
                mutes = new bool[Channels.Count];
            }
            if (isRightChannel == null) {
                isRightChannel = new bool[Channels.Count];
                for (int i = 0; i < Channels.Count; i++) {
                    if (i % 2 == 1) {
                        isRightChannel[i] = true;
                    }
                }
            }
            if (isBoth == null) {
                isBoth = new bool[Channels.Count];
                if (Channels.Count % 2 != 0) {
                    isBoth[Channels.Count - 1] = true;
                }
            }

            //Test.
            if (Channels.Count == 0 || mutes.Where(x => false).Count() == 0) { return; }

            //Organize tracks.
            List<int> lefts = new List<int>();
            List<int> rights = new List<int>();
            for (int i = 0; i < mutes.Length; i++) {
                if (!mutes[i]) {
                    if (isBoth[i]) {
                        lefts.Add(i);
                        rights.Add(i);
                    } else if (isRightChannel[i]) {
                        rights.Add(i);
                    } else {
                        lefts.Add(i);
                    }
                }
            }

            //Convert data.
            Convert(typeof(PCM32Float), BlockSamples);

            //Get divisors.
            double divL = 1 / Math.Sqrt(lefts.Count);
            double divR = 1 / Math.Sqrt(rights.Count);

            //Start mixing.
            List<IAudioEncoding> left = new List<IAudioEncoding>();
            List<IAudioEncoding> right = new List<IAudioEncoding>();
            for (int i = 0; i < NumBlocks; i++) {
                List<float> samplesL = new List<float>();
                List<float> samplesR = new List<float>();
                IAudioEncoding blockL = new PCM32Float();
                IAudioEncoding blockR = new PCM32Float();
                for (int j = 0; j < Channels[0][i].SampleCount(); j++) {
                    double sampleL = 0;
                    double sampleR = 0;
                    for (int k = 0; k < Channels.Count; k++) {
                        if (!mutes[k]) {
                            if (lefts.Contains(k)) {
                                sampleL += Channels[k][i].ToFloatPCM()[j];
                            }
                            if (rights.Contains(k)) {
                                sampleR += Channels[k][i].ToFloatPCM()[j];
                            }
                        }
                    }
                    sampleL /= divL;
                    if (sampleL > 1) { sampleL = 1; }
                    if (sampleL < -1) { sampleL = -1; }
                    samplesL.Add((float)sampleL);
                    sampleR /= divR;
                    if (sampleR > 1) { sampleR = 1; }
                    if (sampleR < -1) { sampleR = -1; }
                    samplesR.Add((float)sampleR);
                }
                blockL.FromFloatPCM(samplesL.ToArray());
                blockR.FromFloatPCM(samplesR.ToArray());
                left.Add(blockL);
                right.Add(blockR);
            }

            //Set data.
            Channels.Clear();
            Channels.Add(left);
            Channels.Add(right);

        }

    }

}