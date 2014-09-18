using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace XNATBS
{
    public class InterfaceStartMenu : Interface
    {
        private Rectangle[] _rectangles;
        public Rectangle GetRectangle(int i)
        {
            return _rectangles[i];
        }

        private Rectangle _backgroundRectangle;
        public Rectangle BackgroundRectangle
        {
            get
            {
                return _backgroundRectangle;
            }
        }

        public override void Update(GameTime gameTime, MouseState mouse, KeyboardState keys)
        {
            HandlerKeyboard(keys);
            _oldKeyboardState = keys;

            HandlerMouse(mouse);
            _oldMouseState = mouse;
        }

        protected override void HandlerMouse(MouseState mouse)
        {
            _mousePosition = new Vector2(mouse.X, mouse.Y);
            if (mouse.LeftButton == ButtonState.Released && _oldMouseState.LeftButton == ButtonState.Pressed)
            {
                int? selected = null;
                for (int i = 0; i < _rectangles.Length; ++i)
                {
                    if (VectorInRectangle(_mousePosition, _rectangles[i]))
                    {
                        selected = i;
                        break;
                    }
                }

                if (selected == null)
                {
                    return;
                }

                switch (selected.Value)
                {
                    case(1):
                        _myGame.SwitchGameState(Game1.GameState.Battle);
                        break;
                    case(2):
                        _myGame.Exit();
                        break;
                }
            }

            
        }

        protected override void HandlerKeyboard(KeyboardState keys)
        {
            // do nothing
        }

        private void DeclareRectangles()
        {
            _rectangles = new Rectangle[3];

            // game title box
            _rectangles[0] = new Rectangle(0, 0, 500, 100);
            // new game
            _rectangles[1] = new Rectangle(0, 150, 500, 100);
            // quit
            _rectangles[2] = new Rectangle(0, 300, 500, 100);

            _backgroundRectangle = new Rectangle(0, 0, _myGame.GraphicsDevice.Viewport.Width, _myGame.GraphicsDevice.Viewport.Height);
        }

        public InterfaceStartMenu(Game1 myGame)
            : base(myGame)
        {
            DeclareRectangles();
        }
    }
}
