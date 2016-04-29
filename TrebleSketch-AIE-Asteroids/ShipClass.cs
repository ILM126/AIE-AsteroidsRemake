using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TrebleSketch_AIE_Asteroids
{
    class ShipClass // Totally not the potato class
    {
        public Debugging Debug;

        public Texture2D Texture;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Acceleration;
        public float SpeedLimit;

        public float Rotation;
        public float RotationDelta;

        public Vector2 Size;
        public float Radius;
        public Vector2 MaxLimit;
        public Vector2 MinLimit;

        public bool Visible;
        public bool Vunlerable;
        public bool Dead;

        public float m_respawnTime;
        public float m_invulnerabliltyTime;
        public float m_respawnTimer;
        public float m_invulnerabliltyTimer;

        public Vector2 m_spawnPosition;

        public int Score;
        public int ScoreIncrements;
        public float Health;

        public void Respawn()
        {
            Debug.WriteToFile("[INFO] Respawn tripped", true);
            if (Dead)
            {
                Health = 150f;
            }
            Position = new Vector2(m_spawnPosition.X
                , m_spawnPosition.Y);
            Velocity = new Vector2(0, 0);
            Acceleration = 0f;
            Rotation = 0f;
            RotationDelta = 0f;
            Dead = false;
            Visible = true;
        }

        public void Die()
        {
            if (Vunlerable)
            {
                Visible = false;
                Vunlerable = false;
                Dead = true;
                Debug.WriteToFile("[INFO] Die With Vunlerale tripped", true);
            }
        }
    }
}
