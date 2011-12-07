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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using GameEngine.Helpers;

namespace GeomClone.Actors
{
    public abstract class Enemy : Actor
    {
        #region Members
        readonly ulong pointsWorth;
        #endregion

        #region Properties
        public ulong PointsWorth { get { return pointsWorth; } }
        #endregion

        #region Constructor
        public Enemy(GeomCloneLevel setCurrentLevel, Vector2 position, float mass, float maxSpeed, float maxForce, float maxTurnRate, float bRadius,ulong score)
            : base(setCurrentLevel, position, new Vector2(0.0f, 0.0f), mass, maxSpeed, maxForce, maxTurnRate, bRadius) { pointsWorth = score; }
        #endregion

        #region Update position
        public override void UpdatePosition(GameTime gameTime)
        {
            base.UpdatePosition(gameTime);
            float moveFactorPerSecond = (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            v.Update(gameTime);
            position = new Vector2(v.Position.X / (float)referenceWidth, v.Position.Y / (float)referenceHeight);
        }
        #endregion

        #region FSM
        public virtual void Die(Vector2 bulletVelocity) { ChangeState(new EnemyDead(CurrentLevel, this, bulletVelocity)); }
        public abstract void Activate();
        internal void OnPlayerDead()
        {
            ResetBehavior();
            Vehicle.Velocity = new Vector2(0, 0);
        }
        #endregion
    }
    public class Rhomb : Enemy
    {
        #region Constructor
        public Rhomb(GeomCloneLevel setCurrentLevel, Player evader, Vector2 position)
            : base(setCurrentLevel, position, 0.1f, 300, 500.0f, 720.0f, 0.007f,50)
        {
            Color c = Color.Aqua;

            rotation = 0.0f;

            float r = 45.0f;
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)8 / referenceWidth, (float)8 / referenceHeight), -r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)8 / referenceWidth, (float)8 / referenceHeight), r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)8 / referenceWidth, -(float)8 / referenceHeight), -r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)8 / referenceWidth, -(float)8 / referenceHeight), r, 0.5f, 0.5f, c));

            Behavior.PursuitOn(evader.Vehicle);
            Behavior.SeparationOn();
            //ChangeState(new BornState(theGame, this));
            ChangeState(new RhombBorn(CurrentLevel, this));
        }
        #endregion

        #region FSM
        public override void Activate()
        {
            ChangeState(new NormalState(CurrentLevel, this, new RhombSqueeze(CurrentLevel, this)));
        }
        #endregion
    }
    public class SmartRhomb : Enemy
    {
        #region Constructor
        public SmartRhomb(GeomCloneLevel currentLevel, Vector2 position, Player evader)
            : base(currentLevel, position, 0.1f, 400.0f, 150.0f, 720.0f, 0.007f, 100)
        {
            Color c = Color.LawnGreen;

            rotation = 0.0f;

            float r = 45.0f;
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)8 / referenceWidth, (float)8 / referenceHeight), -r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)8 / referenceWidth, (float)8 / referenceHeight), r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)8 / referenceWidth, -(float)8 / referenceHeight), -r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)8 / referenceWidth, -(float)8 / referenceHeight), r, 0.5f, 0.5f, c));

            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)8 / referenceWidth, 0), 90.0f, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)8 / referenceWidth, 0), 90.0f, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(0, (float)8 / referenceHeight), 0, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(0, -(float)8 / referenceHeight), 0, 0.25f, 0.5f, c));

            Behavior.PursuitOn(evader.Vehicle);
            Behavior.SeparationOn();
            Behavior.EvadeMultiOn(CurrentLevel.EntityManager.GetBulletsVehicles, 250);
            ChangeState(new BornState(CurrentLevel, this));
        }
        #endregion

        #region FSM
        public override void Activate()
        {
            ChangeState(new NormalState(CurrentLevel, this));
        }
        #endregion
    }

    public class FastRhomb : Enemy
    {
        #region Constructor
        public FastRhomb(GeomCloneLevel level, Player evader, Vector2 position)
            : base(level, position, 0.7f, 500, 1000.0f, 720.0f, 0.007f,50)
        {
            Color c = Color.Pink;

            rotation = 0.0f;

            float r = 45.0f;
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)8 / referenceWidth, (float)8 / referenceHeight), -r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)8 / referenceWidth, (float)8 / referenceHeight), r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)8 / referenceWidth, -(float)8 / referenceHeight), -r, 0.5f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)8 / referenceWidth, -(float)8 / referenceHeight), r, 0.5f, 0.5f, c));

            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, Vector2.Zero, 0, 0.7071f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, Vector2.Zero, 90.0f, 0.7071f, 0.5f, c));

            Behavior.PursuitOn(evader.Vehicle);
            Behavior.SeparationOn();
            ChangeState(new FastRhombBorn(CurrentLevel, this));
        }
        #endregion

        #region FSM
        public override void Activate()
        {
            ChangeState(new NormalState(CurrentLevel, this, new Spin(CurrentLevel, this, 600)));
        }
        public override void Die(Vector2 bulletVelocity)
        {
            ChangeState(new FastRhombDead(CurrentLevel, this, bulletVelocity));
        }
        #endregion
    }
    public class FastSmallRhomb : Enemy
    {
        #region Constructor
        public FastSmallRhomb(GeomCloneLevel currentLevel, Vector2 position)
            : base(currentLevel, position, 0.1f, 600, 700, 720.0f, 0.005f,25)
        {
            Color c = Color.Pink;

            rotation = 0.0f;

            float r = 45.0f;
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)4 / referenceWidth, (float)4 / referenceHeight), -r, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)4 / referenceWidth, (float)4 / referenceHeight), r, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2(-(float)4 / referenceWidth, -(float)4 / referenceHeight), -r, 0.25f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, new Vector2((float)4 / referenceWidth, -(float)4 / referenceHeight), r, 0.25f, 0.5f, c));

            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, Vector2.Zero, 0, 0.3535f, 0.5f, c));
            parts.Add(new ActorPart(Game, "smallRect", Position, rotation, Vector2.Zero, 90.0f, 0.3535f, 0.5f, c));

            //Behavior.SeparationOn();
            Behavior.WanderOn(100, 150, 100);
            Vehicle.Heading = RandomHelper.getRandomVector();
            ChangeState(new NormalState(CurrentLevel, this, new Spin(CurrentLevel, this, -400)));
        }
        #endregion

        #region FSM
        public override void Activate()
        {
            
        }
        #endregion

    }
    static class EnemyGeneratorHelper
    {
        #region Generator function
        public static List<Enemy> SpawnRhombFlock(int size, GeomCloneLevel level, Player evader)
        {
            Vector2 position = new Vector2(RandomHelper.getRandomFloat(), RandomHelper.getRandomFloat());

            List<Enemy> list = new List<Enemy>(size);
            for (int ii = 0; ii < size; ii++)
            {
                Vector2 displacement = RandomHelper.getRandomVector() * 0.1f;
                list.Add(new Rhomb(level, evader, position + displacement));
            }

            return list;
        }
        public static List<Enemy> Spawn3SmallRhombs(GeomCloneLevel level, Vector2 position)
        {
            List<Enemy> list = new List<Enemy>(3);
            for (int ii = 0; ii < 3; ii++)
            {
                Vector2 displacement = RandomHelper.getRandomVector() * 0.1f;
                list.Add(new FastSmallRhomb(level, position + displacement));
            }
            return list;
        }
        #endregion
    }
}
