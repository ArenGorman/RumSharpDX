using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;

namespace Core
{
	public class CameraController
	{
		Game game;
		public Camera Camera { set; get; }

		public Vector3 CameraPosition { set; get; }

		public float Yaw	{ set; get; }
		public float Pitch	{ set; get; }

		public float VelocityMagnitude { set; get; } = 100.0f;
		public float MouseSensitivity = 0.50f;


		public CameraController(Game game, Camera cam)
		{
			this.game	= game;
			Camera		= cam;

			game.InputDevice.MouseMove += args => {
                // Let user move mouse without rotating camera when G pressed
                if (!this.game.InputDevice.IsKeyDown(Keys.G)){
                    Yaw += args.Offset.X * 0.003f * MouseSensitivity;
                    Pitch -= args.Offset.Y * 0.003f * MouseSensitivity;
                }
			};
		}

		public void Update(float deltaSeconds)
		{
			var input = game.InputDevice;

			var rotMat = Matrix.RotationYawPitchRoll(Yaw, Pitch, 0);

			var velDirection = Vector3.Zero;
            if (!game.blockMovementInput)
            {
                if (input.IsKeyDown(Keys.W)) velDirection += new Vector3(1.0f, 0.0f, 0.0f);
                if (input.IsKeyDown(Keys.S)) velDirection += new Vector3(-1.0f, 0.0f, 0.0f);
                if (input.IsKeyDown(Keys.A)) velDirection += new Vector3(0.0f, 0.0f, 1.0f);
                if (input.IsKeyDown(Keys.D)) velDirection += new Vector3(0.0f, 0.0f, -1.0f);
                if (input.IsKeyDown(Keys.M)) Console.WriteLine("M pressed");

                if (input.IsKeyDown(Keys.Space)) velDirection += new Vector3(0.0f, 1.0f, 0.0f);
                if (input.IsKeyDown(Keys.C)) velDirection += new Vector3(0.0f, -1.0f, 0.0f);

                velDirection.Normalize();
            }
			

			var velDir = rotMat.Forward * velDirection.X + Vector3.Up * velDirection.Y + rotMat.Right * velDirection.Z;
			if (velDir.Length() != 0) {
				velDir.Normalize();
			}

			CameraPosition = CameraPosition + velDir * VelocityMagnitude * deltaSeconds;

			Camera.ViewMatrix = Matrix.LookAtLH(CameraPosition, CameraPosition + rotMat.Forward, rotMat.Up);
		}

	}
}
