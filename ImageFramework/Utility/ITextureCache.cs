using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;

namespace ImageFramework.Utility
{
    interface ITextureCache : IDisposable
    {
        /// <summary>
        /// returns one unused texture if available. creates a new texture if not textures were available
        /// </summary>
        [NotNull] ITexture GetTexture();

        /// <summary>
        /// stores the textures for later use
        /// </summary>
        void StoreTexture([NotNull] ITexture tex);

        bool IsCompatibleWith([NotNull] ITexture tex);
    }
}
