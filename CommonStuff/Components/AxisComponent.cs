using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace CommonStuff
{
    public class AxisComponent : GameComponent
    {
        Camera camera;
        Buffer vertices;

        public AxisComponent(Game game, Renderer renderer, Camera cam, Material material = null) : base(game)
        {
            this.Game = game;
            this.camera = cam;
            this.Renderer = renderer;
            this.primTopology = PrimitiveTopology.LineList;
            Position = new Vector3(0);
        }

        public override void Initialize()
        {
            material = new Material(Game, "AxisMaterial", MaterialType.ColorLines);

            layout = new InputLayout(
                Game.Device,
                ShaderSignature.GetInputSignature(material.vertexShaderByteCode),
                new[] {
                        new InputElement("POSITION",    0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR",       0, Format.R32G32B32A32_Float, 16, 0)
                    });

            rastState = new RasterizerState(Game.Device, new RasterizerStateDescription {
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
            vertices = Buffer.Create(Game.Device, points.ToArray(), bufDesc);
            bufBinding = new VertexBufferBinding(vertices, 32, 0);

            constantBuffer = new Buffer(Game.Device, new BufferDescription
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
            Game.Context.UpdateSubresource(ref proj, constantBuffer);
        }

        public override void DestroyResources()
        {
            material.DestroyResources();

        }
    }
}
