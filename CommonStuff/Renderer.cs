using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace CommonStuff
{
    public class Renderer
    {
        public RasterizerState rasterizerState;
        public SamplerState samplerState;
        public Camera camera;
        Game gameInstance;

        public Renderer(Game game)
        {
            this.gameInstance = game;
        }

        public void Initialize()
        {
            rasterizerState = new RasterizerState(gameInstance.Device, new RasterizerStateDescription {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid
            });

            samplerState = new SamplerState(gameInstance.Device, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear,
                ComparisonFunction = Comparison.Always,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 0.0f, 0.0f, 1.0f),
                MaximumLod = int.MaxValue
            });

            gameInstance.Components.ForEach(x => InitializeComponent(ref x));
        }

        //private void CompileShaders(GameComponent component)
        //{

        //}

        //public void Update(float deltaTime)
        //{

        //}

        private void InitializeComponent(ref GameComponent component)
        {
            component.Initialize();
            var layout = component.layout;
            var bufDesc = new BufferDescription
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default,
                SizeInBytes = component.points.Count * 32, // TODO stride check
                StructureByteStride = 32
            };

            component.vertBuffer = Buffer.Create(gameInstance.Device, component.points.ToArray(), bufDesc);
            component.bufBinding = new VertexBufferBinding(component.vertBuffer, 32, 0);
            component.constantBuffer = new Buffer(gameInstance.Device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            });
        }

        public void RenderScene(float deltaSeconds)
        {
            gameInstance.Components.ForEach(x => DrawComponent(x, deltaSeconds));
        }

        public void DrawComponent(GameComponent component, float deltaSeconds)
        {
            component.Update(deltaSeconds);

            //var context = gameInstance.Context;
            var oldState = gameInstance.Context.Rasterizer.State;

            if (component.rastState != null)
            {
                gameInstance.Context.Rasterizer.State = component.rastState;
            }
            else
            {
                gameInstance.Context.Rasterizer.State = rasterizerState;
            }

            gameInstance.Context.InputAssembler.InputLayout = component.layout;
            gameInstance.Context.InputAssembler.PrimitiveTopology = component.primTopology;
            gameInstance.Context.InputAssembler.SetVertexBuffers(0, component.bufBinding);
            gameInstance.Context.VertexShader.Set(component.material.vertexShader);
            gameInstance.Context.PixelShader.Set(component.material.pixelShader);

            gameInstance.Context.VertexShader.SetConstantBuffer(0, component.constantBuffer);

            PixHelper.BeginEvent(Color.Red, "Renderer Draw Event");
            gameInstance.Context.Draw(component.points.Count, 0);
            PixHelper.EndEvent();

            gameInstance.Context.Rasterizer.State = oldState;
        }

        public void DestroyResources()
        {
            samplerState.Dispose();
            rasterizerState.Dispose();
        }
    }
}
