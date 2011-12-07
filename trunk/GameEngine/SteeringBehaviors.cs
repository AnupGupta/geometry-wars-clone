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

namespace GameEngine
{
    public abstract class MovingEntity
    {
        #region Constructor
        public MovingEntity(Vector2 setPosition, Vector2 setVelocity, float setMass, float setMaxSpeed, float setMaxForce, float setMaxTurnRate)
        {   
            position = setPosition;
            velocity = setVelocity;
            heading = Vector2.Zero;
            side = Vector2.Zero;
            mass = setMass;
            maxSpeed = setMaxSpeed;
            maxForce = setMaxForce;
            maxTurnRate = setMaxTurnRate;
        }
        #endregion

        #region Members
        protected Vector2 heading, side, velocity, position;
        protected float mass, maxSpeed, maxForce, maxTurnRate;
        #endregion

        #region Properties
        public Vector2 Position { get { return position; } set { position = value; } }
        public Vector2 Velocity { get { return velocity; } set { velocity = value; } }
        public float Mass { get { return mass; } }
        public Vector2 Side { get { return side; } }
        public float MaxSpeed { get { return maxSpeed; } set { maxSpeed = value; } }
        public float MaxForce { get { return maxForce; } set { maxForce = value; } }
        public float Speed { get { return velocity.Length(); } }
        public float SpeedSquare { get { return velocity.LengthSquared(); } }
        public Vector2 Heading { get { return heading; } set { heading = value; side = VectorHelper.Perpendicular(heading); } }
        public float MaxTurnRate { get { return maxTurnRate; } set { maxTurnRate = value; } }
        #endregion

        #region Functions
        public bool IsSpeedMaxedOut() { return maxSpeed * maxSpeed >= velocity.LengthSquared(); }
        public bool RotateHeadingToFacePosition(Vector2 targetPosition)
        {
            Vector2 toTarget = Vector2.Normalize(targetPosition - position);
            float angle = (float)Math.Acos(Vector2.Dot(heading, toTarget));
            if (angle < 0.00001) return true;
            if (angle > maxTurnRate) angle = maxTurnRate;
            Matrix m = Matrix.CreateRotationZ(angle * VectorHelper.Sign(heading, toTarget));
            Vector2.Transform(heading, m);
            Vector2.Transform(velocity, m);
            side = VectorHelper.Perpendicular(heading);
            return false;
        }
        #endregion

        #region Update
        public virtual void Update(GameTime time) { }
        #endregion
    }
    public class Vehicle : MovingEntity
    {
        #region Constructor
        public Vehicle(Vector2 setPosition, Vector2 setVelocity, float setMass, float setMaxSpeed, float setMaxForce, float setMaxTurnRate, GetVehicles getVehiclesFunction, int width, int height)
            : base(setPosition, setVelocity, setMass, setMaxSpeed, setMaxForce, setMaxTurnRate)
        {
            steering = new SteeringBehaviorsManager(this, getVehiclesFunction, width, height);
        }
        #endregion

        #region Member variables
        SteeringBehaviorsManager steering;
        float elapsedTime;
        #endregion        

        #region Properties
        public SteeringBehaviorsManager Behavior { get { return steering; } }        
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            elapsedTime = (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

            steering.Update(gameTime);

            Vector2 steeringForce = steering.Calculate(gameTime);
            Vector2 acceleration = steeringForce / mass;
            velocity += acceleration * elapsedTime;

            if (velocity.LengthSquared() > maxSpeed * maxSpeed)
            {                
                velocity.Normalize();
                velocity *= maxSpeed;
            }

            position += velocity * elapsedTime;

            if (velocity.LengthSquared() > 0.0000001)
            {
                Heading = Vector2.Normalize(velocity);                
            }

            base.Update(gameTime);
        }
        #endregion

    }
    enum BehaviorType
    {
        none = 100,
        wallAvoidance = 1,
        obstacleAvoidance = 2,
        evade = 3,
        evadeMulti = 3,
        flee = 4,
        separation = 5,
        allignment = 6,
        cohesion = 7,
        seek = 8,
        arrive = 9,
        wander = 10,
        pursuit = 11,
        offsetPursuit = 12,
        interpose = 13,
        hide = 14,
        followPath = 15,
        flock = 16
    }
    enum Deceleration { Slow = 3, Normal = 2, Fast = 1 }
    abstract class SteeringBehavior
    {
        #region Constructor
        public SteeringBehavior(SteeringBehaviorsManager manager, Vehicle theOwner) { owner = theOwner; steeringBehaviorsManager = manager; }
        #endregion

        #region Members
        Vehicle owner;
        SteeringBehaviorsManager steeringBehaviorsManager;
        #endregion

        #region Behavior functions
        public abstract Vector2 Calculate(GameTime time);
        #endregion

        #region Properties
        internal Vehicle Owner { get { return owner; } }
        internal SteeringBehaviorsManager SteeringBehaviorsManager { get { return steeringBehaviorsManager; } }
        #endregion

        #region Update
        public virtual void Update(GameTime time) { }
        #endregion
    }
    class Seek : SteeringBehavior
    {
        #region Constructor
        public Seek(SteeringBehaviorsManager manager, Vehicle theOwner, Vector2 targetPos) : base(manager, theOwner) { targetPosition = targetPos; }
        #endregion

        #region Members
        Vector2 targetPosition;
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            Vector2 DesiredVelocity = Vector2.Normalize(targetPosition - Owner.Position) * Owner.MaxSpeed;

            return (DesiredVelocity - Owner.Velocity);

        }
        #endregion

        #region Properties
        public Vector2 TargetPosition { set { targetPosition = value; } }
        #endregion
    }
    class Flee : SteeringBehavior
    {
        #region Constructor
        public Flee(SteeringBehaviorsManager manager, Vehicle theOwner, Vector2 targetPos) : base(manager,theOwner) { targetPosition = targetPos; }
        #endregion

        #region Members
        Vector2 targetPosition;
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            Vector2 DesiredVelocity = Vector2.Normalize(Owner.Position - targetPosition) * Owner.MaxSpeed;
            return (DesiredVelocity - Owner.Velocity);
        }
        #endregion

        #region Properties
        public Vector2 TargetPosition { set { targetPosition = value; } }
        #endregion
    }
    class Arrive : SteeringBehavior
    {
        #region Constructor
        public Arrive(SteeringBehaviorsManager manager, Vehicle theOwner, Vector2 targetPos, Deceleration decel)
            : base(manager,theOwner)
        { targetPosition = targetPos; deceleration = decel; }
        #endregion

        #region Members
        Vector2 targetPosition;
        Deceleration deceleration;
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            Vector2 ToTarget = targetPosition - Owner.Position;
            float dist = ToTarget.Length();
            if (dist > 0)
            {
                const float DecelerationTweaker = 0.3f;
                float speed = dist / ((float)deceleration * DecelerationTweaker);
                speed = System.Math.Min(speed, Owner.MaxSpeed);
                Vector2 DesiredVelocity = ToTarget * speed / dist;
                return (DesiredVelocity - Owner.Velocity);
            }
            return new Vector2();
        }
        #endregion
    }
    class Pursuit : SteeringBehavior 
    {
        #region Constructor
        public Pursuit(SteeringBehaviorsManager manager, Vehicle theOwner, Vehicle theEvader)
            : base(manager,theOwner)
        { evader = theEvader; seek = new Seek(manager, theOwner, new Vector2()); }
        #endregion

        #region Members
        Vehicle evader;
        Seek seek;
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            Vector2 ToEvader = evader.Position - Owner.Position;
            //float relativeHeading = Vector2.Dot(Owner.Heading, evader.Heading);
            //if ((Vector2.Dot(ToEvader, Owner.Heading) > 0 && relativeHeading < -0.95f))
            //{
            //    seek.TargetPosition = evader.Position;
            //    return seek.Calculate(time);
            //}
            float lookAheadTime = ToEvader.Length() / (Owner.MaxSpeed + evader.Speed);
            seek.TargetPosition = evader.Position + evader.Velocity * lookAheadTime;
            return seek.Calculate(time);
        }
        #endregion
    }
    class Evade : SteeringBehavior
    {
        #region Constructor
        public Evade(SteeringBehaviorsManager manager, Vehicle theOwner, Vehicle thePursuer, float setMinDistance)
            : base(manager,theOwner)
        { pursuer = thePursuer; flee = new Flee(manager, theOwner, new Vector2()); minDistance = setMinDistance; minDistanceSquare = minDistance * minDistance; }
        #endregion

        #region Members
        Vehicle pursuer;
        Flee flee;
        float minDistance;
        float minDistanceSquare;
        public Vehicle Pursuer { get { return pursuer; } set { pursuer = value; } }
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            Vector2 toPursuer = pursuer.Position - Owner.Position;

            if (toPursuer.LengthSquared() > minDistanceSquare) return new Vector2();

            float lookAheadTime = toPursuer.Length() / (Owner.MaxSpeed + pursuer.Speed);
            flee.TargetPosition = pursuer.Position + pursuer.Velocity * lookAheadTime;
            return flee.Calculate(time);
        }
        #endregion
    }
    class EvadeMulti : SteeringBehavior
    {
        #region Constructor
        public EvadeMulti(SteeringBehaviorsManager manager, Vehicle setOwner, float setMinDistance, GetVehicles updatePursuers, bool avoidBullets) 
            : base(manager, setOwner) {

            minDistance = setMinDistance;
            minDistanceSquare = minDistance * minDistance;

            this.avoidBullets = avoidBullets;

            //evade = new Evade(manager, setOwner, null, setMinDistance);

            UpdatePursuers = updatePursuers;

            flee = new Flee(manager, setOwner, new Vector2());
        }
        public EvadeMulti(SteeringBehaviorsManager manager, Vehicle setOwner, float setMinDistance, GetVehicles updatePursuers)
            : this(manager, setOwner, setMinDistance, updatePursuers, false) { }
        #endregion

        #region Members
        GetVehicles UpdatePursuers;
        Flee flee;
        float minDistance;
        float minDistanceSquare;
        bool avoidBullets;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            Vector2 force = new Vector2();
            foreach (Vehicle v in UpdatePursuers())
            {
                if (Vector2.Dot(v.Velocity, Owner.Velocity) / (v.Velocity.LengthSquared() * Owner.Velocity.LengthSquared()) >= 0)
                    continue;
                Vector2 toPursuer = v.Position - Owner.Position;

                if (toPursuer.LengthSquared() < minDistanceSquare)
                {
                    float lookAheadTime = toPursuer.Length() / (Owner.MaxSpeed + v.Speed);
                    flee.TargetPosition = v.Position + v.Velocity * lookAheadTime;

                    force += Owner.MaxForce * (flee.Calculate(time) / Vector2.Distance(Owner.Position, v.Position));
                }
            }
            return force;
        }
        #endregion
    }
    class Wander : SteeringBehavior
    {
        #region Constructor
        public Wander(SteeringBehaviorsManager manager, Vehicle theOwner, float setWandJitter, float setWandRadius, float setWandDist)
            : base(manager,theOwner) 
        {
            wanderJitter = setWandJitter;
            wanderRadius = setWandRadius;
            wanderDistance = setWandDist;
            wanderTarget = RandomHelper.getRandomVector() * wanderRadius;
        }
        #endregion

        #region Members
        Vector2 wanderTarget;
        float wanderJitter, wanderRadius, wanderDistance;
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            float jitterThisTimeSlice = wanderJitter * (float)time.ElapsedGameTime.Milliseconds;

            wanderTarget += RandomHelper.getRandomVector() * jitterThisTimeSlice;

            wanderTarget.Normalize();

            wanderTarget *= wanderRadius;

            Vector2 targetLocal = wanderTarget + new Vector2(wanderDistance, 0);

            Vector2 target = VectorHelper.PointToWorldSpace(targetLocal, Owner.Heading, Owner.Side, Owner.Position);

            return target - Owner.Position;
        }
        #endregion
    }
    class Separation : SteeringBehavior
    {
        #region Constructor
        public Separation(SteeringBehaviorsManager manager, Vehicle theOwner) : base(manager,theOwner) { }
        #endregion

        #region Members
        List<Vehicle> neighbors;
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            neighbors = SteeringBehaviorsManager.GetNeighbors(Owner);
        }
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            Vector2 force=new Vector2();
            foreach (Vehicle v in neighbors)
            {
                if (v != Owner)
                {
                    Vector2 toAgent = Owner.Position - v.Position;
                    force += Vector2.Normalize(toAgent) / toAgent.Length() * Owner.MaxSpeed;
                }
            }
            return force;
        }
        #endregion
    }
    public class Wall
    {
        #region Constructor
        public Wall(Vector2 setFrom, Vector2 setTo, Vector2 setNormal) { from = setFrom; to = setTo; normal = setNormal; }
        #endregion

        #region Members
        Vector2 from, to, normal;
        #endregion

        #region Properties
        public Vector2 From { get { return from; } }
        public Vector2 To { get { return to; } }
        public Vector2 Normal { get { return normal; } }
        #endregion
    }
    class WallAvoidance : SteeringBehavior
    {
        #region Constructor
        public WallAvoidance(SteeringBehaviorsManager manager, Vehicle owner, float setFeelersLength, List<Wall> setWalls)
            : base(manager, owner)
        {
            feelers = new List<Vector2>(3);
            feelersLength = setFeelersLength;
            walls = setWalls;
        }
        #endregion

        #region Members
        List<Vector2> feelers;
        float feelersLength;
        List<Wall> walls;
        #endregion

        #region Behavior functions
        public override Vector2 Calculate(GameTime time)
        {
            CreateFeelers();

            float distToThisIP = 0;
            float distToCLosestIP = float.MaxValue;
            Wall closestWall = null;

            Vector2 steeringForce, point, closestPoint;
            steeringForce = Vector2.Zero;
            closestPoint = Vector2.Zero;

            foreach (Vector2 f in feelers)
            {
                foreach (Wall w in walls)
                {
                    if (VectorHelper.LineIntersection2D(Owner.Position, f, w.From, w.To, out distToThisIP, out point))
                    {
                        if (distToThisIP < distToCLosestIP) {
                            closestWall = w;
                            closestPoint = point;
                        }
                    }
                }

                if (closestWall != null)
                {
                    Vector2 overShoot = f - closestPoint;
                    steeringForce = closestWall.Normal * overShoot.Length();
                }
            }

            return steeringForce;
        }
        private void CreateFeelers()
        {
            feelers.Clear();
            feelers.Add(feelersLength * Owner.Heading);
            feelers.Add(feelersLength * VectorHelper.Rotate(Owner.Heading, 45.0f));
            feelers.Add(feelersLength * VectorHelper.Rotate(Owner.Heading, -45.0f));
        }
        #endregion
    }
    public sealed class SteeringBehaviorsManager
    {
        #region Embedded region struct
        struct Region
        {
            Rectangle rectangle;
            public Region(Rectangle rect) { rectangle = rect; }
            public bool BelongsToRegion(Vector2 vec)
            {
                return rectangle.Contains(new Point((int)vec.X, (int)vec.Y));
            }
        }
        #endregion

        #region Members
        Vehicle owner;
        Vector2 steeringForce;
        SortedDictionary<int, SteeringBehavior> activeBehaviors;
        GetVehicles getVehicles;
        int Width;
        int Height;
        static Dictionary<int, Region> regions = null;
        static Dictionary<int, List<Vehicle>> vehicles = null;
        #endregion

        #region Constructor
        public SteeringBehaviorsManager(Vehicle theVehicle, GetVehicles getVehiclesFunction, int width, int height)
        {
            owner = theVehicle;
            activeBehaviors = new SortedDictionary<int, SteeringBehavior>();
            // Function that returns all the active vehicles. Must be user-defined for flexibility.
            getVehicles = getVehiclesFunction;
            Width = width;
            Height = height;
        }
        #endregion

        #region Update
        public void Update(GameTime gameTime)
        {
            foreach (SteeringBehavior b in activeBehaviors.Values)
                b.Update(gameTime);
        }
        #endregion

        #region Calculate force
        public Vector2 Calculate(GameTime time)
        {
            steeringForce *= 0.0f;

            foreach (KeyValuePair<int, SteeringBehavior> kvp in activeBehaviors)
            {
                Vector2 force = kvp.Value.Calculate(time);
                if (!AccumulateForce(ref steeringForce, force)) return steeringForce;
            }

            return steeringForce;
        }
        bool AccumulateForce(ref Vector2 RunningTot, Vector2 ForceToAdd)
        {

            //calculate how much steering force the vehicle has used so far
            float MagnitudeSoFar = RunningTot.Length();

            //calculate how much steering force remains to be used by this vehicle
            float MagnitudeRemaining = owner.MaxForce - MagnitudeSoFar;

            //return false if there is no more force left to use
            if (MagnitudeRemaining <= 0.0) return false;

            //calculate the magnitude of the force we want to add
            float MagnitudeToAdd = ForceToAdd.Length();

            //if the magnitude of the sum of ForceToAdd and the running total
            //does not exceed the maximum force available to this vehicle, just
            //add together. Otherwise add as much of the ForceToAdd vector is
            //possible without going over the max.
            if (MagnitudeToAdd < MagnitudeRemaining)
            {
                RunningTot += ForceToAdd;
            }

            else
            {
                //add it to the steering force
                ForceToAdd.Normalize();
                RunningTot += (ForceToAdd * MagnitudeRemaining);
            }

            return true;
        }
        #endregion

        #region Set behaviors
        public void FleeOn(Vector2 target) { activeBehaviors.Add((int)BehaviorType.flee, new Flee(this, owner, target)); }
        public void SeekOn(Vector2 target) { activeBehaviors.Add((int)BehaviorType.seek, new Seek(this, owner, target)); }
        public void ArriveOn(Vector2 target) { activeBehaviors.Add((int)BehaviorType.arrive, new Arrive(this, owner, target, Deceleration.Normal)); }
        public void WanderOn(float jitter, float radius, float distance) { activeBehaviors.Add((int)BehaviorType.wander, new Wander(this, owner, jitter, radius, distance)); }
        public void PursuitOn(Vehicle evader) { activeBehaviors.Add((int)BehaviorType.pursuit, new Pursuit(this, owner, evader)); }
        public void EvadeOn(Vehicle pursuer, float minDistance) { activeBehaviors.Add((int)BehaviorType.evade, new Evade(this, owner, pursuer, minDistance)); }
        public void EvadeMultiOn(GetVehicles getPursuer, float minDistance) { activeBehaviors.Add((int)BehaviorType.evadeMulti, new EvadeMulti(this, owner, minDistance, getPursuer)); }
        //public void CohesionOn() { activeBehaviors.Add((int)BehaviorType.cohesion, new Cohesion); }
        public void SeparationOn() { activeBehaviors.Add((int)BehaviorType.separation, new Separation(this, owner)); }
        //public void AlignmentOn() { activeBehaviors2.Add(BehaviorType.allignment, 0); }
        //public void ObstacleAvoidanceOn() { activeBehaviors2.Add(BehaviorType.obstacleAvoidance, 0); }
        //public void WallAvoidanceOn() { activeBehaviors2.Add(BehaviorType.wallAvoidance, 0); }
        //public void FollowPathOn() { activeBehaviors2.Add(BehaviorType.followPath, 0); }
        //public void InterposeOn(Vehicle v1, Vehicle v2) { activeBehaviors.Add((int)BehaviorType.interpose, new Interpose); }
        //public void HideOn(Vehicle v) { activeBehaviors.Add((int)BehaviorType.hide, new Hide); }
        //public void OffsetPursuitOn(Vehicle v1, Vector2 offset) { activeBehaviors.Add(BehaviorType.offsetPursuit, 0); this.offset = offset; target1 = v1; }
        //public void FlockingOn() { CohesionOn(); AlignmentOn(); SeparationOn(); WanderOn(); }

        public void FleeOff() { activeBehaviors.Remove((int)BehaviorType.flee); }
        public void SeekOff() { activeBehaviors.Remove((int)BehaviorType.seek); }
        public void ArriveOff() { activeBehaviors.Remove((int)BehaviorType.arrive); }
        public void WanderOff() { activeBehaviors.Remove((int)BehaviorType.wander); }
        public void PursuitOff() { activeBehaviors.Remove((int)BehaviorType.pursuit); }
        public void EvadeOff() { activeBehaviors.Remove((int)BehaviorType.evade); }
        public void EvadeMultiOff() { activeBehaviors.Remove((int)BehaviorType.evadeMulti); }
        public void CohesionOff() { activeBehaviors.Remove((int)BehaviorType.cohesion); }
        public void SeparationOff() { activeBehaviors.Remove((int)BehaviorType.separation); }
        public void AlignmentOff() { activeBehaviors.Remove((int)BehaviorType.allignment); }
        public void ObstacleAvoidanceOff() { activeBehaviors.Remove((int)BehaviorType.obstacleAvoidance); }
        public void WallAvoidanceOff() { activeBehaviors.Remove((int)BehaviorType.wallAvoidance); }
        public void FollowPathOff() { activeBehaviors.Remove((int)BehaviorType.followPath); }
        public void InterposeOff() { activeBehaviors.Remove((int)BehaviorType.interpose); }
        public void HideOff() { activeBehaviors.Remove((int)BehaviorType.hide); }
        public void OffsetPursuitOff() { activeBehaviors.Remove((int)BehaviorType.offsetPursuit); }
        public void FlockingOff() { CohesionOff(); AlignmentOff(); SeparationOff(); WanderOff(); }

        //bool IsBehaviorOn(BehaviorType b) { return activeBehaviors.ContainsKey((int)b); }
        
        internal void ResetBehavior()
        {
            FleeOff();
            SeekOff();
            ArriveOff();
            WanderOff();
            PursuitOff();
            EvadeOff();
            CohesionOff();
            SeparationOff();
            AlignmentOff();
            ObstacleAvoidanceOff();
            WallAvoidanceOff();
            FollowPathOff();
            InterposeOff();
            HideOff();
            OffsetPursuitOff();
            FlockingOff();
        }
        #endregion

        #region Functions
        public List<Vehicle> GetNeighbors(Vehicle v) { return getVehicles(); }
        Dictionary<int,Region> GetRegions() {
            if (regions == null)
            {
                // the field is split in a matrix
                int nRows = 4;
                int nColumns = 5;

                int deltaX = Width / nColumns;
                int deltaY = Height / nRows;

                // each region overlaps the other by a small amount defined by the offset
                int offset = 10;

                regions = new Dictionary<int,Region>();
                for(int i=0;i<nRows;i++)
                    for (int j = 0; j < nColumns; j++)
                    {
                        regions.Add(i * nRows + j, new Region(new Rectangle(i * deltaX - offset, j * deltaY - offset,
                            Width / nColumns + 2 * offset, Height / nRows + 2 * offset)));
                    }
            }

            return regions;
        }
        private void UpdateVehicles()
        {
            if (vehicles == null)
            {
                vehicles = new Dictionary<int, List<Vehicle>>();
                foreach (KeyValuePair<int, Region> kvp in GetRegions())
                    vehicles[kvp.Key] = new List<Vehicle>();
            }
            foreach (Vehicle v in getVehicles())
                foreach (KeyValuePair<int, Region> kvp in GetRegions())
                {
                    if (kvp.Value.BelongsToRegion(v.Position))
                    {
                        vehicles[kvp.Key].Add(v);
                        continue;
                    }
                }
        }
        #endregion
    }
    public delegate List<Vehicle> GetVehicles();
}
