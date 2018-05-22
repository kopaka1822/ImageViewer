using System.Collections.Generic;
using TextureViewer.Models.Shader;

namespace TextureViewer.Models.Filter
{
    public class FilterModel
    {
        public FilterShader Shader { get; }
        public List<IFilterParameter> Parameters { get; }
        public bool IsSepa { get; }
        public bool IsSingleInvocation { get; }
        public string Name { get; }
        public string Description { get; }
        public string Filename { get; }

        public FilterModel(FilterLoader loader)
        {
            Parameters = loader.Parameters;
            IsSepa = loader.IsSepa;
            IsSingleInvocation = loader.IsSingleInvocation;
            Name = loader.Name;
            Description = loader.Description;
            Filename = loader.Filename;

            Shader = new FilterShader(loader.ShaderSource, IsSepa);
        }

        public void Dispose()
        {
            Shader.Dispose();
        }
    }
}
