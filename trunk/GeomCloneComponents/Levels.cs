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

using System.Collections.Generic;
using GameEngine;
using GameEngine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GeomClone.Actors;
using Microsoft.Xna.Framework.Input;

namespace GeomClone
{   
    public abstract class GeomCloneLevel : BaseLevel
    {
        #region States
        protected enum LevelState { PlayerAlive, PlayerDead }
        internal void OnPlayerDead()
        {
            ChangeState(LevelState.PlayerDead);
        }
        protected void ChangeState(LevelState newState)
        {
            state = newState;
            if (state == LevelState.PlayerDead)
            {
                if (player.numberOfLives == 0)
                    game.ChangeLevel(new GeomCloneMainMenu((GeomCloneGame)game));
                else
                {
                    entityManager.OnPlayerDead();
                    player.OnDead(Input);
                }
            }
            else if (state == LevelState.PlayerAlive)
            {
                entityManager.OnPlayerAlive();
                player.OnAlive(Input);
            }
        }
        #endregion

        #region Constructor
        public GeomCloneLevel(GeomCloneGame setGame)
            : base(setGame)
        {
            Input.SetActionOnTriggered(Microsoft.Xna.Framework.Input.Keys.P, delegate(GameTime time) { setGame.ChangeLevel(new GamePause(game)); });            
            player = new Player(this, "smallRect");
            player.SetMovementControl(Input);
            entityManager = new EntityManager(setGame, player);

            targetSize = new Vector2(
                GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height
                );

            blur = game.Content.Load<Effect>("Blur");
            blur.Parameters["TargetSize"].SetValue(targetSize);
            blur.Parameters["GlowScalar"].SetValue(intensity);
            blur.Parameters["numPixel"].SetValue(30);
            //blur.Parameters["Intensity"].SetValue(hue);

            //Input.SetActionOnPressed(Keys.PageUp, delegate(GameTime t) { intensity += 0.01f; blur.Parameters["GlowScalar"].SetValue(intensity); });
            //Input.SetActionOnPressed(Keys.PageDown, delegate(GameTime t) { intensity -= 0.01f; blur.Parameters["GlowScalar"].SetValue(intensity); });

            //Input.SetActionOnPressed(Keys.Z, delegate(GameTime t) { hue += 0.01f; blur.Parameters["Intensity"].SetValue(hue); });
            //Input.SetActionOnPressed(Keys.X, delegate(GameTime t) { hue -= 0.01f; blur.Parameters["Intensity"].SetValue(hue); });

            target1 = new RenderTarget2D(
                GraphicsDevice,
                (int)targetSize.X,
                (int)targetSize.Y,
                0,
                SurfaceFormat.Color
            );

            target2 = new RenderTarget2D(
                GraphicsDevice,
                (int)targetSize.X,
                (int)targetSize.Y,
                0,
                SurfaceFormat.Color
            );

            fps = new FPSCounter(game, "GeomCloneFont");

            state = LevelState.PlayerAlive;

            borders = new Line2D[4];
            borders[0] = new Line2D(setGame, new Vector2(0.5f, 0.0f), 0.0f, 2.5f, 1600.0f, Color.White);
            borders[1] = new Line2D(setGame, new Vector2(0.5f, 1.0f), 0.0f, 2.5f, 1600.0f, Color.White);
            borders[2] = new Line2D(setGame, new Vector2(0.0f, 0.5f), 90.0f, 2.5f, 1280.0f, Color.White);
            borders[3] = new Line2D(setGame, new Vector2(1.0f, 0.5f), 90.0f, 2.5f, 1280.0f, Color.White);
        }
        #endregion

        #region Properties
        public EntityManager EntityManager { get { return entityManager; } }
        public new GeomCloneGame Game { get { return (GeomCloneGame)game; } }
        internal List<Vehicle> GetNeighbors() { return entityManager.GetEnemyVehicles(); }
        internal Player Player { get { return player; } }
        internal EnemyGenerator EnemyGenerator { get { return enemyGenerator; } set { enemyGenerator = value; } }
        #endregion

        #region Members
        Line2D[] borders;
        EntityManager entityManager;
        protected Texture2D texture;
        protected Player player;
        EnemyGenerator enemyGenerator;
        Effect blur;
        RenderTarget2D target1, target2;
        Vector2 targetSize;
        //float intensity = 0.81f;
        float intensity = 0.385f;
        //float hue = 0.17f;
        //float hue = 0.23f;
        FPSCounter fps;
        LevelState state;
        float counter = 0;
        #endregion

        #region Update
        public override void Update(GameTime time)
        {
            entityManager.Update(time);
            if (state == LevelState.PlayerAlive)
                if (enemyGenerator != null)
                    enemyGenerator.Update(time);

            if (state == LevelState.PlayerDead)
            {
                counter += (float)time.ElapsedGameTime.Milliseconds;
                if (counter > 1500)
                {
                    ChangeState(LevelState.PlayerAlive);
                    counter = 0;
                }
            }

            float scale = 1.5f;
            float movementFactor = 0.6f;

            Matrix tmp = Matrix.CreateScale(scale) * Matrix.CreateTranslation(
                movementFactor * new Vector3(GraphicsDevice.Viewport.Width / 2 - player.Position.X * GraphicsDevice.Viewport.Width * scale,
                     GraphicsDevice.Viewport.Height / 2 - player.Position.Y * GraphicsDevice.Viewport.Height * scale, 0));
            Vector2 vTmp = Vector2.Transform(player.Position, tmp);
            

            TransformMatrix = tmp;

            base.Update(time);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.SetRenderTarget(0, target1);
            //GraphicsDevice.SetRenderTarget(0, null);
            GraphicsDevice.Clear(Color.Black);

            // Draw on the target1
            base.Draw(gameTime);

            // Draw post-screen fx
            #region Effects render
            for (int i = 0; i < blur.CurrentTechnique.Passes.Count; i++)
            {
                if (i == 0)
                    GraphicsDevice.SetRenderTarget(0, target2);
                else
                    GraphicsDevice.SetRenderTarget(0, null);

                game.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);

                blur.Begin();

                EffectPass pass = blur.CurrentTechnique.Passes[i];

                pass.Begin();

                Texture2D tex = (i == 0) ? target1.GetTexture() : target2.GetTexture();

                game.spriteBatch.Draw(tex, new Vector2(0, 0), Color.White);

                pass.End();

                blur.End();

                //if (i == 1)
                //    game.spriteBatch.Draw(target1.GetTexture(), new Vector2(0, 0), Color.White);

                game.spriteBatch.End();
            }
            #endregion
        }
        public override void DrawSprites(GameTime time)
        {   
            entityManager.Draw(time);
            DrawBorders(time);
        }
        public override void DrawStaticSprites(GameTime time)
        {
            entityManager.DrawStatic(time);
            fps.Draw(time);
        }
        public void DrawBorders(GameTime time)
        {
            for (int i = 0; i < 4; ++i)
                borders[i].Draw(time);
        }
        #endregion

        #region EnemyGenerator FSM
        public void ChangeEnemyGenerator(EnemyGenerator newGenerator)
        {
            enemyGenerator = newGenerator;
        }
        #endregion
        
    }
    public class GeomCloneMenu : BaseLevel
    {

        #region Constructor
        public GeomCloneMenu(BaseGame game) : base(game)
        {
            font = game.Content.Load<SpriteFont>("GeomCloneFont");
            menuItems = new System.Collections.ArrayList();
            menuActions = new Dictionary<int, Input.Action>();

            Input.SetActionOnTriggered(Microsoft.Xna.Framework.Input.Keys.Down, ScrollMenuDown);
            Input.SetActionOnTriggered(Microsoft.Xna.Framework.Input.Keys.Up, ScrollMenuUp);
            Input.SetActionOnTriggered(Microsoft.Xna.Framework.Input.Keys.Space, ActivateMenuItem);

            Input.SetActionOnTriggered(PlayerIndex.One, Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickDown, ScrollMenuDown);
            Input.SetActionOnTriggered(PlayerIndex.One, Microsoft.Xna.Framework.Input.Buttons.LeftThumbstickUp, ScrollMenuUp);
            Input.SetActionOnTriggered(PlayerIndex.One, Microsoft.Xna.Framework.Input.Buttons.A, ActivateMenuItem);
        }
        #endregion

        #region Methods
        protected void AddMenuItem(string str, GameEngine.Helpers.Input.Action action)
        {
            menuItems.Add(new StringToDraw(game, str, new Vector2(), font));
            if (menuItems.Count == 1)
                UpdateSelected();

            AddAction(menuItems.Count - 1, action);

            height = 0;
            maxLength = 0;
            foreach (StringToDraw s in menuItems)
            {
                Vector2 v = s.StringSize();
                height += v.Y;
                maxLength = MathHelper.Max(maxLength, v.X);
            }

            float startY = (game.Height() - height) / 2 + ((StringToDraw)menuItems[0]).StringSize().Y / 2;
            float rowHeight=height/menuItems.Count;
            for (int ii = 0; ii < menuItems.Count; ii++)
                ((StringToDraw)menuItems[ii]).Position = new Vector2(0.5f, (startY + ii * rowHeight)/game.Height());

        }
        private void ScrollMenuDown(GameTime time) 
        {
            currentSelectedItem = (currentSelectedItem + 1) % menuItems.Count;
            UpdateSelected();
        }
        private void ScrollMenuUp(GameTime time)
        {
            currentSelectedItem = (currentSelectedItem + menuItems.Count - 1) % menuItems.Count;
            UpdateSelected();
        }
        private void ActivateMenuItem(GameTime time)
        {
            if (menuActions.ContainsKey(currentSelectedItem))
                menuActions[currentSelectedItem](time);
        }
        private void UpdateSelected()
        {
            foreach (StringToDraw s in menuItems)
                s.HighlightOff();
            if (menuItems.Count > 0)
                ((StringToDraw)menuItems[currentSelectedItem]).HighlightOn();
        }
        private void AddAction(int menuItem, GameEngine.Helpers.Input.Action action)
        {
            menuActions.Add(menuItem, action);
        }
        #endregion

        #region Members
        SpriteFont font;
        System.Collections.ArrayList menuItems;
        Dictionary<int, GameEngine.Helpers.Input.Action> menuActions;
        float height, maxLength;
        int currentSelectedItem = 0;
        #endregion

        #region Update
        public override void Update(GameTime time)
        {
            
            base.Update(time);
        }
        #endregion

        #region Draw
        public override void DrawSprites(GameTime time) { }
        public override void DrawStaticSprites(GameTime time)
        {
            foreach (StringToDraw s in menuItems)
                s.Draw(time);
        }
        #endregion

        #region Properties
        protected SpriteFont Font { get { return font; } }
        #endregion
    }
    public class GeomCloneMainMenu : GeomCloneMenu
    {
        #region Constructor
        public GeomCloneMainMenu(GeomCloneGame game)
            : base(game)
        {
            AddMenuItem("Evolved mode", delegate(GameTime time) { game.ChangeLevel(new GeomCloneEvolved(game)); });
            AddMenuItem("Peace Keeper mode", delegate(GameTime time) { game.ChangeLevel(new GeomClonePeaceKeeper(game)); });
            AddMenuItem("Exit", delegate(GameTime time) { game.Exit(); });

        }
        #endregion

        #region Members
        #endregion

        #region Draw
        public override void Draw(GameTime time)
        {   
            base.Draw(time);
        }
        #endregion
    }
    public class GeomClonePeaceKeeper : GeomCloneLevel
    {  
        #region Constructor
        public GeomClonePeaceKeeper(GeomCloneGame setGame)
            : base(setGame)
        {
            ChangeEnemyGenerator(new RhombGenerator(this));
        }
        #endregion
    }
    public class GeomCloneEvolved : GeomCloneLevel
    {
        #region Constructor
        public GeomCloneEvolved(GeomCloneGame setGame)
            : base(setGame)
        {
            player.SetWeaponsControl(Input);
            ChangeEnemyGenerator(new CornerGenerator(this, EnemyType.SmartRhomb, 10));
        }
        #endregion

        #region Members
        float counterTime = 0;
        #endregion

        #region Update
        public override void Update(GameTime time)
        {
            counterTime += (float)time.ElapsedGameTime.Milliseconds;
            if (counterTime > 7000)
            {
                ChangeEnemyGenerator(new CornerGenerator(this, EnemyType.SmartRhomb, 10));
                counterTime = 0;
            }
            base.Update(time);
        }
        #endregion
    }
    public class GeomCloneTestLevel
        : GeomCloneLevel 
    {
        SmartRhomb r;
        #region Constructor
        public GeomCloneTestLevel(GeomCloneGame setGame)
            : base(setGame)
        {
            player.SetWeaponsControl(Input);
            r = new SmartRhomb(this, new Vector2(0.5f,0.8f), player);
            this.EntityManager.AddEnemy(r);
            this.EnemyGenerator = null;
        }
        #endregion
    }
}