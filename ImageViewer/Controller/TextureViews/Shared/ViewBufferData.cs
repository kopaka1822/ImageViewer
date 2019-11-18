using SharpDX;

namespace ImageViewer.Controller.TextureViews.Shared
{
    public struct ViewBufferData
    {
        public Matrix Transform;
        public Vector4 Crop;
        public float Multiplier;
        public float Farplane;
        public int UseAbs;
        public int XAxis;
        public int YAxis;
        public int ZValue;
    }
}
