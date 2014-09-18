using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNATBS
{
    public class MapGenerator
    {
        private RandomStuff _randomator;
        private InterfaceBattle _interface;
        private DrawerBattle _drawer;

        public MapBattle GenerateBasicGrasslands(UInt16 dimensionX, UInt16 dimensionY)
        {
            MapBattle newMap = new MapBattle(_randomator, dimensionX, dimensionY);
            LinkMapToDrawerAndInterface(newMap);

            for (int i = 0; i < dimensionX; ++i)
            {
                for (int j = 0; j < dimensionY; ++j)
                {
                    // Move the constant parameters to constants
                    UInt32 dicethrow = _randomator.NSidedDice(20, 1);
                    Coords currentCoords = new Coords(i,j);

                    if (dicethrow == 1)
                    {
                        newMap.SetTile(currentCoords, new Tile(newMap, currentCoords, Constants.TileGeneratorSwamp));
                    }
                    else if (dicethrow <= 2)
                    {
                        newMap.SetTile(currentCoords, new Tile(newMap, currentCoords, Constants.TileGeneratorWallStone));
                    }
                    else if (dicethrow <= 5)
                    {
                        newMap.SetTile(currentCoords, new Tile(newMap, currentCoords, Constants.TileGeneratorForest));
                    }
                    // the rest is grass by default. throw items and monsters onto it.
                }
            }

            newMap.AnalyzeTileAccessibility();

            PopulateMapWithItems(newMap);
            PopulateMapWithMonsters(newMap);

            return newMap;
        }

        private void LinkMapToDrawerAndInterface(MapBattle map)
        {
            _interface.SetCurrentMapBattle(map);
            _drawer.SetCurrentMap(map);
        }

        public void PopulateMapWithItems(MapBattle map)
        {
            for (int i = 0; i < map.BoundX; ++i)
            {
                for (int j = 0; j < map.BoundY; ++j)
                {
                    UInt32 dicethrow = _randomator.NSidedDice(200, 1);
                    if (dicethrow == 19 && map.TileIsPassable(new Coords(i,j)))
                    {
                        map.CreateItem(new Coords(i, j), Constants.ItemGeneratorShield);
                    }
                }
            }
        }

        public void PopulateMapWithMonsters(MapBattle map)
        {
            map.CreateTeam(Color.Red);
            map.CreateTeam(Color.Blue); //team 1. fix this later.

            for (int i = 0; i < map.BoundX; ++i)
            {
                for (int j = 0; j < map.BoundY; ++j)
                {
                    UInt32 dicethrow = _randomator.NSidedDice(50, 1);
                    if (dicethrow == 20 && map.TileIsPassable(new Coords(i, j)))
                    {
                        Creature goblin = map.CreateCreature(new Coords(CoordsType.Tile, i, j), map.TeamRosterGetTeamFrom(1),
                            Constants.CreatureGeneratorGoblin, new BrainBasicAI(_drawer),_drawer, _interface);
                        //map.CreateSpellForCreature(goblin, Spells.SkillMelee, _drawer, _interface);
                    }
                }
            }
        }

        public MapGenerator(RandomStuff randomator, InterfaceBattle gameInterface, DrawerBattle drawer)
        {
            _randomator = randomator;
            _interface = gameInterface;
            _drawer = drawer;
        }
    }
}
