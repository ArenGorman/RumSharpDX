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
        PixelShader pixelShader;
        VertexShader vertexShader;
        CompilationResult pixelShaderByteCode;
        CompilationResult vertexShaderByteCode;
        InputLayout layout;
        Buffer vertices;
        VertexBufferBinding bufBinding;
        Camera camera;
        Buffer constantBuffer;


        Texture2D texture;
        ShaderResourceView texSRV;
        SamplerState sampler;

        string textureName = "";
        string shaderName = "";
        string modelName = "";
        int elemCount = 0;
        bool hasCubeMap;

        const string shaderFile = "Resources/Shaders/ObjModelShader.hlsl";

        RasterizerState rastState;

        public Vector3 Position;
        public Matrix Transform { set; get; } = Matrix.Identity;

        public ObjModelComponent(Game game, string modelName, string textureName, string shaderName, Camera cam, bool hasCubeMap = false) : base(game)
        {
            camera = cam;
            this.shaderName = shaderName;
            
            this.textureName = textureName;
            this.modelName = modelName;
            this.hasCubeMap = hasCubeMap;

            Position = new Vector3(0, 0, 0);
        }


        public override void Initialize()
        {
            // Compile Vertex and Pixel shaders
            vertexShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "VS" + shaderName, "vs_5_0", ShaderFlags.PackMatrixRowMajor);

            if (vertexShaderByteCode.Message != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
            }

            vertexShader = new VertexShader(Game.Device, vertexShaderByteCode);

            pixelShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "PS" + shaderName, "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            pixelShader = new PixelShader(Game.Device, pixelShaderByteCode);


            // Layout from VertexShader input signature
            int stride;
            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            layout = VertexPositionNormalTex.GetLayout(signature, out stride);

            Game.ObjLoader.LoadObjModel(modelName, out vertices, out elemCount);

            bufBinding = new VertexBufferBinding(vertices, stride, 0);

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

            if (this.hasCubeMap == true)
            {
                texture = Game.TextureLoader.LoadCubeMapFromFiles(textureName);
            }
            else
            {
                if (File.Exists(textureName))
                {
                    texture = Game.TextureLoader.LoadTextureFromFile(textureName);
                }
            }
            if (texture != null)
            {
                texSRV = new ShaderResourceView(Game.Device, texture);
            }
            
            sampler = new SamplerState(Game.Device, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear,
                ComparisonFunction = Comparison.Always,
                BorderColor = new SharpDX.Mathematics.Interop.RawColor4(1.0f, 0.0f, 0.0f, 1.0f),
                MaximumLod = int.MaxValue
            });

        }


        public override void Update(float deltaTime)
        {
            var world = Transform * Matrix.Translation(Position);
            var proj = world * camera.GetViewMatrix() * camera.GetProjectionMatrix();

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

            context.GenerateMips(texSRV);

            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.PixelShader.SetShaderResource(0, texSRV);
            context.PixelShader.SetSampler(0, sampler);
            
            PixHelper.BeginEvent(Color.Red, "ObjModel Draw Event");
            context.Draw(elemCount, 0);
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
