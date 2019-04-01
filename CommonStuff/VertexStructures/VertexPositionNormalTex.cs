using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CommonStuff.VertexStructures
{
	[StructLayout(LayoutKind.Explicit)]
	public struct VertexPositionNormalTex
	{
		[FieldOffset(0)]	public Vector4 Position;
		[FieldOffset(16)]	public Vector4 Normal;
		[FieldOffset(32)]	public Vector4 Tex;


		public static InputLayout GetLayout(ShaderSignature signature, out int stride)
		{
			var layout = new InputLayout(
				Game.Instance.Device,
				signature,
				new[] {
						new InputElement("POSITION",    0, Format.R32G32B32A32_Float, 0, 0),
						new InputElement("NORMAL",      0, Format.R32G32B32A32_Float, 16, 0),
						new InputElement("TEXCOORD",    0, Format.R32G32B32A32_Float, 32, 0)
					});

			stride = 48;
            return layout;
		}
	}

}
