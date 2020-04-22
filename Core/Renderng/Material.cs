using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace Core
{
    public enum MaterialType
    {
        PBR,
        Unlit,
        Wireframe,
        ColorLines,
        CubeMap
    }

    public class IncludeShader : SharpDX.CallbackBase, Include
    {
        private string includeDirectory;
        public string subPath;

        public IncludeShader(string shadersDirectory)
        {
            includeDirectory = shadersDirectory;
        }

        public void Close(Stream stream)
        {
            stream.Dispose();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            return new FileStream(includeDirectory + subPath + fileName, FileMode.Open);
        }
    }

    public class MaterialPropetyBlock
    {
        public Vector3 AlbedoColor = Vector3.One;
        public float AlphaValue = 1.0f;
        public float RoughnessValue = 1.0f;
        public float MetallicValue = 0.5f;
        public Vector2 Tile = Vector2.One;
        public Vector2 Shift = Vector2.Zero;
    };

    public class Material
    {
        private static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string shadersDirectory = assemblyFolder + "/Resources/Shaders/";
        private static IncludeShader includeShader = new IncludeShader(shadersDirectory);

        readonly Game Game;
        readonly Device device;
        public string Name;
        public readonly MaterialType materialType;
        public string textureName { get; set; }

        public VertexShader vertexShader;
        public CompilationResult vertexShaderByteCode;
        public PixelShader pixelShader;
        public CompilationResult pixelShaderByteCode;

        public ShaderResourceView pixelSRV;

        #region PBR shader resource views
        List<Texture2D> pbrTextureSet = new List<Texture2D> { };
        readonly string[] pbrSuffixes = { "albedo", "normal", "roughness", "metalness", "occlusion" };

        public ShaderResourceView pbrAlbedoSRV;
        public ShaderResourceView pbrNormalSRV;
        public ShaderResourceView pbrRoughnessSRV;
        public ShaderResourceView pbrMetalnessSRV;
        public ShaderResourceView pbrOcclusionSRV;
        public ShaderResourceView radianceSRV;
        public ShaderResourceView irradianceSRV;
        public MaterialPropetyBlock PropetyBlock = new MaterialPropetyBlock() { AlbedoColor = Vector3.One * 0.8f };

        #endregion

        public SamplerState sampler;

        public Material(Game game, string name, MaterialType type)
        {
            Game = game;
            device = game.Device;
            Name = name;
            materialType = type;
        }

        public void Initialize()
        {
            CompileShaders();
        }

        public void DestroyResources()
        {
            vertexShader.Dispose();
            vertexShaderByteCode.Dispose();
            pixelShader.Dispose();
            pixelShaderByteCode.Dispose();
        }

        bool PreparePBR()
        {
            
            const string pbrShaderFile = "Resources/Shaders/PBRShader.hlsl";

            vertexShaderByteCode = ShaderBytecode.CompileFromFile(pbrShaderFile, "VSMain", "vs_5_0", ShaderFlags.PackMatrixRowMajor, EffectFlags.None, null, includeShader);
            if (vertexShaderByteCode.Message != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
                return false;
            }
            vertexShader = new VertexShader(device, vertexShaderByteCode);

            pixelShaderByteCode = ShaderBytecode.CompileFromFile(pbrShaderFile, "PSMain", "ps_5_0", ShaderFlags.PackMatrixRowMajor, EffectFlags.None, null, includeShader);
            if (pixelShaderByteCode.Message != null)
            {
                Console.WriteLine(pixelShaderByteCode.Message);
                return false;
            }
            pixelShader = new PixelShader(device, pixelShaderByteCode);

            // Load object textures and generate SRVs
            for (int i = 0; i < pbrSuffixes.Length; i++)
            {
                // Load all pbr textures
                var textureSetItem = textureName.Split('.')[0];
                var textureSetExt = textureName.Split('.')[1];
                // We specify our textureName in constructor as if it was file without suffixes
                var pbrElementTexturePath = textureSetItem + '_' + pbrSuffixes[i] + '.' + textureSetExt;
                if (File.Exists(pbrElementTexturePath))
                {
                    pbrTextureSet.Add(Game.TextureLoader.LoadTextureFromFile(pbrElementTexturePath));
                }
                else
                {
                    pbrTextureSet.Add(Game.TextureLoader.DebugTexture());
                }

                // Create ShaderResourceViews for all components
                if (i == (int)Loaders.TextureSet.albedo)
                {
                    pbrAlbedoSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrAlbedoSRV);
                }
                else if (i == (int)Loaders.TextureSet.normal)
                {
                    pbrNormalSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrNormalSRV);
                }
                else if (i == (int)Loaders.TextureSet.roughness)
                {
                    pbrRoughnessSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrRoughnessSRV);
                }
                else if (i == (int)Loaders.TextureSet.metallness)
                {
                    pbrMetalnessSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrMetalnessSRV);
                }
                else if (i == (int)Loaders.TextureSet.occlusion)
                {
                    pbrOcclusionSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrOcclusionSRV);
                }
            }

            // Load skybox textures

            pbrTextureSet.Add(Game.TextureLoader.LoadCubeMapFromFiles("Resources/Textures/miramar_?.bmp"));
            pbrTextureSet.Add(Game.TextureLoader.LoadCubeMapFromFiles("Resources/Textures/miramar_?_ir.bmp"));
            radianceSRV = new ShaderResourceView(Game.Device, pbrTextureSet[5]);
            irradianceSRV = new ShaderResourceView(Game.Device, pbrTextureSet[6]);



            return true;
        }

        bool PrepareColorLines()
        {
            vertexShaderByteCode = ShaderBytecode.CompileFromFile("Simple.hlsl", "VSMain", "vs_5_0", ShaderFlags.PackMatrixRowMajor);
            vertexShader = new VertexShader(device, vertexShaderByteCode);
            pixelShaderByteCode = ShaderBytecode.CompileFromFile("Simple.hlsl", "PSMain", "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            pixelShader = new PixelShader(device, pixelShaderByteCode);
            if (vertexShaderByteCode.Message != null & pixelShaderByteCode != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
                return false;
            }
            else { return true; }
        }

        bool PrepareUnlit()
        {
            const string shaderFile = "Resources/Shaders/ObjModelShader.hlsl";
            vertexShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "VSMain", "vs_5_0", ShaderFlags.PackMatrixRowMajor);
            vertexShader = new VertexShader(device, vertexShaderByteCode);
            pixelShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "PSMain", "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            pixelShader = new PixelShader(device, pixelShaderByteCode);
            if (vertexShaderByteCode.Message != null & pixelShaderByteCode != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
                return false;
            }
            var texture = Game.TextureLoader.LoadTextureFromFile(textureName);
            if (texture != null)
            {
                pixelSRV = new ShaderResourceView(Game.Device, texture);
            }
            return true;
        }

        bool PrepareWireframe()
        {
            return true;
        }

        bool PrepareCubeMap()
        {
            const string shaderFile = "Resources/Shaders/ObjModelShader.hlsl";
            vertexShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "VSCube", "vs_5_0", ShaderFlags.PackMatrixRowMajor);
            vertexShader = new VertexShader(device, vertexShaderByteCode);
            pixelShaderByteCode = ShaderBytecode.CompileFromFile(shaderFile, "PSCube", "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            pixelShader = new PixelShader(device, pixelShaderByteCode);
            if (vertexShaderByteCode.Message != null & pixelShaderByteCode != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
                return false;
            }
            var texture = Game.TextureLoader.LoadCubeMapFromFiles(textureName);
            if (texture != null)
            {
                pixelSRV = new ShaderResourceView(Game.Device, texture);
            }

            return true;
        }

        public bool CompileShaders()
        {
            bool result = false;
            switch (materialType)
            {
                case MaterialType.PBR:
                    {
                        result = PreparePBR();
                        break;
                    }
                case MaterialType.ColorLines:
                    {
                        result = PrepareColorLines();
                        break;
                    }
                case MaterialType.Unlit:
                    {
                        result = PrepareUnlit();
                        break;
                    }
                case MaterialType.Wireframe:
                    {
                        result = PrepareWireframe();
                        break;
                    }
                case MaterialType.CubeMap:
                    {
                        result = PrepareCubeMap();
                        break;
                    }
            }
            if (materialType != MaterialType.PBR)
            {
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
            else
            {
                sampler = new SamplerState(Game.Device, new SamplerStateDescription()
                {
                    Filter = Filter.MinMagMipLinear,
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    ComparisonFunction = Comparison.Never,
                    MaximumAnisotropy = 16,
                    MipLodBias = 0,
                    MinimumLod = -float.MaxValue,
                    MaximumLod = float.MaxValue
                });
            }
            if (result) { return result; } else { throw(new Exception("Shader compilation failed")); }
        }
    }
}
