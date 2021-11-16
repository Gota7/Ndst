using System;
using System.IO;

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
        public Tile[,] Tiles;

        // A screen tile.
        public class Tile {
            public ushort TileIndex;
            public byte PaletteIndex;
            public FlipFlags FlipFlags;

            public void Read(BinaryReader r, bool affine, ref ushort affinePrefix, int tileCount) {
                if (affine) {
                    PaletteIndex = 0;
                    FlipFlags = FlipFlags.None;
                    ushort index = r.ReadByte();
                    TileIndex = (ushort)(index + affinePrefix);
                    if (index == 0xFF) {
                        affinePrefix += 0x100;
                    }
                    if (affinePrefix >= tileCount) {
                        affinePrefix = 0;
                    }
                } else {
                    ushort val = r.ReadUInt16();
                    TileIndex = (ushort)(val & 0x3FF);
                    PaletteIndex = (byte)((val >> 12) & 0xF);
                    FlipFlags = FlipFlags.None;
                    if ((val & 0x400) > 0) FlipFlags |= FlipFlags.Horizontal;
                    if ((val & 0x800) > 0) FlipFlags |= FlipFlags.Vertical;
                }
            }

        }

        public static Screen GenerateDefault(Graphic g) {
            Screen s = new Screen();
            s.Affine = false;
            s.Graphic = g;
            s.Tiles = new Tile[g.Tiles.GetLength(0), g.Tiles.GetLength(1)];
            for (int i = 0; i < g.Tiles.GetLength(1); i++) {
                for (int j = 0; j < g.Tiles.GetLength(0); j++) {
                    s.Tiles[j, i].TileIndex = (ushort)(i * g.Tiles.GetLength(0) + j);
                }
            }
            return s;
        }

        public void Read(BinaryReader r, int widthInTiles, int heightInTiles, bool affine) {
            Affine = affine;
            Tiles = new Tile[widthInTiles, heightInTiles];
            ushort affinePrefix = 0;
            for (int i = 0; i < heightInTiles; i++) {
                for (int j = 0; j < widthInTiles; j++) {
                    Tiles[j, i] = new Tile();
                    Tiles[j, i].Read(r, Affine, ref affinePrefix, widthInTiles * heightInTiles);
                }
            }
        }

    }

}