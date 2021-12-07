using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Ndst.Graphics {

    // Extract a graphic.
    public class GraphicExtractOptions : ExtractOptions {
        public int ScreenWidthTiles;
        public int ScreenHeightTiles;
        public bool Affine;
        public string ScreenMaskPath;
        public int ScreenMaskIndex = -1;
        public string ScreenPath;
        public int GraphicWidthTiles;
        public int GraphicHeightTiles;
        public bool Is4Bpp;
        public bool OptimizeGraphic;
        public bool FirstColorTransparent;
        public string BackgroundColor;
        public string GraphicPath;
        public int ColorsPerPalette;
        public string PalettePath;
        public int NumPalettes;
        public string OutputPath;

        // TODO: SCREEN MASKS!!!
        public override void ExtractFiles(string destRomFolder, string conversionFolder, ConversionInfo conversionInfo) {

            // Read palette.
            byte[] palBin = conversionInfo.CurrentFormats[PalettePath].RawData();
            var r = Helper.GetReader(palBin);
            Palette pal = new Palette();
            pal.Read(r, ColorsPerPalette * NumPalettes * 2);
            r.Dispose();

            // Read graphic.
            byte[] graphicBin = conversionInfo.CurrentFormats[GraphicPath].RawData();
            r = Helper.GetReader(graphicBin);
            Graphic graphic = new Graphic();
            graphic.Read(r, GraphicWidthTiles, GraphicHeightTiles, Is4Bpp, FirstColorTransparent);
            r.Dispose();

            // Read screen.
            Screen screen;
            if (ScreenPath != null) {
                byte[] screenBin = conversionInfo.CurrentFormats[ScreenPath].RawData();
                r = Helper.GetReader(screenBin);
                screen = new Screen();
                screen.Read(r, ScreenWidthTiles, ScreenHeightTiles, Affine);
                r.Dispose();
                
            } else {
                screen = Screen.GenerateDefault(graphic);
            }

            // Extract the color.
            if (BackgroundColor != null) {
                Image<Argb32> col = new Image<Argb32>(Configuration.Default, 1, 1);
                col[0, 0] = new Argb32(pal.Colors[0].R8, pal.Colors[0].G8, pal.Colors[0].B8);
                col.SaveAsPng(destRomFolder + "/" + BackgroundColor);
            }

            // Finally, extract the image.
            graphic.Palette = pal;
            screen.Graphic = graphic;
            screen.ToImage().SaveAsPng(destRomFolder + "/" + OutputPath);

        }

        public override List<string> GetFileList() {
            List<string> ret = new List<string>();
            if (ScreenPath != null) ret.Add(ScreenPath);
            if (GraphicPath != null) ret.Add(GraphicPath);
            if (PalettePath != null) ret.Add(PalettePath);
            return ret;
        }

        public override void PackFiles() {
            throw new System.NotImplementedException();
        }
        
    }

}