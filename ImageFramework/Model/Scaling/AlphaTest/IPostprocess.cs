using System;
using ImageFramework.DirectX;
using ImageFramework.Utility;

namespace ImageFramework.Model.Scaling.AlphaTest
{
    internal interface IPostprocess : IDisposable
    {
        /// <summary>
        /// runs the postprocess on all mipmaps of the uav texture
        /// </summary>
        /// <param name="uav">texture with uav view</param>
        /// <param name="hasAlpha">indicates if the texture has an alpha channel</param>
        /// <param name="upload">used for constant buffer</param>
        /// <param name="cache">used for temporary texture. Must be the dimension of uav texture</param>
        void Run(ITexture uav, bool hasAlpha, UploadBuffer upload, ITextureCache cache);
    }
}
