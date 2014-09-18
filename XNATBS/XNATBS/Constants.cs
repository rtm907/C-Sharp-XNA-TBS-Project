using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNATBS
{
    // This class contains the global constants
    public static class Constants
    {
        // VERSION: v0.005

        #region Game - related

        public static Int32 defaultMapRandomatorSeed = 907;
        public static bool RecordInput = false;

        // Moving in a diagonal direction takes root(2) time (with movement left/right/top/bottom
        // taking one unit time).

        // This is so I don't have to deal with expanding visibility arrays in the Tile class. FIX later.
        public const UInt32 MaximumNumberOfCreatures = 256;

        #endregion

        #region Drawing - related

        public const ScrollingType Scrolling = ScrollingType.Free;

        public static readonly bool ShowGrid = true;
        public static readonly bool ShowCoordinates = false;
        //public const bool ShowMap = false;
        //public const bool ShowBoundingCircles = true;
        public static readonly bool ZoomingAllowed = true;

        public const int FreeScrollingSpeed = 20;

        public const float ZoomSpeed = 0.25f; // use 2^(-10) in hexadecimal, or something
        public const float ZoomMin = 0.5f;
        public const float ZoomMax = 2f;

        //public static readonly Color BoundingCircleColor = Color.Red;
        //public static readonly Color SelectionBoxColor = Color.White;
        public static readonly Color ColorHalfTransparent = new Color(255,255,255,128);
        public static readonly Color ColorItemName = Color.Blue;
        public static readonly Color ColorItemProperties = Color.DarkBlue;
        public static readonly Color ColorCharacterName = Color.Blue;
        public static readonly Color ColorCharacterProperties = Color.BlueViolet;
        public static readonly Color ColorSpellInfo = Color.Green;

        public const UInt16 FloatingTextDefaultTimer = 100;
        public const UInt16 FloatingTextDefaultSpeed = 1;

        public const UInt16 ButtonDefaultWidth = 32;
        public const UInt16 ButtonDefaultHeight = 64;

        public const UInt16 ButtonSpellPixelSize = 72;

        public const Int32 AnimCreatureMoveBaseDuration = 10;
        public const Int32 AnimWeaponSlashBaseDuration = 20;
        public const Int32 AnimProjectileArrowBaseSpeed = 5;

        public const Int32 FontDefaultSize = 16;
        public const Int32 TextDisplayDefaultVerticalSpacing = 8;
        public const Int32 TextDisplayItemBoxDefaultWidth = 300;
        public const Int32 TextDisplayItemBoxDefaultXOffset = 5;

        #endregion

        #region Map - related

        //the size of the smallest grid member in pixels. must be a divisor of _tileSize.
        public const UInt16 TileSize = 72;

        public const float ZoomDefault = 1f;

        // Default map size
        public const UInt16 MapSize = 16;

        // Max distance threshold for recursive influence generation algorithm (InfluenceSourceMap).
        public const UInt32 InfluenceMapMaxDistance = 20;
        public static readonly float InfluenceMapMinThreshold = (float)Math.Pow(10, (-5));
        public const float VisibilityTreshold = 0.1f;
 
        #endregion

        #region Tile - related
        // float givenPassability, String givenName, float givenVisibilityCoefficient, SpriteTile givenBitmap

        public static readonly TileGenerator TileGeneratorGrass = new TileGenerator("Grass", SpriteTile.TileGrass, TerrainType.Grass,1f);
        public static readonly TileGenerator TileGeneratorWallStone = new TileGenerator("Stone Wall", SpriteTile.TileWall, TerrainType.Wall,0f);
        public static readonly TileGenerator TileGeneratorForest = new TileGenerator("Forest", SpriteTile.TileForest, TerrainType.Forest, 3f);
        public static readonly TileGenerator TileGeneratorSwamp = new TileGenerator("Swamp", SpriteTile.TileSwamp, TerrainType.Swamp, 1.5f);
        //public static readonly TileGenerator TileGeneratorFloorDirt = new TileGenerator(true, "Dirt Floor", 1f, SpriteTile.TileFloorDirt);
        //public static readonly TileGenerator TileGeneratorRoadPaved = new TileGenerator(true, "Paved Road", 1f, SpriteTile.TileRoadPaved);

        #endregion

        #region Item - related

        public static readonly ItemGenerator ItemGeneratorSword = new ItemGenerator("Sword", SpriteItem.ItemSword, ItemType.Weapon,
            new Dictionary<ItemProperty, float>() { { ItemProperty.Damage, 10f }, {ItemProperty.Range, 1f} });

        public static readonly ItemGenerator ItemGeneratorShield = new ItemGenerator("Shield", SpriteItem.ItemShield, ItemType.Armor,
            new Dictionary<ItemProperty, float>() { { ItemProperty.Protection, 1f } });

        public static readonly ItemGenerator ItemGeneratorBow = new ItemGenerator("Bow", SpriteItem.ItemBow, ItemType.Weapon,
            new Dictionary<ItemProperty, float>() { { ItemProperty.Damage, 2f }, { ItemProperty.Range, 5f } });

        public const sbyte InventoryRowSize = 5;
        public const sbyte InventoryRows = 2;
        public const sbyte InventorySize = InventoryRowSize * InventoryRows;
        public const sbyte InventorySlotSpriteSize = 72;

        #endregion

        #region Creature - related

        // Check the respective Enums to see what the numbers represent.
        public static readonly UInt16[] APMoveCostsStandard = new UInt16[(sbyte)TerrainType.COUNT] { 2, 0, 6, 7 };
        public static readonly UInt16[] APActionCostsStandard = new UInt16[(sbyte)APCostTypes.COUNT] { 3, 1, 1 };

        // String givenName, SpriteBatchCreature givenCreatureBitmaps, strength, agility, sensitivity, voice
        public static readonly CreatureGenerator
            CreatureGeneratorHero = new CreatureGenerator("Hero", SpriteBatchCreature.CreatureHero, new UInt16[] { 15, 15, 15, 15 }, new UInt16[] { 4, 4, 4, 4 },
                APMoveCostsStandard, APActionCostsStandard, new Spells[] { Spells.SkillMelee, Spells.SkillRanged });

        public static readonly CreatureGenerator
            CreatureGeneratorGoblin = new CreatureGenerator("Goblin", SpriteBatchCreature.CreatureGoblin, new UInt16[] { 3, 5, 15, 3 }, new UInt16[] { 2, 2, 1, 1 },
                APMoveCostsStandard, APActionCostsStandard, new Spells[] { Spells.SkillMelee });

        public static readonly UInt16[] APCosts = new UInt16[(sbyte)APCostTypes.COUNT] { 4, 1, 1 };

        public static readonly UInt16 StatMax = 50;
        public static readonly UInt16 AdditionalXPNeededPerLevelUp = 100;

        public static readonly sbyte MaximumNumberAllowedSpells = 16;

        public static readonly UInt16 DefaultBandSightRange = 3;

        #endregion

        #region Spells and Skills

        public static readonly String[] SpellDescriptions =
        {
            "Basic melee attack.",
            "Basic ranged attack."
        };

        #endregion

        #region AI - related

        public const float PathfinderStraightPathCorrection = 0f;

        #endregion

        #region Strings

        public const Int32 MaxLineLength = 80;

        //public const Int32 FontSize = 12;

        public static readonly String[] QuipsGreetings = 
        {"Yo, man!",
            "Hello!",
            "How's it going.",
            "Hey.",
            "*nods*"
        };

        public static readonly String[] GnomeNamebits = 
        {"ka", "kri", "kyu", "khe", "ko",
            "sam", "sir", "suk", "sech", "soj",
            "bik", "trom", "shrok", "jem", "kop"
        };

        #endregion
    }
}
