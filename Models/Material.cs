namespace Ndst.Models {

    // Material.
    public class Material {
        public string Name;
        public Vec3? AmbientColor;
        public Vec3? DiffuseColor;
        public Vec3? SpecularColor;
        public float? SpecularExponent;
        public float? Transparency;
        public Vec3? TransmissionColorFilter;
        public float? OpticalDensity; // Index of refraction, 0.001 to 10.

    }

}