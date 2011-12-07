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

#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GameEngine.Helpers;
#endregion

namespace GameEngine
{
    public abstract class BaseLevel : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Members
        protected BaseGame game;
        private Input input;
        Matrix transformMatrix;
        #endregion

        #region Constructor
        public BaseLevel(BaseGame theGame)
            : base(theGame)
        {
            game = theGame;
            Initialize();
            input = new Input();
            Input.SetActionOnPressed(Keys.Escape, delegate(GameTime t) { game.Exit(); });
            transformMatrix = Matrix.Identity;
        }
        #endregion

        //#region Functions
        //internal virtual List<Vehicle> GetNeighbors(Vehicle owner) { return new List<Vehicle>(); }
        //#endregion

        #region Properties
        protected Input Input { get { return input; } }
        protected Matrix TransformMatrix { get { return transformMatrix; } set { transformMatrix = value; } }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);
            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            game.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None, transformMatrix);
            DrawSprites(gameTime);
            game.spriteBatch.End();
            game.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            DrawStaticSprites(gameTime);
            game.spriteBatch.End();
            base.Draw(gameTime);
        }
        public abstract void DrawSprites(GameTime time);
        public abstract void DrawStaticSprites(GameTime time);
        #endregion

    }

    public class GamePause : BaseLevel
    {
        #region Constructor
        public GamePause(BaseGame theGame)
            : base(theGame)
        {
            Input.SetActionOnTriggered(Keys.P, delegate(GameTime time) { game.GoToPreviousLevel(); });
        }
        #endregion

        #region Draw
        public override void Draw(GameTime time)
        {
            // keeps drawing but not updating the previous level
            game.PreviousLevel.Draw(time);
        }
        public override void DrawSprites(GameTime time) { }
        public override void DrawStaticSprites(GameTime time) { }
        #endregion
    }
}
