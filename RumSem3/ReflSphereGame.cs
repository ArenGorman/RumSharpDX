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
        }

        protected override void Initialize()
        {
            this.bgcolor = Color.Gray;
            Components.Add(new PlaneComponent(this, camera));
            Components.Add(new ObjModelComponent(this, "Resources/Models/skySphere.obj", "Resources/Textures/miramar.bmp", "Cube", camera, true));
            base.Initialize();
            
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
