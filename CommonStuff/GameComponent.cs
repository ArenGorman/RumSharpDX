using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CommonStuff
{
	public abstract class GameComponent
	{
        public Game Game;
        public Renderer Renderer;
        public InputLayout layout;
        public RasterizerState rastState;
        public VertexBufferBinding bufBinding;
        public Buffer vertBuffer;
        public Buffer constantBuffer;
        public List<Vector4> points;
        public int buflen;
        public int vertexCount;
        public PrimitiveTopology primTopology;
        public Material material;
        public Vector3 Position;

        public GameComponent(Game game)
		{
			Game = game;
		}

		public abstract void Initialize();
		public abstract void Update(float deltaTime);
		public abstract void Draw(float deltaTime);
		public abstract void DestroyResources();
	}
}
