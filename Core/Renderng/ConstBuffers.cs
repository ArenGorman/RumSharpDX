using System;
using SharpDX;

namespace Core
{
    class ConstBuffers
    {
        public struct ConstBufferPerObjectStruct
        {
            public Matrix WorldViewProjMatrix;
            public Matrix WorldViewMatrix;
            public Matrix WorldMatrix;

            public Vector2 textureTiling;
            public Vector2 textureShift;

            public Vector4 AlbedoColor;
            public float RoughnessValue;
            public float MetallicValue;
            public Vector2 filler;
            public Vector4 optionsMask0;
            public Vector4 optionsMask1;
        }

        public struct ConstBufferPerFrameStruct
        {
            public Matrix Projection;
            public Matrix ProjectionInv;
            public Vector3 CameraPos;
            public float AlphaTest;
            public uint NumLights;
            public uint WindowWidth;
            public uint WindowHeight;
            public uint MaxNumLightsPerTile;
            public uint DirLightsNum;
            public Vector3 filler;
        }

        internal struct LightBufferStruct
        {
            public Matrix viewProjMatrix;
            public Vector4 lightTint;
            public float type;
            public Vector3 position;
            public Vector3 direction;
            public float distanceSqr;
        }

        public struct ConstBufferDirLightStruct
        {
            public Vector3 DirLightDirection;
            public float DirLightIntensity;
            public Vector4 DirLightColor;
        }
    }
}
