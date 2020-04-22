using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace Core
{
    public class Renderer
    {
        public RasterizerState rasterizerState;
        public SamplerState samplerState;
        public Camera camera;
        Game gameInstance;

        private ConstBuffers.LightBufferStruct[] m_LightBuffer;
        private Buffer PerObjConstantBuffer;
        private Buffer PerFrameConstantBuffer;
        private Buffer LightBuffer;
        private ConstBuffers.ConstBufferPerObjectStruct m_PerObjectConstBuffer;
        private ConstBuffers.ConstBufferPerFrameStruct m_PerFrameConstBuffer;


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
            PerObjConstantBuffer = new Buffer(
                gameInstance.Device,
                Utilities.SizeOf<ConstBuffers.ConstBufferPerObjectStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
            PerFrameConstantBuffer = new Buffer(
                gameInstance.Device,
                Utilities.SizeOf<ConstBuffers.ConstBufferPerFrameStruct>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
            m_LightBuffer = new ConstBuffers.LightBufferStruct[] 
            {
                new ConstBuffers.LightBufferStruct(),
                new ConstBuffers.LightBufferStruct(),
                new ConstBuffers.LightBufferStruct()
            };
            LightBuffer = new Buffer(
                gameInstance.Device,
                Utilities.SizeOf<ConstBuffers.LightBufferStruct>() * m_LightBuffer.Length,
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None, 0
            );
            //gameInstance.Components.ForEach(x => InitializeComponent(ref x));
        }

        public void RenderScene(float deltaSeconds)
        {
            foreach(GameComponent x in gameInstance.Components) {
                if (x is MeshComponent) DrawComponent((MeshComponent)x, deltaSeconds);
            }
        }

        public void DrawComponent(MeshComponent component, float deltaSeconds)
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
            gameInstance.Context.VertexShader.Set(component.Material.vertexShader);
            gameInstance.Context.PixelShader.Set(component.Material.pixelShader);
            if (component.Material.materialType != MaterialType.PBR)
                gameInstance.Context.VertexShader.SetConstantBuffer(0, component.constantBuffer);

            // Now prepare shader specific resources
            if (component.Material.materialType == MaterialType.CubeMap | component.Material.materialType == MaterialType.Unlit)
            {
                gameInstance.Context.PixelShader.SetShaderResource(0, component.Material.pixelSRV);
                gameInstance.Context.PixelShader.SetSampler(0, component.Material.sampler);
            }
            else if (component.Material.materialType == MaterialType.PBR)
            {
                gameInstance.Context.PixelShader.SetShaderResource(0, component.Material.pbrAlbedoSRV);
                gameInstance.Context.PixelShader.SetShaderResource(1, component.Material.pbrNormalSRV);
                gameInstance.Context.PixelShader.SetShaderResource(2, component.Material.pbrRoughnessSRV);
                gameInstance.Context.PixelShader.SetShaderResource(3, component.Material.pbrMetalnessSRV);
                gameInstance.Context.PixelShader.SetShaderResource(4, component.Material.pbrOcclusionSRV);
                gameInstance.Context.PixelShader.SetShaderResource(5, component.Material.radianceSRV);
                gameInstance.Context.PixelShader.SetShaderResource(6, component.Material.irradianceSRV);

                // Constant buffers

                m_PerObjectConstBuffer = new ConstBuffers.ConstBufferPerObjectStruct
                {
                    WorldMatrix = Matrix.Translation(component.Position),
                    WorldViewMatrix = Matrix.Translation(component.Position) * camera.ViewMatrix,
                    WorldViewProjMatrix = Matrix.RotationYawPitchRoll(component.Rotation.Y, component.Rotation.X, component.Rotation.Z) * Matrix.Translation(component.Position) * camera.ViewMatrix * camera.GetProjectionMatrix(),
                    textureTiling = component.Material.PropetyBlock.Tile,
                    textureShift = component.Material.PropetyBlock.Shift,

                    AlbedoColor = new Vector4(component.Material.PropetyBlock.AlbedoColor, component.Material.PropetyBlock.AlphaValue),
                    RoughnessValue = component.Material.PropetyBlock.RoughnessValue,
                    MetallicValue = component.Material.PropetyBlock.MetallicValue,

                    optionsMask0 = new Vector4(1, 1, 1, 1),
                    optionsMask1 = new Vector4(1, 0, 1, 0),
                    filler = Vector2.Zero,
                };
                m_PerFrameConstBuffer = new ConstBuffers.ConstBufferPerFrameStruct()
                {
                    Projection = camera.GetProjectionMatrix(),
                    ProjectionInv = Matrix.Invert(camera.GetProjectionMatrix()),
                    CameraPos = gameInstance.camController.CameraPosition,
                    AlphaTest = 0.5f,
                    MaxNumLightsPerTile = (uint)0,
                    NumLights = (uint)400,
                    WindowHeight = (uint)gameInstance.ScrHeight,
                    WindowWidth = (uint)gameInstance.ScrWidth,
                };

                m_LightBuffer[0].viewProjMatrix = Matrix.LookAtLH(new Vector3(-30,200,80), Vector3.Zero, Vector3.Up)
                    * Matrix.OrthoLH(30f, 30f, 0.01f, 500f);
                m_LightBuffer[0].lightTint = Vector4.One * 0.5f;
                m_LightBuffer[0].type = 1.0f;
                m_LightBuffer[0].position = new Vector3(-30, 200, 80);
                m_LightBuffer[0].direction = Matrix.RotationQuaternion(Quaternion.RotationYawPitchRoll(-MathUtil.Pi * 0.5f, -MathUtil.Pi * 0.5f, 0)).Forward;
                m_LightBuffer[0].distanceSqr = 400.0f;

                gameInstance.Context.UpdateSubresource(ref m_PerObjectConstBuffer, PerObjConstantBuffer);
                gameInstance.Context.UpdateSubresource(ref m_PerFrameConstBuffer, PerFrameConstantBuffer);
                gameInstance.Context.UpdateSubresource(m_LightBuffer, LightBuffer);

                gameInstance.Context.VertexShader.SetConstantBuffer(0, PerObjConstantBuffer);
                gameInstance.Context.VertexShader.SetConstantBuffer(1, PerFrameConstantBuffer);
                gameInstance.Context.VertexShader.SetConstantBuffer(2, LightBuffer);
                gameInstance.Context.VertexShader.Set(component.Material.vertexShader);

                gameInstance.Context.PixelShader.SetConstantBuffer(0, PerObjConstantBuffer);
                gameInstance.Context.PixelShader.SetConstantBuffer(1, PerFrameConstantBuffer);
                gameInstance.Context.PixelShader.SetConstantBuffer(2, LightBuffer);
                gameInstance.Context.PixelShader.Set(component.Material.pixelShader);

            }


            PixHelper.BeginEvent(Color.Red, "Renderer Draw Event");
            gameInstance.Context.Draw(component.vertexCount, 0);
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
