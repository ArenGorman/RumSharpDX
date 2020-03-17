using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CommonStuff
{
    public interface IGameComponent
    {
        Game Game { get; set; }
        void Initialize();
        void Update(float deltaTime);
        void Draw(float deltaTime);
        void DestroyResources();
    }

	public abstract class GameComponent
	{
        public Game gameInstance;

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
