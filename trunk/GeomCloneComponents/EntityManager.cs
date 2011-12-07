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
using GeomClone.Actors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GeomClone
{
    public class EntityManager : DrawableGameComponent
    {
        #region Constructor
        public EntityManager(GeomCloneGame setGame, Player setPlayer)
            : base(setGame)
        {
            player = setPlayer;

            enemies = new List<Enemy>();
            bullets = new List<Bullet>();
            enemiesToAdd = new List<Enemy>();
            enemiesToRemove = new List<Enemy>();
            bulletsToRemove = new List<Bullet>();
            game = setGame;
            vehicleList = new List<Vehicle>();
            bulletVehicleList = new List<Vehicle>();
            bombs = new List<Bomb>();
            bombsToRemove = new List<Bomb>();
        }
        #endregion

        #region Members
        GeomCloneGame game;
        protected Player player;
        protected List<Enemy> enemies;
        protected List<Enemy> enemiesToRemove;
        protected List<Enemy> enemiesToAdd;
        protected List<Bullet> bulletsToRemove;
        protected List<Bullet> bullets;
        protected List<Bomb> bombs;
        protected List<Bomb> bombsToRemove;
        bool colliding = false;
        List<Vehicle> vehicleList;
        List<Vehicle> bulletVehicleList;
        int killedEnemies = 0;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            killedEnemies += enemiesToRemove.Count;
            if (killedEnemies>0 && killedEnemies % 10 == 0)
            {
                player.Multiplier++;
                killedEnemies -= 10;
            }

            player.Update(gameTime);

            foreach (Enemy e in enemiesToRemove)
                enemies.Remove(e);

            foreach (Bullet b in bulletsToRemove)
                bullets.Remove(b);

            foreach (Bomb b in bombsToRemove)
                bombs.Remove(b);

            foreach (Enemy e in enemiesToAdd)
                enemies.Add(e);

            //enemies.RemoveAll(delegate(Enemy e) { return enemiesToRemove.Contains(e); });
            
            enemiesToRemove.Clear();
            bulletsToRemove.Clear();
            bombsToRemove.Clear();
            enemiesToAdd.Clear();
            vehicleList.Clear();
            bulletVehicleList.Clear();

            foreach (Bomb b in bombs)
            {
                b.Update(gameTime);
                if (b.ToDispose)
                    bombsToRemove.Add(b);
                else
                {   
                    foreach(Enemy e in enemies)
                        if (b.CollidesWith(e))
                        {
                            e.Die((e.Position-b.Position));
                            player.AddToScore(e.PointsWorth);
                        }
                }
            }
            foreach (Bullet b in bullets)
            {
                bulletVehicleList.Add(b.Vehicle);
                b.Update(gameTime);
                if (b.IsOutOfBoundary())
                    bulletsToRemove.Add(b);
                else
                {
                    foreach (Enemy e in enemies)
                    {
                        if (b.CollidesWith(e))
                        {
                            e.Die(b.Velocity);
                            bulletsToRemove.Add(b);
                            player.AddToScore(e.PointsWorth);                            
                            continue;
                        }
                    }
                }
            }
            foreach (Enemy e in enemies)
            {
                vehicleList.Add(e.Vehicle);
                e.Update(gameTime);
                if (!colliding)
                {
                    if (player.CollidesWith(e))
                        colliding = true;
                }
            }

            if (colliding)
            {
                ((GeomCloneLevel)game.CurrentLevel).OnPlayerDead();
                colliding = false;
            }

            //bullets.RemoveAll(delegate(Bullet b) { return b.Position.X > 1.05 || b.Position.X < -0.05 || b.Position.Y > 1.05 || b.Position.Y < -0.05; });

            //bullets.RemoveAll(delegate(Bullet b)
            //{
            //    foreach (Enemy e in enemies)
            //    {
            //        if (b.CollidesWith(e))
            //        {
            //            e.Die(b.Velocity);
            //            return true;
            //        }
            //    }
            //    return false;
            //});
            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            //if (colliding)
            //    game.CurrentLevel.GraphicsDevice.Clear(Color.Red);
            //else
            //    game.CurrentLevel.GraphicsDevice.Clear(Color.Black);

            player.Draw(gameTime);
            foreach (Enemy e in enemies)
                e.Draw(gameTime);
            foreach (Bullet b in bullets)
                b.Draw(gameTime);
            base.Draw(gameTime);
        }
        public void DrawStatic(GameTime time)
        {
            player.DrawStatic(time);
        }
        #endregion

        #region Game logic
        public void AddEnemy(Enemy e)
        {
            enemiesToAdd.Add(e);
        }
        public void AddEnemies(List<Enemy> l)
        {
            foreach (Enemy e in l) AddEnemy(e);
        }
        public void AddBullet(Bullet b)
        {
            bullets.Add(b);
        }
        internal void AddBomb(Bomb b)
        {
            bombs.Add(b);
        }
        public void AddBullets(List<Bullet> l)
        {
            foreach (Bullet b in l) AddBullet(b);
        }
        public void RemoveEnemy(Enemy e)
        {
            enemiesToRemove.Add(e);
        }
        public List<Vehicle> GetEnemyVehicles()
        {
            return vehicleList;
        }
        internal List<Vehicle> GetBulletsVehicles()
        {
            return bulletVehicleList;
        }
        private void RemoveAllEnemies()
        {
            enemies.Clear();
            bullets.Clear();
            vehicleList.Clear();
            bulletVehicleList.Clear();
            GC.Collect();
        }
        internal void OnPlayerDead()
        {
            foreach (Enemy e in enemies)
                e.OnPlayerDead();
        }
        internal void OnPlayerAlive()
        {
            RemoveAllEnemies();
        }
        #endregion
    }
}
