using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TrebleSketch_AIE_Asteroids
{
    class Cursor
    {
        public enum MenuButtonState
        {
            Render,
            Hover,
            Clicking
        }

        public Vector2 MousePosition;
        public Vector2 MouseSize;
        public int MouseScale;
        public MouseState state;

        public Texture2D MouseTexture;
        public Texture2D MouseTexturePressed;

        public Rectangle CursorRect;

        int i = 0;

        public void Initialize()
        {
            MouseSize = new Vector2(MouseTexture.Width, MouseTexture.Height);
            MouseScale = 1;
        }

        public void Update()
        {
            state = Mouse.GetState();
            MousePosition = new Vector2(state.X, state.Y);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            CursorRect = new Rectangle(
                        0
                        , 0
                        , MouseTexture.Width
                        , MouseTexture.Height);

            if (state.LeftButton == ButtonState.Pressed)
            {
                spriteBatch.Draw(
                    MouseTexturePressed
                    , MousePosition
                    , CursorRect
                    , Color.White);
            }
            else if (state.LeftButton == ButtonState.Released)
            {
                spriteBatch.Draw(
                    MouseTexture
                    , MousePosition
                    , CursorRect
                    , Color.White);
            }
        }

    }
}
