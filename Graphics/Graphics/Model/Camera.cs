using System;
using SharpDX;

namespace Graphics.Model
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }
        public Vector3 Up { get; set; }

        private float _horizontalAngle = (float) Math.PI;
        private float _verticalAngle;

        public void Move(Vector3 v)
        {
            var direction = Vector3.Subtract(Target, Position);
            var right = Vector3.Cross(direction, Up);

            Position += direction*v.X + right*v.Y;
            Target = Position + direction;
        }

        public void Rotate(Vector2 rotation)
        {
            _horizontalAngle += rotation.X;
            _verticalAngle += rotation.Y;

            var sinH = (float) Math.Sin(_horizontalAngle);
            var cosH = (float) Math.Cos(_horizontalAngle);
            var sinV = (float) Math.Sin(_verticalAngle);
            var cosV = (float) Math.Cos(_verticalAngle);
            var direction = new Vector3(cosV * sinH, sinV, cosV * cosH);
            var right = new Vector3((float) Math.Sin(_horizontalAngle - Math.PI / 2), 0, (float) Math.Cos(_horizontalAngle - Math.PI / 2));

            Up = Vector3.Cross(right, direction);
            Target = Position + direction;
        }
    }
}
