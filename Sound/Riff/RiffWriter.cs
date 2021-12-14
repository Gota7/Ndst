using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ndst.Sound.Formats;

namespace Ndst.Sound.Riff {
    
    /// <summary>
    /// RIFF Writer.
    /// </summary>
    public class RiffWriter : BinaryWriter {

        /// <summary>
        /// Backup offsets.
        /// </summary>
        Stack<long> BakOffs = new Stack<long>();

        /// <summary>
        /// Current offset.
        /// </summary>
        public long CurrOffset;

        /// <summary>
        /// Block offset.
        /// </summary>
        private Stack<long> BlockOffs = new Stack<long>();

        //Constructors.
        #region Constructors

        public RiffWriter(Stream output) : base(output) {}

        #endregion

        /// <summary>
        /// Init file.
        /// </summary>
        public void InitFile(string magic) {

            //Prepare file.
            BakOffs = new Stack<long>();
            Write("RIFF".ToCharArray());
            Write((uint)0);
            CurrOffset = BaseStream.Position;
            Write(magic.ToCharArray());

        }

        /// <summary>
        /// Close the file.
        /// </summary>
        public void CloseFile() {

            //Write the offset.
            WriteOffset(8);

        }

        /// <summary>
        /// Start a chunk.
        /// </summary>
        /// <param name="blockName">The block name.</param>
        public void StartChunk(string blockName) {

            //Prepare block.
            BakOffs.Push(CurrOffset);
            Write(blockName.ToCharArray());
            Write((uint)0);
            CurrOffset = BaseStream.Position;
            BlockOffs.Push(CurrOffset);

        }

        /// <summary>
        /// Start a list chunk.
        /// </summary>
        /// <param name="blockName">The block name.</param>
        public void StartListChunk(string blockName) {

            //Prepare block.
            BakOffs.Push(CurrOffset);
            Write("LIST".ToCharArray());
            Write((uint)0);
            CurrOffset = BaseStream.Position;
            BlockOffs.Push(CurrOffset);
            Write(blockName.ToCharArray());

        }

        /// <summary>
        /// End a chunk.
        /// </summary>
        public void EndChunk() {

            //Write the offset.
            WriteOffset(BlockOffs.Pop());
            CurrOffset = BakOffs.Pop();

        }

        /// <summary>
        /// Write an offset.
        /// </summary>
        /// <param name="basePos">The base position.</param>
        public void WriteOffset(long basePos) {
            long bak = BaseStream.Position;
            BaseStream.Position = basePos;
            uint size = (uint)(bak - basePos);
            BaseStream.Position -= 4;
            Write(size);
            BaseStream.Position = bak;
        }

        /// <summary>
        /// Write a wave file.
        /// </summary>
        /// <param name="r">The riff wave.</param>
        public void WriteWave(RiffWave r) {

            //Make it so the sample block is not written.
            r.Loops = false;

            //Write the file and fix chunks.
            long bak = BaseStream.Position;
            Write(Helper.GetBytes(r.Write));
            long bak2 = BaseStream.Position;
            BaseStream.Position = bak;
            Write("LIST".ToCharArray());
            BaseStream.Position += 4;
            Write("wave".ToCharArray());
            BaseStream.Position = bak2;

        }

    }

}