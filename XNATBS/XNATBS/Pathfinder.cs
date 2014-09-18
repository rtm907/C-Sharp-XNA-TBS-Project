using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace XNATBS
{
    /// <summary>
    /// Pathfinder class. Finds routes give a passability map for the grid.
    /// </summary>
    public class Pathfinder
    {
        private BitArray[] _passabilityMap;
        public BitArray[] PassabilityMap
        {
            get
            {
                return _passabilityMap;
            }
        }

        #region Pathfinder

        /// <summary>
        /// Heuristic function for the A* pathfinder
        /// </summary> 
        public delegate float HeuristicFunction(Coords here, Coords end);

        private delegate float hFunction(Coords here);

        /// <summary>
        /// Node struct for the A* pathfinder.
        /// </summary>
        private struct NodeAStar : IComparable
        {
            public Direction connection;
            public float costSoFar;
            public float estimatedTotalCost;

            public Int32 CompareTo(object obj)
            {
                if (!(obj is NodeAStar))
                {
                    throw new Exception("Bad NodeAStar comparison.");
                }

                NodeAStar compareWithMe = (NodeAStar)obj;
                Int32 returnValue = 0;
                float difference = this.estimatedTotalCost - compareWithMe.estimatedTotalCost;
                if (difference > 0)
                {
                    returnValue = 1;
                }
                else if (difference < 0)
                {
                    returnValue = -1;
                }

                return returnValue;
            }

            public NodeAStar(Direction d, float cost, float estimate)
            {
                this.connection = d;
                this.costSoFar = cost;
                this.estimatedTotalCost = estimate;
            }
        }

        private List<Direction> _PathfinderAStar(Coords start, Coords endTopLeft, Coords endBottomRight, BitArray[] _passabilityMap, hFunction h)
        {
            // NOTE: Should later implemented a collision predictor mechanic to work in tandem
            // with the path-finder to provide better agent behavior.
            // NOTE: Consider returning the number of tiles scanned in case no path is found.
            // This will alert a boxed-in creature of its predicament.
            // NOTE: Introduce a flag for a straight-line initial check(for outdoors environmens and
            // for when the goal is near).

            Int32 rangeX = _passabilityMap.Length;
            Int32 rangeY = _passabilityMap[0].Count;

            NodeAStar?[,] nodeArray = new NodeAStar?[rangeX, rangeY];

            NodeAStar startNode = new NodeAStar();
            startNode.costSoFar = 0;
            startNode.estimatedTotalCost = h(start);

            nodeArray[start.X, start.Y] = startNode;

            List<Coords> ListOpen = new List<Coords>();
            ListOpen.Add(start);
            while (ListOpen.Count > 0)
            {
                // I have to use this bool the way I've implemented the algo. Consider rewriting.
                bool resortList = false;

                Coords currentCoords = ListOpen.First();
                // Check to see if goal is reached.
                //if (currentCoords.Equals(endTopLeft))
                if (StaticMathFunctions.CoordinateIsInBox(currentCoords, endTopLeft, endBottomRight))
                {
                    break;
                }

                NodeAStar currentNode = nodeArray[currentCoords.X, currentCoords.Y].Value;
                for (byte i = 0; i <= 5; ++i)
                {
                    Direction currentDir = (Direction)(i);
                    //Coords dirCoords = StaticMathFunctions.DirectionToCoords(currentDir);
                    Coords potential = StaticMathFunctions.CoordsNeighboringInDirection(currentCoords, currentDir);
                    // check if move in dir is allowed
                    if (potential.X >= 0 && potential.X < rangeX && potential.Y >= 0 && potential.Y < rangeY // bounds check
                        && _passabilityMap[potential.X][potential.Y]) // passability check
                    {
                        // Using the simplest cost function possible. Can be easily updated
                        // once tile walkability coefficients are added.
                        //Coords newNodePosition = new Coords(CoordsType.General, currentCoords.X + dirCoords.X, currentCoords.Y + dirCoords.Y);
                        Coords newNodePosition = potential;
                        float accruedCost = currentNode.costSoFar + 1;

                        // Straight line correction
                        if (currentDir == nodeArray[currentCoords.X, currentCoords.Y].Value.connection)
                        {
                            accruedCost -= Constants.PathfinderStraightPathCorrection;
                        }

                        // Check to see if the node under examination is in the closed list.
                        //NodeAStar? oldNode = nodeArray[newNodePosition.X, newNodePosition.Y];
                        if (nodeArray[newNodePosition.X, newNodePosition.Y] != null)
                        {
                            // If node is in closed list, see if it needs updating.
                            if (nodeArray[newNodePosition.X, newNodePosition.Y].Value.costSoFar > accruedCost)
                            {
                                float expectedAdditionalCost =
                                    nodeArray[newNodePosition.X, newNodePosition.Y].Value.estimatedTotalCost -
                                    nodeArray[newNodePosition.X, newNodePosition.Y].Value.costSoFar;
                                NodeAStar nodeToAdd =
                                    new NodeAStar(currentDir, accruedCost, accruedCost + expectedAdditionalCost);
                                nodeArray[newNodePosition.X, newNodePosition.Y] = nodeToAdd;
                                ListOpen.Add(newNodePosition);
                                resortList = true;
                            }
                        }
                        // Node is in open list. Process it.
                        else
                        {
                            float expectedAdditionalCost = h(newNodePosition);
                            NodeAStar nodeToAdd =
                                new NodeAStar(currentDir, accruedCost, accruedCost + expectedAdditionalCost);
                            nodeArray[newNodePosition.X, newNodePosition.Y] = nodeToAdd;
                            ListOpen.Add(newNodePosition);
                            resortList = true;
                        }
                    }
                }

                ListOpen.RemoveAt(0);
                if (resortList)
                {
                    ListOpen.Sort(
                        delegate(Coords c1, Coords c2)
                        {
                            float difference = nodeArray[c1.X, c1.Y].Value.estimatedTotalCost -
                                nodeArray[c2.X, c2.Y].Value.estimatedTotalCost;

                            Int32 returnValue = 0;
                            if (difference > 0)
                            {
                                returnValue = 1;
                            }
                            else if (difference < 0)
                            {
                                returnValue = -1;
                            }
                            return returnValue;
                        }
                    );
                }
            }

            List<Direction> ListRoute = new List<Direction>();

            // Return empty route if the open list is empty, i.e. there is no path to the target
            // Ideally, the game logic should be fixed so that the search isn't even attempted
            // if there is no path between the two points.
            if (ListOpen.Count == 0)
            {
                return ListRoute;
            }

            Coords trackbackCoords = endTopLeft;
            while (trackbackCoords != start)
            {
                Direction newDirection = nodeArray[trackbackCoords.X, trackbackCoords.Y].Value.connection;
                ListRoute.Add(newDirection);
                trackbackCoords = StaticMathFunctions.CoordsNeighboringInDirection(new Coords(CoordsType.Tile, trackbackCoords),
                    StaticMathFunctions.OppositeDirection(newDirection));
            }

            // Might be faster without reversing
            //ListRoute.Reverse();

            // We skip the reversal, so pick directions from the END of the list.
            return ListRoute;
        }

        /// <summary>
        /// Tile-level (coarse) A* pathfinding.
        /// </summary>
        /// <param name="start"> Start Coords </param>
        /// <param name="endTopLeft"> Goal-box TopLeft Coords </param>
        /// <param name="endBottomRight"> Goal-box BottomRight Coords </param>
        /// <param name="h"> Heuristic function </param>
        /// <returns> Route to goal, as a list of Directions </returns>
        public List<Direction> PathfinderAStarCoarse(Coords start, Coords endTopLeft, Coords endBottomRight, HeuristicFunction h)
        {
            return this._PathfinderAStar(new Coords(CoordsType.General, start), new Coords(CoordsType.General, endTopLeft), new Coords(CoordsType.General, endBottomRight), this._passabilityMap,
                delegate(Coords c) { return h(c, StaticMathFunctions.CoordsAverage(endTopLeft, endBottomRight)); });
        }

        /// <summary>
        /// Tile-level (coarse) A* pathfinding.
        /// </summary>
        /// <param name="start"> Start Coords </param>
        /// <param name="end"> Target tile Coords </param>
        /// <param name="h"> Heuristic function </param>
        /// <returns> Route to goal, as a list of Directions </returns>
        public List<Direction> PathfinderAStarCoarse(Coords start, Coords end, HeuristicFunction h)
        {
            return this.PathfinderAStarCoarse(start, end, end, h);
        }

        #endregion


        public Pathfinder(BitArray[] passMap)
        {
            _passabilityMap = passMap;
        }
    }    
    
    /// <summary>
    /// Obtains avilable moves for a given creature.
    /// Is a specialized (less general) A*.
    /// </summary>
    public class MoveRangeCalculator
    {
        private Creature _owner;

        // generalize this to work for general maps
        private MapBattle _currentMap;
        private BitArray[] _rangeMap;
        public BitArray[] CurrentRangeMap
        {
            get
            {
                return _rangeMap;
            }
        }
        private Int32[,] _APCount;

        private Coords _currentOrigin;
        public Coords Origin
        {
            get
            {
                return _currentOrigin;
            }
        }

        private Direction?[,] _directionMap;

        private void CalculateMoveRange(Coords origin, UInt16[] moveCosts, UInt16 availableAP)
        {
            _currentOrigin = origin;

            for (int i = 0; i < _currentMap.BoundX; ++i)
            {
                for (int j = 0; j < _currentMap.BoundY; ++j)
                {
                    _APCount[i, j] = -1;
                    _directionMap[i, j] = null;
                }
            }

            _APCount[origin.X, origin.Y] = availableAP;

            // WARNING: Code repetition with influence maps / A*

            Queue<Coords> currentQueue = new Queue<Coords>();
            Queue<Coords> nextQueue = new Queue<Coords>();

            currentQueue.Enqueue(origin);

            UInt32 currentDistance = 0;

            // main loop
            // Stopping conditions: the two queues are exhausted, OR InfluenceMapMaxDistance is reached
            while
                (
                ((currentQueue.Count > 0) & (nextQueue.Count > 0))
                |
                (currentDistance < Constants.InfluenceMapMaxDistance)
                )
            {
                // Checks if it's time to start the next pass
                if (currentQueue.Count == 0)
                {
                    currentQueue = nextQueue;
                    nextQueue = new Queue<Coords>();
                    currentDistance++;
                    continue;
                }

                Coords currentCoords = currentQueue.Peek();
                //Coords delta1 = currentCoords - origin;
                Tile currentTile = _currentMap.GetTile(currentCoords);

                // Analyzes the neighbors of the current Tile for possible additions to nextQueue
                for (byte i = 0; i < 6; i++)
                {
                    Direction currentDir = (Direction)i;
                    Coords toCheck = StaticMathFunctions.CoordsNeighboringInDirection(currentCoords, currentDir);
                    if (_currentMap.CheckInBounds(toCheck))
                    {
                        Tile targetTile = _currentMap.GetTile(toCheck);
                        UInt16 cost = moveCosts[(sbyte)targetTile.MyTerrainType];

                        if (cost > 0 && _currentMap.TenancyMap[toCheck.X, toCheck.Y] == null) // ignore impassable terrain and ignore occupied tiles
                        {
                            // checks if this approach is cheaper than the best approach so far
                            Int32 currentAPleft = _APCount[toCheck.X, toCheck.Y];
                            Int32 potentialAPleft = _APCount[currentCoords.X, currentCoords.Y] - cost;
                            if (currentAPleft < potentialAPleft)
                            {
                                _APCount[toCheck.X, toCheck.Y] = (UInt16)potentialAPleft;
                                _directionMap[toCheck.X, toCheck.Y] = currentDir;
                                nextQueue.Enqueue(toCheck);
                            }
                        }
                    }
                }

                currentQueue.Dequeue();
            }


            for (int i = 0; i < _currentMap.BoundX; ++i)
            {
                for (int j = 0; j < _currentMap.BoundY; ++j)
                {
                    _rangeMap[i][j] = _APCount[i, j] >= 0;
                }
            }

            //return rangeMap;
        }

        public void Update()
        {
            CalculateMoveRange(_owner.PositionGet(), _owner.APMoveCosts, _owner.GetAP());
        }

        public bool Accessible(Coords c)
        {
            if (!_currentMap.CheckInBounds(c))
            {
                throw new Exception("Out of bounds coordinates passed.");
            }
            return _rangeMap[c.X][c.Y];
        }

        public UInt16 Cost(Coords c)
        {
            if (!_currentMap.CheckInBounds(c))
            {
                throw new Exception("Out of bounds coordinates passed.");
            }

            return ((UInt16)(_APCount[_currentOrigin.X, _currentOrigin.Y] - _APCount[c.X, c.Y]));
        }

        public List<Direction> RetrieveRoute(Coords goal)
        {
            if (_owner.PositionGet() != _currentOrigin)
            {
                Update();
            }

            if (!_currentMap.CheckInBounds(goal) || !_rangeMap[goal.X][goal.Y])
            {
                return null;
            }

            List<Direction> returnList = new List<Direction>();

            Coords trackbackCoords = goal;
            while (trackbackCoords != _currentOrigin)
            {
                Direction newDirection = _directionMap[trackbackCoords.X, trackbackCoords.Y].Value;
                returnList.Add(newDirection);
                trackbackCoords = StaticMathFunctions.CoordsNeighboringInDirection(new Coords(CoordsType.Tile, trackbackCoords),
                    StaticMathFunctions.OppositeDirection(newDirection));
            }

            return returnList;
        }

        private MoveRangeCalculator(MapBattle currentMap)
        {
            _currentMap = currentMap;

            _directionMap = new Direction?[_currentMap.BoundX, _currentMap.BoundY];

            _rangeMap = new BitArray[_currentMap.BoundX];
            for (int i = 0; i < _currentMap.BoundX; ++i)
            {
                _rangeMap[i] = new BitArray(_currentMap.BoundY);
            }

            _APCount = new Int32[_currentMap.BoundX, _currentMap.BoundY];
        }
        public MoveRangeCalculator(Creature owner)
            : this(owner.MapBattlemap)
        {
            _owner = owner;
        }
    }
}
