using System;

namespace Ndst.Models {

    // 3D Vector.
    public struct Vec4 {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public float MagnitudeSquared => X * X + Y * Y + Z * Z + W * W;
        public float Magnitude => (float)Math.Sqrt(MagnitudeSquared);

        public Vec4(float val) {
            X = Y = Z = W = val;
        }

        public Vec4(float x, float y, float z, float w) {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public Vec4(Vec3 v) {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = 1.0f;
        }

        // Add two vectors.
        public static Vec4 operator +(Vec4 a, Vec4 b) {
            return new Vec4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }

        // Sub two vectors.
        public static Vec4 operator -(Vec4 a, Vec4 b) {
            return new Vec4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }

        // Dot two vectors.
        public static float operator *(Vec4 a, Vec4 b) {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        // Scale a vector.
        public static Vec4 operator *(Vec4 a, float b) {
            return new Vec4(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        // Scale a vector.
        public static Vec4 operator *(float b, Vec4 a) {
            return new Vec4(a.X * b, a.Y * b, a.Z * b, a.W * b);
        }

        // Scale a vector.
        public static Vec4 operator /(Vec4 a, float b) {
            return new Vec4(a.X / b, a.Y / b, a.Z / b, a.W / b);
        }

        // Scale a vector.
        public static Vec4 operator /(float b, Vec4 a) {
            return new Vec4(b / a.X, b / a.Y, b / a.Z, b / a.W);
        }

        // Normalize.
        public Vec4 Normalize() {
            return this / Magnitude;
        }

    }

}