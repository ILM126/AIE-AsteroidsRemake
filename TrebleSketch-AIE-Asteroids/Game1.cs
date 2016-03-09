﻿    using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TrebleSketch_AIE_Asteroids
{
    /// <summary>
    /// Name: SpaceXterminator
    /// Description: A Game Where Elon Musk Must Destroy All The Tugboats That Is Stopping His Launches
    /// Version: 0.0.30 (Pre-First Playable)
    /// Developer: Titus Huang (Treble Sketch/ILM126)
    /// Game Engine: MonoGame
    /// Dev Notes: This is my first ever major game of any kind, tons of hard work is still needed >:D
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        MenuClass Menu;

        enum WhereAmI
        {
            MENU = 1,
            GAME = 2,
            SETTINGS = 3,
            GAMEOVER
        }

        int PlayerLives;

        enum ShipTexture
        {
            SHIP,
            FALCON9,
            MUSK,
        }

        ShipClass Ship;

        Vector2 CenterOfShip;

        List<AsteroidClass> myAsteroids;
        public Texture2D asteroidTexture;
        const int NUM_ASTEROIDS = 20;
        Random randNum;

        public Texture2D missleTexture;
        List<MissleClass> myMissles;

        TimeSpan lastShot = new TimeSpan(0, 0, 0, 0, 0);
        TimeSpan shotCoolDown = new TimeSpan(0, 0, 0, 0, 125);

        SpriteFont scoreText;

        public enum ExplosionType
        {
            ASTEROID,
            SHIP,
        }

        class ExplosionsClass
        {
            public float Timer;
            public float Interval;

            public int FrameCount;
            public int CurrentFrame;

            public int FrameWidth;
            public int FrameHeight;

            public Rectangle CurrentSprite;
            public Vector2 Size;
            public Vector2 Position;
            public ExplosionType myType;
        }

        List<ExplosionsClass> datExplosions;

        Texture2D shipExplosionTexture;
        Texture2D asteroidExplosionTexture;

        public event EventHandler // Ahoy There, This is a script from http://derpy.me/MonoGameTimers
            Tick;

        long
            interval,
            elapsedTime;

        public long Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        public long ElapsedTime
        {
            get { return elapsedTime; }
        }



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // InitializeMenu();

            InitializeASteroids();

            InitializeShip();

            myMissles = new List<MissleClass>(); // Magic Missle!

            datExplosions = new List<ExplosionsClass>(); // Boom

            CenterOfShip = Ship.Position;                

            base.Initialize();
        }

        /*private void InitializeMenu(GameTime gameTime)
        {
            if (menuStates == WhereAmI.MENU)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.NumPad1))
                {
                    menuStates = WhereAmI.GAME;
                }
            }
            else if (menuStates == WhereAmI.GAME)
            {

            }

            // Menu.CurrentState = 0; // 0 = Menu || 1 = Game || 2 = Settings || 3 = LMAO
        } */

        public int CheckCurrentState(int CurrentState)
        {
            
            return CurrentState;
        }

        private void InitializeShip() // I ship it! - Lightwing <3
        {
            Ship = new ShipClass();

            Ship.Position = new Vector2(
                graphics.PreferredBackBufferWidth / 2
                , graphics.PreferredBackBufferHeight / 2);
            Ship.Velocity = new Vector2(0, 0);
            Ship.Acceleration = 0;
            Ship.Rotation = 0;
            Ship.RotationDelta = 0;

            Ship.Size = new Vector2(150.0f, 150.0f);
            // Ship.Radius = Ship.Size.Y / 2;   CURRENTLY WORKING ON THIS!!!!!
            Ship.MaxLimit = new Vector2(graphics.PreferredBackBufferWidth + (Ship.Size.X / 2)
                , graphics.PreferredBackBufferHeight + (Ship.Size.Y / 2));
            Ship.MinLimit = new Vector2(0 - (Ship.Size.X / 2), 0 - (Ship.Size.Y / 2));

            Ship.Dead = false;
            Ship.Visible = true;
            Ship.Vunlerable = false;

            Ship.m_invulnerabliltyTimer = 2.0f;
            Ship.m_respawnTimer = 0f;

            Ship.m_respawnTime = 1.0f;
            Ship.m_invulnerabliltyTime = 5.0f;
        }

        private void InitializeASteroids() // You Rock, Woho
        {
            randNum = new Random();

            myAsteroids = new List<AsteroidClass>();

            for (int i = 0; i < NUM_ASTEROIDS; i++)
            {
                AsteroidClass Asteroid = new AsteroidClass();
                Asteroid.Position = new Vector2(randNum.Next(graphics.PreferredBackBufferWidth)
                    , randNum.Next(graphics.PreferredBackBufferHeight));
                Asteroid.Velocity = new Vector2(randNum.Next(-4, 4), randNum.Next(-6, 6));
                Asteroid.RotationDelta = randNum.Next(-1, 1);

                int randSize = randNum.Next(32, 86);
                Asteroid.Size = new Vector2(randSize, randSize);

                Asteroid.MaxLimit = new Vector2(graphics.PreferredBackBufferWidth + (Asteroid.Size.X + 100)
                    , graphics.PreferredBackBufferHeight + (Asteroid.Size.Y + 100));
                Asteroid.MinLimit = new Vector2(0 - (Asteroid.Size.X - 100), 0 - (Asteroid.Size.Y - 100));

                myAsteroids.Add(Asteroid);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);


            // Ship.Texture = Content.Load<Texture2D> ("ship"); // The actual ship
            // Ship.Texture = Content.Load<Texture2D>("falcon-9"); // The Falcon 9 v1.1
            Ship.Texture = Content.Load<Texture2D>("musker"); // Elon Musk Shooting at Those Tug Boats
            // asteroidTexture = Content.Load<Texture2D>("asteroid"); // The actual asteroid
            asteroidTexture = Content.Load<Texture2D>("tugboat");
            missleTexture = Content.Load<Texture2D>("bullet");
            scoreText = Content.Load<SpriteFont>("scoreFont");
            shipExplosionTexture = Content.Load<Texture2D>("explosion");
            asteroidExplosionTexture = Content.Load<Texture2D>("explosion2");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Menu.CurrentState = 1; // 1 = Menu || 2 = Game || 3 = Settings || 4 = Ded || 0 = LMAO ||| At least I tried

            ICheckINput(gameTime);
            ICheckShip(gameTime);
            ICheckASteroids();
            IRotateMISSLes();

            foreach (AsteroidClass Asteroid in myAsteroids)
            {
                // CircleCollisionCheck(Ship.Position, , Asteroid.Position, )
            }

            base.Update(gameTime);
        }

        protected void ICheckINput(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || gameTime. || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Ship.Acceleration = 0f;
            Ship.RotationDelta = 0f;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Ship.Acceleration = -0.1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Ship.Acceleration = 0.05f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Ship.RotationDelta = -0.01f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Ship.RotationDelta = 0.01f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Ship.Velocity = new Vector2(0f, 0f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                ISpawnMISSle(gameTime);
            }
        }

        private void ICheckShip(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Ship.Dead && Ship.m_invulnerabliltyTimer == 0f)
            {
                Ship.m_invulnerabliltyTimer = Ship.m_invulnerabliltyTime;
                Ship.m_respawnTimer = Ship.m_respawnTime;
            }

            if (Ship.m_respawnTimer > 0)
            {
                Ship.m_respawnTimer -= delta;
            }
            else if (Ship.m_respawnTime < 0)
            {
                Ship.m_respawnTimer = 0;
                Ship.Respawn();
            }

            if (Ship.m_invulnerabliltyTimer > 0)
            {
                Ship.m_invulnerabliltyTimer -= delta;
            }
            else if (Ship.m_invulnerabliltyTimer > 0)
            {
                Ship.m_invulnerabliltyTimer = 0;
                Ship.Vunlerable = true;
            }

            Ship.Rotation += Ship.RotationDelta;

            Matrix playerRotationMatrix =
                Matrix.CreateRotationZ(Ship.Rotation);

            Ship.Velocity += Vector2.Transform(
                new Vector2(0, Ship.Acceleration)
                , playerRotationMatrix);

            Ship.Position += Ship.Velocity;

            // Keep thw  within the camera's reach
            if (Ship.Position.X > Ship.MaxLimit.X)
            {
                Ship.Position.X = Ship.MinLimit.X;
            }
            else if (Ship.Position.X < Ship.MinLimit.X)
            {
                Ship.Position.X = Ship.MaxLimit.X;
            }

            if (Ship.Position.Y > Ship.MaxLimit.Y)
            {
                Ship.Position.Y = Ship.MinLimit.Y;
            }
            else if (Ship.Position.Y < Ship.MinLimit.Y)
            {
                Ship.Position.Y = Ship.MaxLimit.Y;
            }

            /* if (Ship.Velocity == "10, 10")
            {
                Ship.Velocity 
            } */

        }

        private void ICheckASteroids()
        {
            foreach (AsteroidClass Asteroid in myAsteroids) // A-steroid's rotation
            {
                Asteroid.Position += Asteroid.Velocity;
                // Asteroid.Rotation += Asteroid.RotationDelta;

                if (Asteroid.Position.X > Asteroid.MaxLimit.X)
                {
                    Asteroid.Velocity.X *= -0.05f;
                }
                else if (Asteroid.Position.X < Asteroid.MinLimit.X)
                {
                    Asteroid.Velocity.X *= -0.05f;
                }

                if (Asteroid.Position.Y > Asteroid.MaxLimit.Y)
                {
                    Asteroid.Velocity.Y *= -0.05f;
                }
                else if (Asteroid.Position.Y < Asteroid.MinLimit.Y)
                {
                    Asteroid.Velocity.Y *= -0.05f;
                }
            }
        }

        protected void ISpawnMISSle(GameTime gameTime)
        {
            TimeSpan timeSinceLastShot = gameTime.TotalGameTime - lastShot;

            if (timeSinceLastShot > shotCoolDown)
            {
                MissleClass missle = new MissleClass();

                missle.Position = Ship.Position;

                missle.Rotation = Ship.Rotation;

                Matrix missleRotationMatrix = Matrix.CreateRotationZ(missle.Rotation);
                missle.Velocity = new Vector2(0, -10);
                missle.Velocity = Vector2.Transform(missle.Velocity, missleRotationMatrix);
                missle.Velocity = missle.Velocity + Ship.Velocity;

                missle.Size = new Vector2(16, 16);

                missle.MaxLimit = new Vector2(graphics.PreferredBackBufferWidth + 500
                    , graphics.PreferredBackBufferHeight + 500);
                missle.MinLimit = new Vector2(-500, -500);

                myMissles.Add(missle);

                lastShot = gameTime.TotalGameTime;
            }
        }

        public void ElonMissles() // EYE MISSLES!!!!
        {

        }

        private void IRotateMISSLes() // You Spin Me Right Round, Baby Right Round
        {
            foreach (MissleClass Missle in myMissles) // MISSle's rotation
            {
                Missle.Position += Missle.Velocity;

                if (Missle.Position.X > Missle.MaxLimit.X)
                {
                    Missle.Velocity.X *= -1;
                }
                else if (Missle.Position.X < Missle.MinLimit.X)
                {
                    Missle.Velocity.X *= -1;
                }

                if (Missle.Position.Y > Missle.MaxLimit.Y)
                {
                    Missle.Velocity.Y *= -1;
                }
                else if (Missle.Position.Y < Missle.MinLimit.Y)
                {
                    Missle.Velocity.Y *= -1;
                }
            }
        }

        private bool CircleCollisionCheck(Vector2 object1Pos
            , float object1Radius
            , Vector2 object2Pos
            , float object2Radius) // Yo, I check for collisions between objects
        {
            float distanceBetweenOrbjects = (object1Pos - object2Pos).Length();
            float sumOfRadii = object1Radius + object2Radius;

            if (distanceBetweenOrbjects < sumOfRadii)
            {
                return true;
            }
            return false;
        }

        private void CheckCollisions()
        {
            List<AsteroidClass> AsteroidDeathRow = new List<AsteroidClass>();
            List<MissleClass> MissleDeathRow = new List<MissleClass>();

            foreach (AsteroidClass Asteroid in myAsteroids)
            {
                bool playerCollisionCheck = CircleCollisionCheck(Ship.Position, Ship.Size.X / 2
                    , Asteroid.Position, Asteroid.Size.X / 2);
                if (playerCollisionCheck)
                {
                    Ship.Die();
                    AsteroidDeathRow.Add(Asteroid);
                    CreateExplosion(Ship.Position, ExplosionType.SHIP);
                }

                foreach (MissleClass Missle in myMissles)
                {
                    bool missleCollisionCheck = CircleCollisionCheck(Missle.Position, Missle.Size.X / 2
                    , Missle.Position, Missle.Size.X / 2);

                    if (missleCollisionCheck)
                    {
                        MissleDeathRow.Add(Missle);
                        AsteroidDeathRow.Add(Asteroid);
                    }
                }
            }

            foreach (AsteroidClass Asteroid in AsteroidDeathRow)
            {
                myAsteroids.Remove(Asteroid);
            }
            foreach (MissleClass Missle in MissleDeathRow)
            {
                myMissles.Remove(Missle);
            }
        }

        private void DrawLives()
        {
            for (int i = 0; i < PlayerLives; ++i)
            {
                spriteBatch.Draw(Ship.Texture
                , Ship.Position
                , null
                , Color.White
                , Ship.Rotation
                , new Vector2(Ship.Texture.Width / 2
                    , Ship.Texture.Height / 2)
                , new Vector2(Ship.Size.X / Ship.Texture.Width, Ship.Size.Y / Ship.Texture.Height)
                , SpriteEffects.None
                , 0);
            }
        }

        private void DrawScore()
        {
            spriteBatch.DrawString(scoreText, "SCORE : ", new Vector2(10, 10), Color.White);
            spriteBatch.DrawString(scoreText, Ship.Score.ToString(), new Vector2(120, 10), Color.White);
        }

        /* public string ChooseShip()
        // http://stackoverflow.com/questions/10216757/adding-inputbox-like-control-to-xna-game Grrrrrrrrrr
        // http://www.dreamincode.net/forums/topic/158381-xna-text-input/
        // http://stackoverflow.com/questions/10154046/making-text-input-in-xna-for-entering-names-chatting
        {
            spriteBatch.DrawString(scoreText, "Choose Your Character: ", new Vector2(20, 40), Color.White);
            GetKeyboardLayoutName
            if (Keyboard.GetState().IsKeyDown)
            {
                Menu.CurrentState = 1;
                spriteBatch.DrawString(scoreText, "This Game has started!", new Vector2(20, 50), Color.White);
            }
        } */

        protected void CreateExplosion(Vector2 SpawnPosition, ExplosionType SpawnedExplosionType)
        {
            ExplosionsClass NewExplosion = new ExplosionsClass();

            NewExplosion.CurrentFrame = 0;
            NewExplosion.FrameCount = 12;
            NewExplosion.FrameWidth = 128;
            NewExplosion.FrameHeight = 128;
            NewExplosion.Size = new Vector2(128, 128);
            NewExplosion.Interval = 1000.0f / 30.0f;
            NewExplosion.Position = SpawnPosition;
            NewExplosion.CurrentSprite = new Rectangle(0, 0, 128, 128);
            NewExplosion.myType = SpawnedExplosionType;

            datExplosions.Add(NewExplosion);
        }

        private void UpdatedExplosions(GameTime gameTime)
        {
            List<ExplosionsClass> ToRemove = new List<ExplosionsClass>();

            foreach (ExplosionsClass Explosion in datExplosions)
            {
                if (Explosion.CurrentFrame > Explosion.FrameCount - 1)
                {
                    ToRemove.Add(Explosion);
                    continue;
                }
                else
                {
                    Explosion.Timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

                    if (Explosion.Timer > Explosion.Interval)
                    {
                        ++Explosion.CurrentFrame;

                        Explosion.CurrentSprite = new Rectangle(
                            Explosion.CurrentFrame * Explosion.FrameWidth
                            , 0
                            , Explosion.FrameWidth
                            , Explosion.FrameHeight);

                        Explosion.Timer = 0;
                    }
                }
            }

            foreach (ExplosionsClass Explosion in ToRemove)
            {
                datExplosions.Remove(Explosion);
            }
        }

        private void DrawExplosions()
        {
            Texture2D TempText;

            foreach (ExplosionsClass Explosion in datExplosions)
            {
                if (Explosion.myType == ExplosionType.ASTEROID)
                {
                    TempText = asteroidExplosionTexture;
                }
                else
                {
                    TempText = shipExplosionTexture;
                }

                spriteBatch.Draw(TempText
                , Explosion.Position
                , Explosion.CurrentSprite
                , Color.White
                , 0
                , new Vector2(Explosion.FrameWidth / 2
                    , Explosion.FrameHeight / 2)
                , new Vector2(Explosion.Size.X / Explosion.FrameWidth
                    , Explosion.Size.Y / Explosion.FrameHeight)
                , SpriteEffects.None
                , 0);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            DrawLives();
            DrawExplosions();
            DrawScore();

            spriteBatch.Draw(Ship.Texture
                , Ship.Position
                , null
                , Color.White
                , Ship.Rotation
                , new Vector2(Ship.Texture.Width / 2
                    , Ship.Texture.Height / 2)
                , new Vector2(Ship.Size.X / Ship.Texture.Width
                    , Ship.Size.Y / Ship.Texture.Height)
                , SpriteEffects.None
                , 0);

            foreach (AsteroidClass Asteroid in myAsteroids)
            {
                spriteBatch.Draw(asteroidTexture
                    , Asteroid.Position
                    , null
                    , Color.Pink
                    , Asteroid.Rotation
                    , new Vector2(asteroidTexture.Width / 2
                        , asteroidTexture.Height / 2)
                    , new Vector2(Asteroid.Size.X / asteroidTexture.Width
                        , Asteroid.Size.Y / asteroidTexture.Height)
                    , SpriteEffects.None
                    , 0);
            }

            foreach (MissleClass Missle in myMissles)
            {
                spriteBatch.Draw(missleTexture
                    , Missle.Position
                    , null
                    , Color.White
                    , Missle.Rotation
                    , new Vector2(missleTexture.Width / 2
                        , missleTexture.Height / 2)
                    , new Vector2(Missle.Size.X / missleTexture.Width, Missle.Size.Y / missleTexture.Height)
                    , SpriteEffects.None
                    , 0);
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
