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
        public Game gameInstance;
        public Renderer Renderer;
        public InputLayout layout;
        public RasterizerState rastState;
        public VertexBufferBinding bufBinding;
        public Buffer vertBuffer;
        public Buffer indexBuffer;
        public Buffer constantBuffer;
        public List<Vector4> points;
        public int vertexCount;
        public PrimitiveTopology primTopology;
        public Material material;
        public Vector3 Position;

        public GameComponent(Game game)
		{
			gameInstance = game;
		}

		public abstract void Initialize();
		public abstract void Update(float deltaTime);
		public abstract void Draw(float deltaTime);
        //TODO add destruction of resources
		public abstract void DestroyResources();
	}
}
