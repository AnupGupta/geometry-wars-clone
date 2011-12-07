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
using GameEngine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GeomClone.Actors
{
    public class Player : Actor
    {
        #region Member variables
        private readonly float rotationSpeed = 720.0f;
        private float moveFactorPerSecond;
        private float shootCount = 0;
        private Vector2 shootDirection;
        public int numberOfLives;
        public int numberOfBombs;
        string texture;
        StringToDraw livesString;
        StringToDraw bombsString;
        StringToDraw scoreString;
        StringToDraw multiplierString;
        ulong score = 0;
        ulong multiplier = 1;
        #endregion

        #region Properties
        public ulong Score { get { return score; } set { score = value; } }
        public ulong Multiplier { get { return multiplier; } set { multiplier = value; } }
        #endregion

        #region Constructor
        public Player(GeomCloneLevel currentLevel, string blockTexture)
            : base(currentLevel, new Vector2(0.5f, 0.5f), new Vector2(0.0f, 0.0f), 1.0f, 550, 800.0f, 720.0f, 0.008f)
        {
            texture = blockTexture;
            Init(3, 3);
        }
        private void Init(int nLives, int nBombs)
        {
            int W = Game.GraphicsDeviceManager.PreferredBackBufferWidth;
            int H = Game.GraphicsDeviceManager.PreferredBackBufferHeight;

            livesString = new StringToDraw(Game, "Lives: " + nLives,
                new Vector2((float)5.0f / W, (float)(H - 25.0f) / H), Game.Content.Load<SpriteFont>("GeomCloneFont"));
            scoreString = new StringToDraw(Game, "Score: 0",
                new Vector2((float)5.0f / W, (float)5.0f / H), Game.Content.Load<SpriteFont>("GeomCloneFont"));
            bombsString = new StringToDraw(Game, "Bombs: " + nBombs,
                new Vector2((float)5.0f / W, (float)(H - 50.0f) / H), Game.Content.Load<SpriteFont>("GeomCloneFont"));


            numberOfLives = nLives;
            numberOfBombs = nBombs;

            Color c = Color.White;
            //Color c = new Color(Color.White, 127);

            rotation = 45.0f;
            moveFactorPerSecond = 0.0f;
            shootDirection = new Vector2(1, 0);

            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2((float)7 / referenceWidth, (float)5 / referenceHeight), -40.0f, 0.4f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2(-(float)7 / referenceWidth, (float)5 / referenceHeight), 40.0f, 0.4f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2((float)5 / referenceWidth, -(float)4 / referenceHeight), -25.0f, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2(-(float)5 / referenceWidth, -(float)4 / referenceHeight), 25.0f, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2((float)12 / referenceWidth, -(float)8 / referenceHeight), 65.0f, 0.325f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2(-(float)12 / referenceWidth, -(float)8 / referenceHeight), -65.0f, 0.325f, 0.5f, c));

            ChangeState(new PlayerBorn(CurrentLevel, this));
        }
        #endregion

        #region Update position
        public override void UpdatePosition(GameTime gameTime)
        {
            base.UpdatePosition(gameTime);

            moveFactorPerSecond = (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            AdjustRotation();
            position += v.MaxSpeed * moveFactorPerSecond * new Vector2(velocity.X / (float)referenceWidth, velocity.Y / (float)referenceHeight);
            v.Position = new Vector2(position.X * (float)referenceWidth, position.Y * (float)referenceHeight);
            v.Velocity = v.MaxSpeed * velocity;
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        public void DrawStatic(GameTime time)
        {
            livesString.Draw(time);
            scoreString.Draw(time);
            
            bombsString.Draw(time);
        }
        #endregion

        #region Set controls
        public void SetMovementControl(Input input)
        {
            input.SetActionOnPressed(Keys.Up, GoUp);
            input.SetActionOnReleased(Keys.Up, ResetSpeed);
            input.SetActionOnPressed(Keys.Down, GoDown);
            input.SetActionOnReleased(Keys.Down, ResetSpeed);
            input.SetActionOnPressed(Keys.Left, GoLeft);
            input.SetActionOnReleased(Keys.Left, ResetSpeed);
            input.SetActionOnPressed(Keys.Right, GoRight);
            input.SetActionOnReleased(Keys.Right, ResetSpeed);

            input.SetLeftStickAction(PlayerIndex.One, SetVelocityDirection);
        }
        public void SetWeaponsControl(Input input)
        {
            input.SetActionOnPressed(Keys.W, ShootUp);
            input.SetActionOnPressed(Keys.S, ShootDown);
            input.SetActionOnPressed(Keys.A, ShootLeft);
            input.SetActionOnPressed(Keys.D, ShootRight);
            input.SetActionOnPressed(Keys.Space, UseBomb);

            input.SetActionOnTriggered(PlayerIndex.One, Buttons.LeftTrigger, UseBomb);
            input.SetActionOnTriggered(PlayerIndex.One, Buttons.RightTrigger, UseBomb);
            input.SetActionOnTriggered(PlayerIndex.One, Buttons.A, UseBomb);
            input.SetRightStickAction(PlayerIndex.One, Shoot);
        }
        public void ResetControls(Input input)
        {
            input.RemoveAction(Keys.Up);
            input.RemoveAction(Keys.Down);
            input.RemoveAction(Keys.Left);
            input.RemoveAction(Keys.Right);
            input.RemoveAction(Keys.W);
            input.RemoveAction(Keys.A);
            input.RemoveAction(Keys.S);
            input.RemoveAction(Keys.D);
            input.RemoveAction(Keys.Space);

            input.RemoveAction(PlayerIndex.One, Buttons.A);
            input.RemoveAction(PlayerIndex.One, Buttons.LeftTrigger);
            input.RemoveAction(PlayerIndex.One, Buttons.RightTrigger);
            input.RemoveLeftStickAction(PlayerIndex.One);
            input.RemoveRightStickAction(PlayerIndex.One);
        }
        #endregion

        #region Movement controls
        public void GoUp(GameTime gameTime)
        {
            velocity.Y = -1;
            velocity.Normalize();
        }
        public void GoDown(GameTime gameTime)
        {
            velocity.Y = 1;
            velocity.Normalize();
        }
        public void GoLeft(GameTime gameTime)
        {
            velocity.X = -1;
            velocity.Normalize();
        }
        public void GoRight(GameTime gameTime)
        {
            velocity.X = 1;
            velocity.Normalize();
        }
        private void AdjustRotation()
        {

            if (velocity.LengthSquared() < 0.001)
                return;

            Vector2 desiredHeading = Vector2.Normalize(velocity);
            Vector2 actualHeading = new Vector2(-(float)System.Math.Sin(MathHelper.Pi / 180.0f * rotation), (float)System.Math.Cos(MathHelper.Pi / 180.0f * rotation));

            float dotProduct = Vector2.Dot(desiredHeading, actualHeading);
            if (Math.Abs(dotProduct - 1) > 0.001)
            {
                if (dotProduct > 0)
                    rotation += moveFactorPerSecond * rotationSpeed;
                else
                    rotation -= moveFactorPerSecond * rotationSpeed;
            }
            rotation %= 360;

        }

        public void SetVelocityDirection(GameTime time, Vector2 direction)
        {
            if (direction.LengthSquared() > 0.001)
                velocity = Vector2.Normalize(direction);
            else
                velocity = Vector2.Zero;
        }

        public void ResetSpeed(GameTime gameTime)
        {
            velocity *= 0.0f;
        }
        #endregion

        #region Weapons controls
        public void ShootUp(GameTime time)
        {
            shootDirection.Y = -1;
            shootDirection.Normalize();
            Shoot(time);
        }
        public void ShootDown(GameTime time)
        {
            shootDirection.Y = 1;
            shootDirection.Normalize();
            Shoot(time);
        }
        public void ShootLeft(GameTime time)
        {
            shootDirection.X = -1;
            shootDirection.Normalize();
            Shoot(time);
        }
        public void ShootRight(GameTime time)
        {
            shootDirection.X = 1;
            shootDirection.Normalize();
            Shoot(time);
        }
        public void Shoot(GameTime time, Vector2 direction)
        {
            if (direction.LengthSquared() < 0.001)
                return;
            shootDirection = Vector2.Normalize(direction);
            Shoot(time);
        }
        public void Shoot(GameTime time)
        {
            if (shootCount >= 180)
            {
                //((GeomCloneLevel)Game.CurrentLevel).EntityManager.AddBullets(Bullet.SpawnBulletCouple(Game, BlockTexture, Position, shootDirection));
                CurrentLevel.EntityManager.AddBullets(Bullet.Spawn5Bullets(CurrentLevel, Position, shootDirection));
                shootCount = 0;
            }
            shootCount += time.ElapsedGameTime.Milliseconds;
        }
        public void UseBomb(GameTime time)
        {
            if (numberOfBombs > 0)
            {
                this.CurrentLevel.EntityManager.AddBomb(new Bomb(CurrentLevel, Position));
                numberOfBombs--;
                bombsString.Text = "Bombs: " + numberOfBombs;
            }
        }
        #endregion

        #region FSM
        internal void AddToScore(ulong val)
        {
            score += multiplier * val;
            scoreString.Text = "Score: " + score;
        }
        internal void OnDead(Input input)
        {
            ChangeState(new PlayerDead(CurrentLevel, this));
            numberOfLives--;
            ResetControls(input);
            livesString.Text = "Lives: " + numberOfLives;
        }
        internal void OnAlive(Input input, bool canShoot)
        {
            parts.Clear();
            Color c = Color.White;
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2((float)7 / referenceWidth, (float)5 / referenceHeight), -40.0f, 0.4f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2(-(float)7 / referenceWidth, (float)5 / referenceHeight), 40.0f, 0.4f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2((float)5 / referenceWidth, -(float)4 / referenceHeight), -25.0f, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2(-(float)5 / referenceWidth, -(float)4 / referenceHeight), 25.0f, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2((float)12 / referenceWidth, -(float)8 / referenceHeight), 65.0f, 0.325f, 0.5f, c));
            parts.Add(new ActorPart(Game, texture, position, rotation, new Vector2(-(float)12 / referenceWidth, -(float)8 / referenceHeight), -65.0f, 0.325f, 0.5f, c));

            ChangeState(new PlayerBorn(CurrentLevel, this));
            SetMovementControl(input);
            if (canShoot)
                SetWeaponsControl(input);
        }
        internal void OnAlive(Input input)
        {
            OnAlive(input, true);
        }
        #endregion

    }
}
