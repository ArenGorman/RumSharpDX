using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Runtime.InteropServices;

namespace CommonStuff.VertexStructures
{
    [StructLayout(LayoutKind.Explicit)]
    public struct VertexPosColUV01NrmTan
    {
        [FieldOffset(0)] public Vector4 Position;
        [FieldOffset(16)] public Vector4 Color;
        [FieldOffset(32)] public Vector4 UV0;
        [FieldOffset(48)] public Vector4 UV1;
        [FieldOffset(64)] public Vector4 Normal;
        [FieldOffset(80)] public Vector4 Tangent;

        public static InputLayout GetLayout(ShaderSignature signature, out int stride)
        {
            var layout = new InputLayout(
                Game.Instance.Device,
                signature,
                new[] {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                        new InputElement("COLOR",    0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                        new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                        new InputElement("TEXCOORD", 1, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                        new InputElement("NORMAL",   0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0),
                        new InputElement("TANGENT",  0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0)
                    });

            stride = 96;
            return layout;
        }
    }
}
