using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonStuff
{
	public abstract class GameComponent
	{
		public Game Game;

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
