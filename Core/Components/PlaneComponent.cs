using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;


namespace Core.Components
{
	public class PlaneComponent : MeshComponent
    {
        Camera camera;

		public PlaneComponent(Game game, Camera cam, Material mat = null) : base(game)
		{
			camera = cam;
            primTopology = PrimitiveTopology.LineList;
            Renderer = game.Renderer;
            Position = new Vector3(0);
		}

		public override void Initialize()
		{
            Material = new Material(gameInstance, "PlaneMaterial", MaterialType.ColorLines);
            Material.Initialize();
            // Layout from VertexShader input signature
            layout = new InputLayout(
                gameInstance.Device,
                ShaderSignature.GetInputSignature(Material.vertexShaderByteCode),
                new[] {
                        new InputElement("POSITION",    0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR",       0, Format.R32G32B32A32_Float, 16, 0)
                    });

            //Instantiate Vertex buiffer from vertex data
            points = new List<Vector4>();
			float dist = 100;
            int divisions = 5;

			for (int i = -divisions; i < divisions + 1; i++) {
				points.Add(new Vector4(dist / divisions * i, 0.0f, dist, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
				points.Add(new Vector4(dist / divisions * i, 0.0f, -dist, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
				points.Add(new Vector4(dist, 0.0f, dist / divisions * i, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
				points.Add(new Vector4(-dist, 0.0f, dist / divisions * i, 1.0f)); points.Add(new Vector4(0.7f, 0.7f, 0.7f, 1.0f));
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

            vertBuffer = Buffer.Create(gameInstance.Device, points.ToArray(), bufDesc);
            bufBinding = new VertexBufferBinding(vertBuffer, 32, 0);

            bufDesc = new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            };
            vertexCount = points.Count;
            constantBuffer = new Buffer(gameInstance.Device, bufDesc);

            rastState = new RasterizerState(gameInstance.Device, new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                FillMode = FillMode.Wireframe
            });
        }

        public override void Update(float deltaTime)
		{
            var world = Matrix.Translation(Position);
            var proj = world * camera.GetViewMatrix() * camera.GetProjectionMatrix();
            base.gameInstance.Context.UpdateSubresource(ref proj, constantBuffer);
		}

		public override void Draw(float deltaTime)
		{
            return;
			//var context = Game.Context;

			//var oldState = context.Rasterizer.State;
			//context.Rasterizer.State = rastState;

			//context.InputAssembler.InputLayout = layout;
			//context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
			//context.InputAssembler.SetVertexBuffers(0, bufBinding);
			//context.VertexShader.Set(vertexShader);
			//context.PixelShader.Set(pixelShader);

			//context.VertexShader.SetConstantBuffer(0, constantBuffer);

			//PixHelper.BeginEvent(Color.Red, "Plane Draw Event");
			//context.Draw(points.Count, 0);
			//PixHelper.EndEvent();

			//context.Rasterizer.State = oldState;
		}

		public override void DestroyResources()
		{
            //pixelShader.Dispose();
            //vertexShader.Dispose();
            //pixelShaderByteCode.Dispose();
            //vertexShaderByteCode.Dispose();
            Material.DestroyResources();
			layout.Dispose();
			//vertices.Dispose();

			rastState.Dispose();

			constantBuffer.Dispose();
		}
	}
}
