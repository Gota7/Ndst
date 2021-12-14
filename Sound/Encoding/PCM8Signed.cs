using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Ndst.Sound.Encoding {

    /// <summary>
    /// Signed 8-bit PCM audio.
    /// </summary>
    public class PCM8Signed : IAudioEncoding {

        /// <summary>
        /// Data.
        /// </summary>
        private sbyte[] Data;

        /// <summary>
        /// Number of samples contained.
        /// </summary>
        /// <returns>Number of samples.</returns>
        public int SampleCount() => Data.Length;

        /// <summary>
        /// Data size contained.
        /// </summary>
        /// <returns>Data size.</returns>
        public int DataSize() => SampleCount();

        /// <summary>
        /// Get the number of samples from a block size.
        /// </summary>
        /// <param name="blockSize">Block size to get the number of samples from.</param>
        /// <returns>Number of samples.</returns>
        public int SamplesFromBlockSize(int blockSize) => blockSize;

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
            Data = r.ReadSBytes((int)dataSize);
        }

        /// <summary>
        /// Write the raw data.
        /// </summary>
        /// <param name="w">File writer.</param>
        public void WriteRaw(BinaryWriter w) {
            foreach (var s in Data) {
                w.Write(s);
            }
        }

        /// <summary>
        /// Convert from floating point PCM to the data.
        /// </summary>
        /// <param name="pcm">PCM data.</param>
        /// <param name="encodingData">Encoding data.</param>
        /// <param name="loopStart">Loop start.</param>
        /// <param name="loopEnd">Loop end.</param>
        public void FromFloatPCM(float[] pcm, object encodingData = null, int loopStart = -1, int loopEnd = -1) {
            Data = pcm.Select(x => (sbyte)(x * sbyte.MaxValue)).ToArray();
        }

        /// <summary>
        /// Convert the data to floating point PCM.
        /// </summary>
        /// <param name="decodingData">Decoding data.</param>
        /// <returns>Floating point PCM data.</returns>
        public float[] ToFloatPCM(object decodingData = null) => Data.Select(x => (float)x / sbyte.MaxValue).ToArray();

        /// <summary>
        /// Trim audio data.
        /// </summary>
        /// <param name="totalSamples">Total number of samples to have in the end.</param>
        public void Trim(int totalSamples) {
            Data = Data.SubArray(0, totalSamples);
        }

        /// <summary>
        /// Change block size.
        /// </summary>
        /// <param name="blocks">Audio blocks.</param>
        /// <param name="newBlockSize">New block size.</param>
        /// <returns>New blocks.</returns>
        public List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize) {

            //New blocks.
            List<IAudioEncoding> newData = new List<IAudioEncoding>();

            //Get all samples.
            List<sbyte> samples = new List<sbyte>();
            foreach (var b in blocks) {
                samples.AddRange((sbyte[])b.RawData());
            }
            sbyte[] s = samples.ToArray();

            //Block size is -1.
            if (newBlockSize == -1) {
                newData.Add(new PCM8Signed() { Data = s });
            }

            //Other.
            else {
                int samplesPerBlock = newBlockSize;
                int currSample = 0;
                while (currSample < samples.Count) {
                    int numToCopy = Math.Min(samples.Count - currSample, samplesPerBlock);
                    newData.Add(new PCM8Signed() { Data = s.SubArray(currSample, numToCopy) });
                    currSample += numToCopy;
                }
            }

            //Return data.
            return newData;

        }

        /// <summary>
        /// Get a property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Retrieved property.</returns>
        public T GetProperty<T>(string propertyName) { return default; }

        /// <summary>
        /// Set a property.
        /// </summary>
        /// <typeparam name="T">Property type to set.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        public void SetProperty<T>(T value, string propertyName) {}

        /// <summary>
        /// Duplicate the audio data.
        /// </summary>
        /// <returns>A copy of the audio data.</returns>
        public IAudioEncoding Duplicate() {
            PCM8Signed ret = new PCM8Signed() { Data = new sbyte[Data.Length] };
            Array.Copy(Data, ret.Data, Data.Length);
            return ret;
        }

    }

}