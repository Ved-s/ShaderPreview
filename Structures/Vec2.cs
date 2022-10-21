using Microsoft.Xna.Framework;
using System;

namespace ShaderPreview.Structures
{
    public struct Vec2
    {
        public float X, Y;

        public static Vec2 Zero => new(0);
        public static Vec2 One => new(1);

        public float Length 
        {
            get => MathF.Sqrt(X * X + Y * Y);
            set 
            {
                float angle = Angle.Radians;
                X = MathF.Cos(angle) * value;
                Y = MathF.Sin(angle) * value;
            }
        }
        public Angle Angle 
        {
            get => new(MathF.Atan2(Y, X));
            set 
            {
                float len = Length;
                X = MathF.Cos(value.Radians) * len;
                Y = MathF.Sin(value.Radians) * len;
            }
        }

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Vec2(float v)
        {
            X = v;
            Y = v;
        }

        public Vec2 Floor() => new(MathF.Floor(X), MathF.Floor(Y));
        public Vec2 Ceiling() => new(MathF.Ceiling(X), MathF.Ceiling(Y));
        public Vec2 Round() => new(MathF.Round(X), MathF.Round(Y));

        public override bool Equals(object? obj)
        {
            return obj is Vec2 vec &&
                   X == vec.X &&
                   Y == vec.Y;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"{X:0.##}, {Y:0.##}";
        }

        public static bool operator ==(Vec2 a, Vec2 b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vec2 a, Vec2 b) => a.X != b.X || a.Y != b.Y;

        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);

        public static Vec2 operator *(Vec2 a, float v) => new(a.X * v, a.Y * v);
        public static Vec2 operator /(Vec2 a, float v) => new(a.X / v, a.Y / v);
        public static Vec2 operator *(Vec2 a, Vec2 b) => new(a.X * b.X, a.Y * b.Y);
        public static Vec2 operator /(Vec2 a, Vec2 b) => new(a.X / b.X, a.Y / b.Y);

        public static implicit operator Vector2(Vec2 vec) => new(vec.X, vec.Y);
        public static explicit operator Vec2(Vector2 vec) => new(vec.X, vec.Y);

        public static implicit operator Point(Vec2 vec) => new((int)vec.X, (int)vec.Y);
        public static explicit operator Vec2(Point p) => new(p.X, p.Y);
    }
}
