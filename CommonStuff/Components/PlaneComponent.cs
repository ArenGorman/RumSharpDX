using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonStuff;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;


namespace CommonStuff
{
	public class PlaneComponent : GameComponent
	{
  //      public PixelShader pixelShader;
		//public VertexShader vertexShader;
  //      CompilationResult pixelShaderByteCode;
  //      CompilationResult vertexShaderByteCode;

        Buffer vertices;
        Camera camera;
        Device device;


		public PlaneComponent(Game game, Camera cam, Material material = null) : base(game)
		{
			camera = cam;
            device = game.Device;
            primTopology = PrimitiveTopology.LineList;
            material = new Material(game, "PlaneMaterial", MaterialType.ColorLines);
            Renderer = game.Renderer;
            Position = new Vector3(0);
		}


		public override void Initialize()
		{
            // Compile Vertex and Pixel shaders
            //vertexShaderByteCode = ShaderBytecode.CompileFromFile("Simple.hlsl", "VSMain", "vs_5_0", ShaderFlags.PackMatrixRowMajor);
            //vertexShader = new VertexShader(device, vertexShaderByteCode);

            //pixelShaderByteCode = ShaderBytecode.CompileFromFile("Simple.hlsl", "PSMain", "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            //pixelShader = new PixelShader(device, pixelShaderByteCode);


            // Layout from VertexShader input signature
            layout = new InputLayout(
                device,
                ShaderSignature.GetInputSignature(material.vertexShaderByteCode),
                new[] {
                        new InputElement("POSITION",    0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR",       0, Format.R32G32B32A32_Float, 16, 0)
                    });

            //Instantiate Vertex buiffer from vertex data

            points = new List<Vector4>();
			float dist = 1000.0f;

			for (int i = -10; i < 11; i++) {
				points.Add(new Vector4(100.0f * i, 0.0f, dist, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
				points.Add(new Vector4(100.0f * i, 0.0f, -dist, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
				points.Add(new Vector4(dist, 0.0f, 100.0f * i, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
				points.Add(new Vector4(-dist, 0.0f, 100.0f * i, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
			}


            var bufDesc = new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SizeInBytes = points.Count * 32,
                StructureByteStride = 32
            };

            vertices = Buffer.Create(device, points.ToArray(), bufDesc);

            bufBinding = new VertexBufferBinding(vertices, 32, 0);

            constantBuffer = new Buffer(device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            });


            rastState = new RasterizerState(device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Wireframe
            });
        }

        public override void Update(float deltaTime)
		{
            var world = Matrix.Translation(Position);
            var proj = world * camera.GetViewMatrix() * camera.GetProjectionMatrix();
            Game.Context.UpdateSubresource(ref proj, constantBuffer);
		}

		public override void Draw(float deltaTime)
		{
            return;
			var context = Game.Context;

			var oldState = context.Rasterizer.State;
			context.Rasterizer.State = rastState;

			context.InputAssembler.InputLayout = layout;
			context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
			context.InputAssembler.SetVertexBuffers(0, bufBinding);
			context.VertexShader.Set(vertexShader);
			context.PixelShader.Set(pixelShader);

			context.VertexShader.SetConstantBuffer(0, constantBuffer);

			PixHelper.BeginEvent(Color.Red, "Plane Draw Event");
			context.Draw(points.Count, 0);
			PixHelper.EndEvent();

			context.Rasterizer.State = oldState;
		}

		public override void DestroyResources()
		{
            //pixelShader.Dispose();
            //vertexShader.Dispose();
            //pixelShaderByteCode.Dispose();
            //vertexShaderByteCode.Dispose();
            material.DestroyResources();
			layout.Dispose();
			vertices.Dispose();

			rastState.Dispose();

			constantBuffer.Dispose();
		}
	}
}
