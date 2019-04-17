using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Windows;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Diagnostics;
using System.Runtime;

namespace CommonStuff
{
    public class Game
    {
		public static Game Instance { protected set; get; }
		public string Name { protected set; get; }

		public List<GameComponent> Components;

	    public event Action<Vector2> ScreenResize;

		#region DirectX Stuff

		Device		device;
		SwapChain	swapChain;
		Factory		factory;
		SwapChainDescription swapDesc;

		public RenderForm		Form		{ protected set; get; }
		public Device			Device		{ get { return device; } }
		public SwapChain		SwapChain	{ get { return swapChain; } }
		public DeviceContext	Context		{ get { return device?.ImmediateContext; } }

		public InputDevice InputDevice { protected set; get; }
        #endregion

        public Renderer Renderer;

        public TextureLoader	TextureLoader	{ protected set; get; }
		public ObjLoader		ObjLoader		{ protected set; get; }
        public GameConsole      GameConsole     { protected set; get; }

        Stopwatch	watches;
		public TimeSpan	TotalTime { protected set; get; }
		


		int clientWidth, clientHeight;
		Texture2D			backBuffer;
		RenderTargetView	renderView;
		Texture2D			depthBuffer;
		DepthStencilView	depthView;
        public Color        bgcolor = Color.Black;
        public bool blockMovementInput = false;
        bool isExitRequested = false;

		public Game(string name, int width = 800, int height = 600)
		{
			Name = name;
			Components = new List<GameComponent>();
            Renderer = new Renderer(this);

            clientWidth		= width;
			clientHeight	= height;

			InputDevice		= new InputDevice(this);
			TextureLoader	= new TextureLoader(this);
			ObjLoader		= new ObjLoader();
            GameConsole     = new GameConsole(this);

			//	For animation rendering applications :
			//	http://msdn.microsoft.com/en-us/library/bb384202.aspx
			GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

			Instance = this;
        }


		protected void PrepareResources()
		{
			Form = new RenderForm(Name) {
				ClientSize = new System.Drawing.Size(clientWidth, clientHeight)
			};

			// SwapChain description
			swapDesc = new SwapChainDescription() {
				BufferCount			= 1,
				ModeDescription		= new ModeDescription(	Form.ClientSize.Width, Form.ClientSize.Height, 
															new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed			= true,
				OutputHandle		= Form.Handle,
				SampleDescription	= new SampleDescription(1, 0),
				SwapEffect			= SwapEffect.Discard,
				Usage				= Usage.RenderTargetOutput
			};

#if DEBUG
			SharpDX.Configuration.EnableObjectTracking = true;
#endif

			Device.CreateWithSwapChain(DriverType.Hardware,
				DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
				swapDesc, out device, out swapChain);

			// Ignore all windows events
			factory = swapChain.GetParent<Factory>();
			factory.MakeWindowAssociation(Form.Handle, WindowAssociationFlags.IgnoreAll);

			CreateBackBuffer();
        }


		
		public void Run()
		{
			PrepareResources();

			Initialize();
            GameConsole.Initialize();
            Components.ForEach(x => x.Initialize());

			watches = new Stopwatch();
			watches.Start();

			TotalTime = watches.Elapsed;

			RenderLoop.Run(Form, UpdateInternal);

			watches.Stop();	

			DestroyResources();
		}


		protected void UpdateInternal()
		{
			var curTime = watches.Elapsed;
			float deltaTime = (float)(curTime - TotalTime).TotalSeconds;
			TotalTime = curTime;

			ResizeCheck();

			PrepareFrame();


			Update(deltaTime);
            GameConsole.Update(deltaTime);


            Draw(deltaTime);

			
			EndFrame();

			if(isExitRequested) {
				Form.Close();
			}
		}


	    public void Exit()
	    {
		    isExitRequested = true;
	    }

		protected virtual void Initialize()
		{

		}


		protected virtual void Update(float deltaTime)
		{
			Components.ForEach(x => x.Update(deltaTime));
		}


		protected virtual void Draw(float deltaTime)
		{
			Components.ForEach(x => x.Draw(deltaTime));
            Renderer.RenderScene(deltaTime);
            GameConsole.Draw(deltaTime);
		}


		protected void PrepareFrame()
		{
			Context.ClearState();

			Context.OutputMerger.SetTargets(depthView, renderView);
			Context.Rasterizer.SetViewport(new Viewport(0, 0, Form.ClientSize.Width, Form.ClientSize.Height, 0.0f, 1.0f));
						
			Context.ClearRenderTargetView(renderView, bgcolor);
            Context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
            //Context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
        }


		protected void EndFrame()
		{
			swapChain.Present(1, PresentFlags.None);
		}


		protected void DestroyResources()
		{
			// Release all resources
			renderView.Dispose();
			backBuffer.Dispose();
			Context.ClearState();
			Context.Flush();
			Context.Dispose();
			device.Dispose();
			swapChain.Dispose();
			factory.Dispose();

			depthBuffer?.Dispose();
			depthView?.Dispose();

			Components.ForEach(x => x.DestroyResources());

			if (SharpDX.Configuration.EnableObjectTracking) Console.WriteLine(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
		}


		void ResizeCheck()
		{
			if (clientWidth != Form.ClientSize.Width || clientHeight != Form.ClientSize.Height) {
				clientWidth		= Form.ClientSize.Width;
				clientHeight	= Form.ClientSize.Height;

				backBuffer?.Dispose();
				renderView?.Dispose();
				depthBuffer?.Dispose();
				depthView?.Dispose();

				SwapChain.ResizeBuffers(swapDesc.BufferCount, clientWidth, clientHeight, swapDesc.ModeDescription.Format, SwapChainFlags.None);

				CreateBackBuffer();

				var sc = ScreenResize;
				sc?.Invoke(new Vector2(clientWidth, clientHeight));
			}
		}

		void CreateBackBuffer()
		{
			// New RenderTargetView from the backbuffer
			backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
			renderView = new RenderTargetView(device, backBuffer);

			depthBuffer = new Texture2D(device, new Texture2DDescription {
				ArraySize = 1,
				MipLevels = 1,
				Format = Format.R32_Typeless,
				BindFlags		= BindFlags.DepthStencil | BindFlags.ShaderResource,
				CpuAccessFlags	= CpuAccessFlags.None,
				OptionFlags		= ResourceOptionFlags.None,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				Width	= backBuffer.Description.Width,
				Height	= backBuffer.Description.Height
			});

			depthView = new DepthStencilView(device, depthBuffer, new DepthStencilViewDescription {
				Format = Format.D32_Float,
				Dimension = DepthStencilViewDimension.Texture2D,
				Flags = DepthStencilViewFlags.None
			});
        }


		public bool IsActive
		{
			get {
				return Form.Focused;
			}
		}
	}
}
