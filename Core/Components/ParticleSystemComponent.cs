using System;
using SharpDX;

namespace Core.Components
{
    struct Particle
    {
        Vector3 pos;
        Vector3 prevPos;
        Vector3 velocity;

        float life;

    }

    class ParticleSystemComponent : MeshComponent
    {
        Game game;

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
