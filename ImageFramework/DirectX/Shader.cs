using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace ImageFramework.DirectX
{
    public class Shader : IDisposable
    {
        public enum Type
        {
            Vertex,
            Pixel,
            Compute,
            Geometry
        }

        private readonly ShaderFlags flags = ShaderFlags.WarningsAreErrors | ShaderFlags.IeeeStrictness | ShaderFlags.EnableStrictness
// shader debugging does not work atm. anyways..
//#if DEBUG
//                                             | ShaderFlags.Debug | ShaderFlags.SkipOptimization | ShaderFlags.DebugNameForBinary;
//#else
                                             | ShaderFlags.OptimizationLevel3;
//#endif
        private VertexShader vertex;
        public VertexShader Vertex
        {
            get
            {
                Debug.Assert(ShaderType == Type.Vertex);
                return vertex;
            }
        }

        private PixelShader pixel;

        public PixelShader Pixel
        {
            get
            {
                Debug.Assert(ShaderType == Type.Pixel);
                return pixel;
            }
        }

        private ComputeShader compute;

        public ComputeShader Compute
        {
            get
            {
                Debug.Assert(ShaderType == Type.Compute);
                return compute;
            }
        }

        private GeometryShader geometry;

        public GeometryShader Geometry
        {
            get
            {
                Debug.Assert(ShaderType == Type.Geometry);
                return geometry;
            }
        }

        public Type ShaderType { get; }

        public Shader(Type type, string source, string debugName, out byte[] outBytecode, bool getBytecode = true)
        {
            ShaderType = type;

            try
            {
                using (var byteCode = ShaderBytecode.Compile(
                    source,
                    "main",
                    GetProfile(type),
                    flags,
                    EffectFlags.None,
                    debugName,
                    SecondaryDataFlags.None,
                    null))
                {
                    outBytecode = getBytecode ? byteCode.Bytecode.Data : null;

                    switch (type)
                    {
                        case Type.Vertex:
                            vertex = new VertexShader(Device.Get().Handle, byteCode);
                            break;
                        case Type.Pixel:
                            pixel = new PixelShader(Device.Get().Handle, byteCode);
                            break;
                        case Type.Compute:
                            compute = new ComputeShader(Device.Get().Handle, byteCode);
                            break;
                        case Type.Geometry:
                            geometry = new GeometryShader(Device.Get().Handle, byteCode);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"{debugName} compilation failed: {e.Message}\nsource:\n{AddLineNumbers(source)}");
            }
        }

        public Shader(Type type, string source, string debugName)
        : this(type, source, debugName, out var tmp, false)
        {}

        private string AddLineNumbers(string source)
        {
            int curLine = 1;
            var r = new StringReader(source);
            string line;
            var res = "";

            while ((line = r.ReadLine()) != null)
            {
                if (line.StartsWith("#line 1"))
                    curLine = 0;

                if(!String.IsNullOrWhiteSpace(line))
                    res += $"{curLine}: {line}\n";

                ++curLine;
            }

            return res;
        }

        private static string GetProfile(Type t)
        {
            switch (t)
            {
                case Type.Vertex: return "vs_5_0";
                case Type.Pixel: return "ps_5_0";
                case Type.Compute: return "cs_5_0";
                case Type.Geometry: return "gs_5_0";
                default: Debug.Assert(false);
                    return "";
            }
        }

        public void Dispose()
        {
            vertex?.Dispose();
            pixel?.Dispose();
            compute?.Dispose();
            geometry?.Dispose();
        }
    }
}
