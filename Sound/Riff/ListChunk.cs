using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ndst.Sound.Riff {

    /// <summary>
    /// List chunk.
    /// </summary>
    public class ListChunk : Chunk {

        /// <summary>
        /// Chunks.
        /// </summary>
        public List<Chunk> Chunks = new List<Chunk>();

        /// <summary>
        /// Get a chunk.
        /// </summary>
        /// <param name="magic">The magic.</param>
        /// <returns>The chunk.</returns>
        public Chunk GetChunk(string magic) {
            return Chunks.Where(x => x.Magic.Equals(magic)).FirstOrDefault();
        }

    }

}