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
using GameEngine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
#endregion

namespace GameEngine
{
    public class BaseGame : Game
    {
        #region Member variables
        protected GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        Vector2 targetSize;
        #endregion

        #region Constructor
        public BaseGame():base()
        {
            //Content = new ContentManager(Services);
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            //graphics.SynchronizeWithVerticalRetrace = false;
            
        }

        protected override void Initialize()
        {   
            spriteBatch = new SpriteBatch(GraphicsDevice);
            targetSize = new Vector2(
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height
                );

            base.Initialize();
        }
        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
        {   
            if (currentLevel != null)
                currentLevel.Update(gameTime);

            base.Update(gameTime);
            //if (gameTime.IsRunningSlowly)
            //{ }
        }
        #endregion

        #region Level FSM
        private BaseLevel previousLevel, currentLevel;
        public BaseLevel PreviousLevel { get { return previousLevel; } }

        public void ChangeLevel(BaseLevel nextLevel)
        {
            previousLevel = currentLevel;
            currentLevel = nextLevel;
        }

        public void GoToPreviousLevel()
        {   
            BaseLevel temp = currentLevel;
            currentLevel = previousLevel;
            previousLevel = null;
        }
        #endregion

        #region Draw
        protected override void Draw(GameTime gameTime)
        {   
            if (currentLevel != null)
                currentLevel.Draw(gameTime);

            base.Draw(gameTime);
        }
        #endregion

        #region Functions
        //internal List<Vehicle> GetNeighbors(Vehicle owner)
        //{
        //    return currentLevel.GetNeighbors(owner);
        //}
        public void SetResolution(int w, int h)
        {
            graphics.PreferredBackBufferHeight = h;
            graphics.PreferredBackBufferWidth = w;
            graphics.ApplyChanges();
            targetSize = new Vector2(w, h);
        }
        #endregion

        #region Properties
        public BaseLevel CurrentLevel { get { return currentLevel; } }
        public int Height() { return graphics.PreferredBackBufferHeight; }
        public int Width() { return graphics.PreferredBackBufferWidth; }
        public GraphicsDeviceManager GraphicsDeviceManager { get { return graphics; } }
        #endregion
    }
}
