using System;
using System.Collections;
using System.Collections.Generic;

namespace Ndst.Graphics {

    // CIE color space for having distances be closer to human perception.
    public struct CIELAB {
        public double L;
        public double A;
        public double B;

        // Create a CIE color from RGB5.
        public CIELAB(RGB5 rgb) {
            double r = rgb.R5 / RGB5.MAX_COMPONENT_VALUE;
            double g = rgb.G5 /RGB5.MAX_COMPONENT_VALUE;
            double b = rgb.B5 / RGB5.MAX_COMPONENT_VALUE;
            double x, y, z;

            r = (r > 0.04045) ? Math.Pow((r + 0.055) / 1.055, 2.4) : r / 12.92;
            g = (g > 0.04045) ? Math.Pow((g + 0.055) / 1.055, 2.4) : g / 12.92;
            b = (b > 0.04045) ? Math.Pow((b + 0.055) / 1.055, 2.4) : b / 12.92;

            x = (r * 0.4124 + g * 0.3576 + b * 0.1805) / 0.95047;
            y = (r * 0.2126 + g * 0.7152 + b * 0.0722) / 1.00000;
            z = (r * 0.0193 + g * 0.1192 + b * 0.9505) / 1.08883;

            x = (x > 0.008856) ? Math.Pow(x, 1/3) : (7.787 * x) + 16/116;
            y = (y > 0.008856) ? Math.Pow(y, 1/3) : (7.787 * y) + 16/116;
            z = (z > 0.008856) ? Math.Pow(z, 1/3) : (7.787 * z) + 16/116;

            L = 116 * y - 16;
            A = 500 * (x - y);
            B = 200 * (y - z);
        }

        // Convert back to RGB5.
        public RGB5 ToRGB5() {
            double y = (L + 16) / 116;
            double x = A / 500 + y;
            double z = y - B / 200;
            double r, g, b;

            x = 0.95047 * ((x * x * x > 0.008856) ? x * x * x : (x - 16/116) / 7.787);
            y = 1.00000 * ((y * y * y > 0.008856) ? y * y * y : (y - 16/116) / 7.787);
            z = 1.08883 * ((z * z * z > 0.008856) ? z * z * z : (z - 16/116) / 7.787);

            r = x *  3.2406 + y * -1.5372 + z * -0.4986;
            g = x * -0.9689 + y *  1.8758 + z *  0.0415;
            b = x *  0.0557 + y * -0.2040 + z *  1.0570;

            r = (r > 0.0031308) ? (1.055 * Math.Pow(r, 1/2.4) - 0.055) : 12.92 * r;
            g = (g > 0.0031308) ? (1.055 * Math.Pow(g, 1/2.4) - 0.055) : 12.92 * g;
            b = (b > 0.0031308) ? (1.055 * Math.Pow(b, 1/2.4) - 0.055) : 12.92 * b;

            return new RGB5() {
                R5 = (byte)(Math.Max(0, Math.Min(1, r)) * RGB5.MAX_COMPONENT_VALUE),
                G5 = (byte)(Math.Max(0, Math.Min(1, g)) * RGB5.MAX_COMPONENT_VALUE),
                B5 = (byte)(Math.Max(0, Math.Min(1, b)) * RGB5.MAX_COMPONENT_VALUE)
            };

        }

        // Distance squared
        public double DeltaESquared(CIELAB other) {
            var deltaL = L - other.L;
            var deltaA = A - other.A;
            var deltaB = B - other.B;
            var c1 = Math.Sqrt(A * A + B * B);
            var c2 = Math.Sqrt(other.A * other.A + other.B * other.B);
            var deltaC = c1 - c2;
            var deltaH = deltaA * deltaA + deltaB * deltaB - deltaC * deltaC;
            deltaH = deltaH < 0 ? 0 : Math.Sqrt(deltaH);
            var sc = 1.0 + 0.045 * c1;
            var sh = 1.0 + 0.015 * c1;
            var deltaLKlsl = deltaL / (1.0);
            var deltaCkcsc = deltaC / (sc);
            var deltaHkhsh = deltaH / (sh);
            var i = deltaLKlsl * deltaLKlsl + deltaCkcsc * deltaCkcsc + deltaHkhsh * deltaHkhsh;
            return i;
        }

        public static CIELAB AverageColor(List<CIELAB> colors) {
            throw new System.NotImplementedException();
        }

        public static double CalculateVariance(List<CIELAB> colors) {
            double ret = 0;
            CIELAB avg = AverageColor(colors);
            foreach (var c in colors) {
                ret += c.DeltaESquared(avg); // (x - xBar)^2
            }
            return ret / (colors.Count - 1); // Use sample variance instead of population. Stats!
        }

    }

}