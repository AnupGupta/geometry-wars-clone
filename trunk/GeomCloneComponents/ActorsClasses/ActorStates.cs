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
using GameEngine.Helpers;

namespace GeomClone.Actors
{
    #region Actor FSM
    public abstract class ActorState
    {
        #region Constructor
        public ActorState(GeomCloneLevel setCurrentLevel, Actor setOwner, Animation setAnimation)
        {
            owner = setOwner; currentLevel = setCurrentLevel;
            Owner.ChangeAnimation(setAnimation);
        }
        #endregion

        #region Members
        Actor owner;
        GeomCloneLevel currentLevel;
        #endregion

        #region Properties
        public virtual bool Collidable { get { return true; } }
        public Actor Owner { get { return owner; } }
        protected GeomCloneLevel CurrentLevel { get { return currentLevel; } }
        #endregion

        #region Update
        public abstract void Update(GameTime time);
        #endregion
    }
    class NormalState : ActorState
    {
        #region Constructor
        public NormalState(GeomCloneLevel setCurrentLevel, Actor owner)
            : base(setCurrentLevel, owner, null)
        {
            Owner.ChangeAnimation(new NoAnimation(setCurrentLevel, Owner));
        }
        public NormalState(GeomCloneLevel level, Actor owner, Animation normalAnimation)
            : base(level, owner, normalAnimation) { }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            Owner.UpdatePosition(gameTime);
        }
        #endregion
    }
    class BornState : ActorState
    {
        #region Constructor
        public BornState(GeomCloneLevel setCurrentLevel, Actor owner)
            : base(setCurrentLevel, owner, null)
        {
            Owner.ChangeAnimation(new Flash(setCurrentLevel, Owner, 400));
        }
        public BornState(GeomCloneLevel level, Actor owner, Animation bornAnimation) : base(level, owner, bornAnimation) { timeOutCount = 0; }
        #endregion

        #region Members
        protected float timeOutCount;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            timeOutCount += gameTime.ElapsedGameTime.Milliseconds;
            if (timeOutCount > 1500)
                Owner.ChangeState(new NormalState(CurrentLevel, Owner));
        }
        #endregion

        #region Properties
        public override bool Collidable { get { return false; } }
        #endregion
    }
    class EnemyDead : ActorState
    {
        #region Constructor
        public EnemyDead(GeomCloneLevel level, Enemy enemy, Vector2 bulletVelocity)
            : base(level, enemy, null)
        {
            // Activate proper animation
            Owner.ChangeAnimation(new FadeOutDead(level, Owner));
            // Eliminate any eventual behavior
            Owner.ResetBehavior();
            // Sets the velocity of the dead particle to have the same direction of the bullet velocity
            Owner.Vehicle.Velocity = 100 * bulletVelocity;
        }
        #endregion

        #region Members
        float elapsedTime = 0;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            Owner.UpdatePosition(gameTime);
            elapsedTime += gameTime.ElapsedGameTime.Milliseconds;
            if (elapsedTime >= 1500)
                CurrentLevel.EntityManager.RemoveEnemy((Enemy)Owner);
        }
        #endregion

        #region Properties
        public override bool Collidable
        {
            get
            {
                return false;
            }
        }
        #endregion
    }
    class FastRhombDead : EnemyDead
    {
        #region Constructor
        public FastRhombDead(GeomCloneLevel level,FastRhomb enemy,Vector2 bulletVelocity)
            : base(level, enemy, bulletVelocity)
        {
            CurrentLevel.EntityManager.AddEnemies(EnemyGeneratorHelper.Spawn3SmallRhombs(level, enemy.Position));
        }
        #endregion
    }
    class RhombBorn : BornState
    {
        #region Constructor
        public RhombBorn(GeomCloneLevel level, Rhomb owner) : base(level, owner) { }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            timeOutCount += gameTime.ElapsedGameTime.Milliseconds;
            if (timeOutCount > 1500)
                Owner.ChangeState(new NormalState(CurrentLevel, Owner, new RhombSqueeze(CurrentLevel, (Rhomb)Owner)));
        }
        #endregion
    }
    class PlayerBorn : BornState
    {
        #region Constructor
        public PlayerBorn(GeomCloneLevel level, Actor owner) : base(level, owner) { }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            Owner.UpdatePosition(gameTime);
            base.Update(gameTime);
        }
        #endregion
    }
    class FastRhombBorn : BornState
    {
        #region Constructor
        public FastRhombBorn(GeomCloneLevel level, FastRhomb owner) : base(level, owner) { }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            timeOutCount += gameTime.ElapsedGameTime.Milliseconds;
            if (timeOutCount > 1500)
                Owner.ChangeState(new NormalState(CurrentLevel, Owner, new Spin(CurrentLevel, Owner, 600)));
        }
        #endregion
    }
    class PlayerDead : ActorState
    {
        #region Constructor
        public PlayerDead(GeomCloneLevel level, Player player)
            : base(level, player, null)
        {
            // Activate proper animation
            Owner.ChangeAnimation(new FadeOutDead(level, Owner));

        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime) { }
        #endregion

        #region Properties
        public override bool Collidable
        {
            get
            {
                return false;
            }
        }
        #endregion
    }
    #endregion

}
