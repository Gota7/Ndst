namespace Ndst.Graphics {

    // Color reduction algorithm taken from https://github.com/Garhoogin/NitroPaint/blob/main/NitroPaint/isplt.c
    // No, I don't know how this works, don't @me.

    // Histogram entry.
    public class HistogramEntry {
        int Y, I, Q, A;
        HistogramEntry Next;
        double Weight;
        double Value;
    }

    // Color node.
    public class ColorNode {
        bool IsLeaf;
        double Weight;
        double Priority;
        int Y, I, Q, A;
        int PivotIndex;
        int StartIndex;
        int EndIndex;
        ColorNode Left;
        ColorNode Right;
    }

}