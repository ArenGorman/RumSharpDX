using System;

using SharpDX;
using SharpDX.Direct3D11;
using Core;
using Core.Components;
using System.Windows.Forms;

using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using Core.Physics;

namespace RumSem3
{
    class ReflSphereGame : Game
    {
        Camera camera;

        public ReflSphereGame(string name) : base(name)
        {
            camera = new Camera(this);
            camController = new CameraController(this, camera);
            base.Renderer.camera = camera;
            camController.CameraPosition = new Vector3(0.0f, 40.0f, 200.0f);
            camController.Pitch = -0.15f;
        }

        unsafe protected override void Initialize()
        {
            this.bgcolor = Color.SlateGray;
            Components.Add(new PlaneComponent(this, this.camera));
            Components.Add(new AxisComponent(this, this.Renderer, this.camera));
            Components.Add(new ObjModelComponent(this, "Resources/Models/skySphere.obj",
                "Resources/Textures/miramar_?.bmp", MaterialType.CubeMap,
                this.camera));

            Components.Add(new ObjModelComponent(this, "Resources/Models/teapot.obj",
                "Resources/Textures/teapot.png", MaterialType.PBR,
                this.camera)
            {
                Position = new Vector3(50.0f, .0f, .0f),
            });

            Components.Add(new ObjModelComponent(this, "Resources/Models/teapot.obj", 
                "Resources/Textures/teapot_metalness.png", MaterialType.Unlit,
                camera)
            {
                Position = new Vector3(-50.0f, .0f, .0f),
                Rotation = new Vector3(.0f, (float)Math.PI, .0f)
            });
            Components.Add(new ObjModelComponent(this, "Resources/Models/sphere.obj",
                "Resources/Textures/miramar_dn.bmp", MaterialType.Unlit, camera));

            Components.Add(new AudioComponent(this, "Resources/Sounds/storm_02.wav"));
            Components.Add(new UserInterface(this));


            base.Initialize();
            foreach (GameComponent x in Components)
            {
                if (x is AudioComponent) ((AudioComponent)x).PlaySimpleSound();
            }

            //Simulation = Simulation.Create(BufferPool,
            //    new NarrowPhaseCallbacks(),
            //    new PlanetaryGravityCallbacks() { PlanetCenter = new System.Numerics.Vector3(-50.0f, .0f, .0f), Gravity = 100000 });



        }

        protected override void Update(float deltaTime)
        {
            camController.Update(deltaTime);
            ((ObjModelComponent)Components[2]).Rotation += new Vector3(0.0f, 0.0001f, 0.0f);
            if (InputDevice.IsKeyDown(Keys.Escape))
                Exit();
            base.Update(deltaTime);
        }
    }
}
