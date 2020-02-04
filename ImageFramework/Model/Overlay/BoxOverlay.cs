using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

        public BoxOverlay(Models models, DirectX.Shader pixel)
        {
            this.images = models.Images;
            this.cbuffer = models.SharedModel.Upload;
            Boxes.CollectionChanged += BoxesOnCollectionChanged;
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
            // top left corner on mip 0 (included)
            public int StartX;
            public int StartY;
            // bottom right corner on mip 0 (included)
            public int EndX;
            public int EndY;

            // border color
            public Color Color;
            
            // border size
            public int Border;
        }

        public ObservableCollection<Box> Boxes { get; } = new ObservableCollection<Box>();

        public override void Render(int layer, int mipmap, Size3 size)
        {
            Debug.Assert(HasWork);

            if(hasChanges)
                UpdateData();

            Shader.Bind(new VertexBufferBinding(positionBuffer.Handle, 2 * sizeof(float), 0));
            Shader.Draw(Boxes, cbuffer);
            Shader.Unbind();
        }

        private float ToCanonical(int pos, int size)
        {
            Debug.Assert(pos <= size);
            Debug.Assert(pos >= 0);
            return (float) (pos) / (float) (size) * 2.0f - 1.0f;
        }

        private void UpdateData()
        {
            if(Boxes.Count == 0) return;

            int floatCount = Boxes.Count * 2 * 4; // each box has 4 edges with 2 float component
            if (positionBuffer == null || positionBuffer.ByteSize < floatCount * sizeof(float))
            {
                // resize buffer
                positionBuffer?.Dispose();
                positionBuffer = new UploadBuffer(floatCount * sizeof(float));
            }

            // fill buffer
            var data = new float[floatCount];
            var imgSize = images.Size;
            for (var i = 0; i < Boxes.Count; i++)
            {
                var box = Boxes[i];
                float xStart = ToCanonical(box.StartX, imgSize.X);
                float xEnd = ToCanonical(box.EndX + 1, imgSize.X);
                float yStart = ToCanonical(box.StartY, imgSize.Y);
                float yEnd = ToCanonical(box.EndY + 1, imgSize.Y);

                // top left
                data[i * 8] = xStart;
                data[i * 8 + 1] = yStart;
                // top right
                data[i * 8 + 2] = xEnd;
                data[i * 8 + 3] = yStart;
                // bot left
                data[i * 8 + 4] = xStart;
                data[i * 8 + 5] = yEnd;
                // bot right
                data[i * 8 + 6] = xEnd;
                data[i * 8 + 7] = yEnd;
            }

            // upload buffer
            positionBuffer.SetData(data);
        }

        public override bool HasWork => Boxes.Count != 0;

        public override void Dispose()
        {
            shader?.Dispose();
            positionBuffer?.Dispose();
        }
    }
}
