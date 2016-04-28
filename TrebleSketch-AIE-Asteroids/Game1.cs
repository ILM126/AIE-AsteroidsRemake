using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Design;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using System.IO;

namespace TrebleSketch_AIE_Asteroids
{
    /// <summary>
    /// Name: SpaceXterminator
    /// Description: A Game Where Elon Musk Must Destroy All The Tugboats That Is Stopping His Launches
    /// Version: 0.0.126 (First Playable)
    /// Developer: Titus Huang (Treble Sketch/ILM126)
    /// Game Engine: MonoGame
    /// Dev Notes: This is my first ever major game of any kind, tons of hard work is still needed >:D
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region Stuff :P
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

        Debugging Debug;

        ShipClass Ship;

        Vector2 CenterOfShip;

        List<AsteroidClass> myAsteroids;
        public Texture2D asteroidTexture;
        const int NUM_ASTEROIDS = 40;
        Random randNum;

        public Texture2D missleTexture;
        List<MissleClass> myMissles;

        TimeSpan lastShot = new TimeSpan(0, 0, 0, 0, 0);
        TimeSpan shotCoolDown = new TimeSpan(0, 0, 0, 0, 125);

        TimeSpan lastChange;

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

        public Song backgroundMusicIntro;
        public Song backgroundMusicCore;
        public Song backgroundMusicEnd;
        public Song backgroundMusicFull;

        class LifeClass
        {
            public Vector2 Size;
            public Texture2D Texture;
        }

        LifeClass Life;

        string GameVersionBuild;


#endregion

        public Game1()
        {
            Debug = new Debugging();
            File.Delete(Debug.GetCurrentDirectory());
            GameVersionBuild = "v0.0.124 (27/04/16)";
            Debug.WriteToFile("[INFO] Starting SpaceXterminator " + GameVersionBuild, true);
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

            InitializeASteroids();

            InitializeShip();

            Ship.Debug = Debug;

            myMissles = new List<MissleClass>(); // Magic Missle!

            datExplosions = new List<ExplosionsClass>(); // Boom

            CenterOfShip = Ship.Position;

            InitializeLife();

            base.Initialize();
        }

        public int CheckCurrentState(int CurrentState)
        {
            return CurrentState;
        }

        void InitializeShip() // I ship it! - Lightwing <3
        {
            Ship = new ShipClass();

            Ship.Position = new Vector2(
                graphics.PreferredBackBufferWidth / 2
                , graphics.PreferredBackBufferHeight / 2);
            Ship.Velocity = new Vector2(0, 0);
            Ship.Acceleration = 0;
            Ship.Rotation = 0;
            Ship.RotationDelta = 0;

            Ship.Size = new Vector2(85.0f, 85.0f);
            Ship.Radius = Ship.Size.Y / 2; // CURRENTLY WORKING ON THIS!!!!!
            Ship.MaxLimit = new Vector2(graphics.PreferredBackBufferWidth + (Ship.Size.X / 2)
                , graphics.PreferredBackBufferHeight + (Ship.Size.Y / 2));
            Ship.MinLimit = new Vector2(0 - (Ship.Size.X / 2), 0 - (Ship.Size.Y / 2));

            Ship.Dead = false;
            Ship.Visible = true;
            Ship.Vunlerable = false;

            Ship.m_invulnerabliltyTimer = 5f;
            Ship.m_respawnTimer = 0f;

            Ship.m_respawnTime = 2f;
            Ship.m_invulnerabliltyTime = 7f;

            Ship.m_spawnPosition = Ship.Position;

            PlayerLives = 3;
            Ship.Health = 150f;
            Ship.ScoreIncrements = 15;
        }

        void InitializeASteroids() // You Rock, Woho
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
                Asteroid.MinLimit = new Vector2(0 - (Asteroid.Size.X + 100), 0 - (Asteroid.Size.Y + 100));

                Asteroid.DamageDealt = 5f;

                myAsteroids.Add(Asteroid);
            }
        }

        public void InitializeLife()
        {  
            Life = new LifeClass();

            Life.Size = new Vector2(10f, 105.2f);
        }

        // public static Song FromUri(string name, Uri uri);

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
            Life.Texture = Content.Load<Texture2D>("falcon-9");
            scoreText = Content.Load<SpriteFont>("scoreFont");
            shipExplosionTexture = Content.Load<Texture2D>("explosion");
            asteroidExplosionTexture = Content.Load<Texture2D>("explosion2");
            backgroundMusicIntro = Content.Load<Song>("ExtremeMugginsIntro");
            backgroundMusicCore = Content.Load<Song>("ExtremeMugginsCore");
            backgroundMusicEnd = Content.Load<Song>("ExtremeMugginsEnd");
            backgroundMusicFull = Content.Load<Song>("ExtremeMuggingsFull");

        }

        public void ToggleMusic(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.M))
            {
                if (MediaPlayer.State != MediaState.Playing)
                {
                    // MediaPlayer.Play(backgroundMusicIntro);
                    // MediaPlayer.Play(backgroundMusicCore);
                    // MediaPlayer.Play(backgroundMusicEnd);
                    MediaPlayer.Play(backgroundMusicFull);
                }
            }
            TimeSpan last = gameTime.TotalGameTime - lastChange;
            if (Keyboard.GetState().IsKeyDown(Keys.L) && last > new TimeSpan(0,0,0,2,0)) 
            {
                MediaPlayer.IsRepeating = !MediaPlayer.IsRepeating;
                lastChange = gameTime.TotalGameTime;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.K))
            {
                MediaPlayer.Stop();
            }
        }

        public void ICheckINput(GameTime gameTime)
        {
            Ship.Acceleration = 0f;
            Ship.RotationDelta = 0f;
            Ship.SpeedLimit = 20f;

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                Ship.Acceleration = -0.08f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                Ship.Acceleration = 0.05f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                Ship.RotationDelta = -0.03f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                Ship.RotationDelta = 0.03f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
            {
                Ship.Velocity = new Vector2(0, 0);
            }
            if (Ship.Visible == true)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    ISpawnMISSle(gameTime);
                }
            }
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Debug.WriteToFile("[INFO] Exiting game...", true);
                Exit();
            }
            if (myAsteroids.Count == 0)
            {
                InitializeASteroids();
            }

            ICheckINput(gameTime);
            ICheckShip(gameTime);
            ICheckASteroids();
            IRotateMISSLes();
            CheckCollisions();
            UpdatedExplosions(gameTime);

            GetCentre();

            ToggleMusic(gameTime);

            base.Update(gameTime);
        }

        public void GetCentre()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                Ship.Position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
                Ship.Velocity = new Vector2(0, 0);
                Ship.Rotation = 0;
            }
        }

        void GetCentreNow()
        {
                Ship.Position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
                Ship.Velocity = new Vector2(0, 0);
                Ship.Rotation = 0;
        }

        void ICheckShip(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Ship.m_invulnerabliltyTimer != 0)
            {
                Debug.WriteToFile("[DEBUG] m_invulnerabliltyTimer: " + Ship.m_invulnerabliltyTimer.ToString(), false);
            }
            if (Ship.Dead && Ship.m_invulnerabliltyTimer == 0f)
            {
                Ship.m_invulnerabliltyTimer = Ship.m_invulnerabliltyTime;
                Ship.m_respawnTimer = Ship.m_respawnTime;
                Debug.WriteToFile("[INFO] Timer Tripped after death", true);
            }

            if (Ship.m_respawnTimer > 0)
            {
                Ship.m_respawnTimer -= delta;
                Debug.WriteToFile("[INFO] Respawn in: " + Ship.m_respawnTimer.ToString(), false);
            }
            else if (!Ship.Visible)
            {
                Ship.m_respawnTimer = 0;
                Ship.Respawn();
                Debug.WriteToFile("[INFO] Player Respawned", true);
                Debug.WriteToFile("[INFO] Health after respawn is: " + Ship.Health.ToString(), true);
            }

            if (Ship.m_invulnerabliltyTimer > 0)
            {
                Ship.m_invulnerabliltyTimer -= delta;
                Ship.Vunlerable = false;
            }
            else if (Ship.m_invulnerabliltyTimer < 0)
            {
                Ship.m_invulnerabliltyTimer = 0;
                if (Ship.Dead == true && Ship.Visible == false)
                {
                    GetCentreNow();
                    Ship.Vunlerable = false;
                }
                Ship.Vunlerable = true;
            }

            Ship.Rotation += Ship.RotationDelta;

            Matrix playerRotationMatrix =
                Matrix.CreateRotationZ(Ship.Rotation);

            if (Ship.Velocity.X <= Ship.SpeedLimit || Ship.Velocity.Y <= Ship.SpeedLimit)
            {
                Ship.Velocity += Vector2.Transform(
                new Vector2(0, Ship.Acceleration)
                , playerRotationMatrix);

            }
            //Ship.Velocity += Vector2.Transform(
            //    new Vector2(0, Ship.Acceleration)
            //    , playerRotationMatrix);

            Ship.Position += Ship.Velocity;

            //Debug.WriteToFile("[DEBUG] Ship Y Velocity: " + Ship.Velocity.Y.ToString(), false);
            //Debug.WriteToFile("[DEBUG] Ship X Velocity: " + Ship.Velocity.X.ToString(), false);

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

        void ICheckASteroids()
        {
            foreach (AsteroidClass Asteroid in myAsteroids) // A-steroid's rotation
            {
                Asteroid.Position += Asteroid.Velocity;
                // Asteroid.Rotation += Asteroid.RotationDelta;

                if (Asteroid.Position.X > Asteroid.MaxLimit.X)
                {
                    Asteroid.Velocity.X *= -1f;
                }
                else if (Asteroid.Position.X < Asteroid.MinLimit.X)
                {
                    Asteroid.Velocity.X *= -1f;
                }

                if (Asteroid.Position.Y > Asteroid.MaxLimit.Y)
                {
                    Asteroid.Velocity.Y *= -1f;
                }
                else if (Asteroid.Position.Y < Asteroid.MinLimit.Y)
                {
                    Asteroid.Velocity.Y *= -1f;
                }
            }
        }

        public void ISpawnMISSle(GameTime gameTime)
        {
            TimeSpan timeSinceLastShot = gameTime.TotalGameTime - lastShot;

            if (timeSinceLastShot > shotCoolDown)
            {
                MissleClass missle = new MissleClass();

                missle.Position = Ship.Position;

                missle.Rotation = Ship.Rotation;

                Matrix missleRotationMatrix = Matrix.CreateRotationZ(missle.Rotation);
                missle.Velocity = new Vector2(0, -7);
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

        void IRotateMISSLes() // You Spin Me Right Round, Baby Right Round
        {
            foreach (MissleClass Missle in myMissles) // MISSle's rotation
            {
                Missle.Position += Missle.Velocity;

                if (Missle.Position.X > Missle.MaxLimit.X)
                {
                    Missle.Velocity.X *= 1;
                }
                else if (Missle.Position.X < Missle.MinLimit.X)
                {
                    Missle.Velocity.X *= 1;
                }

                if (Missle.Position.Y > Missle.MaxLimit.Y)
                {
                    Missle.Velocity.Y *= 1;
                }
                else if (Missle.Position.Y < Missle.MinLimit.Y)
                {
                    Missle.Velocity.Y *= 1;
                }
            }
        }

        bool CircleCollisionCheck(Vector2 object1Pos
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

        void CheckCollisions()
        {
            List<AsteroidClass> AsteroidDeathRow = new List<AsteroidClass>();
            List<MissleClass> MissleDeathRow = new List<MissleClass>();

            foreach (AsteroidClass Asteroid in myAsteroids)
            {
                bool playerCollisionCheck = CircleCollisionCheck(Ship.Position, Ship.Size.X / 2
                    , Asteroid.Position, Asteroid.Size.X / 2);
                if (playerCollisionCheck)
                {
                    if (Ship.Visible)
                    {
                        if (Ship.Vunlerable)
                        {
                            Ship.Health -= Asteroid.DamageDealt;
                        }
                        AsteroidDeathRow.Add(Asteroid);
                        CreateExplosion(Asteroid.Position, ExplosionType.ASTEROID);
                        Debug.WriteToFile("[INFO] Ship Health is now: " + Ship.Health + "HP", true);
                    }
                    if (Ship.Vunlerable && Ship.Visible)
                    {
                        if (Ship.Health == 0)
                        {
                            CreateExplosion(Ship.Position, ExplosionType.SHIP);
                            Ship.Dead = true;
                            Ship.Die();
                            PlayerLives--;
                        }
                    }
                }

                foreach (MissleClass Missle in myMissles)
                {
                    bool missleCollisionCheck = CircleCollisionCheck(Missle.Position, Missle.Size.X / 2
                    , Asteroid.Position, Asteroid.Size.X / 2);

                    if (missleCollisionCheck)
                    {
                        Ship.Score += Ship.ScoreIncrements;
                        MissleDeathRow.Add(Missle);
                        AsteroidDeathRow.Add(Asteroid);
                        CreateExplosion(Asteroid.Position, ExplosionType.ASTEROID);
                        Debug.WriteToFile("[INFO] An Elon Missle hit a Barge Ship", true);
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

        void DrawLives(SpriteBatch spriteBatch)
        {
            // Debug.WriteToFile("[DEBUG] No. of Player Lives: " + PlayerLives, false);
            for (int i = 0; i < PlayerLives; ++i)
            {
                spriteBatch.Draw(Life.Texture
                , new Vector2(i*10 + 25, 85)
                , null
                , Color.White
                , 0
                , new Vector2(Life.Texture.Width / 2
                    , Life.Texture.Height / 2)
                , new Vector2(Life.Size.X / Life.Texture.Width, Life.Size.Y / Life.Texture.Height)
                , SpriteEffects.None
                , 0);
            }
        }

        void DrawScore()
        {
            spriteBatch.DrawString(scoreText, "SCORE : " + Ship.Score.ToString(), new Vector2(10, 10), Color.White);
        }

        void DrawHealth()
        {
            spriteBatch.DrawString(scoreText, "HEALTH : " + Ship.Health.ToString(), new Vector2(150, 10), Color.White);
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

        void UpdatedExplosions(GameTime gameTime)
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

        void DrawExplosions()
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

            if (Ship.Visible == true)
            {
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
            }

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

            DrawExplosions();
            DrawHealth();
            DrawScore();
            DrawLives(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}