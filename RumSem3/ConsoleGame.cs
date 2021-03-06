﻿using System;
using SharpDX;
using Core;
using System.Windows.Forms;

namespace RumSem3
{
    class ConsoleGame : Game
    {
        Camera camera;

        public ConsoleGame(string name) : base(name)
        {
            camera = new Camera(this);
            camController = new CameraController(this, camera);
        }

        protected override void Initialize()
        {
            Components.Add(new Core.Components.PlaneComponent(this, camera));
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
