using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CommonStuff
{
    public class RenderableComponent : GameComponent
    {

        public RenderableComponent(Game game) : base(game)
        {
            Console.Write("Init");
        }

        public override void Initialize()
        {
            Console.Write("Init");
        }
		public override void Update(float deltaTime)
        {
            Console.Write("Init");
        }
		public override void Draw(float deltaTime)
        {
            Console.Write("Init");
        }
        //TODO add destruction of resources
		public override void DestroyResources()
        {
            Console.Write("Init");
        }
	}
}
