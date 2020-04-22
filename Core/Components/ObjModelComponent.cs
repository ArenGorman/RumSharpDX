using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.Mathematics;

using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using System.IO;
using Core.VertexStructures;


namespace Core.Components
{
    public class ObjModelComponent : MeshComponent
    {
        Camera camera;

        string modelName = "";
        int elemCount = 0;

        public ObjModelComponent(Game game, string modelName, string textureName, MaterialType materialType, Camera cam, Material mat=null) : base(game)
        {
            camera = cam;
            this.modelName = modelName;
            Renderer = game.Renderer;
            if (mat != null)
            {
                Material = mat;
            }
            else
            {
                Material = new Material(game, Path.GetFileName(modelName).Split('.')[0], materialType)
                {
                    textureName = textureName
                };
            }

            Position = default(Vector3);
        }

        public override void Initialize()
        {
            Material.Initialize();
            
            // Layout from VertexShader input signature
            int stride;
            var signature = ShaderSignature.GetInputSignature(Material.vertexShaderByteCode);

            if (Material.materialType == MaterialType.PBR)
                layout = VertexPosColUV01NrmTan.GetLayout(signature, out stride);
            else
                layout = VertexPositionNormalTex.GetLayout(signature, out stride);

            if (Material.materialType == MaterialType.PBR)
                gameInstance.ObjLoader.LoadObjModel(modelName, out vertBuffer, out indexBuffer, out elemCount, true);
            else
                gameInstance.ObjLoader.LoadObjModel(modelName, out vertBuffer, out indexBuffer, out elemCount);



            //indexBuffer = Buffer.Create(gameInstance.Device, renderer.Geometry.Indexes, IndexBufferDescription);

            vertexCount = elemCount;
            constantBuffer = new Buffer(gameInstance.Device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            });
            bufBinding = new VertexBufferBinding(vertBuffer, stride, 0);
            primTopology = PrimitiveTopology.TriangleList;
            rastState = new RasterizerState(gameInstance.Device, new RasterizerStateDescription
            {
                CullMode = CullMode.Back,
                FillMode = FillMode.Solid
            });
        }

        public override void Update(float deltaTime)
        {
            var world = Matrix.Translation(Position);
            //var orient = Matrix.RotationYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);
            var orient = Quaternion.RotationAxis(new Vector3(1, 0, 0), Rotation.X) *
                Quaternion.RotationAxis(new Vector3(0, 1, 0), Rotation.Y) *
                Quaternion.RotationAxis(new Vector3(0, 0, 1), Rotation.Z);
            var proj = Matrix.RotationQuaternion(orient) * world * camera.GetViewMatrix() * camera.GetProjectionMatrix();

            //Constant buffers
            if (Material.materialType != MaterialType.PBR)
                gameInstance.Context.UpdateSubresource(ref proj, constantBuffer);
        }


        public override void Draw(float deltaTime)
        {
            return;
            //var context = Game.Context;

            //var oldState = context.Rasterizer.State;
            //context.Rasterizer.State = rastState;

            //context.InputAssembler.InputLayout = layout;
            //context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            //context.InputAssembler.SetVertexBuffers(0, bufBinding);
            //context.VertexShader.Set(vertexShader);
            //context.PixelShader.Set(pixelShader);
            //context.GenerateMips(texSRV);
            //context.PixelShader.SetShaderResource(0, pbrAlbedoSRV);
            //context.PixelShader.SetShaderResource(1, pbrNormalSRV);
            //context.PixelShader.SetShaderResource(2, pbrRoughnessSRV);
            //context.PixelShader.SetShaderResource(3, pbrMetalnessSRV);
            //context.PixelShader.SetShaderResource(4, pbrOcclusionSRV);
            //context.VertexShader.SetConstantBuffer(0, constantBuffer);
            ////context.PixelShader.SetShaderResource(0, texSRV);
            //context.PixelShader.SetSampler(0, sampler);
            //PixHelper.BeginEvent(Color.Red, "ObjModel Draw Event");
            //context.Draw(elemCount, 0);
            //PixHelper.EndEvent();

            //context.Rasterizer.State = oldState;
        }

        public override void DestroyResources()
        {
            layout.Dispose();
            vertBuffer.Dispose();

            rastState.Dispose();

            constantBuffer.Dispose();
        }


    }
}
