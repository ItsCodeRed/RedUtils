using System;
using System.Drawing;
using System.Numerics;
using RedUtils.Math;
using RLBotDotNet.Renderer;

namespace RedUtils
{
    public class DebugDraw
    {
        private static readonly float Sqr2 = MathF.Sqrt(2);
        
        public Color color = Color.White;
        private Renderer _renderer;

        public DebugDraw(Renderer renderer)
        {
            _renderer = renderer;
        }

        public void Text2D(string text, Vec3 upperLeft, int scale = 1, Color? color = null)
        {
            _renderer.DrawString2D(text, color ?? this.color, NumVec2(upperLeft), scale, scale);
        }
        
        public void Text3D(string text, Vec3 upperLeft, int scale = 1, Color? color = null)
        {
            _renderer.DrawString3D(text, color ?? this.color, NumVec(upperLeft), scale, scale);
        }

        public void Rect3D(Vec3 pos, int width, int height, bool fill = true, Color? color = null)
        {
            // TODO Test if centered
            _renderer.DrawRectangle3D(color ?? this.color, NumVec(pos), width, height, fill);
        }

        public void Line3D(Vec3 start, Vec3 end, Color? color = null)
        {
            _renderer.DrawLine3D(color ?? this.color, NumVec(start), NumVec(end));
        }

        public void Line2D(Vec3 start, Vec3 end, Color? color = null)
        {
            _renderer.DrawLine2D(color ?? this.color, NumVec2(start), NumVec2(end));
        }

        public void Polyline3D(Vec3[] points, Color? color = null)
        {
            Vector3[] numericPoints = new Vector3[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                numericPoints[i] = new Vector3(points[i].x, points[i].y, points[i].z);
            }

            _renderer.DrawPolyLine3D(color ?? this.color, numericPoints);
        }

        public void Polyline2D(Vec3[] points, Color? color = null)
        {
            Vector2[] numericPoints = new Vector2[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                numericPoints[i] = new Vector2(points[i].x, points[i].y);
            }

            _renderer.DrawPolyLine2D(color ?? this.color, numericPoints);
        }

        public void Circle(Vec3 pos, Vec3 normal, float radius, Color? color = null)
        {
            // TODO Test
            Vec3 arm = normal.Cross(pos).Normalize() * radius;
            int pieces = (int)MathF.Pow(radius, 0.7f) + 5;
            float angle = 2 * MathF.PI / pieces;
            Mat3x3 rotMat = Mat3x3.RotationFromAxis(normal.Normalize(), angle);

            Vec3[] points = new Vec3[pieces + 1];
            for (int i = 0; i <= pieces; i++) {
                arm = rotMat.Dot(arm);
                points[i] = pos + arm;
            }

            Polyline3D(points, color);
        }

        public void Cross(Vec3 pos, float size, Color? color = null)
        {
            Line3D(pos + size * Vec3.X, pos - size * Vec3.X, color);
            Line3D(pos + size * Vec3.Y, pos - size * Vec3.Y, color);
            Line3D(pos + size * Vec3.Z, pos - size * Vec3.Z, color);
        }

        public void CrossAngled(Vec3 pos, float size, Color? color = null)
        {
            float r = 1f / Sqr2;
            Line3D(pos + new Vec3(r, r, r), pos + new Vec3(-r, -r, -r), color);
            Line3D(pos + new Vec3(r, r, -r), pos + new Vec3(-r, -r, r), color);
            Line3D(pos + new Vec3(r, -r, -r), pos + new Vec3(-r, r, r), color);
            Line3D(pos + new Vec3(r, -r, r), pos + new Vec3(-r, r, -r), color);
        }

        public void Cube(Vec3 pos, float size, Color? color = null)
        {
            Cube(pos, new Vec3(size, size, size), color);
        }

        public void Cube(Vec3 pos, Vec3 size, Color? color = null)
        {
            Vec3 half = size / 2;
            Line3D(pos + new Vec3(-half.x, -half.y, -half.z), pos + new Vec3(-half.x, -half.y, half.z), color);
            Line3D(pos + new Vec3(half.x, -half.y, -half.z), pos + new Vec3(half.x, -half.y, half.z), color);
            Line3D(pos + new Vec3(-half.x, half.y, -half.z), pos + new Vec3(-half.x, half.y, half.z), color);
            Line3D(pos + new Vec3(half.x, half.y, -half.z), pos + new Vec3(half.x, half.y, half.z), color);
            Line3D(pos + new Vec3(-half.x, -half.y, -half.z), pos + new Vec3(-half.x, half.y, -half.z), color);
            Line3D(pos + new Vec3(half.x, -half.y, -half.z), pos + new Vec3(half.x, half.y, -half.z), color);
            Line3D(pos + new Vec3(-half.x, -half.y, half.z), pos + new Vec3(-half.x, half.y, half.z), color);
            Line3D(pos + new Vec3(half.x, -half.y, half.z), pos + new Vec3(half.x, half.y, half.z), color);
            Line3D(pos + new Vec3(-half.x, -half.y, -half.z), pos + new Vec3(half.x, -half.y, -half.z), color);
            Line3D(pos + new Vec3(-half.x, -half.y, half.z), pos + new Vec3(half.x, -half.y, half.z), color);
            Line3D(pos + new Vec3(-half.x, half.y, -half.z), pos + new Vec3(half.x, half.y, -half.z), color);
            Line3D(pos + new Vec3(-half.x, half.y, half.z), pos + new Vec3(half.x, half.y, half.z), color);
        }

        public void RotatedCube(Vec3 pos, Mat3x3 rotation, Vec3 size, Color? color = null)
        {
            Vec3 half = size / 2;
            Line3D(pos + rotation.Dot(new Vec3(-half.x, -half.y, -half.z)), pos + rotation.Dot(new Vec3(-half.x, -half.y, half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(half.x, -half.y, -half.z)), pos + rotation.Dot(new Vec3(half.x, -half.y, half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(-half.x, half.y, -half.z)), pos + rotation.Dot(new Vec3(-half.x, half.y, half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(half.x, half.y, -half.z)), pos + rotation.Dot(new Vec3(half.x, half.y, half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(-half.x, -half.y, -half.z)), pos + rotation.Dot(new Vec3(-half.x, half.y, -half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(half.x, -half.y, -half.z)), pos + rotation.Dot(new Vec3(half.x, half.y, -half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(-half.x, -half.y, half.z)), pos + rotation.Dot(new Vec3(-half.x, half.y, half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(half.x, -half.y, half.z)), pos + rotation.Dot(new Vec3(half.x, half.y, half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(-half.x, -half.y, -half.z)), pos + rotation.Dot(new Vec3(half.x, -half.y, -half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(-half.x, -half.y, half.z)), pos + rotation.Dot(new Vec3(half.x, -half.y, half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(-half.x, half.y, -half.z)), pos + rotation.Dot(new Vec3(half.x, half.y, -half.z)), color);
            Line3D(pos + rotation.Dot(new Vec3(-half.x, half.y, half.z)), pos + rotation.Dot(new Vec3(half.x, half.y, half.z)), color);
        }

        public void Octahedron(Vec3 pos, float size, Color? color = null)
        {
            float half = size / 2;
            Line3D(pos + new Vec3(half, 0, 0), pos + new Vec3(0, half, 0), color);
            Line3D(pos + new Vec3(0, half, 0), pos + new Vec3(-half, 0, 0), color);
            Line3D(pos + new Vec3(-half, 0, 0), pos + new Vec3(0, -half, 0), color);
            Line3D(pos + new Vec3(0, -half, 0), pos + new Vec3(half, 0, 0), color);
            Line3D(pos + new Vec3(half, 0, 0), pos + new Vec3(0, 0, half), color);
            Line3D(pos + new Vec3(0, 0, half), pos + new Vec3(-half, 0, 0), color);
            Line3D(pos + new Vec3(-half, 0, 0), pos + new Vec3(0, 0, -half), color);
            Line3D(pos + new Vec3(0, 0, -half), pos + new Vec3(half, 0, 0), color);
            Line3D(pos + new Vec3(0, half, 0), pos + new Vec3(0, 0, half), color);
            Line3D(pos + new Vec3(0, 0, half), pos + new Vec3(0, -half, 0), color);
            Line3D(pos + new Vec3(0, -half, 0), pos + new Vec3(0, 0, -half), color);
            Line3D(pos + new Vec3(0, 0, -half), pos + new Vec3(0, half, 0), color);
        }

        private Vector3 NumVec(Vec3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }
        
        private Vector2 NumVec2(Vec3 v)
        {
            return new Vector2(v.x, v.y);
        }
    }
}