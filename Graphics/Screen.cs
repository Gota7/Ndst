using System;

namespace Ndst.Graphics {

    [Flags]
    public enum FlipFlags : byte {
        None = 0,
        Horizontal = 0b1 << 0,
        Vertical = 0b1 << 1
    }

    // Lays out graphic tiles.
    public class Screen {
        public Graphic Graphic;
        public bool Affine;

        // A screen tile.
        public class Tile {
            public byte TileIndex;
            public byte PaletteIndex;
            public FlipFlags FlipFlags;
        }

    }

}