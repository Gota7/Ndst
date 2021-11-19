using System.IO;

namespace Ndst.Graphics {

    // Contains tiles.
    public class Graphic {
        public Palette Palette;
        public bool Is4BPP;
        public bool FirstColorTransparent;
        public Tile[,] Tiles;

        // A tile.
        public class Tile {
            public byte[,] Colors = new byte[8,8];
            
            public void Read(BinaryReader r, bool is4BPP) {
                if (is4BPP) {
                    for (int i = 0; i < 8; i++) {
                        for (int j = 0; j < 4; j++) {
                            byte col = r.ReadByte();
                            Colors[j * 2, i] = (byte)(col & 0x0F);
                            Colors[j * 2 + 1, i] = (byte)((col & 0xF0) >> 4);
                        }
                    }
                } else {
                    for (int i = 0; i < 8; i++) {
                        for (int j = 0; j < 8; j++) {
                            Colors[j, i] = r.ReadByte();
                        }
                    }
                }
            }

            public void Write(BinaryWriter w, bool is4BPP) {
                if (is4BPP) {
                    for (int i = 0; i < 8; i++) {
                        for (int j = 0; j < 4; j++) {
                            byte col = (byte)((Colors[j * 2 + 1, i] << 4) & 0xF0);
                            col |= (byte)(Colors[j * 2, i] & 0x0F);
                            w.Write(col);
                        }
                    }
                } else {
                    for (int i = 0; i < 8; i++) {
                        for (int j = 0; j < 8; j++) {
                            w.Write(Colors[j, i]);
                        }
                    }
                }
            }

            public Tile FlipX() {
                Tile ret = new Tile();
                for (int i = 0; i < 8; i++) {
                    for (int j = 0; j < 8; j++) {
                        ret.Colors[j, i] = Colors[8 - j - 1, i];
                    }
                }
                return ret;
            }

            public Tile FlipY() {
                Tile ret = new Tile();
                for (int i = 0; i < 8; i++) {
                    for (int j = 0; j < 8; j++) {
                        ret.Colors[i, j] = Colors[i, 8 - j - 1];
                    }
                }
                return ret;
            }

        }

        public void Read(BinaryReader r, int widthInTiles, int heightInTiles, bool is4BPP, bool firstColorTransparent) {
            Is4BPP = is4BPP;
            FirstColorTransparent = firstColorTransparent;
            Tiles = new Tile[widthInTiles, heightInTiles];
            for (int i = 0; i < heightInTiles; i++) {
                for (int j = 0; j < widthInTiles; j++) {
                    Tiles[j, i] = new Tile();
                    Tiles[j, i].Read(r, Is4BPP);
                }
            }
        }

        public void Write(BinaryWriter w) {
            for (int i = 0; i < Tiles.GetLength(1); i++) {
                for (int j = 0; j < Tiles.GetLength(0); j++) {
                    Tiles[j, i].Write(w, Is4BPP);
                }
            }
        }

    }

}