using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace LM2L
{
    class glRenderer
    {
        internal int Width, Height;

        public void Resize(int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;

            GL.Viewport(0, 0, Width, Height);

            //Camera.RecalculateMatrices();
        }
    }

    public class Camera
    {
        public Vector3 Scale;
        public Vector3 Rotation;
        public Vector3 Translation;

        public Vector3 UpVector;
        public Vector3 Target;

        public Matrix4 ProjectionMatrix;
        public Matrix4 ViewMatrix;
    }
}
