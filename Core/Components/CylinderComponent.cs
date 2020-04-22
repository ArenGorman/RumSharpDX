using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;


namespace Core.Components
{
    public class CylinderComponent : MeshComponent
    {
        private Camera _camera;
        public uint size = 10;

        public CylinderComponent(Game game, Camera cam, Renderer renderer, Material mat = null) : base(game)
        {
            _camera = cam;
            primTopology = PrimitiveTopology.TriangleList;
            Position = new Vector3(0, 0, 0);
        }

        public override void Initialize()
        {
            points = new List<Vector4>{
                new Vector4(-5.0f, -5.0f, 0.0f, 1.0f),
                new Vector4(-5.0f, 5.0f, 0.0f, 1.0f),
                new Vector4(5.0f, 5.0f, 0.0f, 1.0f),
                new Vector4(5.0f, 5.0f, 0.0f, 1.0f),
                new Vector4(-5.0f, -5.0f, 10.0f, 1.0f),
                new Vector4(-5.0f, 5.0f, 10.0f, 1.0f),
                new Vector4(5.0f, 5.0f, 10.0f, 1.0f),
                new Vector4(5.0f, 5.0f, 10.0f, 1.0f)
                    };
            var bufDesc = new BufferDescription {
                BindFlags       = BindFlags.VertexBuffer,
                CpuAccessFlags  = CpuAccessFlags.None,
                OptionFlags     = ResourceOptionFlags.None,
                Usage           = ResourceUsage.Default
            };
            vertBuffer = Buffer.Create(gameInstance.Device, points.ToArray(), bufDesc);
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

        public override void DestroyResources()
        {
            throw new NotImplementedException();
        }
    }
}
