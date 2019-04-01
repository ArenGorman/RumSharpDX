using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace CommonStuff
{
	public class Camera
	{
		private Game Game;

		public Matrix ViewMatrix;
		public Matrix ProjMatrix;

		public Camera(Game game)
		{
			Game = game;

			ViewMatrix = Matrix.Identity;
			ProjMatrix = Matrix.Identity;
        }


		public Matrix GetViewMatrix()
		{
			return ViewMatrix;
		}

		public Matrix GetProjectionMatrix()
		{
			//return Matrix.OrthoOffCenterLH(0, Game.Form.ClientSize.Width, 0, Game.Form.ClientSize.Height, 0.1f, 1000.0f);
			//return Matrix.OrthoLH(Game.Form.ClientSize.Width, Game.Form.ClientSize.Height, 0.1f, 1000.0f);
			return Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(45), (float)Game.Form.ClientSize.Width/Game.Form.ClientSize.Height,
				0.1f, 10000.0f);
			//return ProjMatrix;
		}

	}
}
