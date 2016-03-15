using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TrebleSketch_AIE_Asteroids
{
    class AsteroidClass // This metal class rocks!
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Acceleration; // User can choose the rate of acceleration ( ͡° ͜ʖ ͡°)
        public float Rotation;
        public float RotationDelta;

        public Vector2 Size;
        public float Radius;

        public Vector2 MaxLimit;
        public Vector2 MinLimit;

        public int DamageDealt; // Future Feature

        // AsteroidClass Asteroid;
    }
}
