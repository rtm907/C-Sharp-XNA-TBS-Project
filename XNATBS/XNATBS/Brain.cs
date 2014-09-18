using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNATBS
{
    public abstract class Brain
    {
        protected Unit _owner;
        public Unit MyCreature
        {
            get
            {
                return _owner;
            }
            set
            {
                _owner = value;
            }
        }

        protected List<Action> _actionsList = new List<Action>();

        // Current words the creature is saying.
        //protected ActionSaySomething _opinion;

        public virtual void Update()
        {

        }

        #region Memory

        protected List<Unit> _observedFriends = new List<Unit>();
        protected List<Unit> _observedEnemies = new List<Unit>();

        public void ObservedCreaturesAdd(Unit critter)
        {
            // don't add self to list.
            if (!critter.Equals(this._owner))
            {
                if (critter.Team == _owner.Team)
                {
                    _observedFriends.Add(critter);
                }
                else
                {
                    _observedEnemies.Add(critter);
                    _owner.Team.ObservedEnemyAdd(critter);
                }
            }
        }
        public void ObservedCreaturesRemove(Unit critter)
        {
            if (critter.Team == _owner.Team)
            {
                _observedFriends.Remove(critter);
            }
            else
            {
                _observedEnemies.Remove(critter);
                _owner.Team.ObservedEnemyRemove(critter);
            }
        }

        private List<Item> _observedItems = new List<Item>();
        public void ObservedItemsAdd(Item newItem)
        {
            this._observedItems.Add(newItem);
        }
        public void ObservedItemsRemove(Item oldItem)
        {
            this._observedItems.Remove(oldItem);
        }

        #endregion

        public Brain()
        {

        }

        public Brain(Creature owner)
        {
            _owner = owner;
        }
    }

    public class BrainPlayerControlled : Brain
    {
    }

    public class BrainDead : Brain
    {
        public override void Update()
        {
        }

        public BrainDead()
            : base()
        {
        }
    }

    public class BrainBasicAI : Brain
    {
        private DrawerBattle _drawer;

        /// <summary>
        /// Returns null if there is no enemy in attack range; reference to the enemy if oneis found.
        /// </summary>
        private Creature EnemyInRange()
        {
            foreach (Creature critter in _observedEnemies)
            {
                if (critter.PositionGet().DistanceTo(_owner.PositionGet()) < (_owner as Creature).GetAttackRange())
                {
                    return critter;
                }
            }

            return null;
        }

        private Creature EnemyNearest()
        {
            if (_observedEnemies.Count == 0)
            {
                return null;
            }

            Creature nearestEnemy = _observedEnemies[0] as Creature;
            Int32 nearestDistance = nearestEnemy.PositionGet().DistanceTo(_owner.PositionGet());
            for (int i = 1; i < _observedEnemies.Count; ++i)
            {
                Creature currentEnemy = _observedEnemies[i] as Creature;
                Int32 currentDistance = currentEnemy.PositionGet().DistanceTo(_owner.PositionGet());
                if (currentDistance < nearestDistance)
                {
                    nearestDistance = currentDistance;
                    nearestEnemy = currentEnemy;
                }
            }

            return nearestEnemy;
        }

        private Coords NearestAcessibleHex(Coords goal,MoveRangeCalculator range)
        {
            Coords here = range.Origin;

            Int32 currentBestDistance = -1;
            Coords currentBest = new Coords();
            // Lame.
            BitArray[] rangeMap = range.CurrentRangeMap;
            for (int i = 0; i < rangeMap.Length; ++i)
            {
                for (int j = 0; j < rangeMap[i].Count; ++j)
                {
                    Coords currentCoords = new Coords(i,j);
                    Int32 currentDistance = goal.DistanceTo(currentCoords);
                    if (rangeMap[i][j] && (currentDistance<currentBestDistance || currentBestDistance == -1))
                    {
                        currentBestDistance = currentDistance;
                        currentBest = currentCoords;
                    }
                }
            }

            return currentBest;
        }

        private Coords RandomAccessibleHex(MoveRangeCalculator range)
        {
            List<Coords> possibleMoves=new List<Coords>();

            BitArray[] rangeMap = range.CurrentRangeMap;
            for (int i = 0; i < rangeMap.Length; ++i)
            {
                for (int j = 0; j < rangeMap[i].Count; ++j)
                {
                    if (rangeMap[i][j])
                    {
                        possibleMoves.Add(new Coords(i, j));
                    }
                }
            }

            RandomStuff randomator = _owner.MapGeneral.Randomator;
            return possibleMoves[(Int32)(randomator.NSidedDice((UInt16)possibleMoves.Count, 1) - 1)];
        }

        public override void Update()
        {
            // have agent update move space to be safe
            _owner.MyMoveRangeCalculator.Update();

            // careful with this loop
            while ((_owner as Creature).GetAP() > 0)
            {
                // if no enemies in sight, engage in idle behavior
                if (base._observedEnemies.Count == 0)
                {
                    // idle behavior
                    Coords goal = RandomAccessibleHex(_owner.MyMoveRangeCalculator);
                    if (goal != _owner.PositionGet())
                    {
                        Action goSomewhere = new ActionMove((_owner as Creature), _owner.MyMoveRangeCalculator.RetrieveRoute(goal),
                            _owner.MyMoveRangeCalculator.Cost(goal), _drawer);
                        goSomewhere.Execute();
                    }

                    return;
                }

                // if enemy in range, attack
                Creature potentialTarget = EnemyNearest();

                float range = (_owner as Creature).GetAttackRange();
                if (potentialTarget.PositionGet().DistanceTo(_owner.PositionGet()) <= (_owner as Creature).GetAttackRange())
                {
                    UInt16 ap = (_owner as Creature).GetAP();
                    if ((_owner as Creature).GetAP() > (_owner as Creature).SpellCurrent.ExecutionTime)
                    {
                        Action attack = new ActionUseSpell(_owner as Creature, (_owner as Creature).SpellCurrent, potentialTarget.PositionGet());
                        attack.Execute();
                    }
                    else
                    {
                        // not enough AP. end turn.
                        return;
                    }
                }
                else
                {
                    Coords goal = NearestAcessibleHex(potentialTarget.PositionGet(), _owner.MyMoveRangeCalculator);
                    if (goal != _owner.PositionGet())
                    {
                        Action goToAction = new ActionMove(_owner, _owner.MyMoveRangeCalculator.RetrieveRoute(goal), 
                            _owner.MyMoveRangeCalculator.Cost(goal), _drawer);
                        goToAction.Execute();
                    }
                    else
                    {
                        return;
                    }
                }

                // no enemy in range - approach nearest enemy
            }
        }

        public BrainBasicAI(Creature owner, DrawerBattle drawer)
            : base(owner)
        {
            _drawer = drawer;
        }

        public BrainBasicAI(DrawerBattle drawer)
            : base()
        {
            _drawer = drawer;
        }
    }
}
