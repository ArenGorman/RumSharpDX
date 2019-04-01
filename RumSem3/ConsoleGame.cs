using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using CommonStuff;
using System.Windows.Forms;

namespace RumSem3
{
    class ConsoleGame : Game
    {
        Camera camera;
        CameraController camController;

        public ConsoleGame(string name) : base(name)
        {
            camera = new Camera(this);
            camController = new CameraController(this, camera);
        }

        protected override void Initialize()
        {
            Components.Add(new PlaneComponent(this, camera));
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
