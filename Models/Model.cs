using System.Collections.Generic;

namespace Ndst.Models {

    // 3D model.
    public class Model : Node {
        public bool EnableMaterials;
        public new List<Mesh> Meshes = new List<Mesh>();
        public new List<Material> Materials = new List<Material>();

        // Default constructor.
        public Model() {}
        
        // From a model file.
        public Model(string filePath) {
            
        }

        // Write a model.
        public void SaveModel(string filePath, string format) {
            switch (format) {
                case "Obj":
                    WavefrontObj obj = new WavefrontObj();
                    obj.WriteFile(filePath, this);
                    break;
            }
        }

        // Generate material names.
        public void GenerateMaterialNames() {
            List<string> takenNames = new List<string>();
            int matNum = 0;
            foreach (var m in Materials) {
                while (takenNames.Contains("Mat" + matNum)) {
                    matNum++;
                }
                if (m.Name == null) {
                    m.Name = "Mat" + matNum++;
                }
                if (takenNames.Contains(m.Name)) {
                    m.Name = "Mat" + matNum++;
                }
                takenNames.Add(m.Name);
            }
        }

    }

}