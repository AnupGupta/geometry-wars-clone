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

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using GeomClone.Actors;

namespace GeomClone
{
    public enum EnemyType { Rhomb, FastRhomb, SmartRhomb }
    public abstract class EnemyGenerator : GameComponent
    {
        #region Constructor
        public EnemyGenerator(GeomCloneLevel setLevel, float setTimeOutTime, float setWaitingTime)
            : base(setLevel.Game)
        {
            timeOutTime = setTimeOutTime;
            waitingTime = setWaitingTime;
            
            currentLevel = setLevel;
            entityManager = currentLevel.EntityManager;
        }
        #endregion

        #region Members
        protected GeomCloneLevel currentLevel;
        protected float timerCount = 0;
        float waitingCount = 0;
        float timeOutTime;
        float waitingTime;
        EntityManager entityManager;
        bool active = true;
        #endregion

        #region Properties
        public bool Active { get { return active; } set { active = value; } }
        public new GeomCloneGame Game { get { return Game; } }
        #endregion

        #region Update
        public override void Update(GameTime time)
        {
            if (waitingCount < waitingTime)
                waitingCount += time.ElapsedGameTime.Milliseconds;
            else
                if (active)
                {
                    timerCount += time.ElapsedGameTime.Milliseconds;
                    if (timerCount > timeOutTime)
                    {
                        entityManager.AddEnemies(GenerateEnemies());
                        timerCount = 0;
                    }
                }
        }
        #endregion

        #region Abstract methods
        protected abstract List<Enemy> GenerateEnemies();
        #endregion
    }
    public class RhombGenerator : EnemyGenerator
    {
        #region Constructor
        public RhombGenerator(GeomCloneLevel level) : base(level, 1500, 0) { }
        #endregion

        #region Members
        int rhombsPerFlock = 4;
        #endregion

        #region Methods
        protected override List<Enemy> GenerateEnemies()
        {
            return EnemyGeneratorHelper.SpawnRhombFlock(rhombsPerFlock, currentLevel, currentLevel.Player);
        }
        #endregion

        #region Properties
        public int RhombsPerFlock { set { rhombsPerFlock = value; } }
        #endregion
    }
    public class CornerGenerator : EnemyGenerator
    {
        #region Constructor
        public CornerGenerator(GeomCloneLevel level,EnemyType setType, int setNumberOfEnemiesPerCorner)
            : base(level, 200, 1000)
        {
            numberOfEnemies = setNumberOfEnemiesPerCorner;
            type = setType;
        }
        #endregion

        #region Members
        int numberOfEnemies;
        int count = 0;
        EnemyType type;
        #endregion

        #region Methods
        protected override List<Enemy> GenerateEnemies()
        {
            List<Enemy> list = new List<Enemy>(4);
            if (++count <= numberOfEnemies)
            {
                list.Add(GenerateEnemy(new Vector2(0, 0)));
                list.Add(GenerateEnemy(new Vector2(0, 1)));
                list.Add(GenerateEnemy(new Vector2(1, 1)));
                list.Add(GenerateEnemy(new Vector2(1, 0)));
                foreach (Enemy e in list)
                    e.Activate();
            }
            else { Active = false; }
            return list;
        }
        private Enemy GenerateEnemy(Vector2 position)
        {
            switch (type)
            {
                case EnemyType.FastRhomb:
                    return new FastRhomb(currentLevel, currentLevel.Player, position);
                case EnemyType.SmartRhomb:
                    return new SmartRhomb(currentLevel, position, currentLevel.Player);
                default:
                    return new Rhomb(currentLevel, currentLevel.Player, position);
            }
        }
        #endregion

    }
    public class RandomEnemyGenerator : EnemyGenerator
    {
        #region Constructor
        public RandomEnemyGenerator(GeomCloneLevel level) : base(level, 200, 1000) { }
        #endregion

        #region Methods
        protected override List<Enemy> GenerateEnemies()
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
    
}
