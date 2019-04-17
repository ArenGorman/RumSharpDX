using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using CommonStuff;
using System.Windows.Forms;

namespace RumSem3
{
    class ReflSphereGame : Game
    {
        Camera camera;
        CameraController camController;

        public ReflSphereGame(string name) : base(name)
        {
            camera = new Camera(this);
            camController = new CameraController(this, camera);
            base.Renderer.camera = camera;
            camController.CameraPosition = new Vector3(0.0f, 40.0f, 200.0f);
            camController.Pitch = -0.15f;
        }

        protected override void Initialize()
        {
            this.bgcolor = Color.SlateGray;
            Components.Add(new PlaneComponent(this, camera));
            Components.Add(new AxisComponent(this, this.Renderer, camera));

            //Components.Add(new ObjModelComponent(this, "Resources/Models/skySphere.obj", "Resources/Textures/miramar.bmp", "Cube", camera, true));
            //Components.Add(new ObjModelComponent(this, "Resources/Models/teapot.obj", "Resources/Textures/teapot_albedo.png", "Main", camera));
            base.Initialize();
            base.Renderer.Initialize();
            
        }

        protected override void Update(float deltaTime)
        {
            camController.Update(deltaTime);
            if (InputDevice.IsKeyDown(Keys.Escape))
                Exit();
            base.Update(deltaTime);
        }
    }
}
