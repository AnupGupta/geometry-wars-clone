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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameEngine.Helpers;
using Microsoft.Xna.Framework.Input;
#endregion

namespace GeomClone
{
    public class GeomCloneGame : GameEngine.BaseGame
    {
        #region Constructor
        public GeomCloneGame() { }
        #endregion

        #region Load/Unload
        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            SetResolution(1280, 1024);

            //GameEngine.GameObject2DAbstract.SetResolution(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);

            //ChangeLevel(new GeomClonePeaceKeeper(this));

            ChangeLevel(new GeomCloneMainMenu(this));

            //ChangeLevel(new GeomCloneTestLevel(this));
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        #region Update
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        #endregion

        #region Draw
        protected override void Draw(GameTime gameTime)
        {   
            base.Draw(gameTime);
        }
        #endregion

    }
}
