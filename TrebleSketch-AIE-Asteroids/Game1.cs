using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using EclipsingGameUtils;

namespace TrebleSketch_AIE_Asteroids
{
    /// <summary>
    /// Name: SpaceXterminator
    /// Description: A Game Where Elon Musk Must Destroy All The Tugboats That Is Stopping His Launches
    /// Version: 0.0.135 (First Playable)
    /// Developer: Titus Huang (Treble Sketch/ILM126)
    /// Game Engine: MonoGame
    /// Dev Notes: This is my first ever major game of any kind, tons of hard work is still needed >:D
    /// *** Ask Max about radians and stuff, where the missles will spawn over Elon's eyes no matter what orientation he is
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        #region Stuff :P
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
        float NUM_ASTEROIDS;
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

        Vector2 CentreScreen;

        float AsteroidLevel;
        float Level_Multiplier;

        float Level_Easy;
        float Level_Medium;
        float Level_Hard;
        float Level_Conspiracy;

        public int SceneID; // How I handle my scene, pretty bad. But it works for quick deployment! :D
                            // This version will be the messy one, but future games will implement a simplier version.
        bool Paused;                           
        string SceneName;
        bool PlayerInScene;
        bool AsteroidsInScene;
        bool GameFirstLoad = true;
        bool LoadViaCode = false;

        public class Message // http://gamedev.stackexchange.com/questions/28532/timer-for-pop-up-text-in-xna
        {
            public string Text { get; set; }
            public TimeSpan Appeared { get; set; }
            public Vector2 Position { get; set; }
        }

        // static readonly Vector2 BattleTextDisplacement = new Vector2(80, -100);
        List<Message> messages;
        Message ListMessages;
        TimeSpan messageDisappear = new TimeSpan(0, 0, 0, 0, 125);
        TimeSpan MaxAgeMessage = new TimeSpan(0, 0, 0, 5, 0);

        TimeSpan timeNow = new TimeSpan(0, 0, 0, 0, 0);

        Cursor MouseMovement;
        InputHandler UserInput;

        public Rectangle CursorRect;

        public class Buttons
        {
            public Texture2D MainMenu_StartButton;
            public Texture2D MainMenu_StartButton_Hover;
            public Texture2D MainMenu_StartButton_Clicked;
        }

        public Buttons MenuButton;

        TimeSpan lastRepeatChange;
        TimeSpan lastAudioChange;
        bool playedOnce;
        #endregion

        public Game1()
        {
            Debug = new Debugging();
            File.Delete(Debug.GetCurrentDirectory());
            GameVersionBuild = "v0.0.135 (29/04/16)";
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
            Debug.WriteToFile("[INFO] Started Initializing game...", true);

            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            graphics.ApplyChanges();

            CentreScreen = new Vector2(graphics.PreferredBackBufferWidth / 2
                , graphics.PreferredBackBufferHeight / 2);

            InitializeClasses();
            BeginInitialization();
            InitializeScene();

            base.Initialize();

            Debug.WriteToFile("[INFO] Finished initializing game...", true);
        }

        #region Initialization
        void BeginInitialization()
        {
            Debug.WriteToFile("[DEBUG] Initializing Ship", false);
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

            Debug.WriteToFile("[DEBUG] Initialized", false);

            Debug.WriteToFile("[DEBUG] Initializing Life and MouseMovement", false);
            Life.Size = new Vector2(10f, 105.2f);
            MouseMovement.CursorRect = CursorRect;
            Debug.WriteToFile("[DEBUG] Initialized", false);
        }

        void InitializeShip() // I ship it! - Lightwing <3
        {
            Debug.WriteToFile("[DEBUG] Loading Ship", false);
            Ship.Dead = false;
            Ship.Visible = true;
            Ship.Vunlerable = false;

            Ship.m_invulnerabliltyTimer = 5f;
            Ship.m_respawnTimer = 0f;

            Ship.m_respawnTime = 2f;
            Ship.m_invulnerabliltyTime = 7f;

            Ship.m_spawnPosition = CentreScreen;

            Ship.m_spawnPosition = Ship.Position;

            PlayerLives = 3;
            Ship.Health = 150f;
            Ship.ScoreIncrements = 15;
            Debug.WriteToFile("[INFO] Ship loaded", true);
        }

        void InitializeASteroids() // You Rock, Woho
        {
            randNum = new Random();

            for (int i = 0; i < (int)NUM_ASTEROIDS; i++)
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

                Asteroid.DamageDealt = 5f;

                myAsteroids.Add(Asteroid);
            }
            Debug.WriteToFile("[DEBUG] Loading " + myAsteroids.Count + " Asteroids onto screen", false);
        }

        void InitializeClasses()
        {
            MouseMovement = new Cursor();
            UserInput = new InputHandler();
            MenuButton = new Buttons();

            Ship = new ShipClass();
            Life = new LifeClass();

            ListMessages = new Message();
            messages = new List<Message>();
            myAsteroids = new List<AsteroidClass>();
            myMissles = new List<MissleClass>(); // Magic Missle!
            datExplosions = new List<ExplosionsClass>(); // Boom

            Ship.Debug = Debug;
        }

        void InitializeScene()
        {
            SceneID = 0;
            SceneName = "Tests Scene";
            Debug.WriteToFile("[INFO] Scene Changed to: " + SceneName, true);

            Level_Easy = 0.60f;
            Level_Medium = 1.25f;
            Level_Hard = 1.8f;
            Level_Conspiracy = 5f;
            Level_Multiplier = Level_Easy;
            AsteroidLevel = 0;
            NUM_ASTEROIDS = AsteroidLevel * Level_Multiplier + 7;
        }

        #endregion

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Debug.WriteToFile("[INFO] Started loading content...", true);

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
            MouseMovement.MouseTexture = Content.Load<Texture2D>("Cursor-v1");
            MouseMovement.MouseTexturePressed = Content.Load<Texture2D>("Cursor-v1-clicked");
            MenuButton.MainMenu_StartButton = Content.Load<Texture2D>("menu-StartGameButton-v1");
            MenuButton.MainMenu_StartButton_Hover = Content.Load<Texture2D>("menu-StartGameButton-v1-hover");
            MenuButton.MainMenu_StartButton_Clicked = Content.Load<Texture2D>("menu-StartGameButton-v1-clicked");

            Debug.WriteToFile("[INFO] Finished loading content...", true);
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
            MessageOnLoad(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || InputHandler.IsKeyDownOnce(Keys.Escape))
            {
                Debug.WriteToFile("[INFO] Exiting game...", true);
                Exit();
            }

            SceneManagement(gameTime);
            GameSceneManagement(gameTime);

            ICheckINput(gameTime);
            MouseMovement.Update();
            ToggleMusic(gameTime);

            while (messages.Count > 0 && messages[0].Appeared + MaxAgeMessage < gameTime.TotalGameTime)
            {
                messages.RemoveAt(0);
                Debug.WriteToFile("[DEBUG] Message being removed", false);
            }

            base.Update(gameTime);
        }

        #region Updates
        void MessageOnLoad(GameTime gameTime)
        {
            if (GameFirstLoad)
            {
                messages.Add(new Message()
                {
                    Text = "Scene ID: " + SceneID,
                    Appeared = gameTime.TotalGameTime,
                    Position = CentreScreen
                });
                Debug.WriteToFile("[DEBUG] Message Appeared Time: " + messages[0].Appeared.ToString(), false);
                GameFirstLoad = false;
                Debug.WriteToFile("[DEBUG] Game First Load: " + GameFirstLoad.ToString(), false);
            }
        }

        public void ICheckINput(GameTime gameTime)
        {
            if (PlayerInScene)
            {
                Ship.Acceleration = 0f;
                Ship.RotationDelta = 0f;
                Ship.SpeedLimit = 20f;

                if (InputHandler.IsKeyDownOnce(Keys.W))
                {
                    Ship.Acceleration = -0.08f;
                }
                if (InputHandler.IsKeyDownOnce(Keys.S))
                {
                    Ship.Acceleration = 0.05f;
                }
                if (InputHandler.IsKeyDownOnce(Keys.A))
                {
                    if (InputHandler.IsKeyDownOnce(Keys.LeftShift))
                    {
                        Ship.RotationDelta = -0.10f;
                    }
                    else
                    {
                        Ship.RotationDelta = -0.03f;
                    }
                }
                if (InputHandler.IsKeyDownOnce(Keys.D))
                {
                    if (InputHandler.IsKeyDownOnce(Keys.LeftShift))
                    {
                        Ship.RotationDelta = 0.10f;
                    }
                    else
                    {
                        Ship.RotationDelta = 0.03f;
                    }
                }
                if (InputHandler.IsKeyDownOnce(Keys.Q))
                {
                    Ship.Velocity = new Vector2(0, 0);
                }
                if (Ship.Visible == true)
                {
                    if (InputHandler.IsKeyDownOnce(Keys.Space))
                    {
                        ISpawnMISSle(gameTime);
                    }
                }
                GetCentre();
            }

            if (InputHandler.IsKeyDownOnce(Keys.D0))
            {
                SceneID = 0;
                Debug.WriteToFile("[INFO] Scene Changed to: " + SceneName, true);
                messages.Add(new Message()
                {
                    Text = "Scene ID: " + SceneID,
                    Appeared = gameTime.TotalGameTime,
                    Position = CentreScreen
                });
            }
            if (InputHandler.IsKeyDownOnce(Keys.D1))
            {
                SceneID = 1;
                Debug.WriteToFile("[INFO] Scene Changed to: " + SceneName, true);
                messages.Add(new Message()
                {
                    Text = "Scene ID: " + SceneID,
                    Appeared = gameTime.TotalGameTime,
                    Position = CentreScreen
                });
            }
            if (InputHandler.IsKeyDownOnce(Keys.D2))
            {
                SceneID = 2;
                Debug.WriteToFile("[INFO] Scene Changed to: " + SceneName, true);
                messages.Add(new Message()
                {
                    Text = "Scene ID: " + SceneID,
                    Appeared = gameTime.TotalGameTime,
                    Position = CentreScreen
                });
                if (!Paused)
                {
                    myAsteroids.Clear();
                    AsteroidLevel = 0;
                    AsteroidLevel++;
                    Ship.Score = 0;
                    InitializeShip();
                    InitializeASteroids();
                    CenterOfShip = Ship.Position;
                    Debug.WriteToFile("[DEBUG] Ship Position: " + Ship.Position, false);
                }
                NUM_ASTEROIDS = AsteroidLevel * Level_Multiplier + 7;
            }
            if (InputHandler.IsKeyDownOnce(Keys.D3))
            {
                SceneID = 3;
                Debug.WriteToFile("[INFO] Scene Changed to: " + SceneName, true);
                messages.Add(new Message()
                {
                    Text = "Scene ID: " + SceneID,
                    Appeared = gameTime.TotalGameTime,
                    Position = CentreScreen
                });
            }
            if (InputHandler.IsKeyDownOnce(Keys.D4))
            {
                SceneID = 4;
                Debug.WriteToFile("[INFO] Scene Changed to: " + SceneName, true);
                messages.Add(new Message()
                {
                    Text = "Scene ID: " + SceneID,
                    Appeared = gameTime.TotalGameTime,
                    Position = CentreScreen
                });
            }
        }

        void SceneManagement(GameTime gameTime)
        {
            switch (SceneID)
            {
                case 0:
                    SceneName = "Tests Scene";
                    if (LoadViaCode)
                    {
                        messages.Add(new Message()
                        {
                            Text = "Scene ID: " + SceneID,
                            Appeared = gameTime.TotalGameTime,
                            Position = CentreScreen
                        });
                        LoadViaCode = false;
                    }
                    PlayerInScene = false;
                    AsteroidsInScene = false;
                    break;
                case 1:
                    SceneName = "Main Menu";
                    if (LoadViaCode)
                    {
                        messages.Add(new Message()
                        {
                            Text = "Scene ID: " + SceneID,
                            Appeared = gameTime.TotalGameTime,
                            Position = CentreScreen
                        });
                        LoadViaCode = false;
                    }
                    PlayerInScene = false;
                    AsteroidsInScene = false;
                    break;
                case 2:
                    SceneName = "Game";
                    if (LoadViaCode)
                    {
                        messages.Add(new Message()
                        {
                            Text = "Scene ID: " + SceneID,
                            Appeared = gameTime.TotalGameTime,
                            Position = CentreScreen
                        });
                        LoadViaCode = false;
                    }
                    ICheckShip(gameTime);
                    IRotateMISSLes();
                    UpdatedExplosions(gameTime);
                    CheckCollisions();
                    PlayerInScene = true;
                    AsteroidsInScene = true;
                    break;
                case 3:
                    SceneName = "Settings - Diffculty";
                    if (LoadViaCode)
                    {
                        messages.Add(new Message()
                        {
                            Text = "Scene ID: " + SceneID,
                            Appeared = gameTime.TotalGameTime,
                            Position = CentreScreen
                        });
                        LoadViaCode = false;
                    }
                    PlayerInScene = false;
                    AsteroidsInScene = false;
                    break;
                case 4:
                    SceneName = "Game Over";
                    if (LoadViaCode)
                    {
                        messages.Add(new Message()
                        {
                            Text = "Scene ID: " + SceneID,
                            Appeared = gameTime.TotalGameTime,
                            Position = CentreScreen
                        });
                        LoadViaCode = false;
                    }
                    if (timeNow + MaxAgeMessage < gameTime.TotalGameTime)
                    {
                        SceneID = 1;
                        timeNow = new TimeSpan(0, 0, 0, 0, 0);
                    }
                    PlayerInScene = false;
                    AsteroidsInScene = false;
                    break;
                default:
                    SceneName = "Deafult Scene";
                    if (LoadViaCode)
                    {
                        messages.Add(new Message()
                        {
                            Text = "Scene ID: " + SceneID,
                            Appeared = gameTime.TotalGameTime,
                            Position = CentreScreen
                        });
                        LoadViaCode = false;
                    }
                    PlayerInScene = false;
                    AsteroidsInScene = false;
                    break;
            }
        }

        void GameSceneManagement(GameTime gameTime)
        {
            if (AsteroidsInScene)
            {
                ICheckASteroids();
                if (myAsteroids.Count == 0)
                {
                    AsteroidLevel++;
                    Debug.WriteToFile("[DEBUG] Starting Level: " + AsteroidLevel, false);
                    Debug.WriteToFile("[DEBUG] Level Multiplier: " + Level_Multiplier, false);
                    switch ((int)AsteroidLevel)
                    {
                        case 0:
                        case 1:
                            NUM_ASTEROIDS = AsteroidLevel + Level_Multiplier + 5;
                            break;
                        case 2:
                        case 3:
                            NUM_ASTEROIDS = AsteroidLevel * Level_Multiplier + 7;
                            break;
                        case 4:
                        case 5:
                        case 6:
                            NUM_ASTEROIDS = (AsteroidLevel * 2f) * Level_Multiplier + 15;
                            break;
                        case 7:
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                            NUM_ASTEROIDS = (AsteroidLevel * 3f) * Level_Multiplier + 20;
                            break;
                        case 12:
                        case 13:
                        case 14:
                            NUM_ASTEROIDS = (AsteroidLevel * 3.5f) * Level_Multiplier + 20;
                            break;
                        case 15:
                        case 16:
                        case 17:
                            NUM_ASTEROIDS = (AsteroidLevel * 5f) * Level_Multiplier + 15;
                            break;
                        case 18:
                        case 19:
                        case 20:
                            NUM_ASTEROIDS = (AsteroidLevel * 4f) * Level_Multiplier + 30;
                            break;
                        default:
                            NUM_ASTEROIDS = AsteroidLevel * Level_Multiplier + 5;
                            LoadViaCode = false;
                            break;
                    }
                    InitializeASteroids();
                    Ship.Vunlerable = true;
                }
            }
            else
            {
                myAsteroids.Clear();
            }

            if (PlayerInScene)
            {
                if (PlayerLives == 0)
                {
                    SceneID = 4;
                    timeNow = gameTime.TotalGameTime;
                }
            }
            else
            {
                Ship.Visible = false;
                Ship.Dead = true;
                Ship.Vunlerable = false;
                Ship.Die();
            }
        }

        public void ToggleMusic(GameTime gameTime)
        {
            TimeSpan last = gameTime.TotalGameTime - lastAudioChange;
            if (Keyboard.GetState().IsKeyDown(Keys.M))
            {
                if (!playedOnce)
                {
                    if (MediaPlayer.State != MediaState.Playing)
                    {
                        MediaPlayer.Play(backgroundMusicFull); // PLAY DIS
                        MediaPlayer.Volume -= 0.75f;
                        playedOnce = true;
                        Debug.WriteToFile(backgroundMusicFull.Name.ToString() + " just played for the first time", true);
                    }
                }
                else {
                    if (last > new TimeSpan(0, 0, 0, 5, 0))
                    {
                        if (MediaPlayer.State != MediaState.Playing)
                        {
                            MediaPlayer.Play(backgroundMusicFull); // PLAY DIS
                        }
                    }
                    lastAudioChange = gameTime.TotalGameTime;
                }
            }
            TimeSpan lastRepeat = gameTime.TotalGameTime - lastRepeatChange;
            if (Keyboard.GetState().IsKeyDown(Keys.L) && lastRepeat > new TimeSpan(0, 0, 0, 2, 0))
            {
                MediaPlayer.IsRepeating = !MediaPlayer.IsRepeating;
                lastRepeatChange = gameTime.TotalGameTime;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.K))
            {
                MediaPlayer.Stop();
                Debug.WriteToFile(backgroundMusicFull.Name.ToString() + " just stopped", true);
            }
        }

        #endregion

        #region Detectors/Collisions
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
                Ship.Visible = true;
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

                missle.Position = new Vector2 (Ship.Position.X + 12, Ship.Position.Y);

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
            if (timeSinceLastShot > shotCoolDown)
            {
                MissleClass missle = new MissleClass();

                missle.Position = new Vector2 (Ship.Position.X - 12, Ship.Position.Y);

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
                        if (Ship.Health != 0 && !Ship.Vunlerable)
                        {
                            Debug.WriteToFile("[INFO] Ship Health is now: " + Ship.Health + "HP", true);
                        }
                    }
                    if (Ship.Vunlerable && Ship.Visible)
                    {
                        if (Ship.Health == 0)
                        {
                            CreateExplosion(Ship.Position, ExplosionType.SHIP);
                            Ship.Dead = true;
                            Ship.Die();
                            PlayerLives--;
                            Debug.WriteToFile("[INFO] Ship Loses Life, now on: " + PlayerLives, true);
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
                        //Debug.WriteToFile("[INFO] An Elon Missle hit a Barge Ship", true);
                        Debug.WriteToFile("[DEBUG] " + myAsteroids.Count + " Asteroids left on screen", false);
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
        #endregion

        #region Draw UI
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
            spriteBatch.DrawString(scoreText, "HEALTH : " + Ship.Health.ToString(), new Vector2(170, 10), Color.White);
        }

        void DrawLevel()
        {
            spriteBatch.DrawString(scoreText, "Level : " + (int)AsteroidLevel, new Vector2(320, 10), Color.White);
        }
        #endregion

        #region Explosions
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
        #endregion

        #region GetCentres
        public void GetCentre()
        {
            if (InputHandler.IsKeyDownOnce(Keys.E))
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
        #endregion

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

            if (SceneID == 2)
            {
                DrawExplosions();
                DrawHealth();
                DrawScore();
                DrawLevel();
                DrawLives(spriteBatch);
            }

            if (SceneID == 4)
            {
                spriteBatch.DrawString(scoreText, "Game Over Pal!", new Vector2(CentreScreen.X, CentreScreen.Y - 50), Color.Red);
            }

            MouseMovement.Draw(spriteBatch);

            foreach (var message in messages)
                spriteBatch.DrawString(scoreText, message.Text, message.Position, Color.Lime);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}