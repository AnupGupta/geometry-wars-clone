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

#region Using statements
using System.Collections.Generic;
using GameEngine;
using GameEngine.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace GeomClone.Actors
{
    #region ActorPart class
    public class ActorPart : GameObject2D
    {
        #region Members
        Vector2 displacement;
        float rotationOffset;
        #endregion

        #region Constructor
        public ActorPart(GeomCloneGame theGame, string theTexture, Vector2 thePosition, float theRotation, Vector2 setDisplacement,
            float setRotationOffset, float scaleX, float scaleY, Color color)
            : base(theGame, theTexture, thePosition, theRotation, scaleX, scaleY, color)
        {
            displacement = setDisplacement;
            rotationOffset = setRotationOffset;
            UpdatePositionAndRotation(thePosition, theRotation);
            Color = new Color(color, (byte)160);
        }
        #endregion

        #region Update position and rotation
        public void UpdatePositionAndRotation(Vector2 pos, float rot)
        {
            Vector3 temp = Vector3.Transform(new Vector3(displacement.X * referenceWidth, displacement.Y * referenceHeight, 0.0f),
                Matrix.CreateRotationZ(MathHelper.Pi / 180.0f * rot));
            Vector2 tempPosition = new Vector2(temp.X / referenceWidth, temp.Y / referenceHeight);
            Position = pos + tempPosition;
            Rotation = rot + rotationOffset;
        }
        #endregion

        #region Properties
        public Color Color { get { return TheColor; } set { TheColor = value; } }
        public Vector2 Displacement { get { return displacement; } set { displacement = value; } }
        public float RotationOffset { get { return rotationOffset; } set { rotationOffset = value; } }
        internal new float ScaleX { get { return base.ScaleX; } set { base.ScaleX = value; } }
        #endregion
    }
    #endregion

    public abstract class Actor : GameObject2DAbstract
    {
        #region Member variables
        protected static readonly float gameSpeed = 0.9f;
        protected Vehicle v;
        protected List<ActorPart> parts;
        protected Vector2 position, velocity;
        protected float rotation;
        ActorState currentState;
        float bRadius;
        Animation currentAnimation;
        //ulong ID;
        //static ulong nextID = 1;
        GeomCloneLevel currentLevel;
        #endregion

        #region Properties
        public Vector2 Position { get { return position; } }        
        public Vector2 Velocity { get { return velocity; } }
        internal float Rotation { get { return rotation; } set { rotation = value; } }
        public SteeringBehaviorsManager Behavior { get { return v.Behavior; } }
        public float Speed { get { return velocity.Length(); } }
        internal List<ActorPart> Parts { get { return parts; } }
        internal Vehicle Vehicle { get { return v; } }
        public bool Collidable { get { return currentState.Collidable; } }
        protected GeomCloneLevel CurrentLevel { get { return currentLevel; } }
        protected GeomCloneGame Game { get { return currentLevel.Game; } }
        public float BRadius { get { return bRadius; } }
        //public ulong GetID { get { return ID; } }
        #endregion

        #region Constructor
        public Actor(GeomCloneLevel setCurrentLevel, Vector2 setPosition, Vector2 setVelocity, float setMass, float setMaxSpeed,
            float setMaxForce, float setMaxTurnRate, float setBRadius)
            : base(setCurrentLevel.Game)
        {
            currentLevel = setCurrentLevel;
            //ID = nextID++;
            position = setPosition;
            bRadius = setBRadius;
            velocity = setVelocity * gameSpeed;
            rotation = 0.0f;
            parts = new List<ActorPart>();
            v = new Vehicle(new Vector2(position.X * referenceWidth, position.Y * referenceHeight), velocity, setMass, setMaxSpeed * gameSpeed, setMaxForce, setMaxTurnRate, CurrentLevel.GetNeighbors, GameObject2D.referenceWidth, GameObject2D.referenceHeight);
            //CheckPositionBoundaries();
            ChangeState(new NormalState(CurrentLevel, this));
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            currentAnimation.Update(gameTime);
            currentState.Update(gameTime);            
            base.Update(gameTime);
        }
        #endregion

        #region Draw
        public override void Draw(GameTime gameTime)
        {
            currentAnimation.Draw(gameTime);
        }
        #endregion

        #region Position logic
        public virtual void UpdatePosition(GameTime gameTime)
        {
            CheckPositionBoundaries();
            foreach (ActorPart o in parts)
            {   
                o.UpdatePositionAndRotation(position, rotation + 90);
                o.Update(gameTime);
            }
            //CheckPositionBoundaries();
        }

        protected virtual void CheckPositionBoundaries()
        {
            bool changed = false;
            if (position.X > 0.99f)
            {
                position.X = 0.99f;
                changed = true;
            }
            else if (position.X < 0.01f)
            {
                position.X = 0.01f;
                changed = true;
            }
            if (position.Y > 0.987f)
            {
                position.Y = 0.987f;
                changed = true;
            }
            else if (position.Y < 0.013f)
            {
                position.Y = 0.013f;
                changed = true;
            }
            if(changed)
                Vehicle.Position = new Vector2(position.X * (float)referenceWidth, position.Y * (float)referenceHeight); ;
        }
        #endregion

        #region Collisions
        public virtual bool CollidesWith(Actor a)
        {
            if (Collidable && a.Collidable)
                if (Vector2.DistanceSquared(Position, a.Position) <= (BRadius + a.BRadius) * (BRadius + a.BRadius))
                    foreach (ActorPart myPart in parts)
                    {
                        foreach (ActorPart otherPart in a.Parts)
                            if (myPart.collidesWith(otherPart))
                                return true;
                    }
            return false;
        }
        #endregion

        #region State logic
        internal void ChangeState(ActorState newState)
        {
            currentState = newState;
        }
        internal void ChangeAnimation(Animation newAnimation)
        {
            currentAnimation = newAnimation;
        }
        #endregion

        #region Behavior
        internal void ResetBehavior()
        {
            this.Behavior.ResetBehavior();
        }
        #endregion
    }

    public class Bullet : Actor
    {
        #region Constructor
        private Bullet(GeomCloneLevel currentLevel, Vector2 position, Vector2 direction, float maxSpeed)
            : base(currentLevel, position, direction * maxSpeed, 0.01f, maxSpeed, 0, 0, 0.005f)
        {
            rotation = (float)(180.0 / Math.PI * Math.Acos((double)direction.X) * Math.Sign((double)direction.Y)) + 90.0f;

            parts.Add(new ActorPart(Game, "smallRect", position, rotation, Vector2.Zero, 90.0f, 0.15f, 0.5f, Color.White));
            parts.Add(new ActorPart(Game, "smallRect", position, rotation, new Vector2(-(float)6 / referenceWidth, (float)1 / referenceHeight), 10.0f, 0.28f, 0.5f, Color.White));
            parts.Add(new ActorPart(Game, "smallRect", position, rotation, new Vector2(-(float)6 / referenceWidth, -(float)1 / referenceHeight), -10.0f, 0.28f, 0.5f, Color.White));
        }
        #endregion

        #region Update position
        public override void UpdatePosition(GameTime gameTime)
        {
            base.UpdatePosition(gameTime);
            float moveFactorPerSecond = (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
            v.Update(gameTime);
            position = new Vector2(v.Position.X / (float)referenceWidth, v.Position.Y / (float)referenceHeight);
        }
        protected override void CheckPositionBoundaries() { }
        public bool IsOutOfBoundary()
        {
            return Position.X > 1.001 || Position.X < -0.001 || Position.Y > 1.001 || Position.Y < -0.001;
        }
        #endregion

        #region Generator function
        public static List<Bullet> SpawnBulletCoupleOld(GeomCloneLevel currentLevel, Vector2 position, Vector2 direction)
        {
            Vector2 direction1 = Vector2.Transform(direction, Matrix.CreateRotationZ((float)(Math.PI / 180.0 * 1.0)));
            Vector2 direction2 = Vector2.Transform(direction, Matrix.CreateRotationZ((float)(-Math.PI / 180.0 * 1.0)));

            List<Bullet> list = new List<Bullet>(2);
            list.Add(new Bullet(currentLevel, position, direction1, 700));
            list.Add(new Bullet(currentLevel, position, direction2, 700));
            return list;
        }
        public static List<Bullet> SpawnBulletCouple(GeomCloneLevel currentLevel, Vector2 position, Vector2 direction)
        {
            Vector2 perp = VectorHelper.Perpendicular(direction);
            perp.Normalize();
            Vector2 pos1 = new Vector2(position.X * referenceWidth, position.Y * referenceHeight);
            Vector2 pos2 = pos1 - 4 * perp;
            pos1 += 4 * perp;
            pos1.X *= (float)1 / referenceWidth;
            pos1.Y *= (float)1 / referenceHeight;
            pos2.X *= (float)1 / referenceWidth;
            pos2.Y *= (float)1 / referenceHeight;

            List<Bullet> list = new List<Bullet>(2);
            list.Add(new Bullet(currentLevel, pos1, direction, 700));
            list.Add(new Bullet(currentLevel, pos2, direction, 700));
            return list;
        }
        public static List<Bullet> Spawn5Bullets(GeomCloneLevel currentLevel, Vector2 position, Vector2 direction)
        {
            Vector2 direction1 = Vector2.Transform(direction, Matrix.CreateRotationZ((float)(Math.PI / 180.0 * 1.1)));
            Vector2 direction2 = Vector2.Transform(direction, Matrix.CreateRotationZ((float)(-Math.PI / 180.0 * 1.1)));
            Vector2 direction3 = Vector2.Transform(direction, Matrix.CreateRotationZ((float)(Math.PI / 180.0 * 2.8)));
            Vector2 direction4 = Vector2.Transform(direction, Matrix.CreateRotationZ((float)(-Math.PI / 180.0 * 2.8)));
            
            Vector2 perp = VectorHelper.Perpendicular(direction);
            perp.Normalize();
            
            Vector2 transformedPosition = new Vector2(position.X * referenceWidth, position.Y * referenceHeight);

            Vector2 pos0 = transformedPosition + 5 * direction;
            Vector2 pos1 = transformedPosition + 5 * perp + 3 * direction;
            Vector2 pos2 = transformedPosition - 5 * perp + 3 * direction;
            Vector2 pos3 = transformedPosition + 6 * perp;
            Vector2 pos4 = transformedPosition - 6 * perp;

            Vector2 scale = new Vector2((float)1.0f / referenceWidth, (float)1.0f / referenceHeight);

            pos0 *= scale;
            pos1 *= scale;
            pos2 *= scale;
            pos3 *= scale;
            pos4 *= scale;

            List<Bullet> list = new List<Bullet>(2);
            list.Add(new Bullet(currentLevel, pos0, direction, 650));
            list.Add(new Bullet(currentLevel, pos1, direction1, 650));
            list.Add(new Bullet(currentLevel, pos2, direction2, 650));
            list.Add(new Bullet(currentLevel, pos3, direction3, 650));
            list.Add(new Bullet(currentLevel, pos4, direction4, 650));
            return list;
        }
        #endregion
    }

    public class Bomb : Actor
    {
        #region Constructor
        public Bomb(GeomCloneLevel currLev, Vector2 pos)
            : base(currLev, pos, new Vector2(), 1.0f, 0, 0, 0, 0.005f)
        {
        }
        #endregion

        #region Members
        float counter = 0.0f;
        bool toDispose = false;
        float radius = 0.0f;
        public bool ToDispose { get { return toDispose; } }
        #endregion

        #region Update
        public override void Update(GameTime time)
        {
            counter += (float)time.ElapsedGameTime.Milliseconds;
            if (counter < 4000)
            {
                // 300 pixel per second
                radius = counter * 0.001f * 700.0f * gameSpeed;
            }
            else
                toDispose = true;
            base.Update(time);
        }
        #endregion

        #region Collision
        public override bool CollidesWith(Actor a)
        {
            Vector2 scale = new Vector2(referenceWidth,referenceHeight);
            float distSquare = Vector2.DistanceSquared(a.Position * scale, Position * scale);
            // approximate square formula for (radius-20)*(radius-20)
            return distSquare < radius * radius && distSquare > radius * radius - 2.0f * radius * 20.0f;
        }
        #endregion
    }
}
