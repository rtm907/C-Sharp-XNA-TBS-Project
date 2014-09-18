using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace XNATBS
{
    /// <summary>
    /// Tracks visibility for the agents on a map.
    /// </summary>
    public class VisiblityTracker
    {
        private UInt16 _sizeX;
        public UInt16 SizeX
        {
            get
            {
                return _sizeX;
            }
        }

        private UInt16 _sizeY;
        public UInt16 SizeY
        {
            get
            {
                return _sizeY;
            }
        }

        //private UInt16 _pixelsPerBoxX;
        //private UInt16 _pixelsPerBoxY;

        private float[,] _visibilityMap;
        public float[,] VisibilityMap
        {
            get
            {
                return _visibilityMap;
            }
            set
            {
                _visibilityMap = value;
            }
        }

        /*
        private BitArray[] _passabilityMap;
        public BitArray[] PassabilityMap
        {
            get
            {
                return _passabilityMap;
            }
            set
            {
                _passabilityMap = value;
            }
        }
        */

        // -1 means unexplored
        // 0 means explored but not seen
        // from there on add 1 for every team member who sees hex
        private SortedList<sbyte, Int16[,]> _teamVisibilityTracker;
        private void InitializeTeamVisibilityTracker(Team team)
        {
            _teamVisibilityTracker.Add(team.UniqueID, new Int16[_sizeX, _sizeY]);
            
            Int16[,] tracker = _teamVisibilityTracker[team.UniqueID];
            // set everything to unexplored by default?
            for (int i = 0; i < _sizeX; ++i)
            {
                for (int j = 0; j < _sizeY; ++j)
                {
                    tracker[i, j] = -1;
                }
            }
        }

        /// <summary>
        /// Returns the visibility status of coord c for a team: -1 is unexplored, 0 unseen, a positive return 
        /// indicates the number of team members observing the tile.
        /// </summary>
        public Int16 VisibilityCheck(Coords c, Team team)
        {
            return _teamVisibilityTracker[team.UniqueID][c.X, c.Y];
        }

        private bool TeamInitCheck(Team team)
        {
            return _teamVisibilityTracker.ContainsKey(team.UniqueID);
        }

        // if 'increase' is true, add 1 to the number of a team's creatures seeing a tile
        private void TeamVisibilityUpdate(Coords c, Team team, bool increase)
        {
            Int16[,] tracker = _teamVisibilityTracker[team.UniqueID];
            if (increase)
            {
                if (tracker[c.X, c.Y] == -1)
                {
                    tracker[c.X, c.Y] += 2;
                }
                else
                {
                    ++tracker[c.X, c.Y];
                }
            }
            else
            {
                --tracker[c.X, c.Y];
            }
        }

        // list of the creatures in tile i,j.
        private LinkedList<Unit>[,] _residents;// = new LinkedList<Creature>();
        /*
        public LinkedList<Creature> VisibilityResidents(Coords c)
        {
            if (c.Type == CoordsType.Tile)
            {
                return _residents[c.X, c.Y];
            }

            return _residents[c.X / _pixelsPerBoxX, c.Y / _pixelsPerBoxY];
        }
        */
        //private BitArray[,] _visibilityTrackers;// = new BitArray((Int32)Constants.MaximumNumberOfCreatures);
        private SortedList<UInt32, Unit>[,] _visibilityTrackers;

        /// <summary>
        /// Checks if an agent sees the tile at coordinates 'c'.
        /// </summary>
        public bool VisibilityCheck(Coords c, Unit critter)
        {
            //Coords c = new Coords(CoordsType.Tile, critter.PositionDouble);

            return _visibilityTrackers[c.X, c.Y].ContainsKey(critter.UniqueID);
        }

        /// <summary>
        /// Updates the visibility flag for an agent and a tile.
        /// ('True' if agent 'sees' the gridtile.)
        /// </summary>
        public void VisibilityUpdate(Coords c, Unit critter, bool newValue)
        {
            //Int32 ID = (Int32)critter.UniqueID;
            //Coords c = new Coords(CoordsType.Tile, critter.PositionDouble);
            if(!TeamInitCheck(critter.Team))
            {
                InitializeTeamVisibilityTracker(critter.Team);
            }

            if (newValue)
            {
                if (!_visibilityTrackers[c.X, c.Y].ContainsKey(critter.UniqueID))
                {
                    _visibilityTrackers[c.X, c.Y].Add(critter.UniqueID, critter);
                    TeamVisibilityUpdate(c, critter.Team, true);
                    //TilePassable thisAsPassable = this as TilePassable;
                    if (_visibilityMap[c.X,c.Y] > 0)
                    {
                        this.VisibilityInformTileVisibilityChange(c, newValue, critter);
                    }
                }
            }
            else
            {
                if (_visibilityTrackers[c.X, c.Y].ContainsKey(critter.UniqueID))
                {
                    _visibilityTrackers[c.X, c.Y].Remove(critter.UniqueID);
                    TeamVisibilityUpdate(c, critter.Team, false);
                    //TilePassable thisAsPassable = this as TilePassable;
                    if (_visibilityMap[c.X,c.Y] > 0)
                    {
                        this.VisibilityInformTileVisibilityChange(c, newValue, critter);
                    }
                }
            }
        }

        /// <summary>
        /// Informs the agents observing the gridtile of a change in its residency.
        /// ('True' if a new tenant has arrived.)
        /// </summary>
        public void VisibilityInformResidencyChange(Coords c, bool arrivalOrDeparture, Unit critter)
        {
            //Coords c = new Coords(CoordsType.Tile, critter.PositionDouble);

            // inform of new arrival
            if (arrivalOrDeparture)
            {
                foreach (KeyValuePair<UInt32, Unit> someCreature in this._visibilityTrackers[c.X, c.Y])
                {
                    someCreature.Value.CreatureBrain.ObservedCreaturesAdd(critter);
                }
            }
            else
            {
                foreach (KeyValuePair<UInt32, Unit> someCreature in this._visibilityTrackers[c.X, c.Y])
                {
                    someCreature.Value.CreatureBrain.ObservedCreaturesRemove(critter);
                }
            }
        }

        /// <summary>
        /// Updates an agent's visibility info according to his change in visibility status for a gridtile.
        /// ('True' if the gridtile has become visible to the creature.)
        /// </summary>
        public void VisibilityInformTileVisibilityChange(Coords c, bool visibleOrNot, Unit critter)
        {
            //Coords c = new Coords(CoordsType.Tile, critter.PositionDouble);

            ResidentsInitCheck(c);

            if (this._residents[c.X, c.Y].Count > 0)
            {
                if (visibleOrNot)
                {
                    foreach (Creature observed in this._residents[c.X, c.Y])
                    {
                        critter.CreatureBrain.ObservedCreaturesAdd(observed);
                    }
                }
                else
                {
                    foreach (Creature observed in this._residents[c.X, c.Y])
                    {
                        critter.CreatureBrain.ObservedCreaturesRemove(observed);
                    }
                }
            }

        }

        /// <summary>
        /// Checks if the residents list of a gridtile has been initialized.
        /// </summary>
        private void ResidentsInitCheck(Coords c)
        {
            if (_residents[c.X, c.Y] == null)
            {
                _residents[c.X, c.Y] = new LinkedList<Unit>();
            }
        }

        /// <summary>
        /// Adds a new tenant to a gridtile.
        /// </summary>
        public void VisibilityResidentAdd(Coords c, Unit member)
        {
            //Coords c = new Coords(CoordsType.Tile, member.PositionDouble);

            ResidentsInitCheck(c);
            _residents[c.X, c.Y].AddFirst(member);
            VisibilityInformResidencyChange(c, true, member);
        }
        /// <summary>
        /// Removes a tenant from a gridtile.
        /// </summary>
        public void VisibilityResidentRemove(Coords c, Unit member)
        {
            //Coords c = new Coords(CoordsType.Tile, member.PositionDouble);

            ResidentsInitCheck(c);
            _residents[c.X, c.Y].Remove(member);
            VisibilityInformResidencyChange(c, false, member);
        }

        /// <summary>
        /// Returns true if the agent standing at c1 can see c2.
        /// If diagonal strictness if flagged, returns false when the ray passes through
        /// an intersection, one of the lots on which is impassable.
        /// </summary>
        public bool RayTracerVisibilityCheckTile(Coords c1, Coords c2, bool diagonalStrictness, UInt16 sightRange)
        {
            // CODE REPETITION! CONSIDER REVISING.
            // NOTES: 
            // -This is not done for hexes, it's for squares
            // -I rasterize the grid by another factor of 4 so I can shift the y-odd rows one to the right. This leads to twice 
            // as many checks, half unnecessary.


            // This is only for tiles.
            if (c1.Type != CoordsType.Tile || c2.Type != CoordsType.Tile)
            {
                throw new Exception("Non-Tile coords passed for RayTracerVisibilityCheckTile.");
                //return false;
            }

            Int32 x0 = c1.X*2+c1.Y%2;
            Int32 y0 = c1.Y*2;
            Int32 x1 = c2.X*2+c2.Y%2;
            Int32 y1 = c2.Y*2;
            
            if (x0 < x1) { ++x0; }
            else if(x0>x1){ ++x1; }

            if (y0 < y1) { ++y0; }
            else { ++y1; }

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int x = x0;
            int y = y0;
            int n = 1 + dx + dy;
            int x_inc = (x1 > x0) ? 1 : -1;
            int y_inc = (y1 > y0) ? 1 : -1;
            int error = dx - dy;
            dx *= 2;
            dy *= 2;

            //n *= 2;
            //n -= 1;
            //x *= 2;
            //y *= 2;

            float accrued = 0;

            for (; n > 0; --n)
            {
                Coords currentCoords = new Coords(CoordsType.Tile, (x-(y/2)%2)/2, (y)/2);
                if (y < 0) { continue; }

                accrued += _visibilityMap[currentCoords.X, currentCoords.Y];

                // We ignore accrued visibility for now. Can add it later.
                if (((this._visibilityMap[currentCoords.X, currentCoords.Y] == 0) || accrued > sightRange )
                    &&
                    (currentCoords != c2)
                    )
                {
                    return false;
                }

                if (!diagonalStrictness)
                {
                    if (error > 0)
                    {
                        x += x_inc;
                        error -= dy;
                    }
                    else if (error < 0)
                    {
                        y += y_inc;
                        error += dx;
                    }
                    else
                    {
                        x += x_inc;
                        error -= dy;
                        y += y_inc;
                        error += dx;
                        --n;
                    }
                }
                else
                {
                    if (error > 0)
                    {
                        x += x_inc;
                        error -= dy;
                    }
                    else
                    {
                        y += y_inc;
                        error += dx;
                    }

                }
            }

            return true;
        }

        /*
                 /// <summary>
        /// Returns true if the agent standing at c1 can see c2.
        /// If diagonal strictness if flagged, returns false when the ray passes through
        /// an intersection, one of the lots on which is impassable.
        /// </summary>
        public bool RayTracerVisibilityCheckTile(Coords c1, Coords c2, bool diagonalStrictness)
        {
            // CODE REPETITION! CONSIDER REVISING.

            // This is only for tiles.
            if (c1.Type != CoordsType.Tile || c2.Type != CoordsType.Tile)
            {
                throw new Exception("Non-Tile coords passed for RayTracerVisibilityCheckTile.");
                //return false;
            }

            Int32 x0 = c1.X;
            Int32 y0 = c1.Y;
            Int32 x1 = c2.X;
            Int32 y1 = c2.Y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int x = x0;
            int y = y0;
            int n = 1 + dx + dy;
            int x_inc = (x1 > x0) ? 1 : -1;
            int y_inc = (y1 > y0) ? 1 : -1;
            int error = dx - dy;
            dx *= 2;
            dy *= 2;

            for (; n > 0; --n)
            {
                Coords currentCoords = new Coords(CoordsType.Tile, x, y);

                // We ignore accrued visibility for now. Can add it later.
                if ((this._visibilityMap[currentCoords.X, currentCoords.Y] == 0) && (currentCoords != c2))
                {
                    return false;
                }

                if (!diagonalStrictness)
                {
                    if (error > 0)
                    {
                        x += x_inc;
                        error -= dy;
                    }
                    else if (error < 0)
                    {
                        y += y_inc;
                        error += dx;
                    }
                    else
                    {
                        x += x_inc;
                        error -= dy;
                        y += y_inc;
                        error += dx;
                        --n;
                    }
                }
                else
                {
                    if (error > 0)
                    {
                        x += x_inc;
                        error -= dy;
                    }
                    else
                    {
                        y += y_inc;
                        error += dx;
                    }

                }
            }

            return true;
        }
         */

        public void DeregisterCreature(Unit removeMe)
        {
            if (!(removeMe is Creature))
            {
                throw new Exception("Fix visibility tracker for bands; look at the PositionTile method for Units.");
            }
            VisibilityResidentRemove((removeMe as Creature).PositionGet(), removeMe);
            VisibilityInformResidencyChange((removeMe as Creature).PositionGet(), false, removeMe);

            BitArray[] agentFOV = removeMe.FieldOfView;
            for (int i = 0; i < agentFOV.Length; ++i)
            {
                for (int j = 0; j < agentFOV[0].Count; ++j)
                {
                    if (agentFOV[i][j])
                    {
                        _visibilityTrackers[i, j].Remove(removeMe.UniqueID);
                    }
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sizeX">Grid width in tiles.</param>
        /// <param name="sizeY">Grid height in tiles.</param>
        /// <param name="pixelsPerBoxX">Tile width.</param>
        /// <param name="pixelsPerBoxY">Tile height.</param>
        /// <param name="passMap">Terrain passability map.</param>
        /// <param name="visMap">Terrain visibility map.</param>
        public VisiblityTracker(UInt16 sizeX, UInt16 sizeY, float[,] visMap)
        {
            _visibilityMap = visMap;
            //_passabilityMap = passMap;

            _teamVisibilityTracker = new SortedList<sbyte, Int16[,]>();
            _visibilityTrackers = new SortedList<UInt32, Unit>[sizeX, sizeY];
            _residents = new LinkedList<Unit>[sizeX, sizeY];
            for (int i = 0; i < sizeX; ++i)
            {
                for (int j = 0; j < sizeY; ++j)
                {
                    _visibilityTrackers[i, j] = new SortedList<UInt32, Unit>();
                }
            }

            _sizeX = sizeX;
            _sizeY = sizeY;
            //_pixelsPerBoxX = pixelsPerBoxX;
            //_pixelsPerBoxY = pixelsPerBoxY;
        }
    }
}
