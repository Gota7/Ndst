using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ndst.Sound {
    
    /// <summary>
    /// A file containing streamed audio data.
    /// </summary>
    public abstract class SoundFile {

        /// <summary>
        /// Get the supported encodings.
        /// </summary>
        /// <returns>The supported encodings.</returns>
        public abstract Type[] SupportedEncodings();

        /// <summary>
        /// The name of the format.
        /// </summary>
        /// <returns>The name of the file.</returns>
        public abstract string Name();

        /// <summary>
        /// Extensions for the sound file.
        /// </summary>
        /// <returns>The extensions.</returns>
        public abstract string[] Extensions();

        /// <summary>
        /// Description of the sound file.
        /// </summary>
        /// <returns>The description.</returns>
        public abstract string Description();

        /// <summary>
        /// If the file supports tracks.
        /// </summary>
        /// <returns>If the file supports tracks.</returns>
        public abstract bool SupportsTracks();

        /// <summary>
        /// Get the preferred encoding.
        /// </summary>
        /// <returns>The preferred encoding.</returns>
        public abstract Type PreferredEncoding();

        // Read the file.
        public abstract void Read(BinaryReader r);

        // Write the file.
        public abstract void Write(BinaryWriter w);

        /// <summary>
        /// Blank constructor.
        /// </summary>
        public SoundFile() {}

        // Read from a file.
        public void Open(string path) {
            var r = Helper.GetReader(path);
            Read(r);
            r.Dispose();
        }

        // Save a file.
        public void Save(string path) {
            var w = Helper.GetWriter(path);
            Write(w);
            w.Dispose();
        }

        /// <summary>
        /// Audio data.
        /// </summary>
        public AudioData Audio = new AudioData();

        /// <summary>
        /// If the audio loops.
        /// </summary>
        public bool Loops { get; set; }

        /// <summary>
        /// Start loop.
        /// </summary>
        public uint LoopStart { get; set; }

        /// <summary>
        /// Original loop start.
        /// </summary>
        public uint OriginalLoopStart { get; set; }

        /// <summary>
        /// Ending loop.
        /// </summary>
        public uint LoopEnd { get; set; }

        /// <summary>
        /// Original loop end.
        /// </summary>
        public uint OriginalLoopEnd { get; set; }

        /// <summary>
        /// The sample rate of the audio file.
        /// </summary>
        public uint SampleRate { get; set; }

        /// <summary>
        /// Tracks.
        /// </summary>
        public List<TrackData> Tracks = new List<TrackData>();

        /// <summary>
        /// Track data.
        /// </summary>
        public class TrackData {

            /// <summary>
            /// Channels.
            /// </summary>
            public List<int> Channels = new List<int>();

            /// <summary>
            /// Track properies.
            /// </summary>
            public Dictionary<string, TrackProperty> Properties = new Dictionary<string, TrackProperty>();

            /// <summary>
            /// Duplicate the track data.
            /// </summary>
            /// <returns>Copy of the track data.</returns>
            public TrackData Duplicate() {
                TrackData t = new TrackData();
                foreach (var c in Channels) {
                    t.Channels.Add(c);
                }
                foreach (var p in Properties) {
                    t.Properties.Add(p.Key, new TrackProperty(p.Value.Type, p.Value.Data));
                }
                return t;
            }

        }

        /// <summary>
        /// Track property.
        /// </summary>
        public class TrackProperty {

            /// <summary>
            /// Data.
            /// </summary>
            public object Data;

            /// <summary>
            /// Type.
            /// </summary>
            public Type Type;

            /// <summary>
            /// Create a track property.
            /// </summary>
            /// <param name="type">Data type.</param>
            /// <param name="data">Data.</param>
            public TrackProperty(Type type, object data) {
                Type = type;
                Data = data;
            }
            
            /// <summary>
            /// Set data.
            /// </summary>
            /// <typeparam name="T">Type of data to get.</typeparam>
            /// <returns>Data.</returns>
            public T GetData<T>() => (T)Data;

            /// <summary>
            /// Set data.
            /// </summary>
            /// <typeparam name="T">Data type.</typeparam>
            /// <param name="data">Data to set.</param>
            public void SetData<T>(T data) {
                Type = typeof(T);
                Data = data;
            }

        }

        /// <summary>
        /// Before converting.
        /// </summary>
        public virtual void BeforeConversion() {}

        /// <summary>
        /// When converting from another streamed audio file.
        /// </summary>
        public virtual void AfterConversion() {}

        /// <summary>
        /// Convert from another streamed audio file.
        /// </summary>
        /// <param name="other">The other file converted from.</param>
        /// <param name="targetBlockSize">Target block size.</param>
        public void FromOtherStreamFile(SoundFile other, int targetBlockSize = -2) {

            //Set data.
            Loops = other.Loops;
            LoopStart = other.LoopStart;
            LoopEnd = other.LoopEnd;
            OriginalLoopStart = other.OriginalLoopStart;
            OriginalLoopEnd = other.OriginalLoopEnd;
            SampleRate = other.SampleRate;
            Audio = other.Audio.Duplicate();

            //Set track data.
            Tracks = new List<TrackData>();
            foreach (var t in other.Tracks) {
                Tracks.Add(t.Duplicate());
            }

            //Hook.
            BeforeConversion();

            //Convert to preffered encoding.
            if (PreferredEncoding() != null && !Audio.EncodingType.Equals(PreferredEncoding())) {
                Audio.Convert(PreferredEncoding(), targetBlockSize == -2 ? Audio.BlockSize : targetBlockSize, Loops ? (int)LoopStart : -1, Loops ? (int)LoopEnd : -1);
                AfterConversion();
                return;
            }

            //If any of the encodings match then that's good.
            var e = SupportedEncodings();
            foreach (var t in e) {
                if (t.Equals(other.Audio.EncodingType)) {
                    AfterConversion();
                    return;
                }
            }

            //By default convert to first type on list.
            Audio.Convert(SupportedEncodings()[0], targetBlockSize == -2 ? Audio.BlockSize : targetBlockSize, Loops ? (int)LoopStart : -1, Loops ? (int)LoopEnd : -1);

            //Activate on conversion hook.
            AfterConversion();

        }

        /// <summary>
        /// Convert from another stream file.
        /// </summary>
        /// <param name="other">Other stream file.</param>
        /// <param name="audioEncoding">Audio encoding to use.</param>
        /// <param name="targetBlockSize">Target block size.</param>
        public void FromOtherStreamFile(SoundFile other, Type audioEncoding, int targetBlockSize = -2) {

            //Set data.
            Loops = other.Loops;
            LoopStart = other.LoopStart;
            LoopEnd = other.LoopEnd;
            OriginalLoopStart = other.OriginalLoopStart;
            OriginalLoopEnd = other.OriginalLoopEnd;
            SampleRate = other.SampleRate;
            Audio = other.Audio.Duplicate();

            //Set track data.
            Tracks = new List<TrackData>();
            foreach (var t in other.Tracks) {
                Tracks.Add(t.Duplicate());
            }

            //Hook.
            BeforeConversion();   

            //Set channel data.
            Audio.Convert(audioEncoding, targetBlockSize == -2 ? Audio.BlockSize : targetBlockSize, Loops ? (int)LoopStart : -1, Loops ? (int)LoopEnd : -1);

            //Activate on conversion hook.
            AfterConversion();

        }

        /// <summary>
        /// Convert this to another sound format.
        /// </summary>
        /// <typeparam name="T">Type to convert to.</typeparam>
        /// <returns>Converted audio format.</returns>
        public T Convert<T>() where T : SoundFile {
            T ret = Activator.CreateInstance<T>();
            ret.FromOtherStreamFile(this);
            return ret;
        }

        /// <summary>
        /// Convert this to another sound format.
        /// </summary>
        /// <param name="targetType">Target type to convert to.</param>
        /// <returns>Converted sound file.</returns>
        public SoundFile Convert(Type targetType) {
            SoundFile ret = (SoundFile)Activator.CreateInstance(targetType);
            ret.FromOtherStreamFile(this);
            return ret;
        }

        /// <summary>
        /// Align a loop to a block interval.
        /// </summary>
        /// <param name="blockSamples">How many samples per block.</param>
        public void AlignLoopToBlock(uint blockSamples) {

            //Get new loop point.
            uint newLoopStart = LoopStart;
            if (LoopStart % blockSamples != 0) {

                //Get closest distance.
                uint dist1 = LoopStart / blockSamples * blockSamples;
                uint dist2 = (LoopStart / blockSamples + 1) * blockSamples;
                bool backward = Math.Abs(dist1 - LoopStart) < Math.Abs(dist2 - LoopStart);

                //Move loop point backwards.
                if (backward || (LoopEnd + dist2) >= Audio.NumSamples) {
                    LoopEnd -= LoopStart - dist1;
                    LoopStart = dist1;
                }

                //Move loop point forwards.
                else {
                    LoopEnd += dist2 - LoopStart;
                    LoopStart = dist2;
                    if (LoopEnd > Audio.NumSamples) { LoopEnd = (uint)Audio.NumSamples - 1; }
                }

            }

            //Set loop point.
            LoopStart = newLoopStart;

        }

        /// <summary>
        /// Trim data after loop end.
        /// </summary>
        public void TrimAfterLoopEnd() {
            if (Loops || LoopEnd != 0) {
                Audio.Trim((int)LoopEnd + 1);
            }
        }

    }

}