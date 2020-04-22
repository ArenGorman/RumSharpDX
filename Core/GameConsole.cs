using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;

using RawRectangleF = SharpDX.Mathematics.Interop.RawRectangleF;
using FontFactory = SharpDX.DirectWrite.Factory;


namespace Core
{
    public class GameConsole : GameComponent
    { 
        public bool ShowConsole {protected set; get; }
        SharpDX.Direct2D1.Factory D2DFactory;
        public int sizeX = 600, sizeY = 525;
        public byte textLimit = 19;
        Vector2 consolePos;
        List<string> consoleLog;
        string rawInput;
        string inputInvitation;
        RenderTarget renderTarget2D;
        RenderTargetView renderView;
        Texture2D backBuffer;
        InputDevice input;
        Surface surface;
        SolidColorBrush solidColorBrush;
        RectangleGeometry backgroundGeometry;
        RoundedRectangleGeometry inputGeometry;
        GameTextWriter textWriter;
        OutputCapture outCapture;
        LuaCompiller compiller;


        public GameConsole(Game game) : base(game)
        {
            this.gameInstance = game;
            
        }

        public override void Initialize()
        {
            outCapture = new OutputCapture();
            
            this.input = gameInstance.InputDevice;
            ShowConsole = false;
            rawInput = "";
            consoleLog = new List<string>(new string[] { "" });
            consolePos = new Vector2(20.0f, 20.0f);

            gameInstance.Form.KeyPress += Input;
            compiller = new LuaCompiller();

            D2DFactory = new SharpDX.Direct2D1.Factory();
            backgroundGeometry = new RectangleGeometry(D2DFactory, new RawRectangleF(consolePos.X, consolePos.Y, sizeX, sizeY));
            inputGeometry = new RoundedRectangleGeometry(D2DFactory, new RoundedRectangle() { RadiusX=3, RadiusY=3, Rect=new RectangleF(24, sizeY - 28, sizeX - 28, 24) });
            backBuffer = Texture2D.FromSwapChain<Texture2D>(gameInstance.SwapChain, 0);
            renderView = new RenderTargetView(gameInstance.Device, backBuffer);

            surface = backBuffer.QueryInterface<Surface>();
            
            renderTarget2D = new SharpDX.Direct2D1.RenderTarget
                (
                    D2DFactory, 
                    surface, 
                    new SharpDX.Direct2D1.RenderTargetProperties
                    (
                        new SharpDX.Direct2D1.PixelFormat
                        (
                            Format.Unknown,
                            SharpDX.Direct2D1.AlphaMode.Premultiplied
                        )
                    )
                );

            solidColorBrush = new SolidColorBrush(renderTarget2D, new SharpDX.Color(1.0f, 1.0f, 1.0f, 0.4f));
            textWriter = new GameTextWriter(renderTarget2D);
            textWriter.Offset = consolePos.X;


            Console.WriteLine("Text console initiated");
        }

        public override void Update(float deltaTime)
        { 
            var consoleOutput = outCapture.Captured.ToString();
            consoleLog.Clear();
            // Need to remove last LF symbol to avoid useless empty string after split
            //consoleLog = new List<string>(consoleOutput.Remove(consoleOutput.Length - 1).Split(new char[] { '\n' }));
            consoleLog = new List<string>(consoleOutput.Remove(consoleOutput.Length - 1).Split(new char[] { '\n' }));
            // Blinking input invitation
            if (gameInstance.TotalTime.Seconds % 2 == 0) { inputInvitation = "  "; }
            else { inputInvitation = "_ "; }
        }

        public override void Draw(float deltaTime)
        {
            if (ShowConsole == true)
            {
                renderTarget2D.BeginDraw();
                solidColorBrush.Color = new SharpDX.Color(1.0f, 1.0f, 1.0f, 0.4f);
                renderTarget2D.FillGeometry(backgroundGeometry, solidColorBrush, null);
                solidColorBrush.Color = new SharpDX.Color(0.1f, 0.2f, 0.6f, 0.2f);
                renderTarget2D.FillGeometry(inputGeometry, solidColorBrush, null);
                if (consoleLog.Count > textLimit)
                {
                    // print only last lines
                    textWriter.Write
                        (
                            consoleLog.GetRange(consoleLog.Count - textLimit, textLimit),
                            new Vector2(sizeX, sizeY),
                            new Vector2(0,0)
                        );
                }
                else
                {
                    textWriter.Write(consoleLog, new Vector2(sizeX, sizeY), new Vector2(0, 0));
                }
                
                textWriter.Write(new List<string> { rawInput + inputInvitation }, new Vector2(sizeX-48, 24), new Vector2(0, sizeY-48));
                renderTarget2D.EndDraw();
                gameInstance.SwapChain.Present(0, PresentFlags.None);
            }
        }

        public override void DestroyResources()
        {
            renderTarget2D.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();
        }

        public void Input(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.NumPad0) // IDK why but tilde has the same code as numpad0
            {
                ShowConsole = !ShowConsole;
                gameInstance.blockMovementInput = ShowConsole;
            }
            else if(rawInput.Length > 0 && (int)e.KeyChar == (int)Keys.Back )
            {
                rawInput = rawInput.Substring(0, rawInput.Length - 1);
            }
            else if ((int)e.KeyChar == (int)Keys.Enter)
            {
                Console.WriteLine(rawInput);
                try
                {
                    string res = "";
                    res = compiller.DoCode(rawInput);
                    Console.WriteLine(res);
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }

                rawInput = "";
            }
            else if (gameInstance.blockMovementInput)
            {
                rawInput += e.KeyChar.ToString();
            }
        }

        public void Show()
        {
            ShowConsole = true;
        }

        public void Hide()
        {
            ShowConsole = false;
        }


    }

    public class GameTextWriter
    {
        FontFactory     fontFactory;
        TextFormat      textFormat;
        SolidColorBrush textBrush;
        RenderTarget    renderTarget;
        Color4          textColor;
        float   offset;
        public Color4   TextColor
        {
            set
            {
                textColor = value;
            }
        }
        public float    Offset {
            set
            {
                offset = value + 5;
            }
        }

        public GameTextWriter(RenderTarget renderTarget)
        {
            this.renderTarget = renderTarget;
            fontFactory = new SharpDX.DirectWrite.Factory();
            textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            textFormat = new TextFormat(fontFactory, "DejaVu Sans Mono", 14);
            textBrush = new SharpDX.Direct2D1.SolidColorBrush(renderTarget, textColor);
            Offset = 20;
        }
        public void SetTextFormat(string fontName, float fontSize)
        {
            textFormat = new TextFormat(fontFactory, fontName, fontSize);
        }

        public void Write(List<String> text, Vector2 size, Vector2 pos)
        {

            int k = 0;
            foreach (string s in text)
            {
                TextLayout textLayout = new TextLayout(fontFactory, "> " + s, textFormat, size.X, size.Y);
                renderTarget.DrawTextLayout(new Vector2(pos.X + offset, pos.Y + offset * (k + 1)), textLayout, textBrush);
                k++;
            }
        }
    }

    /// <summary>
    /// Helper class that allows to copy stdout to buffer
    /// </summary>
    public class OutputCapture : System.IO.TextWriter, IDisposable
    {
        private System.IO.TextWriter stdOutWriter;
        public System.IO.TextWriter Captured { get; private set; }
        public override Encoding Encoding { get { return Encoding.ASCII; } }

        public OutputCapture()
        {
            this.stdOutWriter = System.Console.Out;
            System.Console.SetOut(this);
            Captured = new System.IO.StringWriter();
        }

        override public void Write(string output)
        {
            // Capture the output and also send it to StdOut
            Captured.Write(output);
            stdOutWriter.Write(output);
        }

        override public void WriteLine(string output)
        {
            // Capture the output and also send it to StdOut
            Captured.WriteLine(output);
            stdOutWriter.WriteLine(output);
        }
    }
}
