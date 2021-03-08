using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Texture3D = ImageFramework.DirectX.Texture3D;

namespace ImageFramework.Model.Overlay
{
    public class ArrowOverlay : OverlayBase
    {
        private readonly ImagesModel images;
        private readonly UploadBuffer cbuffer;
        private UploadBuffer positionBuffer;
        private ArrowOverlayShader shader;
        private ArrowOverlayShader Shader => shader ?? (shader = new ArrowOverlayShader());

        public ArrowOverlay(Models models)
        {
            images = models.Images;
            cbuffer = models.SharedModel.Upload;
            Arrows.CollectionChanged += ArrowsOnCollectionChanged;
            images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ArrowsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // update arrows
            OnHasChanged();
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.Size): // TODO changes when size changes?
                    OnHasChanged();
                    break;
                case nameof(ImagesModel.ImageType):
                    // not supported
                    if (images.ImageType == typeof(Texture3D))
                        Arrows.Clear();
                    break;

            }
        }

        public struct Arrow
        {
            // arrow start
            public Float2 Start;
            // arrow end (with arrowhead)
            public Float2 End;
            // arrow color
            public Color Color;
            // line width
            public int Width;
        }

        public ObservableCollection<Arrow> Arrows { get; } = new ObservableCollection<Arrow>();

        public override void Render(LayerMipmapSlice lm, Size3 size)
        {
            Debug.Assert(HasWork);

            UpdateData(lm.Mipmap);

            Shader.Bind(new VertexBufferBinding(positionBuffer.Handle, 2 * sizeof(float), 0));
            Shader.Draw(Arrows, cbuffer, lm.Mipmap, size.XY, 9);
            Shader.Unbind();
        }

        public override void Dispose()
        {
            positionBuffer?.Dispose();
            shader?.Dispose();
        }

        public override bool HasWork => Arrows.Count != 0;

        // transform [0, 1] to [-1, 1]
        private static float ToCanonical(float texcoord)
        {
            return texcoord * 2.0f - 1.0f;
        }

        private void UpdateData(int mipmap)
        {
            int floatCount = Arrows.Count * 2 * 9; // each arrow has 9 vertices with 2 floats each
            if (positionBuffer == null || positionBuffer.ByteSize < floatCount * sizeof(float))
            {
                // resize buffer
                positionBuffer?.Dispose();
                positionBuffer = new UploadBuffer(floatCount * sizeof(float), BindFlags.VertexBuffer);
            }

            // fill buffer
            var data = new float[floatCount];
            var dim = images.Size.GetMip(mipmap);
            var pixelDim = Size2.Zero.ToCoords(dim.XY); // size of a single pixel in float coordinates
            var widthScale = Math.Max(pixelDim.X, pixelDim.Y);

            for (var i = 0; i < Arrows.Count; ++i)
            {
                var a = Arrows[i];
                var width = Math.Max(a.Width >> mipmap, 1);
                var off = i * 2 * 9;

                var dir = (a.End - a.Start).Normalize();
                var tangent = dir.RotateCCW();

                // arrow quad:
                float halfWidth = width * 0.5f * widthScale;
                var start1 = a.Start + tangent * halfWidth;
                var start2 = a.Start - tangent * halfWidth;

                var end1 = a.End + tangent * halfWidth - dir * halfWidth;
                var end2 = a.End - tangent * halfWidth - dir * halfWidth;

                // 1st triangle
                data[off + 0] = ToCanonical(start1.X);
                data[off + 1] = ToCanonical(start1.Y);
                data[off + 2] = ToCanonical(end1.X);
                data[off + 3] = ToCanonical(end1.Y);
                data[off + 4] = ToCanonical(end2.X);
                data[off + 5] = ToCanonical(end2.Y);
                // 2nd triangle
                data[off + 6] = ToCanonical(start1.X);
                data[off + 7] = ToCanonical(start1.Y);
                data[off + 8] = ToCanonical(end2.X);
                data[off + 9] = ToCanonical(end2.Y);
                data[off + 10] = ToCanonical(start2.X);
                data[off + 11] = ToCanonical(start2.Y);

                // arrow head
                float headScale = 2.0f * width * widthScale;
                var head = a.End;
                var left = a.End - dir * headScale + tangent * headScale;
                var right = a.End - dir * headScale - tangent * headScale;

                data[off + 12] = ToCanonical(head.X);
                data[off + 13] = ToCanonical(head.Y);
                data[off + 14] = ToCanonical(right.X);
                data[off + 15] = ToCanonical(right.Y);
                data[off + 16] = ToCanonical(left.X);
                data[off + 17] = ToCanonical(left.Y);
            }

            // upload buffer
            positionBuffer.SetData(data);
        }
    }
}
 