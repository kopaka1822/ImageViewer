using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Query;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model
{
    /// <summary>
    /// data that is usually used by multiple models
    /// </summary>
    public class SharedModel : IDisposable
    {
        public MitchellNetravaliScaleShader ScaleShader { get; }
        public QuadShader QuadShader { get; } = new QuadShader();
        public ConvertFormatShader Convert { get; }
        public UploadBuffer Upload { get; }
        public DownloadBuffer Download { get; }

        internal SyncQuery Sync { get; }

        public SharedModel()
        {
            Upload = new UploadBuffer(256); // big enough for 4 matrix4
            Download = new DownloadBuffer(256);
            ScaleShader = new MitchellNetravaliScaleShader(QuadShader, Upload);
            Convert = new ConvertFormatShader(QuadShader, Upload);
            Sync = new SyncQuery();
        }

        public void Dispose()
        {
            Convert?.Dispose();
            ScaleShader?.Dispose();
            QuadShader?.Dispose();
            Upload?.Dispose();
            Sync?.Dispose();
        }
    }
}
