using System.Collections.Generic;
using System.IO;

namespace Ndst.Graphics {

    // Palette.
    public class Palette {
        public List<RGB5> Colors = new List<RGB5>();

        public void Read(BinaryReader r, int len) {
            Colors = new List<RGB5>();
            for (int i = 0; i < len / 2; i++) {
                Colors.Add(new RGB5(r.ReadUInt16()));
            }
        }

        public void Write(BinaryWriter w, int len) {
            foreach (var c in Colors) {
                w.Write(c.Val);
            }
        }

    }

}