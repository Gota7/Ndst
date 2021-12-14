using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ndst.Sound.Encoding {

    /// <summary>
    /// Interactive Multimedia Association ADPCM.
    /// </summary>
    public class ImaAdpcm : IAudioEncoding {

        /// <summary>
        /// Data.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Last sample.
        /// </summary>
        public int Sample;

        /// <summary>
        /// Last index.
        /// </summary>
        public int Index;

        /// <summary>
        /// Number of samples contained.
        /// </summary>
        /// <returns>Number of samples.</returns>
        public int SampleCount() => Data.Length * 2;

        /// <summary>
        /// Data size contained.
        /// </summary>
        /// <returns>Data size.</returns>
        public int DataSize() => Data.Length + 4;

        /// <summary>
        /// Get the number of samples from a block size.
        /// </summary>
        /// <param name="blockSize">Block size to get the number of samples from.</param>
        /// <returns>Number of samples.</returns>
        public int SamplesFromBlockSize(int blockSize) => (blockSize - 4) * 2;

        /// <summary>
        /// Raw data.
        /// </summary>
        /// <returns>Raw data.</returns>
        public object RawData() => Data;

        /// <summary>
        /// Read the raw data.
        /// </summary>
        /// <param name="r">File reader.</param>
        /// <param name="numSamples">Number of samples.</param>
        /// <param name="dataSize">Data size.</param>
        public void ReadRaw(BinaryReader r, uint numSamples, uint dataSize) {
            Sample = r.ReadInt16();
            Index = r.ReadInt16();
            Data = r.ReadBytes((int)(dataSize - 4));
        }

        /// <summary>
        /// Write the raw data.
        /// </summary>
        /// <param name="w">File writer.</param>
        public void WriteRaw(BinaryWriter w) {
            w.Write((short)Sample);
            w.Write((short)Index);
            w.Write(Data);
        }

        /// <summary>
        /// Convert from floating point PCM to the data.
        /// </summary>
        /// <param name="pcm">PCM data.</param>
        /// <param name="encodingData">Encoding data.</param>
        /// <param name="loopStart">Loop start.</param>
        /// <param name="loopEnd">Loop end.</param>
        public void FromFloatPCM(float[] pcm, object encodingData = null, int loopStart = -1, int loopEnd = -1) {
            ImaAdpcmEncoder e = new ImaAdpcmEncoder(pcm, out Sample, out Index);
            Data = e.Encode();
        }

        /// <summary>
        /// Convert the data to floating point PCM.
        /// </summary>
        /// <param name="decodingData">Decoding data.</param>
        /// <returns>Floating point PCM data.</returns>
        public float[] ToFloatPCM(object decodingData = null) {
            ImaAdpcmDecoder d = new ImaAdpcmDecoder(Sample, Index, Data, 0);
            return d.Decode();
        }

        /// <summary>
        /// Trim audio data.
        /// </summary>
        /// <param name="totalSamples">Total number of samples to have in the end.</param>
        public void Trim(int totalSamples) {
            Data = Data.SubArray(0, totalSamples / 2 + (totalSamples % 2 == 1 ? 1 : 0));
        }

        /// <summary>
        /// Change block size.
        /// </summary>
        /// <param name="blocks">Audio blocks.</param>
        /// <param name="newBlockSize">New block size.</param>
        /// <returns>New blocks.</returns>
        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize) {

            //There is no better way.
            AudioData a = new AudioData() { Channels = new List<List<IAudioEncoding>>() { blocks } };
            a.Convert(typeof(ImaAdpcm), newBlockSize);
            return a.Channels[0];

        }

        /// <summary>
        /// Get a property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Retrieved property.</returns>
        public T GetProperty<T>(string propertyName) {
            if (propertyName.ToLower().Equals("sample")) {
                return (T)(object)Sample;
            } else if (propertyName.ToLower().Equals("index")) {
                return (T)(object)Index;
            }
            return default;
        }

        /// <summary>
        /// Set a property.
        /// </summary>
        /// <typeparam name="T">Property type to set.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        public void SetProperty<T>(T value, string propertyName) {
            if (propertyName.ToLower().Equals("sample")) {
                Sample = (int)(object)value;
            } else if (propertyName.ToLower().Equals("index")) {
                Index = (int)(object)value;
            }
        }

        /// <summary>
        /// Duplicate the audio data.
        /// </summary>
        /// <returns>A copy of the audio data.</returns>
        public IAudioEncoding Duplicate() {
            ImaAdpcm ret = new ImaAdpcm() { Data = new byte[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            ret.Sample = Sample;
            ret.Index = Index;
            return ret;
        }

    }

    /// <summary>
    /// IMA-ADPCM Math.
    /// </summary>
    public class ImaAdpcmMath {

        /// <summary>
        /// Index table.
        /// </summary>
        public static readonly int[] IndexTable = new int[16]
        {
      -1,
      -1,
      -1,
      -1,
      2,
      4,
      6,
      8,
      -1,
      -1,
      -1,
      -1,
      2,
      4,
      6,
      8
        };

        /// <summary>
        /// Step table.
        /// </summary>
        public static readonly int[] StepTable = new int[89]
        {
      7,
      8,
      9,
      10,
      11,
      12,
      13,
      14,
      16,
      17,
      19,
      21,
      23,
      25,
      28,
      31,
      34,
      37,
      41,
      45,
      50,
      55,
      60,
      66,
      73,
      80,
      88,
      97,
      107,
      118,
      130,
      143,
      157,
      173,
      190,
      209,
      230,
      253,
      279,
      307,
      337,
      371,
      408,
      449,
      494,
      544,
      598,
      658,
      724,
      796,
      876,
      963,
      1060,
      1166,
      1282,
      1411,
      1552,
      1707,
      1878,
      2066,
      2272,
      2499,
      2749,
      3024,
      3327,
      3660,
      4026,
      4428,
      4871,
      5358,
      5894,
      6484,
      7132,
      7845,
      8630,
      9493,
      10442,
      11487,
      12635,
      13899,
      15289,
      16818,
      18500,
      20350,
      22385,
      24623,
      27086,
      29794,
      short.MaxValue
        };

        /// <summary>
        /// Clamp a sample value.
        /// </summary>
        /// <param name="value">Value to clamp.</param>
        /// <returns>Clamped sample value.</returns>
        public static short ClampSample(int value) {
            if (value < -32767)
                value = -32767;
            if (value > (int)short.MaxValue)
                value = (int)short.MaxValue;
            return (short)value;
        }

        /// <summary>
        /// Clamp an index value.
        /// </summary>
        /// <param name="value">Value to clamp.</param>
        /// <returns>Clamped value.</returns>
        public static int ClampIndex(int value) {
            if (value < 0)
                value = 0;
            if (value > 88)
                value = 88;
            return value;
        }

    }

    /// <summary>
    /// IMA-ADPCM Decoder.
    /// </summary>
    public class ImaAdpcmDecoder {

        /// <summary>
        /// Last sample.
        /// </summary>
        public int Sample;

        /// <summary>
        /// Last index.
        /// </summary>
        public int Index;

        /// <summary>
        /// Offset into the data.
        /// </summary>
        public int Offset;

        /// <summary>
        /// If to decode the 2nd nibble.
        /// </summary>
        public bool SecondNibble;
        
        /// <summary>
        /// Data to read.
        /// </summary>
        private byte[] Data;

        /// <summary>
        /// Create an IMA-ADPCM decoder.
        /// </summary>
        /// <param name="sample">Last sample.</param>
        /// <param name="index">Last index.</param>
        /// <param name="data">Data to decode.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="secondNibble">If to get the 2nd nibble value.</param>
        public ImaAdpcmDecoder(int sample, int index, byte[] data, int offset = 0, bool secondNibble = false) {
            Sample = sample;
            Index = index;
            Data = data;
            Offset = offset;
            SecondNibble = secondNibble;
        }

        /// <summary>
        /// Convert data to float array.
        /// </summary>
        /// <returns>Data as a float.</returns>
        public float[] Decode() {
            List<float> ret = new List<float>();
            while (Offset < Data.Length) {
                ret.Add((float)GetSample() / short.MaxValue);
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Get a sample.
        /// </summary>
        /// <returns>The sample.</returns>
        private short GetSample() {
            short sample = this.GetSample((byte)((int)Data[Offset] >> (SecondNibble ? 4 : 0) & 15));
            if (this.SecondNibble)
                ++this.Offset;
            SecondNibble = !SecondNibble;
            return sample;
        }

        /// <summary>
        /// Get a sample from a nibble.
        /// </summary>
        /// <param name="nibble">Nibble to decode.</param>
        /// <returns>Decoded sample.</returns>
        private short GetSample(byte nibble) {
            Sample = ImaAdpcmMath.ClampSample(Sample + (ImaAdpcmMath.StepTable[Index] / 8 + ImaAdpcmMath.StepTable[Index] / 4 * ((int)nibble & 1) + ImaAdpcmMath.StepTable[Index] / 2 * ((int)nibble >> 1 & 1) + ImaAdpcmMath.StepTable[Index] * ((int)nibble >> 2 & 1)) * (((int)nibble >> 3 & 1) == 1 ? -1 : 1));
            Index = ImaAdpcmMath.ClampIndex(Index + ImaAdpcmMath.IndexTable[(int)nibble & 7]);
            return (short)Sample;
        }

    }

    /// <summary>
    /// IMA-ADPCM Encoder.
    /// </summary>
    public class ImaAdpcmEncoder {

        /// <summary>
        /// Last sample.
        /// </summary>
        private int Sample;

        /// <summary>
        /// Index.
        /// </summary>
        private int Index;

        /// <summary>
        /// Data to encode.
        /// </summary>
        private float[] Data;

        /// <summary>
        /// Initialize the encoder.
        /// </summary>
        /// <param name="data">Data to encode.</param>
        /// <param name="sample">Last sample.</param>
        /// <param name="index">Last index.</param>
        public ImaAdpcmEncoder(float[] data, out int sample, out int index) {
            Sample = sample = ConvertFloat(data[0]);
            Index = index = GetBestTableIndex((ConvertFloat(data[1]) - ConvertFloat(data[0])) * 8);
            Data = data;
        }

        /// <summary>
        /// Encode audio data.
        /// </summary>
        /// <returns>Encoded audio data.</returns>
        public byte[] Encode() {
            byte[] data = new byte[Data.Length / 2 + (Data.Length % 2 != 0 ? 1 : 0)];
            bool secondNibble = false;
            int dataPtr = 0;
            for (int i = 0; i < Data.Length; i++) {
                int config = GetBestConfig(Index, ConvertFloat(Data[i]) - Sample);
                Sample = ImaAdpcmMath.ClampSample(Sample + (ImaAdpcmMath.StepTable[Index] / 8 + ImaAdpcmMath.StepTable[Index] / 4 * (config & 1) + ImaAdpcmMath.StepTable[Index] / 2 * (config >> 1 & 1) + ImaAdpcmMath.StepTable[Index] * (config >> 2 & 1)) * ((config >> 3 & 1) == 1 ? -1 : 1));
                Index = ImaAdpcmMath.ClampIndex(this.Index + ImaAdpcmMath.IndexTable[config & 7]);
                if (!secondNibble) {
                    data[dataPtr] |= (byte)((config & 0xF) << 0);
                } else {
                    data[dataPtr] |= (byte)((config & 0xF) << 4);
                    dataPtr++;
                }
                secondNibble = !secondNibble;
            }
            return data;
        }

        /// <summary>
        /// Convert a float to a short sample.
        /// </summary>
        /// <param name="sample">Sample to convert.</param>
        /// <returns>Converted sample.</returns>
        private short ConvertFloat(float sample) => (short)(sample * short.MaxValue);

        /// <summary>
        /// Get the best tabl index.
        /// </summary>
        /// <param name="diff">Difference from last sample.</param>
        /// <returns></returns>
        private int GetBestTableIndex(int diff) {
            int num1 = int.MaxValue;
            int num2 = -1;
            for (int index = 0; index < ImaAdpcmMath.StepTable.Length; ++index) {
                int num3 = Math.Abs(Math.Abs(diff) - ImaAdpcmMath.StepTable[index]);
                if (num3 < num1) {
                    num1 = num3;
                    num2 = index;
                }
            }
            return num2;
        }

        /// <summary>
        /// Get best configuration.
        /// </summary>
        /// <param name="index">Step table index.</param>
        /// <param name="diff">Difference from last sample.</param>
        /// <returns>Best configuration.</returns>
        private int GetBestConfig(int index, int diff) {
            int num1 = 0;
            if (diff < 0)
                num1 |= 8;
            diff = Math.Abs(diff);
            int num2 = ImaAdpcmMath.StepTable[index] / 8;
            if (Math.Abs(num2 - diff) >= ImaAdpcmMath.StepTable[index]) {
                num1 |= 4;
                num2 += ImaAdpcmMath.StepTable[index];
            }
            if (Math.Abs(num2 - diff) >= ImaAdpcmMath.StepTable[index] / 2) {
                num1 |= 2;
                num2 += ImaAdpcmMath.StepTable[index] / 2;
            }
            if (Math.Abs(num2 - diff) >= ImaAdpcmMath.StepTable[index] / 4) {
                num1 |= 1;
                int num3 = num2 + ImaAdpcmMath.StepTable[index] / 4;
            }
            return num1;
        }

    }

}