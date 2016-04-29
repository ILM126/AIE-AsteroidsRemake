using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EclipsingGameUtils
{
    public enum MouseButton
    {
        Right,
        Left,
        Middle
    }

    class InputHandler
    {
        private static MouseState currentState, lastState;
        private static KeyboardState currentStateK, lastStateK;
        private static GamePadState currentStateC, lastStateC;


        /// <summary>
        /// check if key is pressed, and wasnt pressed in last update.
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public static bool IsKeyDownOnce(Keys k)
        {
            currentStateK = Keyboard.GetState();
            if (currentStateK.IsKeyDown(k) && !lastStateK.IsKeyDown(k))
                return true;
            return false;
        }

        /// <summary>
        /// Check if key is not pressed, and was pressed last update.
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        public static bool IsKeyUpOnce(Keys k)
        {
            currentStateK = Keyboard.GetState();
            if (currentStateK.IsKeyUp(k) && !lastStateK.IsKeyUp(k))
                return true;
            return false;
        }

        /// <summary>
        /// Gets if the cursor intersects a rectangle.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool MouseInRectangle(Rectangle input)
        {
            currentState = Mouse.GetState();
            if (new Rectangle(currentState.Position, new Point(1, 1)).Intersects(input))
                return true;
            // Console.WriteLine("[INFO] Mouse Positon: " + currentState.Position.ToString());
            return false;
        }

        /// <summary>
        /// Get if a mouse has been clicked once.
        /// </summary>
        /// <param name="button">Which button to check</param>
        /// <returns></returns>
        public bool MouseButtonClickedOnce(MouseButton button)
        {
            currentState = Mouse.GetState();
            if (button == MouseButton.Left)
            {
                if (currentState.LeftButton == ButtonState.Pressed && lastState.LeftButton != ButtonState.Pressed)
                    return true;
            }
            else if (button == MouseButton.Right)
            {
                if (currentState.RightButton == ButtonState.Pressed && lastState.RightButton != ButtonState.Pressed)
                    return true;
            }
            else {
                if (currentState.MiddleButton == ButtonState.Pressed && lastState.MiddleButton != ButtonState.Pressed)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Run this at the end of each update, IMPORTANT!
        /// </summary>
        public static void Update()
        {
            lastState = currentState;
            lastStateK = currentStateK;
            lastStateC = currentStateC;
        }
    }
}