using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XNATBS
{
    public class DrawerStartMenu : Drawer
    {
        private SpriteFont _font;
        private Texture2D _background;
        private Texture2D _title;

        private InterfaceStartMenu _myInterface;

        protected override void Load_content()
        {
            // add game title here
            //throw new NotImplementedException();
            _font = _content.Load<SpriteFont>("hudFont");
            _background = _content.Load<Texture2D>("MenuBox");
            _title = _content.Load<Texture2D>("GameTitle");
        }

        public override void Update(GameTime gameTime)
        {
            // do nothing
            //throw new NotImplementedException();
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(_background, _myInterface.BackgroundRectangle, Color.White);

            spriteBatch.Draw(_title, _myInterface.GetRectangle(0), Color.Yellow);
            //spriteBatch.DrawString(_font, "Rado's game", new Vector2(0, 0), Color.Red);

            spriteBatch.Draw(_background, _myInterface.GetRectangle(1), Color.Yellow);
            spriteBatch.DrawString(_font, "New Game", new Vector2(0, 150), Color.Blue);

            spriteBatch.Draw(_background, _myInterface.GetRectangle(2), Color.Yellow);
            spriteBatch.DrawString(_font, "Quit", new Vector2(0, 300), Color.Blue);

            spriteBatch.End();
        }

        public DrawerStartMenu(Game1 myGame, ContentManager content, InterfaceStartMenu myInterface)
            : base(myGame, content)
        {
            _myInterface = myInterface;
            Load_content();
        }
    }
}
