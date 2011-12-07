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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngine.Helpers
{
    class FPSCounter : DrawableGameComponent
    {
        #region Constructor
        public FPSCounter(BaseGame setGame, string fontName)
            : base(setGame)
        {
            font = setGame.Content.Load<SpriteFont>(fontName);
            int W = setGame.GraphicsDevice.Viewport.Width;
            int H = setGame.GraphicsDevice.Viewport.Height;
            fps = new StringToDraw(setGame, "FPS=", new Vector2((W - 100.0f) / (float)W, 5.0f / (float)H), font);
            
        }
        #endregion

        #region Members
        StringToDraw fps;
        SpriteFont font;
        float lastFrameRate = 0;
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            float frameRate=(1000.0f / (float)gameTime.ElapsedGameTime.Milliseconds);
            if (Math.Abs(frameRate - lastFrameRate) > 0.5f)
                fps.Text = "FPS=" + frameRate;
            lastFrameRate = frameRate;

            fps.Draw(gameTime);

            
            base.Draw(gameTime);
        }
        #endregion
    }
}
