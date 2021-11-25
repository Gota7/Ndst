using System;
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
            throw new System.Exception();
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

        /*

            Ok, so there are various algorithms for splitting colors, but I personally love the "median-cut" algorithm, as it provides pretty good results.
            Unfortunately, this only supports splitting a palette into powers of 2. This is perfect for DS, as all palettes are powers of 2.
            ...Except it's not. We typically need 1 color reserved for say transparency or background color. This is a problem as we need 2^n - 1 "free colors".
            My solution to this was to modify the median-cut algorithm to allow any number of colors to be generated.
            My *Median-Cut Approximation* algorithm executes as follows:

            1. Put all colors into a list.
            2. See which component in the list (R, G, or B) has the biggest range.
            3. Sort elements in the list by this biggest range component from smallest to biggest.
            4. If splitting each list into two will produce a number of lists less than or equal to the desired amount of colors, split this list into two lists at the middle, and run the mean cut algorithm until the total amount of colors matches the amount of lists or until the next split will make too many colors.
            5. (I added this one): If splitting each list into two will produce too many colors, then "score" each list by taking the variance of each list. Split only the list with the greatest standard deviation until the desired amount of colors is reached.
            6. Variance of each list is calculated by finding the average color, and calculating the total squared distance of each color from it.
            7. Take the average color of each list, and this provides the finalized palette.

        */

        // Execute the median-cut algorithm.
        public static List<RGB5> MedianCutApprox(int numCols, List<RGB5> colors) {

            //Run the algorithm.
            int bucketsFilled = 1;
            List<List<RGB5>> buckets = new List<List<RGB5>>();
            buckets.Add(colors);
            while (bucketsFilled < numCols) {
                List<List<RGB5>> newBuckets = new List<List<RGB5>>();
                foreach (var l in buckets) {
                    List<RGB5> v1, v2;
                    FillBucket(l, out v1, out v2);
                    newBuckets.Add(v1);
                    newBuckets.Add(v2);
                }
                buckets = newBuckets;
                bucketsFilled *= 2;
            }
            
            // Now that we have each bucket, we take the "average" color of each one.
            List<RGB5> ret = new List<RGB5>();
            foreach (var b in buckets) {
                ret.Add(new RGB5() {
                    R5 = (byte)(b.Sum(x => x.R5) / b.Count),
                    G5 = (byte)(b.Sum(x => x.G5) / b.Count),
                    B5 = (byte)(b.Sum(x => x.B5) / b.Count)
                });
            }
            return ret;

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
                List<RGB5> bucket = null;
                if (rRange >= gRange && rRange >= bRange) {
                    bucket = colors.OrderBy(x => x.R5).ToList();
                } else if (gRange >= rRange && gRange >= bRange) {
                    bucket = colors.OrderBy(x => x.G5).ToList();
                } else {
                    bucket = colors.OrderBy(x => x.B5).ToList();
                }
                vec1.AddRange(bucket.GetRange(0, bucket.Count / 2));
                vec2.AddRange(bucket.GetRange(bucket.Count / 2, bucket.Count - bucket.Count / 2));
            }

            // Split a bucket into two.
            void SplitBucketToTwo() {

            }

            // Calculate variance of a bucket.
            double BucketVariance(List<RGB5> cols) {
                int averageR = cols.Sum(x => x.R5) / cols.Count;
                int averageG = cols.Sum(x => x.G5) / cols.Count;
                int averageB = cols.Sum(x => x.B5) / cols.Count;
                int variance = 0;
                foreach (var c in cols) {
                    variance += (int)Math.Pow(c.R5 - averageR, 2);
                    variance += (int)Math.Pow(c.G5 - averageG, 2);
                    variance += (int)Math.Pow(c.B5 - averageB, 2);
                }
                return variance / Math.Sqrt(cols.Count);
            }
            
        }

    }

}