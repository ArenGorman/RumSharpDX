using System;
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
using System.IO;
using CommonStuff.VertexStructures;


namespace CommonStuff
{
    public class ObjModelComponent : GameComponent
    {

        Buffer vertices;
        Buffer indices;
        Camera camera;

        string modelName = "";

        int elemCount = 0;

        public Matrix Transform { set; get; } = Matrix.Identity;

        public ObjModelComponent(Game game, string modelName, string textureName, MaterialType materialType, Camera cam, Material mat=null) : base(game)
        {
            camera = cam;
            this.modelName = modelName;
            Renderer = game.Renderer;
            if (mat != null)
            {
                this.material = mat;
            }
            else
            {
                this.material = new Material(game, Path.GetFileName(modelName).Split('.')[0], materialType);
                this.material.textureName = textureName;
            }

            Position = new Vector3(0);
        }

        public override void Initialize()
        {
            material.Initialize();
            
            // Layout from VertexShader input signature
            int stride;
            var signature = ShaderSignature.GetInputSignature(material.vertexShaderByteCode);

            if (material.materialType == MaterialType.PBR)
                layout = VertexPosColUV01NrmTan.GetLayout(signature, out stride);
            else
                layout = VertexPositionNormalTex.GetLayout(signature, out stride);

            if (material.materialType == MaterialType.PBR)
                gameInstance.ObjLoader.LoadObjModel(modelName, out vertices, out indices, out elemCount, true);
            else
                gameInstance.ObjLoader.LoadObjModel(modelName, out vertices, out indices, out elemCount);



            //indexBuffer = Buffer.Create(gameInstance.Device, renderer.Geometry.Indexes, IndexBufferDescription);

            vertexCount = elemCount;
            this.constantBuffer = new Buffer(gameInstance.Device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            });
            vertBuffer = vertices;
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
            var world = Transform * Matrix.Translation(Position);
            var proj = world * camera.GetViewMatrix() * camera.GetProjectionMatrix();

            //Constant buffers
            if (material.materialType != MaterialType.PBR)
                gameInstance.Context.UpdateSubresource(ref proj, this.constantBuffer);
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
            vertices.Dispose();

            rastState.Dispose();

            constantBuffer.Dispose();
        }


    }
}
