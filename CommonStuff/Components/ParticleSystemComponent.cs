using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CommonStuff
{
    struct Particle
    {
        Vector3 pos;
        Vector3 prevPos;
        Vector3 velocity;

        float life;

    }

    class ParticleSystemComponent : GameComponent
    {
        Game game;

        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        new public uint vertexCount;


        public ParticleSystemComponent(Game game) : base(game)
        {
            gameInstance = game;

        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void DestroyResources()
        {
            throw new NotImplementedException();
        }

        public override void Draw(float deltaTime)
        {
            throw new NotImplementedException();
        }

        public override void Update(float deltaTime)
        {
            throw new NotImplementedException();
        }
    }
}
