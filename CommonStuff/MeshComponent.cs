using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CommonStuff
{
    public abstract class MeshComponent : GameComponent
    {
        public Vector3 Position;
        public Renderer Renderer;
        public Material Material;

        public InputLayout layout;
        public RasterizerState rastState;
        public VertexBufferBinding bufBinding;
        public Buffer vertBuffer;
        public Buffer indexBuffer;
        public Buffer constantBuffer;
        public List<Vector4> points;
        public int vertexCount;
        public PrimitiveTopology primTopology;

        public MeshComponent(Game game) : base(game)
        {
            Console.Write(String.Format("Init {0}\n", this.GetType().ToString()));
        }

	}
}
