using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D11;

namespace CommonStuff
{
	public class ObjLoader
	{

		public void LoadObjModel(string fileName, out Buffer vertexBuffer, out int verticesCount)
		{
			vertexBuffer	= null;
			verticesCount	= 0;

			if (!File.Exists(fileName)) {
				throw new FileNotFoundException();
			}

			var positions	= new List<Vector4>();
			var normals		= new List<Vector4>();
			var texCoords	= new List<Vector4>();

			var vertices = new List<VertexStructures.VertexPositionNormalTex>();

			var lines = File.ReadAllLines(fileName);
			for(int i = 0; i < lines.Length; i++) {
				var line = lines[i];
				if (string.IsNullOrEmpty(line)) continue;

				line = line.TrimStart();
				if (line[0] == '#') continue; // Skip comment

				var vals = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
				if (vals.Length == 0) continue;

				switch (vals[0])
				{
					case "v":
						positions.Add(ParsePosition(vals));
                        break;
					case "vt":
						texCoords.Add(ParseTex(vals));
						break;
					case "vn":
						normals.Add(ParseNormal(vals));
						break;
					case "f":
						if (vals.Length < 4) continue;

						var v0 = GenerateVertex(vals[1], positions, texCoords, normals);

						for (int fInd = 2; fInd < vals.Length-1; fInd++) {
							var v1 = GenerateVertex(vals[fInd], positions, texCoords, normals);
							var v2 = GenerateVertex(vals[fInd+1], positions, texCoords, normals);

							vertices.Add(v0);
							vertices.Add(v1);
							vertices.Add(v2);
						}
						break;
					default:
						continue;
				}
			}

			if (vertices.Count == 0) return;
			verticesCount = vertices.Count;
			vertexBuffer = Buffer.Create(Game.Instance.Device, vertices.ToArray(), new BufferDescription {
				BindFlags		= BindFlags.VertexBuffer,
				CpuAccessFlags	= CpuAccessFlags.None,
				OptionFlags		= ResourceOptionFlags.None,
				Usage			= ResourceUsage.Default,
			});
        }

		VertexStructures.VertexPositionNormalTex GenerateVertex(string elem, List<Vector4> positions, List<Vector4> texCoords, List<Vector4> normals)
		{
			int p, t, n;
			ParseFaceElement(elem, out p, out t, out n);
			var v = new VertexStructures.VertexPositionNormalTex {
				Position	= positions[p],
				Normal		= n == -1 ? Vector4.Zero : normals[n],
				Tex			= t == -1 ? Vector4.Zero : texCoords[t]
			};

			return v;
		}

		void ParseFaceElement(string elem, out int posInd, out int texInd, out int norInd)
		{
			posInd = texInd = norInd = -1;

			var indeces = elem.Split('/');

			if(indeces.Length == 0) {
				return;
			}

			if(indeces.Length == 1) { // Position only
				posInd = int.Parse(indeces[0]) - 1;
			}
			if (indeces.Length == 2) { // Position and texture coordinates provided
				posInd = int.Parse(indeces[0]) - 1;
				texInd = int.Parse(indeces[1]) - 1;
			}
			if (indeces.Length == 3) { // Position, texture coordinates (maybe) and normal provided
				posInd = int.Parse(indeces[0]) - 1;
				if(!string.IsNullOrEmpty(indeces[1])) texInd = int.Parse(indeces[1]) - 1;
				norInd = int.Parse(indeces[2]) - 1;
			}
		}

		Vector4 ParsePosition(string[] values)
		{
			var x = float.Parse(values[1], CultureInfo.InvariantCulture);
			var y = float.Parse(values[2], CultureInfo.InvariantCulture);
			var z = float.Parse(values[3], CultureInfo.InvariantCulture);

			float w = 1.0f;
			if(values.Length > 4) {
				w = float.Parse(values[4], CultureInfo.InvariantCulture);
			}

			return new Vector4(x, y, z, w);
		}

		Vector4 ParseNormal(string[] values)
		{
			var x = float.Parse(values[1], CultureInfo.InvariantCulture);
			var y = float.Parse(values[2], CultureInfo.InvariantCulture);
			var z = float.Parse(values[3], CultureInfo.InvariantCulture);

			var normal = new Vector4(x, y, z, 0);
			normal.Normalize();

			return normal;
		}

		Vector4 ParseTex(string[] values)
		{
			var u = float.Parse(values[1], CultureInfo.InvariantCulture);
			var v = float.Parse(values[2], CultureInfo.InvariantCulture);

			return new Vector4(u, v, 0, 0);
		}
	}
}
