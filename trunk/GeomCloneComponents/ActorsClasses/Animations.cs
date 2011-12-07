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
using GameEngine;
using Microsoft.Xna.Framework.Graphics;
using GameEngine.Helpers;
#endregion

namespace GeomClone.Actors
{
    #region Animation FSM
    public abstract class Animation
    {
        #region Constructor
        public Animation(GeomCloneLevel setCurrentLevel, Actor setOwner)
        {
            owner = setOwner;
            currentLevel = setCurrentLevel;
        }
        #endregion

        #region Members
        Actor owner;
        GeomCloneLevel currentLevel;
        #endregion

        #region Properties
        protected Actor Owner { get { return owner; } }
        protected GeomCloneLevel CurrentLevel { get { return currentLevel; } }
        #endregion

        #region Draw
        public virtual void Draw(GameTime time) { }
        #endregion

        #region Update
        public virtual void Update(GameTime time) { }
        #endregion
    }
    class NoAnimation : Animation
    {
        #region Constructor
        public NoAnimation(GeomCloneLevel level, Actor owner) : base(level, owner) { }
        #endregion

        #region Draw
        public override void Draw(GameTime time)
        {
            foreach (ActorPart o in Owner.Parts)
                o.Draw(time);
            base.Draw(time);
        }
        #endregion
    }
    class Flash : Animation
    {
        #region Constructor
        public Flash(GeomCloneLevel level, Actor owner, float setTimeInterval) : base(level, owner) { timeInterval = setTimeInterval; }
        #endregion

        #region Members
        float timeOutCount = 0;
        float timeInterval;
        #endregion

        #region Draw
        public override void Draw(GameTime time)
        {
            timeOutCount += time.ElapsedGameTime.Milliseconds;
            if (timeOutCount % timeInterval >= 0 && timeOutCount % timeInterval < (timeInterval - 100))
            {
                foreach (ActorPart o in Owner.Parts)
                    o.Draw(time);
            }

            base.Draw(time);
        }
        #endregion
    }
    class RhombSqueeze : NoAnimation
    {
        #region Constructor
        public RhombSqueeze(GeomCloneLevel level, Rhomb owner) : base(level, owner) { }
        #endregion

        #region Members
        float angle = 45;
        bool switchOscillationDirection = true;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            float elapsedTime = gameTime.ElapsedGameTime.Milliseconds;

            if (Math.Abs(angle) <= 38)
                switchOscillationDirection = true;
            else if(Math.Abs(angle) >= 52)
                switchOscillationDirection = false;

            if(switchOscillationDirection)
                angle += Math.Sign(angle) * elapsedTime / 100;
            else
                angle -= Math.Sign(angle) * elapsedTime / 100;

            foreach (ActorPart p in Owner.Parts)
            {
                p.RotationOffset = Math.Sign(p.RotationOffset) * angle;
            }
            base.Update(gameTime);
        }
        #endregion
    }
    class FadeOutDead : NoAnimation
    {
        #region Constructor
        public FadeOutDead(GeomCloneLevel level, Actor owner) : base(level, owner) { }
        #endregion

        #region Members
        float alphaValue = 150;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            alphaValue -= 0.2f * (float)gameTime.ElapsedGameTime.Milliseconds;
            foreach (ActorPart p in Owner.Parts)
            {
                p.Color = new Color(p.Color, (byte)MathHelper.Clamp(alphaValue, 0, 255));
                p.Displacement += 0.004f * gameTime.ElapsedGameTime.Milliseconds * (p.Displacement + 0.05f * RandomHelper.getRandomVector());
                p.ScaleX *= (float)(1 - 0.004 * gameTime.ElapsedGameTime.Milliseconds);
            }
            base.Update(gameTime);
        }
        #endregion
    }
    class Spin : NoAnimation
    {
        #region Constructor
        public Spin(GeomCloneLevel game, Actor owner, float setDegreesPerSecond) : base(game, owner) { degreesPerMilliSecond = setDegreesPerSecond / 1000; }
        #endregion

        #region Members
        float degreesPerMilliSecond;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            Owner.Rotation += degreesPerMilliSecond * (float)gameTime.ElapsedGameTime.Milliseconds;
            base.Update(gameTime);
        }
        #endregion
    }
    #endregion
}
