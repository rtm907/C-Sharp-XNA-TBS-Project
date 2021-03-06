﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

namespace XNATBS
{
    /// <summary>
    /// Map class. The map is a double array of Tiles.
    /// Contains the Pathfinding algos, such as A* and Bresenheim lines.
    /// Contains Checking methods for tile visibility/ walkability/ in-bounds.
    /// </summary>
    public abstract class Map
    {
        #region Properties

        // Map dimensions
        protected UInt16 _xMax;
        /// <summary>
        /// The X bound of this map.
        /// </summary>
        public UInt16 BoundX
        {
            get
            {
                return _xMax;
            }
        }
        protected UInt16 _yMax;
        /// <summary>
        /// The Y bound of this map.
        /// </summary>
        public UInt16 BoundY
        {
            get
            {
                return _yMax;
            }
        }

        /*
        private Game1 _game;
        public Game1 Game
        {
            get
            {
                return _game;
            }
        }
        */

        protected UInt32 _pixelMaxX;
        public UInt32 PixelBoundX
        {
            get
            {
                return _pixelMaxX;
            }
        }
        protected UInt32 _pixelMaxY;
        public UInt32 PixelBoundY
        {
            get
            {
                return _pixelMaxY;
            }
        }

        protected Tile[,] _tiles;
        /// <summary>
        /// Double Tile array representing the map
        /// </summary>
        public Tile[,] Tiles
        {
            get
            {
                return this._tiles;
            }
        }

        protected float[,] _visibilityMap;
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

        // Counts spawned creatures. Used to issue unique IDs. Consider moving this to some kind of a 
        // 'world' or 'game' type.
        protected UInt32 _creatureCount = 0;
        public UInt32 CreatureCount
        {
            get
            {
                return _creatureCount;
            }
        }

        protected UInt32 _itemCount = 0;
        public UInt32 ItemCount
        {
            get
            {
                return this._itemCount;
            }
        }

        protected sbyte _teamCount = 0;
        public sbyte TeamCount
        {
            get
            {
                return _teamCount;
            }
        }

        protected RandomStuff _randomator;
        /// <summary>
        /// Reference to the random number generator for this map
        /// </summary>
        public RandomStuff Randomator
        {
            get
            {
                return this._randomator;
            }
            set
            {
                this._randomator = value;
            }
        }

        protected VisiblityTracker _myVisibilityTracker;
        public VisiblityTracker MyVisibilityTracker
        {
            get
            {
                return _myVisibilityTracker;
            }
        }

        /*
        private Pathfinder _myPathfinder;
        public Pathfinder MyPathfinder
        {
            get
            {
                return _myPathfinder;
            }
        }
        */

        protected SortedDictionary<sbyte, Team> _teamRoster = new SortedDictionary<sbyte, Team>();
        /*
        public SortedDictionary<sbyte, Team> TeamRoster
        {
            get
            {
                return _teamRoster;
            }
        }
        */
        protected SortedDictionary<UInt32, Creature> _menagerie = new SortedDictionary<UInt32, Creature>();
        /// <summary>
        /// Monsters belonging to the map.
        /// </summary>
        public SortedDictionary<UInt32, Creature> Menagerie
        {
            get
            {
                return this._menagerie;
            }
            set
            {
                this._menagerie = value;
            }
        }

        protected SortedDictionary<UInt32, Creature> _mortuary = new SortedDictionary<UInt32, Creature>();
        public SortedDictionary<UInt32, Creature> Mortuary
        {
            get
            {
                return this._mortuary;
            }
            set
            {
                this._mortuary = value;
            }
        }

        // Item catalogue, indexed by ID
        protected SortedDictionary<UInt32, Item> _catalogue = new SortedDictionary<UInt32, Item>();
        public SortedDictionary<UInt32, Item> Catalogue
        {
            get
            {
                return this._catalogue;
            }
            set
            {
                this._catalogue = value;
            }
        }
        
        #endregion

        #region Methods
        public Tile GetTile(Coords coords)
        {
            if (coords.Type == CoordsType.Pixel)
            {
                coords = new Coords(CoordsType.Tile, coords);
            }

            return _tiles[coords.X, coords.Y];
        }
        public Tile GetTile(Int32 X, Int32 Y)
        {
            return _tiles[X, Y];
        }
        public void SetTile(Coords coords, Tile newValue)
        {
            _tiles[coords.X, coords.Y] = newValue;
        }

        public bool TileIsPassable(Coords c)
        {
            return Constants.APMoveCostsStandard[(sbyte)GetTile(c).MyTerrainType] > 0;
        }

        #region Creature / Item registers

        /// <summary>
        /// Returns the creature with ID 'key'
        /// </summary>
        public Creature MenagerieGetCreatureFrom(UInt32 key)
        {
            return this._menagerie[key];
        }
        /// <summary>
        /// Add creature to the menagerie. They 'key' is the creature ID.
        /// </summary>
        public void MenagerieAddCreatureTo(UInt32 key, Creature newGuy)
        {
            this._menagerie[key] = newGuy;
        }
        /// <summary>
        /// Deletes the creature with 'ID' key from the dictionary.
        /// </summary>
        public void MenagerieDeleteCreatureFrom(UInt32 key)
        {
            this._menagerie.Remove(key);
        }
        public void MortuaryAddCreatureTo(UInt32 key, Creature newGuy)
        {
            this._mortuary[key] = newGuy;
        }

        /// <summary>
        /// Issues ID to a creature.
        /// </summary>
        public UInt32 IssueCreatureID()
        {
            if (_creatureCount == Constants.MaximumNumberOfCreatures)
            {
                throw new Exception("Maximum number of creatures reached.");
            }
            return this._creatureCount++;
        }

        public void CatalogueAddItemTo(UInt32 key, Item newItem)
        {
            this._catalogue.Add(key, newItem);
        }
        public void CatalogueDeleteItemFrom(UInt32 key)
        {
            this._catalogue.Remove(key);
        }

        /// <summary>
        /// Issues a new Item ID and increments item count.
        /// </summary>
        /// <returns></returns>
        public UInt32 IssueItemID()
        {
            return this._itemCount++;
        }

        public Team TeamRosterGetTeamFrom(sbyte key)
        {
            return this._teamRoster[key];
        }
        public void TeamRosterAddTeamTo(sbyte key, Team newTeam)
        {
            this._teamRoster[key] = newTeam;
        }
        public void TeamRosterDeleteTeamFrom(sbyte key, Team newTeam)
        {
            this._teamRoster.Remove(key);
        }
        public sbyte IssueTeamID()
        {
            return this._teamCount++;
        }


        #endregion

        /// <summary>
        ///  Analyzes and remembers tile accessibility. Starts at northwest corner and goes through the array,
        ///  checking east / southeast / south / southwest on the current tile and in case of accessibility
        ///  recording the result in both directions.
        /// </summary>
        public void AnalyzeTileAccessibility()
        {
            Tile currentTile;

            for (UInt16 i = 0; i < this._xMax; i++)
            {
                for (UInt16 j = 0; j < this._yMax; j++)
                {
                    Tile east, southEast, southWest;
                    currentTile = this._tiles[i, j];
                    if (Constants.APMoveCostsStandard[(sbyte) currentTile.MyTerrainType] == 0)
                    {
                        continue;
                    }

                    //_vacancyMap[i][j] = true;

                    _visibilityMap[i, j] = currentTile.VisibilityCoefficient;

                    // Sort of wasteful, hopefully compiler does this smartly
                    if (i < _xMax - 1)
                    {
                        east = this.GetTile(StaticMathFunctions.CoordsNeighboringInDirection(new Coords(CoordsType.Tile, i, j), Direction.East));
                        if (east.IsPassable())
                        {
                            currentTile.AllowedMovesSet(Direction.East, true);
                            east.AllowedMovesSet(Direction.West, true);
                        }
                    }

                    if ((i < _xMax - 1) & (j < _yMax - 1))
                    {
                        southEast = this.GetTile(StaticMathFunctions.CoordsNeighboringInDirection(new Coords(CoordsType.Tile, i, j), Direction.Southeast));
                        if (southEast.IsPassable())
                        {
                            currentTile.AllowedMovesSet(Direction.Southeast, true);
                            southEast.AllowedMovesSet(Direction.Northwest, true);
                        }
                    }

                    if ((i > 0) & (j < _yMax - 1))
                    {
                        southWest = this.GetTile(StaticMathFunctions.CoordsNeighboringInDirection(new Coords(CoordsType.Tile, i, j), Direction.Southwest));
                        if (southWest.IsPassable())
                        {
                            currentTile.AllowedMovesSet(Direction.Southwest, true);
                            southWest.AllowedMovesSet(Direction.Northeast, true);
                        }
                    }
                }
            }
        }

        #region Raytracers

        /// <summary>
        /// Returns the tiles under the given line.
        /// Borrowed from: http://playtechs.blogspot.com/2007/03/raytracing-on-grid.html (James McNeill)
        /// </summary>
        public List<Coords> RayTracer(Coords c1, Coords c2)
        {
            List<Coords> returnVal = new List<Coords>();

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
                //visit(x, y);
                returnVal.Add(new Coords(c1.Type, x, y));

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

            return returnVal;
        }

        public bool RayTracerVisibilityCheckPixel(Vector c1, Vector c2)
        {
            double x0 = c1.X;
            double y0 = c1.Y;
            double x1 = c2.X;
            double y1 = c2.Y;

            double dx = Math.Abs(x1 - x0);
            double dy = Math.Abs(y1 - y0);

            int x = (int)(Math.Floor(x0));
            int y = (int)(Math.Floor(y0));

            int n = 1;
            int x_inc, y_inc;
            double error;

            if (dx == 0)
            {
                x_inc = 0;
                error = Double.PositiveInfinity;
            }
            else if (x1 > x0)
            {
                x_inc = 1;
                n += (int)(Math.Floor(x1)) - x;
                error = (Math.Floor(x0) + 1 - x0) * dy;
            }
            else
            {
                x_inc = -1;
                n += x - (int)(Math.Floor(x1));
                error = (x0 - Math.Floor(x0)) * dy;
            }

            if (dy == 0)
            {
                y_inc = 0;
                error -= Double.PositiveInfinity;
            }
            else if (y1 > y0)
            {
                y_inc = 1;
                n += (int)(Math.Floor(y1)) - y;
                error -= (Math.Floor(y0) + 1 - y0) * dx;
            }
            else
            {
                y_inc = -1;
                n += y - (int)(Math.Floor(y1));
                error -= (y0 - Math.Floor(y0)) * dx;
            }


            Coords c2Tile = new Coords(CoordsType.Tile, c2);

            for (; n > 0; --n)
            {
                Coords currentCoords = new Coords(CoordsType.Tile, x, y);

                // We ignore accrued visibility for now. Can add it later.
                if ((this._visibilityMap[currentCoords.X, currentCoords.Y] == 0) && (currentCoords != c2Tile))
                {
                    return false;
                }

                if (error > 0)
                {
                    y += y_inc;
                    error -= dx;
                }
                else
                {
                    x += x_inc;
                    error += dy;
                }
            }

            return true;
        }



        /*
        /// <summary>
        /// Performs a terrain passability check betwee two points by doing pixel validity checks at interval delta.
        /// </summary>
        public List<Creature> RayTracerPassabilityCheckRough(Creature client, Vector v1, Vector v2, double delta)
        {
            Vector difference = v2 - v1;
            Vector deltaV = difference;
            deltaV.ScaleToLength(delta);

            Vector currentPosition = v1;

            for (int i = 0; i < difference.Length() / deltaV.Length(); ++i)
            {
                Coords pixel = new Coords(CoordsType.Pixel, currentPosition);
                List<Creature> collision = _myCollider.CreatureClippingCheck(client, pixel, false);
                if (collision == null || collision.Count > 0)
                {
                    return collision;
                }
                currentPosition += deltaV;
            }

            return new List<Creature>();
        }
        */

        /*
        /// <summary>
        /// Returns the Bresenham line between p0 and p1; Borrowed the code
        /// from some dude whose name I don't have, who in turn borrowed from Wikipedia.
        /// </summary>
        private List<Coords> BresenhamLine(Coords p0, Coords p1)
        {
            List<Coords> returnList = new List<Coords>();

            Boolean steep = Math.Abs(p1.Y - p0.Y) > Math.Abs(p1.X - p0.X);

            if (steep == true)
            {
                Coords tmpPoint = new Coords(CoordsType.Tile, p0.X, p0.Y);
                p0 = new Coords(CoordsType.Tile, tmpPoint.Y, tmpPoint.X);

                tmpPoint = p1;
                p1 = new Coords(CoordsType.Tile, tmpPoint.Y, tmpPoint.X);
            }

            Int32 deltaX = Math.Abs(p1.X - p0.X);
            Int32 deltaY = Math.Abs(p1.Y - p0.Y);
            Int32 error = 0;
            Int32 deltaError = deltaY;
            Int32 yStep = 0;
            Int32 xStep = 0;
            Int32 y = p0.Y;
            Int32 x = p0.X;

            if (p0.Y < p1.Y)
            {
                yStep = 1;
            }
            else
            {
                yStep = -1;
            }

            if (p0.X < p1.X)
            {
                xStep = 1;
            }
            else
            {
                xStep = -1;
            }

            Int32 tmpX = 0;
            Int32 tmpY = 0;

            while (x != p1.X)
            {

                x += xStep;
                error += deltaError;

                //if the error exceeds the X delta then
                //move one along on the Y axis
                if ((2 * error) > deltaX)
                {
                    y += yStep;
                    error -= deltaX;
                }

                //flip the coords if they're steep
                if (steep)
                {
                    tmpX = y;
                    tmpY = x;
                }
                else
                {
                    tmpX = x;
                    tmpY = y;
                }

                //check the point generated is legal
                //and if it is add it to the list
                if (_myCollider.CheckInBounds(new Coords(CoordsType.Tile, tmpX, tmpY)) == true)
                {
                    returnList.Add(new Coords(CoordsType.Tile, tmpX, tmpY));
                }
                else
                {   //a bad point has been found, so return the list thus far
                    return returnList;
                }

            }

            return returnList;
        }
        */

        /// <summary>
        /// Checks if the Bresenham line between p0 and p1 goes only through visible tiles
        /// !!! Code repetition, should redo.
        /// </summary>
        public bool BresenhamLineCheckVisible(Coords p0, Coords p1)
        {
            if (p0.Equals(p1))
            {
                return true;
            }

            Boolean steep = Math.Abs(p1.Y - p0.Y) > Math.Abs(p1.X - p0.X);

            // fix this stupidity
            Coords p0original = new Coords(CoordsType.Tile, p0.X, p0.Y);
            Coords p1original = new Coords(CoordsType.Tile, p1.X, p1.Y);

            if (steep == true)
            {
                Coords tmpPoint = new Coords(CoordsType.Tile, p0.X, p0.Y);
                p0 = new Coords(CoordsType.Tile, tmpPoint.Y, tmpPoint.X);

                tmpPoint = p1;
                p1 = new Coords(CoordsType.Tile, tmpPoint.Y, tmpPoint.X);
            }

            Int32 deltaX = Math.Abs(p1.X - p0.X);
            Int32 deltaY = Math.Abs(p1.Y - p0.Y);
            Int32 error = 0;
            Int32 deltaError = deltaY;
            Int32 yStep = 0;
            Int32 xStep = 0;
            Int32 y = p0.Y;
            Int32 x = p0.X;

            if (p0.Y < p1.Y)
            {
                yStep = 1;
            }
            else
            {
                yStep = -1;
            }

            if (p0.X < p1.X)
            {
                xStep = 1;
            }
            else
            {
                xStep = -1;
            }

            Int32 tmpX = 0;
            Int32 tmpY = 0;


            float visibilityTotal = 1f;

            while (x != p1.X)
            {

                x += xStep;
                error += deltaError;

                //if the error exceeds the X delta then
                //move one along on the Y axis
                if ((2 * error) > deltaX)
                {
                    y += yStep;
                    error -= deltaX;
                }

                //flip the coords if they're steep
                if (steep)
                {
                    tmpX = y;
                    tmpY = x;
                }
                else
                {
                    tmpX = x;
                    tmpY = y;
                }

                // check the point generated is legal
                // using passability check. creatures will leave shadows. should write a visibility
                // check later
                Coords currentCoords = new Coords(CoordsType.Tile, tmpX, tmpY);
                // for this to look good you must make sure it takes account of the eucledean distances over which the coeffcients hold
                // otherwise you get square FOVs.
                visibilityTotal *= this._visibilityMap[currentCoords.X, currentCoords.Y];

                if (
                    (visibilityTotal < Constants.VisibilityTreshold)
                    &
                    (!(currentCoords.Equals(p0original) | currentCoords.Equals(p1original)))
                    )
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region collision / bounds / etc -checkers

        public bool CheckInBounds(Coords c)
        {
            return (c.X >= 0) && (c.Y >= 0) && (c.X < this._xMax) && (c.Y < this._yMax);
        }

        /*
        /// <summary>
        /// Checks if tile is see-through or not.
        /// </summary>
        public bool CheckSightValidity(Coords point)
        {
            if (!(_myCollider.CheckInBounds(point)))
                return false;

            return _visibilityMap[point.X, point.Y] > 0;
        }
        */


        /*
        /// <summary>
        /// Checks if tile allows passage
        /// Some of this is redundant now that I have Tile.SeeAllowedMove. Should rethink.
        /// </summary>
        public bool CheckTilePassageValidity(Coords point)
        {
            if (!this.CheckSightValidity(point))
            {
                return false;
            }

            return _passabilityMap[point.X][point.Y];
        }
        */

        #endregion

        #endregion

        #region Constructors

        // Constructs an xSize by ySize map. Default Tile set to TileBasicFloor.
        public Map(RandomStuff randomator, UInt16 xSize, UInt16 ySize)
        {
            // creates and fills the tile array
            this._xMax = xSize;
            this._yMax = ySize;

            this._pixelMaxX = (UInt32)(xSize * Constants.TileSize);
            this._pixelMaxY = (UInt32)(ySize * Constants.TileSize);

            _tiles = new Tile[xSize, ySize];
            _visibilityMap = new float[xSize, ySize];

        }

        // Constructs a square map. Default Tile set to TileBasicFloor.
        public Map(RandomStuff randomator, UInt16 dimension)
            : this(randomator, dimension, dimension)
        {
        }
        #endregion
    }

    public class MapBattle : Map
    {
        private Creature[,] _tenancyMap;
        public Creature[,] TenancyMap
        {
            get
            {
                return _tenancyMap;
            }
            set
            {
                _tenancyMap = value;
            }
        }

        #region Item / Creature / Team spawners

        /// <summary>
        /// Spawns the player on the 'ground' at 'startPoint'
        /// returns a reference to the Player so one can more easily take care of references.
        /// </summary>
        /*
        public Creature SpawnPlayer(Coords startPoint)
        {
            //Player player = new Player(this, startPoint, this.IssueCreatureID());
            //this.PlayerReference = player;
            return null;
        }
        */
        public Creature CreateCreature(Coords startPoint, Team team, CreatureGenerator generator, Brain creatureBrain, DrawerBattle myDrawer, InterfaceBattle myInterface)
        {
            Creature newguy = new Creature(this, startPoint, (UInt16)this.IssueCreatureID(), team, generator, creatureBrain);
            team.MemberRegister(newguy);
            for (int i = 0; i < generator.SpellEndowment.Length; ++i)
            {
                CreateSpellForCreature(newguy, generator.SpellEndowment[i], myDrawer, myInterface);
            }
            return newguy;
        }

        private void CreateSpellForCreature(Creature agent, Spells type, DrawerBattle myDrawer, InterfaceBattle myInterface)
        {
            Spell newSpell = null;
            switch (type)
            {
                case Spells.SkillMelee:
                    newSpell = new SkillBasicAttackMelee(agent, myDrawer, myInterface, SpellTarget.Hex, 1,
                        Constants.APActionCostsStandard[(sbyte)APCostTypes.AttackMelee]);
                    break;
                case Spells.SkillRanged: // FIX RANGE
                    newSpell = new SKillBasicAttackRanged(agent, myDrawer, myInterface, SpellTarget.Hex, 1,
                        Constants.APActionCostsStandard[(sbyte)APCostTypes.AttackMelee]);
                    break;
            }

            agent.SpellAdd(newSpell);
        }

        public void CreateItem(Coords startPoint, ItemGenerator item)
        {

            //Coords bedLocation = new Coords((Int32)((bottomRight.X + topLeft.X) * 0.5), (Int32)((bottomRight.Y + topLeft.Y) * 0.5));
            if (TileIsPassable(startPoint))
            {
                Item newItem = new Item(this.IssueItemID(), item);
                this.CatalogueAddItemTo(newItem.ID, newItem);
                Tile itemTile = this.GetTile(startPoint);
                itemTile.InventoryAddItem(newItem);
            }
            else
            {
                throw new Exception("Unable to generate item on impassable terrain.");
            }
        }

        public void CreateItem(Creature itemOwner, ItemGenerator item)
        {
            Item newItem = new Item(this.IssueItemID(), item);
            this.CatalogueAddItemTo(newItem.ID, newItem);
            itemOwner.InventoryAddItem(newItem);
        }

        public void CreateTeam(Color teamColor)
        {
            Team newTeam = new Team(this, this.IssueTeamID(), teamColor);
        }

        #endregion

        public MapBattle(RandomStuff randomator, UInt16 xSize, UInt16 ySize)
            : base(randomator, xSize, ySize)
        {
            _tenancyMap = new Creature[xSize, ySize];
            for (Int32 i = 0; i < xSize; i++)
            {
                for (Int32 j = 0; j < ySize; j++)
                {
                    _tiles[i, j] = new Tile(this, new Coords(CoordsType.Tile, i, j), Constants.TileGeneratorGrass);
                }
            }

            this._myVisibilityTracker = new VisiblityTracker(xSize, ySize, _visibilityMap);

            // initializes the random number generator associated with this map
            this._randomator = randomator;
        }

        public MapBattle(RandomStuff randomator, UInt16 dimension)
            : this(randomator, dimension, dimension)
        {
        }
    }

    public class MapWorld : Map
    {
        public MapWorld(RandomStuff randomator, UInt16 xSize, UInt16 ySize)
            : base(randomator, xSize, ySize)
        {
        }

        public MapWorld(RandomStuff randomator, UInt16 dimension)
            : this(randomator, dimension, dimension)
        {
        }
    }
}