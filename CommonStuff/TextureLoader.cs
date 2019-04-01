using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.IO;
using SharpDX.WIC;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace CommonStuff
{
	public class TextureLoader
	{
		Game game;
		public ImagingFactory Factory { protected set; get; }


		public TextureLoader(Game game)
		{
			this.game = game;

			Factory = new ImagingFactory();
        }


		public Texture2D LoadTextureFromFile(string fileName)
		{
			Texture2D tex = null;

			var decoder = new BitmapDecoder(Factory, fileName, DecodeOptions.CacheOnDemand);
			var frame	= decoder.GetFrame(0);
			//var pixFormat = frame.PixelFormat;

			//var queryReader = frame.MetadataQueryReader;

			FormatConverter converter = new FormatConverter(Factory);
			converter.Initialize(frame, PixelFormat.Format32bppPRGBA);

			var width	= converter.Size.Width;
			var height	= converter.Size.Height;

			int stride = width * 4;

			using (var buffer = new SharpDX.DataBuffer(stride * height))
			{
				converter.CopyPixels(stride, buffer.DataPointer, buffer.Size);

				tex = new Texture2D(game.Device, new Texture2DDescription
				{
					Width	= width,
					Height	= height,
					ArraySize = 1,
					BindFlags		= BindFlags.ShaderResource,
					Usage			= ResourceUsage.Default,
					CpuAccessFlags	= CpuAccessFlags.None,
					Format		= Format.R8G8B8A8_UNorm,
					MipLevels	= 1,
					OptionFlags = ResourceOptionFlags.None,
					SampleDescription = new SampleDescription(1, 0)
				}, new[] { new SharpDX.DataBox(buffer.DataPointer, stride, buffer.Size) });
			}
			//queryReader.Dump(Console.Out);

			return tex;
		}
	}
}
