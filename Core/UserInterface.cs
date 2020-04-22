using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Core
{
    public class UserInterface : GameComponent
    {
        private readonly IntPtr _context;
        private readonly ImGuiIOPtr _io;
        public Rectangle FormBounds { get; set; }
        public VertexShader VertexShader { get; set; }
        public PixelShader PixelShader { get; set; }
        private Blob PSBlob { get; set; }
        private Viewport Viewport;
        private Buffer ConstantBuffer { get; set; }
        private Buffer VertexBuffer { get; set; }
        private Buffer IndexBuffer { get; set; }
        public InputLayout Layout { get; set; }

        public readonly int sizeOfImDrawIdx = 2;
        public readonly int sizeOfImDrawVert = 20;
        private int _indexBufferSize;
        public int IndexBufferSizeBytes { get; set; }
        private bool _transparentState1 = true;
        private int _vertexBufferSize;

        private int VertexBufferSize
        {
            get => _vertexBufferSize;
            set
            {
                VertexBufferSizeBytes = value * sizeOfImDrawVert;
                _vertexBufferSize = value;
            }
        }

        public int VertexBufferSizeBytes { get; set; }

        private int IndexBufferSize
        {
            get => _indexBufferSize;
            set
            {
                IndexBufferSizeBytes = value * sizeOfImDrawIdx;
                _indexBufferSize = value;
            }
        }

        unsafe public UserInterface(Game game) : base(game)
        {
            //throw new NotImplementedException();
            
            _context = ImGui.CreateContext();
            ImGui.SetCurrentContext(_context);
            _io = ImGui.GetIO();
            
            ImGui.StyleColorsDark();
            

            Console.WriteLine("UI init");
        }
        public unsafe override void Initialize()
        {
            _io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out var width, out var height, out var bytesPerPixel);
            
            var rect = new DataRectangle(new IntPtr(pixelData), width * bytesPerPixel);
            var texture = new Texture2D(gameInstance.Device, new Texture2DDescription {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            }, rect);
            sizeOfImDrawVert = Utilities.SizeOf<ImDrawVert>();
            sizeOfImDrawIdx = Utilities.SizeOf<ushort>();
            VertexBufferSize = 10000;
            IndexBufferSize = 30000;

            var shaderResourceView = new ShaderResourceView(gameInstance.Device, texture);
            var vertexShaderByteCode =
                ShaderBytecode.CompileFromFile("Resources/Shaders/ImGuiVS.hlsl", "VS", "vs_4_0");


            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Resources/Shaders/ImGuiPS.hlsl", "PS", "ps_4_0");
            VertexShader = new VertexShader(gameInstance.Device, vertexShaderByteCode);
            PixelShader = new PixelShader(gameInstance.Device, pixelShaderByteCode);

            VertexBuffer = new Buffer(gameInstance.Device,
                new BufferDescription
                {
                    Usage = ResourceUsage.Dynamic,
                    BindFlags = BindFlags.VertexBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    SizeInBytes = VertexBufferSizeBytes
                });

            IndexBuffer = new Buffer(gameInstance.Device,
                new BufferDescription
                {
                    Usage = ResourceUsage.Dynamic,
                    BindFlags = BindFlags.IndexBuffer,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    SizeInBytes = IndexBufferSizeBytes
                });

            ConstantBuffer = new Buffer(Dx11.D11Device,
                new BufferDescription
                {
                    BindFlags = BindFlags.ConstantBuffer,
                    Usage = ResourceUsage.Dynamic,
                    OptionFlags = ResourceOptionFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    SizeInBytes = Utilities.SizeOf<Matrix4x4>()
                });

            var inputElements = new[]
            {
                new InputElement
                {
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    Format = Format.R32G32_Float,
                    Slot = 0,
                    AlignedByteOffset = 0,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                },
                new InputElement
                {
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    Format = Format.R32G32_Float,
                    Slot = 0,
                    AlignedByteOffset = InputElement.AppendAligned,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                },
                new InputElement
                {
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    Format = Format.R8G8B8A8_UNorm,
                    Slot = 0,
                    AlignedByteOffset = InputElement.AppendAligned,
                    Classification = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                }
            };

            Layout = new InputLayout(gameInstance.Device, ShaderSignature.GetInputSignature(vertexShaderByteCode),
                inputElements);

            //ImGui.NewFrame();
            //ImGui.ShowDemoWindow();
            //ImGui.Begin("Hello world!");
            //ImGui.Text("This is some useful text");
            //ImGui.End();
        }

        public override void Update(float deltaTime)
        {
            //throw new NotImplementedException();
        }
        public override void Draw(float deltaTime)
        {
            ImGui.Render();
            if (_io.DisplaySize.X <= 0.0f || _io.DisplaySize.Y <= 0.0f)
                return;
            var data = ImGui.GetDrawData();

        }

        private ShaderResourceView _PrepareUITexture()
        {
            throw new NotImplementedException();
        }

        public override void DestroyResources()
        {
            ImGui.DestroyContext();
        }
    }
}
