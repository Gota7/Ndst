using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ndst.Sound.Encoding {

    /// <summary>
    /// An audio encoding.
    /// </summary>
    public interface IAudioEncoding {

        /// <summary>
        /// Number of samples contained.
        /// </summary>
        /// <returns>Number of samples.</returns>
        int SampleCount();

        /// <summary>
        /// Data size contained.
        /// </summary>
        /// <returns>Data size.</returns>
        int DataSize();

        /// <summary>
        /// Get the number of samples from a block size.
        /// </summary>
        /// <param name="blockSize">Block size to get the number of samples from.</param>
        /// <returns>Number of samples.</returns>
        int SamplesFromBlockSize(int blockSize);

        /// <summary>
        /// Raw data.
        /// </summary>
        /// <returns>Raw data.</returns>
        object RawData();

        /// <summary>
        /// Read the raw data.
        /// </summary>
        /// <param name="r">File reader.</param>
        /// <param name="numSamples">Number of samples.</param>
        /// <param name="dataSize">Data size.</param>
        void ReadRaw(BinaryReader r, uint numSamples, uint dataSize);

        /// <summary>
        /// Write the raw data.
        /// </summary>
        /// <param name="w">File writer.</param>
        void WriteRaw(BinaryWriter w);

        /// <summary>
        /// Convert from floating point PCM to the data.
        /// </summary>
        /// <param name="pcm">PCM data.</param>
        /// <param name="encodingData">Encoding data.</param>
        /// <param name="loopStart">Loop start.</param>
        /// <param name="loopEnd">Loop end.</param>
        void FromFloatPCM(float[] pcm, object encodingData = null, int loopStart = -1, int loopEnd = -1);

        /// <summary>
        /// Convert the data to floating point PCM.
        /// </summary>
        /// <param name="decodingData">Decoding data.</param>
        /// <returns>Floating point PCM data.</returns>
        float[] ToFloatPCM(object decodingData = null);

        /// <summary>
        /// Trim audio data.
        /// </summary>
        /// <param name="totalSamples">Total number of samples to have in the end.</param>
        void Trim(int totalSamples);

        /// <summary>
        /// Change block size.
        /// </summary>
        /// <param name="blocks">Audio blocks.</param>
        /// <param name="newBlockSize">New block size.</param>
        /// <returns>New blocks.</returns>
        List<IAudioEncoding> ChangeBlockSize(List<IAudioEncoding> blocks, int newBlockSize);

        /// <summary>
        /// Get a property.
        /// </summary>
        /// <typeparam name="T">Property type.</typeparam>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Retrieved property.</returns>
        T GetProperty<T>(string propertyName);

        /// <summary>
        /// Set a property.
        /// </summary>
        /// <typeparam name="T">Property type to set.</typeparam>
        /// <param name="value">Value to set.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        void SetProperty<T>(T value, string propertyName);

        /// <summary>
        /// Duplicate the audio data.
        /// </summary>
        /// <returns>A copy of the audio data.</returns>
        IAudioEncoding Duplicate();

    }

}