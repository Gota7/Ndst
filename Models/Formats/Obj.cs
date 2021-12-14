using System.Collections.Generic;
using System.IO;

namespace Ndst.Models {

    public class WavefrontObj : IModelFormat
    {
        public string FormatName() => "Obj";

        public bool IsOfFormat(string filePath) {
            return filePath.ToLower().EndsWith(".obj");
        }

        public Model FromFile(string filePath) {
            throw new System.NotImplementedException();
        }

        public void WriteFile(string filePath, Model m) {

            // Set up.
            List<string> o = new List<string>();
            Dictionary<int, int> vertOffs = new Dictionary<int, int>();
            Dictionary<int, int> texOffs = new Dictionary<int, int>();
            Dictionary<int, int> normOffs = new Dictionary<int, int>();
            int vertNum = 0;
            int vertTexNum = 0;
            int vertNormNum = 0;
            int meshNum = 0;
            string mtlName = Path.GetFileNameWithoutExtension(filePath) + ".mtl";
            string mtlPath = Path.GetDirectoryName(filePath) + mtlName;
            if (m.EnableMaterials) {
                o.Add("mtllib " + mtlName);
            }
            foreach (var h in m.Meshes) {
                vertOffs.Add(meshNum++, vertNum + 1);
                foreach (var v in h.Vertices) {
                    o.Add("v " + v.X + " " + v.Y + " " + v.Z);
                    vertNum++;
                }
            }

            // Finish writing the model.
            meshNum = 0;
            foreach (var h in m.Meshes) {
                WriteMesh(h, meshNum++);
            }
            System.IO.File.WriteAllLines(filePath, o);

            // Observe a mesh.
            void ObserveMesh(Mesh h) {

            }

            // Write a mesh.
            void WriteMesh(Mesh h, int id) {
                int vertOff = vertOffs[id];
                foreach (var f in h.Faces) {
                    string[] vertices = new string[f.VertexIndices.Count];
                    for (int i = 0; i < vertices.Length; i++) {
                        vertices[i] = (vertOff + f.VertexIndices[i]).ToString();
                        if (f.HasTextures && m.EnableMaterials) {
                            vertices[i] += "/" + f.VertexTextureIndices[i];
                        }
                    }
                    o.Add("f " + string.Join(' ', vertices));
                }
            }

            // Write materials.
            List<string> mtl = new List<string>();
            foreach (var mat in m.Materials) {
                mtl.Add("newmtl " + mat.Name);
                //if (mat.AmbientColor != null)
                if (mat.DiffuseColor != null) mtl.Add("\tKd " + mat.DiffuseColor.Value.X + " " + mat.DiffuseColor.Value.Y + " " + mat.DiffuseColor.Value.Z); 
            }
            System.IO.File.WriteAllLines(mtlPath, mtl);

        }

    }

}