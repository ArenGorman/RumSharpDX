﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonStuff;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics;

using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;

namespace CommonStuff
{
	public class TriangleComponent : GameComponent
	{
		PixelShader			pixelShader;
		VertexShader		vertexShader;
		CompilationResult	pixelShaderByteCode;
		CompilationResult	vertexShaderByteCode;
		InputLayout			layout;
		Vector4[]			points;
		Buffer				vertices;
		VertexBufferBinding bufBinding;
		Camera				camera;
		Buffer				constantBuffer;

		RasterizerState rastState;

		public Vector3 Position;


		public TriangleComponent(Game game, Camera cam) : base(game)
		{
			camera = cam;

			Position = new Vector3(0, 0, 0);
		}


		public override void Initialize()
		{
			// Compile Vertex and Pixel shaders
			vertexShaderByteCode = ShaderBytecode.CompileFromFile("Simple.hlsl", "VSMain", "vs_5_0", ShaderFlags.PackMatrixRowMajor);

			if (vertexShaderByteCode.HasErrors) {
				Console.WriteLine(vertexShaderByteCode.Message);
			}


			vertexShader = new VertexShader(Game.Device, vertexShaderByteCode);

			pixelShaderByteCode = ShaderBytecode.CompileFromFile("Simple.hlsl", "PSMain", "ps_5_0", ShaderFlags.PackMatrixRowMajor);
			pixelShader = new PixelShader(Game.Device, pixelShaderByteCode);


			// Layout from VertexShader input signature
			layout = new InputLayout(
				Game.Device,
				ShaderSignature.GetInputSignature(vertexShaderByteCode),
				new[] {
						new InputElement("POSITION",	0, Format.R32G32B32A32_Float, 0, 0),
						new InputElement("COLOR",		0, Format.R32G32B32A32_Float, 16, 0)
					});
			
			// Instantiate Vertex buiffer from vertex data
			points = new[] {
								new Vector4(0.0f, 50.5f,	0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
								new Vector4(50.5f, -50.5f,	0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
								new Vector4(-50.5f, -50.5f,	0.0f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
							};

			var bufDesc = new BufferDescription {
				BindFlags		= BindFlags.VertexBuffer,
				CpuAccessFlags	= CpuAccessFlags.None,
				OptionFlags		= ResourceOptionFlags.None,
				Usage			= ResourceUsage.Default,
			};

			vertices = Buffer.Create(Game.Device, points, bufDesc);

			bufBinding = new VertexBufferBinding(vertices, 32, 0);

			constantBuffer = new Buffer(Game.Device, new BufferDescription {
				BindFlags		= BindFlags.ConstantBuffer,
				CpuAccessFlags	= CpuAccessFlags.None,
				OptionFlags		= ResourceOptionFlags.None,
				SizeInBytes		= Utilities.SizeOf<Matrix>(),
				Usage			= ResourceUsage.Default
			});


			rastState = new RasterizerState(Game.Device, new RasterizerStateDescription {
				CullMode = CullMode.None,
				FillMode = FillMode.Solid
			});
        }

		public override void Update(float deltaTime)
		{
			var world	= Matrix.Translation(Position);
			var proj	= world * camera.GetViewMatrix() * camera.GetProjectionMatrix();

            Game.Context.UpdateSubresource(ref proj, constantBuffer);
		}

		public override void Draw(float deltaTime)
		{
			var context = Game.Context;

			var oldState = context.Rasterizer.State;
			context.Rasterizer.State = rastState;

			context.InputAssembler.InputLayout = layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
			context.InputAssembler.SetVertexBuffers(0, bufBinding);
			context.VertexShader.Set(vertexShader);
			context.PixelShader.Set(pixelShader);

			context.VertexShader.SetConstantBuffer(0, constantBuffer);

			PixHelper.BeginEvent(Color.Red, "Triangle Draw Event");
			context.Draw(3, 0);
			PixHelper.EndEvent();

			context.Rasterizer.State = oldState;
		}

		public override void DestroyResources()
		{
			pixelShader.Dispose();
			vertexShader.Dispose();
			pixelShaderByteCode.Dispose();
			vertexShaderByteCode.Dispose();
			layout.Dispose();
			vertices.Dispose();

			rastState.Dispose();

			constantBuffer.Dispose();
        }
	}
}
