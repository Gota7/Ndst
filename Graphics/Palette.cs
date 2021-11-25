using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Ndst.Graphics {

    // Palette.
    public class Palette {
        public List<RGB5> Colors = new List<RGB5>();
        public int IndexSize = 0x10;

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

        // Limit the color palette.
        public static List<RGB5> LimitColorPalette(Image img, int maxColors, Argb32 firstColor, out int[,] newGraphic) {

            // Return palette.
            List<RGB5> ret = new List<RGB5>();
            ret.Add(Argb32ToRGB5(firstColor));
            Dictionary<Argb32, RGB5> mappedColors = new Dictionary<Argb32, RGB5>();
            mappedColors.Add(firstColor, ret[0]);
            
            // Generate color palette.

            // Argb32 to RGB5.
            RGB5 Argb32ToRGB5(Argb32 a) {
                return new RGB5() { R8 = a.R, G8 = a.G, B8 = a.B };
            }
            
        }

        // Execute the median-cut algorithm.
        public static List<RGB5> MedianCut() {

            // Essentially, what we do here is measure which component of RGB has the greatest range.
            // Then we sort by the component that has the greatest range, say if G has the greatest range of values we sort by the G component.
            void FillBucket(List<RGB5> colors, out List<RGB5> vec1, out List<RGB5> vec2) {
                vec1 = new List<RGB5>();
                vec2 = new List<RGB5>();
                int rMin = 255;
                int rMax = -1;
                int gMin = 255;
                int gMax = -1;
                int bMin = 255;
                int bMax = -1;
                foreach (var c in colors) {
                    if (c.R5 > rMax) {
                        rMax = c.R5;
                    }
                    if (c.R5 < rMin) {
                        rMin = c.R5;
                    }
                    if (c.G5 > gMax) {
                        gMax = c.G5;
                    }
                    if (c.G5 < gMin) {
                        gMin = c.G5;
                    }
                    if (c.B5 > bMax) {
                        bMax = c.B5;
                    }
                    if (c.B5 < bMin) {
                        bMin = c.B5;
                    }
                }
                int rRange = rMax - rMin;
                int gRange = gMax - gMin;
                int bRange = bMax - bMin;
                IOrderedEnumerable<RGB5> bucket = null;
                if (rRange >= gRange && rRange >= bRange) {
                    bucket = colors.OrderBy(x => x.R5);
                } else if (gRange >= rRange && gRange >= bRange) {
                    bucket = colors.OrderBy(x => x.G5);
                } else {
                    bucket = colors.OrderBy(x => x.B5);
                }
                
            }
            
        }

    }

}