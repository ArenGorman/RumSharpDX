using System;
using System.IO;
using System.Collections.Generic;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

namespace CommonStuff
{
    public enum MaterialType
    {
        PBR,
        Unlit,
        Wireframe,
        ColorLines,
        CubeMap
    }

    public class PBRProperties
    {
        public Vector3 AlbedoColor = Vector3.One;
        public float AlphaValue = 1.0f;
        public float RoughnessValue = 0.5f;
        public float MetallicValue = 0.0f;
        public Vector2 Tile = Vector2.One;
        public Vector2 Shift = Vector2.Zero;
    }

    public class Material
    {
        readonly Game Game;
        readonly Device device;
        public string Name;
        public readonly MaterialType materialType;
        string textureName;

        public VertexShader vertexShader;
        public CompilationResult vertexShaderByteCode;
        public PixelShader pixelShader;
        public CompilationResult pixelShaderByteCode;

        public ShaderResourceView pixelSRV;

        #region PBR shader resource views
        List<Texture2D> pbrTextureSet;
        readonly string[] pbrSuffixes = { "albedo", "normal", "roughness", "metalness", "occlusion" };

        public ShaderResourceView pbrAlbedoSRV;
        public ShaderResourceView pbrNormalSRV;
        public ShaderResourceView pbrRoughnessSRV;
        public ShaderResourceView pbrMetalnessSRV;
        public ShaderResourceView pbrOcclusionSRV;

        public PBRProperties matProperties;
        #endregion

        public SamplerState sampler;

        public Material(Game game, string name, MaterialType type)
        {
            Game = game;
            device = game.Device;
            Name = name;
            materialType = type;
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
            vertexShaderByteCode = ShaderBytecode.CompileFromFile(pbrShaderFile, "VSMain", "vs_5_0", ShaderFlags.PackMatrixRowMajor);
            vertexShader = new VertexShader(device, vertexShaderByteCode);
            pixelShaderByteCode = ShaderBytecode.CompileFromFile(pbrShaderFile, "PSMain", "ps_5_0", ShaderFlags.PackMatrixRowMajor);
            pixelShader = new PixelShader(device, pixelShaderByteCode);
            if (vertexShaderByteCode.Message != null & pixelShaderByteCode != null)
            {
                Console.WriteLine(vertexShaderByteCode.Message);
                return false;
            }

            // Load all textures and generate SRVs
            for (int i = 0; i < pbrSuffixes.Length; i++)
            {
                // Load all pbr textures
                var textureSetItem = textureName.Split('.')[0];
                var textureSetExt = textureName.Split('.')[1];
                // We specify our textureName in constructor as if it was file without suffixes
                var pbrElementTexturePath = textureSetItem + pbrSuffixes[i] + '.' + textureSetExt;
                if (File.Exists(pbrElementTexturePath))
                {
                    pbrTextureSet[i] = Game.TextureLoader.LoadTextureFromFile(pbrElementTexturePath);
                }
                else
                {
                    pbrTextureSet[i] = Game.TextureLoader.DebugTexture();
                }

                // Create ShaderResourceViews for all components
                if (i == (int)TextureSet.albedo)
                {
                    pbrAlbedoSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrAlbedoSRV);
                }
                else if (i == (int)TextureSet.normal)
                {
                    pbrNormalSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrNormalSRV);
                }
                else if (i == (int)TextureSet.roughness)
                {
                    pbrRoughnessSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrRoughnessSRV);
                }
                else if (i == (int)TextureSet.metallness)
                {
                    pbrMetalnessSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrMetalnessSRV);
                }
                else if (i == (int)TextureSet.occlusion)
                {
                    pbrOcclusionSRV = new ShaderResourceView(Game.Device, pbrTextureSet[i]);
                    Game.Context.GenerateMips(pbrOcclusionSRV);
                }
            }


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
            if (texture != null)
            {
                texSRV = new ShaderResourceView(Game.Device, texture);
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
            bool result;
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
            return true;
        }
    }
}
