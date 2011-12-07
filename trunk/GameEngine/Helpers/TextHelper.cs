// Copyright 2010 Giovanni Botta

// This file is part of GeomClone.

// GeomClone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// GeomClone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeomClone.  If not, see <http://www.gnu.org/licenses/>.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Helpers
{
    public class StringToDraw : GameObject2DAbstract
    {
        #region Constructor
        public StringToDraw(BaseGame setGame, string txt, Vector2 pos, SpriteFont setFont)
            : base(setGame)
        {
            text = txt;
            position = pos;
            font = setFont;
            color = Color.YellowGreen;
            game = setGame;
        }
        #endregion

        #region Members
        string text;
        Vector2 position;
        Color color;
        BaseGame game;
        SpriteFont font;
        #endregion

        #region Properties
        public Vector2 Position { get { return position; } set { position = value; } }
        public string Text { get { return text; } set { text = value; } }
        #endregion

        #region Draw
        public override void Draw(GameTime time)
        {
            game.spriteBatch.DrawString(font, text, new Vector2(position.X * Width, position.Y * Height), color);
        }
        #endregion

        #region Functions
        public void HighlightOn()
        {
            color = Color.White;
        }
        public void HighlightOff()
        {
            color = Color.YellowGreen;
        }
        public Vector2 StringSize() { return font.MeasureString(text); }
        #endregion
    }
}
