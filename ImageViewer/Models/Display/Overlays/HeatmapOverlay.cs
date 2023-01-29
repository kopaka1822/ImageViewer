using ImageFramework.DirectX;
using ImageFramework.Model.Overlay;
using ImageFramework.Model;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Models.Display.Overlays
{
    public class HeatmapOverlay : OverlayBase
    {


        private readonly ImagesModel images;
        private readonly HeatmapModel heatmap;
        private readonly UploadBuffer cbuffer;
        private UploadBuffer positionsBuffer;
        private VertexBufferBinding vertexBufferBinding;
        private HeatmapOverlayShader shader;
        private HeatmapOverlayShader Shader => shader ?? (shader = new HeatmapOverlayShader());

        public HeatmapOverlay(ImageFramework.Model.Models models, HeatmapModel parent)
        {
            this.cbuffer = models.SharedModel.Upload;
            this.images = models.Images;
            this.heatmap = parent;
            this.heatmap.PropertyChanged += HeatmapOnPropertyChanged;
        }

        private void HeatmapOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // TODO make more precise
            OnHasChanged();
        }

        public override void Render(LayerMipmapSlice lm, Size3 size)
        {
            Debug.Assert(HasWork);

            UpdateData(lm.Mipmap);

            // draw box
            Shader.Bind(vertexBufferBinding);
            Shader.Draw(heatmap, cbuffer, lm.Mipmap, size.XY);
            Shader.Unbind();
        }

        public override bool RequireD2D => false; // TODO enable d2d for text output 
        public override void RenderD2D(LayerMipmapSlice lm, Size3 size, Direct2D d2d)
        {
            /*using (var ctx = d2d.Begin())
            {
                ctx.Text(new Float2(0.0f, 0.0f), new Float2(1000.0f, 12.0f), 10.0f, new Color(1.0f, 1.0f, 1.0f, 1.0f), "hello world");
            }

            // draw text
            if (!String.IsNullOrEmpty(heatmap.MinText) || !String.IsNullOrEmpty(heatmap.MaxText))
            {
                //using(var d2d = new Direct2D(texture))
            }*/
        }

        private void UpdateData(int mipmap)
        {
            if (positionsBuffer == null)
            {
                // each box has 4 edges with 2 float positions
                positionsBuffer = new UploadBuffer(2 * 4 * sizeof(float));
                vertexBufferBinding = new VertexBufferBinding(positionsBuffer.Handle, 2 * sizeof(float), 0);
            }

            // fill buffer
            var data = new float[2 * 4];
            var dim = images.Size.GetMip(mipmap);
            // offset float coordinates by half a unit so the quad covers the area
            var offset = Size2.Zero.ToCoords(dim.XY);

            // top left
            data[0] = ToCanonical(heatmap.Start.X - offset.X);
            data[1] = ToCanonical(heatmap.Start.Y - offset.Y);
            // top right
            data[2] = ToCanonical(heatmap.End.X + offset.X);
            data[3] = ToCanonical(heatmap.Start.Y - offset.Y);
            // bot left
            data[4] = ToCanonical(heatmap.Start.X - offset.X);
            data[5] = ToCanonical(heatmap.End.Y + offset.Y);
            // bot right
            data[6] = ToCanonical(heatmap.End.X + offset.X);
            data[7] = ToCanonical(heatmap.End.Y + offset.Y);

            // upload data
            positionsBuffer.SetData(data);
        }

        // transform [0, 1] to [-1, 1]
        private static float ToCanonical(float texcoord)
        {
            return texcoord * 2.0f - 1.0f;
        }

        public override void Dispose()
        {
            shader?.Dispose();
            positionsBuffer?.Dispose();
        }

        public override bool HasWork => true;


    }
}
