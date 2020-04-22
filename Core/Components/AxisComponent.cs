using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace Core.Components
{
    public class AxisComponent : MeshComponent
    {
        Camera camera;

        public AxisComponent(Game game, Renderer renderer, Camera cam, Material mat = null) : base(game)
        {
            camera = cam;
            Renderer = renderer;
            primTopology = PrimitiveTopology.LineList;
            Position = new Vector3(0);
        }

        public override void Initialize()
        {
            Material = new Material(gameInstance, "AxisMaterial", MaterialType.ColorLines);
            Material.Initialize();
            layout = new InputLayout(
                gameInstance.Device,
                ShaderSignature.GetInputSignature(Material.vertexShaderByteCode),
                new[] {
                        new InputElement("POSITION",    0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR",       0, Format.R32G32B32A32_Float, 16, 0)
                    });

            rastState = new RasterizerState(gameInstance.Device, new RasterizerStateDescription {
                CullMode = CullMode.None,
                FillMode = FillMode.Wireframe
            });

            points = new List<Vector4>();
            points.Add(Vector4.Zero); points.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
            points.Add(Vector4.UnitX * 100.0f); points.Add(new Vector4(1.0f, 0.0f, 0.0f, 1.0f));

            points.Add(Vector4.Zero); points.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));
            points.Add(Vector4.UnitY * 100.0f); points.Add(new Vector4(0.0f, 1.0f, 0.0f, 1.0f));

            points.Add(Vector4.Zero); points.Add(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            points.Add(Vector4.UnitZ * 100.0f); points.Add(new Vector4(0.0f, 0.0f, 1.0f, 1.0f));

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
            vertexCount = points.Count;

            constantBuffer = new Buffer(gameInstance.Device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            });
        }

       

        public override void Draw(float deltaTime)
        {
            return;
        }

        public override void Update(float deltaTime)
        {
            var world = Matrix.Translation(Position);
            var proj = world * camera.GetViewMatrix() * camera.GetProjectionMatrix();
            gameInstance.Context.UpdateSubresource(ref proj, constantBuffer);
        }

        public override void DestroyResources()
        {
            Material.DestroyResources();

        }
    }
}
