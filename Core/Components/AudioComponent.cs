using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;
using SharpDX.X3DAudio;
using Core;


namespace Core.Components
{
    public enum AudioComponentType
    {
        point,
        ambient,
        volumetric
    }

    public class AudioComponent : GameComponent
    {
        public Vector3 Position;

        private XAudio2 xaudio2;
        private MasteringVoice masteringVoice;

        private SoundStream stream;
        private WaveFormat waveFormat;
        private AudioBuffer buffer;

        public string Filename;
        public AudioComponent(Game game, string filename, AudioComponentType type=AudioComponentType.point) : base(game)
        {
            xaudio2 = new XAudio2(XAudio2Version.Version27);
            Filename = filename;
            masteringVoice = new MasteringVoice(xaudio2);

        }
        public override void DestroyResources()
        {
            xaudio2.Dispose();
            if (stream != null) stream.Dispose();
        }

        public override void Draw(float deltaTime)
        {
        }

        public override void Initialize()
        {
            if (!File.Exists(Filename))
            {
                Console.WriteLine($"Could not load sound: {Filename}");
                throw new FileNotFoundException();
            }
            stream = new SoundStream(File.OpenRead(Filename));
            waveFormat = stream.Format;
            buffer = new AudioBuffer
            {
                Stream = stream.ToDataStream(),
                AudioBytes = (int)stream.Length,
                Flags = BufferFlags.EndOfStream
            };
            stream.Close();
            //Console.WriteLine($"Init Audio component ({Filename})");
        }

        public override void Update(float deltaTime)
        {

        }

        public void PlaySimpleSound()
        {
            var sourceVoice = new SourceVoice(xaudio2, waveFormat, false);
            sourceVoice.SubmitSourceBuffer(buffer, stream.DecodedPacketsInfo);
            sourceVoice.Start();
        }
    }
}
