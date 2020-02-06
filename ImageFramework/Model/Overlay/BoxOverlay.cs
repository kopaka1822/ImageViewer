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
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Overlay
{
    public class BoxOverlay : OverlayBase
    {
        private readonly ImagesModel images;
        private readonly UploadBuffer cbuffer;
        private UploadBuffer positionBuffer;
        private BoxOverlayShader shader;
        private BoxOverlayShader Shader => shader ?? (shader = new BoxOverlayShader());
        private bool hasChanges = false;

        public BoxOverlay(Models models)
        {
            this.images = models.Images;
            this.cbuffer = models.SharedModel.Upload;
            Boxes.CollectionChanged += BoxesOnCollectionChanged;
            images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.Size):
                    if (!hasChanges)
                    {
                        hasChanges = true;
                        OnHasChanged();
                    }
                    break;
            }
        }

        private void BoxesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // update positions
            if (!hasChanges)
            {
                hasChanges = true;
                OnHasChanged();
            }
        }

        public struct Box
        {
            // top left corner
            public Float2 Start;
            // bottom right corner
            public Float2 End;

            // border color
            public Color Color;
            
            // border size
            public int Border;
        }

        public ObservableCollection<Box> Boxes { get; } = new ObservableCollection<Box>();

        public override void Render(int layer, int mipmap, Size3 size)
        {
            Debug.Assert(HasWork);

            UpdateData(mipmap);

            Shader.Bind(new VertexBufferBinding(positionBuffer.Handle, 2 * sizeof(float), 0));
            Shader.Draw(Boxes, cbuffer, mipmap, size.XY);
            Shader.Unbind();
        }

        private void UpdateData(int mipmap)
        {
            if(Boxes.Count == 0) return;

            int floatCount = Boxes.Count * 2 * 4; // each box has 4 edges with 2 float component
            if (positionBuffer == null || positionBuffer.ByteSize < floatCount * sizeof(float))
            {
                // resize buffer
                positionBuffer?.Dispose();
                positionBuffer = new UploadBuffer(floatCount * sizeof(float), BindFlags.VertexBuffer);
            }

            // fill buffer
            var data = new float[floatCount];
            var dim = images.Size.GetMip(mipmap);
            // offset float coordinates by half a unit so the quad covers the area
            var offset = Size2.Zero.ToCoords(dim.XY);
            
            for (var i = 0; i < Boxes.Count; i++)
            {
                var box = Boxes[i];

                // top left
                data[i * 8] = ToCanonical(box.Start.X - offset.X);
                data[i * 8 + 1] = ToCanonical(box.Start.Y - offset.Y);
                // top right
                data[i * 8 + 2] = ToCanonical(box.End.X + offset.X);
                data[i * 8 + 3] = ToCanonical(box.Start.Y - offset.Y);
                // bot left
                data[i * 8 + 4] = ToCanonical(box.Start.X - offset.X);
                data[i * 8 + 5] = ToCanonical(box.End.Y + offset.Y);
                // bot right
                data[i * 8 + 6] = ToCanonical(box.End.X + offset.X);
                data[i * 8 + 7] = ToCanonical(box.End.Y + offset.Y);
            }

            // upload buffer
            positionBuffer.SetData(data);
        }

        // transform [0, 1] to [-1, 1]
        private static float ToCanonical(float texcoord)
        {
            return texcoord * 2.0f - 1.0f;
        }

        public override bool HasWork => Boxes.Count != 0;

        public override void Dispose()
        {
            shader?.Dispose();
            positionBuffer?.Dispose();
        }
    }
}
