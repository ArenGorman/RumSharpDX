using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.IO;
using SharpDX.WIC;
using SharpDX.DXGI;
using SharpDX.Direct3D11;

namespace Core.Loaders
{
    public enum TextureSet
    {
        albedo,
        normal,
        roughness,
        metallness,
        occlusion
    }
    public class TextureLoader
	{
		Game game;
		private static ImagingFactory Factory { set; get; }


		public TextureLoader(Game game)
		{
			this.game = game;

			Factory = new ImagingFactory();
        }

        public Texture2D LoadCubeMapFromFiles(string fileSetName)
        {
            Texture2D tex;
            //FormatConverter converter = new FormatConverter(Factory);

            List<byte[]> buffers = new List<byte[]>();

            string[] filePostfixes = { "ft", "bk", "up", "dn", "rt", "lf" };
            var width = 16;
            var height = 16;
            int stride = width * 4;

            for (int i = 0; i < filePostfixes.Length; i++)
            {
                string fileName = fileSetName.Split('?')[0] + filePostfixes[i] + fileSetName.Split('?')[1];
                BitmapSource converter = LoadBitmap(fileName);
                //var decoder = new BitmapDecoder(Factory, fileName, DecodeOptions.CacheOnDemand);
                //var frame = decoder.GetFrame(0);
                //converter.Initialize(frame, PixelFormat.Format32bppPRGBA);

                width = converter.Size.Width;
                height = converter.Size.Height;
                stride = width * 4;

                byte[] data = new byte[converter.Size.Height * stride];

                converter.CopyPixels(data, stride);
                buffers.Add(data);
            }
            tex = new Texture2D(game.Device, new Texture2DDescription {
                ArraySize = 6,
                MipLevels = 1,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                OptionFlags = ResourceOptionFlags.TextureCube | ResourceOptionFlags.GenerateMipMaps,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                Width = width,
                Height = height
            });
            var buffersArray = buffers.ToArray();
            for (int i = 0; i < 6; i++)
            {
                
                IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffersArray[i], 0);
                var dataBox = new SharpDX.DataBox(ptr, stride, stride * height);
                game.Device.ImmediateContext.UpdateSubresource(dataBox,
                    tex, SharpDX.Direct3D11.Resource.CalculateSubResourceIndex(0, i, 1));
            }

            return tex;
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

        public Texture2D DebugTexture()
        {
            byte[] buffer= Enumerable.Repeat<byte>(0, 512 * 4).ToArray();
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            Texture2DDescription debugTextureDescription = new Texture2DDescription
            {
                Width = 512,
                Height = 512,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            };
            return new Texture2D(game.Device, debugTextureDescription, new[] { new SharpDX.DataBox(ptr, 512 * 4, 512) });
        }

        private static BitmapSource LoadBitmap(string filename)
        {
            var bitmapDecoder = new BitmapDecoder(
                Factory,
                filename,
                DecodeOptions.CacheOnDemand
            );

            var formatConverter = new FormatConverter(Factory);

            formatConverter.Initialize(
                bitmapDecoder.GetFrame(0),
                PixelFormat.Format32bppPRGBA,
                BitmapDitherType.None,
                null,
                0.0,
                BitmapPaletteType.Custom);

            return formatConverter;
        }

    }
}
