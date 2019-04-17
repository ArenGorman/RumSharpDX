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
        ////PixelShader pixelShader;
        ////PixelShader pixelPBRShader;
        ////VertexShader vertexShader;
        ////CompilationResult pixelShaderByteCode;
        ////CompilationResult pixelPBRShaderByteCode;
        ////CompilationResult vertexShaderByteCode;
        Buffer vertices;
        Camera camera;
        Material material;

        //Buffer perFrameConstantBuffer;
        //Buffer perObjConstantBuffer;
        //Buffer LightBuffer;

        // REMOVE
        Buffer constantBuffer;

        ////ConstBuffers.ConstBufferPerFrameStruct s_perFrameCB;
        ////ConstBuffers.ConstBufferPerObjectStruct s_perObjCB;
        ////ConstBuffers.ConstBufferDirLightStruct s_DirLightCB;


        ////Texture2D texture;
        ////List<Texture2D> pbrTextureSet;

        ////ShaderResourceView texSRV;
        ////ShaderResourceView pbrAlbedoSRV;
        ////ShaderResourceView pbrNormalSRV;
        ////ShaderResourceView pbrRoughnessSRV;
        ////ShaderResourceView pbrMetalnessSRV;
        ////ShaderResourceView pbrOcclusionSRV;

        ////SamplerState sampler;

        ////string textureName = "";
        ////string shaderName = "";
        string modelName = "";
        ////readonly string[] pbrSuffixes = { "albedo", "normal", "roughness", "metalness", "occlusion" };
        int elemCount = 0;
        bool hasCubeMap;

        //const string shaderFile = "Resources/Shaders/ObjModelShader.hlsl";
        //const string pbrShaderFile = "Resources/Shaders/PBRShader.hlsl";

        public Matrix Transform { set; get; } = Matrix.Identity;

        public ObjModelComponent(Game game, string modelName, string textureName, MaterialType materialType, Material mat, Camera cam, bool hasCubeMap = false) : base(game)
        {
            camera = cam;
            this.modelName = modelName;
            if (mat != null)
            {
                material = mat;
            }
            else
            {
                material = new Material(game, Path.GetFileName(modelName), materialType);
            }
            this.hasCubeMap = hasCubeMap;

            Position = new Vector3(0);
        }


        public override void Initialize()
        {

            // Compile Vertex and Pixel shaders
            //vertexShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "VS" + shaderName, "vs_5_0", ShaderFlags.PackMatrixRowMajor);

            //if (vertexShaderByteCode.Message != null)
            //{
            //    Console.WriteLine(vertexShaderByteCode.Message);
            //}

            //vertexShader = new VertexShader(Game.Device, vertexShaderByteCode);

            ////pixelShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "PS" + shaderName, "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            ////pixelShader = new PixelShader(Game.Device, pixelShaderByteCode);

            //pixelPBRShaderByteCode = ShaderBytecode.CompileFromFile(pbrShaderFile, "PS" + shaderName, "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            //pixelPBRShader = new PixelShader(Game.Device, pixelPBRShaderByteCode);


            // Layout from VertexShader input signature
            int stride;
            var signature = ShaderSignature.GetInputSignature(material.vertexShaderByteCode);
            layout = VertexPositionNormalTex.GetLayout(signature, out stride);

            Game.ObjLoader.LoadObjModel(modelName, out vertices, out elemCount);

            constantBuffer = new Buffer(Game.Device, new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utilities.SizeOf<Matrix>(),
                Usage = ResourceUsage.Default
            });


            rastState = new RasterizerState(Game.Device, new RasterizerStateDescription
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
            Game.Context.UpdateSubresource(ref proj, constantBuffer);
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
