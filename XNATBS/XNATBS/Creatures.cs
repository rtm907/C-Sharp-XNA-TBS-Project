using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
//using System.Windows.Forms;

namespace XNATBS
{
    public abstract class Unit
    {
        // Creature unique ID
        protected UInt32 _uniqueID;
        public UInt32 UniqueID
        {
            get
            {
                return this._uniqueID;
            }
            set
            {
                this._uniqueID = value;
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

        protected MoveRangeCalculator _myMoveRangeCalculator;
        public MoveRangeCalculator MyMoveRangeCalculator
        {
            get
            {
                return _myMoveRangeCalculator;
            }
        }

        protected BitArray[] _fieldOfView;
        public BitArray[] FieldOfView
        {
            get
            {
                return _fieldOfView;
            }
        }

        protected Coords _positionTile;
        public virtual Coords PositionGet()
        {
            return _positionTile;
        }
        public virtual void PositionSet(Coords newValue)
        {
            _positionTile = newValue;
        }

        protected SpriteBatchCreature _myBitmap;
        public SpriteBatchCreature MyBitmap
        {
            get
            {
                return _myBitmap;
            }
            set
            {
                _myBitmap = value;
            }
        }

        protected Team _myTeam;
        public Team Team
        {
            get
            {
                return _myTeam;
            }
        }

        /*
        // Returns true if coords c are on the creature's FOV list.
        // There is some redundancy here, because both the tile and the creature store the
        // visibility info.
        public bool Sees(Coords c)
        {
            //return this._FOV.Contains(c);
            return this._fieldOfView[c.X][c.Y];
        }
        */

        protected Brain _creatureBrain;
        public Brain CreatureBrain
        {
            get
            {
                return _creatureBrain;
            }
            set
            {
                this._creatureBrain = value;
            }
        }

        public abstract UInt16 GetSightRange();

        protected Map _map;
        public Map MapGeneral
        {
            get
            {
                return _map;
            }
            set
            {
                _map = value;
            }
        }

        public void FieldOfViewUpdate()
        {
            //UInt16 range = _statsDerivative[(sbyte)StatDerivative.SightRange];
            UInt16 range = GetSightRange();
            //Map currentMap = this.InhabitedMap;

            if (range < 0)
            {
                return;
            }

            //REMOVE REDUNDANCY HERE
            BitArray[] update = new BitArray[_map.BoundX];
            for (int i = 0; i < _map.BoundX; ++i)
            {
                update[i] = new BitArray(_map.BoundY);
            }

            for (Int32 i = -range; i <= range; i++)
            {
                for (Int32 j = -range; j <= range; j++)
                {
                    Coords current = new Coords(CoordsType.Tile, _positionTile.X + i, _positionTile.Y + j);
                    if (
                        !this._map.CheckInBounds(current)
                        ||
                        (StaticMathFunctions.DistanceBetweenTwoCoordsHex(this._positionTile, current) > range)
                        )
                    {
                        continue;
                    }

                    bool val = _myVisibilityTracker.RayTracerVisibilityCheckTile(this._positionTile, current, true, range);

                    update[current.X][current.Y] = val;
                }
            }

            // determine values that were changed
            for (int i = 0; i < _map.BoundX; ++i)
            {
                update[i] = update[i].Xor(_fieldOfView[i]);
            }

            // update changes
            for (int i = 0; i < _map.BoundX; ++i)
            {
                for (int j = 0; j < _map.BoundY; ++j)
                {
                    if (update[i][j])
                    {
                        bool val = _fieldOfView[i][j];
                        _fieldOfView[i][j] = !val;
                        //_inhabitedMap.GetTile(i, j).VisibilityUpdate(this, !val);
                        _myVisibilityTracker.VisibilityUpdate(new Coords(CoordsType.Tile, i, j), this, !val);
                    }
                }
            }
        }

        // The label below the creature (creature name?)
        protected String _labelLower;
        public String LabelLower
        {
            get
            {
                return this._labelLower;
            }
            set
            {
                this._labelLower = value;
            }
        }

        // The label above the creature (for talking)
        protected String _labelUpper;
        public String LabelUpper
        {
            get
            {
                return this._labelUpper;
            }
            set
            {
                this._labelUpper = value;
            }
        }

        public Unit(Map myMap, Coords startPos, UInt16 ID, Team team, Brain creatureBrain)
        {
            this._myTeam = team;
            _map = myMap;
            
            this._uniqueID = ID;

        }

    }

    /// <summary>
    /// Creatures interface.
    /// Stores the creatures ID and Coords, its brain, map and tile references, its sight range,
    /// and other relevant data.
    /// </summary>
    public class Creature : Unit
    {
        #region Properties

        private String _name;
        public String Name
        {
            get
            {
                return _name;
            }
        }

        // Reference to the Map the creature lives in. 
        protected MapBattle _inhabitedMap;
        public MapBattle MapBattlemap
        {
            get
            {
                return this._inhabitedMap;
            }
            set
            {
                this._inhabitedMap = value;
            }
        }

        public override Coords PositionGet()
        {
            return _positionTile;
        }
        public override void PositionSet(Coords value)
        {
                if (value != _positionTile)
                {
                    this._inhabitedMap.TenancyMap[_positionTile.X, _positionTile.Y] = null;
                    //this._inhabitedMap.VacancyMap[_positionTile.X][_positionTile.Y] = true;
                    _myVisibilityTracker.VisibilityResidentRemove(_positionTile, this);
                    _myVisibilityTracker.VisibilityResidentAdd(value, this);
                    this._positionTile = value;
                    this._inhabitedMap.TenancyMap[value.X, value.Y] = this;
                    //this._inhabitedMap.VacancyMap[value.X][value.Y] = false;
                    this.FieldOfViewUpdate();
                }
        }

        #region Inventory

        private Inventory _myInventoryBackpack;
        public Inventory InventoryBackpack
        {
            get
            {
                return _myInventoryBackpack;
            }
        }
        private Inventory _myInventoryEquipped;
        public Inventory InventoryEquipped
        {
            get
            {
                return _myInventoryEquipped;
            }
        }

        /// <summary>
        /// Adds item to general inventory at given index.
        /// </summary>
        public void InventoryAddItem(sbyte index, Item toAdd)
        {
            _myInventoryBackpack.ItemAddToList(index, toAdd);
        }

        /// <summary>
        /// Adds item to general inventory.
        /// </summary>
        public void InventoryAddItem(Item toAdd)
        {
            _myInventoryBackpack.ItemAddToList(toAdd);
        }

        /// <summary>
        ///  Adds item to specified inventory.
        /// </summary>
        public void InventoryAddItem(Item toAdd, InventoryType type)
        {
            if (_myInventoryEquipped.GetItem((sbyte)type) != null)
            {
                _myInventoryEquipped.ItemArray[(sbyte)type] = toAdd;
            }
        }

        /// <summary>
        /// Removes item from the general inventory.
        /// </summary>
        public void InventoryRemoveItem(Item toRemove)
        {
            _myInventoryBackpack.ItemRemoveFromList(toRemove);
        }

        /// <summary>
        ///  Removes item from specified inventory.
        /// </summary>
        public void InventoryRemoveItem(Item toRemove, InventoryType type)
        {
            _myInventoryEquipped.ItemRemoveFromList(toRemove);
        }

        #endregion

        #region Spells / Skills

        private UInt16[] _APMoveCosts;
        public UInt16[] APMoveCosts
        {
            get
            {
                return _APMoveCosts;
            }
        }
        public UInt16 GetAPMoveCost(TerrainType terrain)
        {
            return _APMoveCosts[(sbyte)terrain];
        }

        private UInt16[] _APActionCosts;
        public UInt16 GetAPActionCost(APCostTypes action)
        {
            return _APActionCosts[(sbyte)action];
        }

        private UInt16 _skillPoints;
        public UInt16 SkillPoints
        {
            get
            {
                return _skillPoints;
            }
        }
        public void SkillPointsSubstract(UInt16 i)
        {
            if (_skillPoints >= i)
            {
                _skillPoints -= i;
            }
        }
        public void SkillPointsAdd(UInt16 i)
        {
            _skillPoints += i;
        }

        #endregion

        private UInt16 _level;
        public UInt16 Level
        {
            get
            {
                return _level;
            }
        }

        #region stats

        /// <summary>
        /// Basic creature stats.
        /// </summary>
        public enum StatBasic : sbyte
        {
            HP = 0,
            AP,
            XP,
            COUNT
        }

        /// <summary>
        /// Main creature stats.
        /// </summary>
        public enum StatMain : sbyte
        {
            Strength = 0,
            Agility,
            Sensitivity,
            Voice,
            COUNT
        }

        public enum StatDerivative : sbyte
        {
            SightRange = 0,
            COUNT
        }

        private UInt16[,] _statsBasic;
        /// <summary>
        /// Returns the value of the basic stat of the type requested.
        /// If 'current' is true, returns current value; otherwise returns max value.
        /// </summary>
        public UInt16 GetStatBasic(StatBasic statType, bool current)
        {
            return _statsBasic[(sbyte)statType,current ? 0 : 1];
        }
        public void SetStatBasic(StatBasic statType, bool current, UInt16 value)
        {
            _statsBasic[(sbyte)statType,current ? 0 : 1] = value;
        }
        public void AddToStatBasic(StatBasic statType, Int32 delta)
        {
            //StatAdditionOutcome returnValue = StatAdditionOutcome.Unremarkable;
            UInt16 statValue = _statsBasic[(sbyte)statType,0];
            if (delta < 0 && statValue <= Math.Abs(delta))
            {
                _statsBasic[(sbyte)statType,0] = 0;
                // if stat is HP, creature dies
                if (statType == StatBasic.HP)
                {
                    this.Death();
                }
            }
            else if ((statValue + delta) >= _statsBasic[(sbyte)statType,1])
            {
                _statsBasic[(sbyte)statType,0] = _statsBasic[(sbyte)statType,1];
                // if stat is XP, level up
                if (statType == StatBasic.XP)
                {
                    this.LevelUp();
                }
            }
            else
            {
                _statsBasic[(sbyte)statType,0] = (UInt16)(statValue + delta);
            }

            if (statType == StatBasic.AP)
            {
                // Update unit move-range
                _myMoveRangeCalculator.Update();
            }



            //return returnValue;
        }

        public UInt16 GetHP()
        {
            return GetStatBasic(StatBasic.HP, true);
        }
        public UInt16 GetHPMax()
        {
            return GetStatBasic(StatBasic.HP, false);
        }
        public UInt16 GetAP()
        {
            return GetStatBasic(StatBasic.AP, true);
        }
        public override UInt16 GetSightRange()
        {
            return GetStatDerivative(StatDerivative.SightRange);
        }

        private UInt16[,] _statsMain;
        /// <summary>
        /// Returns the value of the basic stat of the type requested.
        /// If 'current' is true, returns current value; otherwise returns base value.
        /// </summary>
        public UInt16 GetStatMain(StatMain statType, bool current)
        {
            return _statsMain[(sbyte)statType,current ? 1 : 0];
        }
        /// <summary>
        /// Sets main stat to give value.
        /// If 'current' is true, takes current value of the stat, otherwise takes base value.
        /// </summary>
        public void SetStatMain(StatMain statType, bool current, UInt16 value)
        {
            _statsMain[(sbyte)statType,current ? 1 : 0] = value;
        }

        private UInt16[] _statsDerivative;
        /// <summary>
        /// Returns the derivative stat of the requested type.
        /// </summary>
        public UInt16 GetStatDerivative(StatDerivative statType)
        {
            return _statsDerivative[(sbyte)statType];
        }
        public void SetStatDerivative(StatDerivative statType, UInt16 value)
        {
            _statsDerivative[(sbyte)statType] = value;
        }

        #endregion

        #region Spells and Skills

        private List<Spell> _spells=new List<Spell>();
        public List<Spell> Spells
        {
            get
            {
                return _spells;
            }
        }

        public void SpellAdd(Spell newSpell)
        {
            _spells.Add(newSpell);
        }
        
        private UInt16 _spellCurrent;
        public Spell SpellCurrent
        {
            get
            {
                if (_spells.Count > 0)
                {
                    return _spells[_spellCurrent];
                }
                return null;
            }
        }

        public void SpellSelectNext()
        {
            _spellCurrent = (UInt16)((_spellCurrent + 1) % _spells.Count);
        }
        public void SpellSelectPrevious()
        {
            _spellCurrent = (UInt16)((_spellCurrent + _spells.Count - 1) % _spells.Count);
        }
        public Spell SpellAtIndex(int i)
        {
            if (_spells.Count > i)
            {
                return _spells[i];
            }
            return null;
        }

        #endregion

        private bool _dead;
        public bool Dead
        {
            get
            {
                return _dead;
            }
            set
            {
                _dead = value;
            }
        }

        private List<Effect> _effects = new List<Effect>();
        public void EffectRegister(Effect newEffect)
        {
            _effects.Add(newEffect);
        }
        public void EffectRemove(Effect oldEffect)
        {
            _effects.Remove(oldEffect);
        }

        #endregion

        #region Methods

        public void UpdateEffects()
        {
            for (int i = 0; i < _effects.Count; ++i)
            {
                Effect current = _effects[i];
                current.Update();
                if (current.Expired())
                {
                    _effects.RemoveAt(i);
                    --i;
                }
            }
        }

        public Int32 CompareTo(object obj)
        {
            if (!(obj is Creature))
            {
                throw new Exception("Bad Creature comparison.");
            }

            Creature compared = (Creature)obj;

            return (Int32)(this._uniqueID - compared._uniqueID);
        }

        public void LevelUp()
        {
            // fix various variables
            ++_level;
            this.SetStatBasic(StatBasic.XP, true, 0);
            this.SetStatBasic(StatBasic.XP, false, (UInt16) (GetStatBasic(StatBasic.XP, false) + Constants.AdditionalXPNeededPerLevelUp));

            ++_skillPoints;
        }

        public override bool Equals(object obj)
        {
            return (obj is Creature) && this == (Creature)obj;
        }

        public override Int32 GetHashCode()
        {
            return (Int32)_uniqueID;
        }

        public UInt16 GetAttackRange()
        {
            Item weapon = this._myInventoryEquipped.GetItem((sbyte)InventoryType.HandWeapon);
            // = weaponSlot.Size() > 0 ? weaponSlot.ItemList[0] : null;
            return weapon == null ? (UInt16)1 : (UInt16) weapon.ItemFunctions[ItemProperty.Range];
        }

        public float GetAttackDamage()
        {
            Item weapon = this._myInventoryEquipped.GetItem((sbyte)InventoryType.HandWeapon);
            //Item weapon = weaponSlot.Size() > 0 ? weaponSlot.ItemList[0] : null;
            return weapon == null ? GetStatMain(StatMain.Strength, true) / 4 : weapon.ItemFunctions[ItemProperty.Damage] + GetStatMain(StatMain.Strength, true) / 4;
        }

        public float GetArmor()
        {
            Item shield = this._myInventoryEquipped.GetItem((sbyte)InventoryType.HandShield);
            //Item shield = shieldSlot.Size() > 0 ? shieldSlot.ItemList[0] : null;
            return shield == null ? GetStatMain(StatMain.Agility, true) / 4 : shield.ItemFunctions[ItemProperty.Protection] + GetStatMain(StatMain.Agility, true) / 4;
        }

        public Item ItemPick()
        {
            Tile currentTile = _inhabitedMap.GetTile(this._positionTile);
            if (currentTile.MyInventory.Size() > 0 && !_myInventoryBackpack.Full())
            {
                Item newAcquisition = currentTile.MyInventory.GetItem(0);
                currentTile.InventoryRemoveItem(newAcquisition);
                this.InventoryAddItem(newAcquisition);
                return newAcquisition;
            }
            return null;
        }

        /// <summary>
        /// Creature processes incurred hit.
        /// </summary>
        /// <param name="attackerID">ID of the attacker</param>
        /// <param name="struckMember">Struck body part</param>
        /// <param name="hitMagnitude">Hit magnitude</param>
        /*
        public void HitIncurred(Creature attacker, UInt16 hitMagnitude)
        {
            // elementary combat model
            this._statHP = (UInt16) Math.Max(0, _statHP - (hitMagnitude - _statArmor));

            if (_statHP == 0)
            {
                this._dead = true;
            }
        }
        */

        /// <summary>
        /// Creature death clean-up.
        /// </summary>
        public void Death()
        {
            // remove self from current tile and menagerie
            this._myVisibilityTracker.DeregisterCreature(this);
            this._inhabitedMap.TenancyMap[_positionTile.X, _positionTile.Y] = null;
            //this._inhabitedMap.VacancyMap[_positionTile.X][_positionTile.Y] = true;
            this._inhabitedMap.MortuaryAddCreatureTo(this._uniqueID, this);
            this._inhabitedMap.MenagerieDeleteCreatureFrom(this._uniqueID);
            this._myTeam.MemberRemove(this);
            // clear brain/ memory
            this._creatureBrain = null;
            this._dead = true;
        }

        public override string ToString()
        {
            return this._name;
        }

        #endregion

        #region Constructors

        private delegate UInt16 StatFormula(UInt16 mainStat);

        private void GenerateStats(UInt16[] means, UInt16[] deviations, StatFormula HPformula, StatFormula APformula)
        {
            RandomStuff randomator = _inhabitedMap.Randomator;

            _statsMain = new UInt16[(sbyte)StatMain.COUNT, 2];
            _statsBasic = new UInt16[(sbyte)StatBasic.COUNT, 2];
            _statsDerivative = new UInt16[(sbyte)StatDerivative.COUNT];

            for (int i = 0; i < (sbyte)StatMain.COUNT; ++i)
            {
                UInt16 statValue = (UInt16)randomator.DiscreteNormalDistributionSample(means[i], deviations[i], 1, Constants.StatMax);
                _statsMain[i, 0] = statValue; //base
                _statsMain[i, 1] = statValue; //current
            }

            // Create either methods or delegates support for the evaluation of secondary stats.

            // HP init
            //_statsBasic[(sbyte)StatBasic.HP, 1] = (UInt16)(GetStatMain(StatMain.Strength, false) * 4);
            _statsBasic[(sbyte)StatBasic.HP, 1] = HPformula(GetStatMain(StatMain.Strength, false));
            _statsBasic[(sbyte)StatBasic.HP, 0] = _statsBasic[(sbyte)StatBasic.HP, 1]; //current HP = max HP

            _statsBasic[(sbyte)StatBasic.AP, 1] = APformula(GetStatMain(StatMain.Agility, false));
            _statsBasic[(sbyte)StatBasic.AP, 0] = _statsBasic[(sbyte)StatBasic.AP, 1]; //current AP = max AP

            _statsBasic[(sbyte)StatBasic.XP, 1] = Constants.AdditionalXPNeededPerLevelUp;
            // Sight init
            _statsDerivative[(sbyte)StatDerivative.SightRange] = (UInt16)(10 + GetStatMain(StatMain.Sensitivity, false) / 2);

        }

        private void GenerateStats(UInt16[] means, UInt16[] deviations)
        {
            GenerateStats(means, deviations, StaticMathFunctions.StandardFormulaHP, StaticMathFunctions.StandardFormulaAP);
        }

        public Creature(MapBattle currentMap, Coords startPos, UInt16 ID, Team team, CreatureGenerator generator, Brain creatureBrain)
            : base(currentMap, startPos, ID, team, creatureBrain)
        {
            this._name = generator.name;


            this._myInventoryBackpack = new Inventory(this, Constants.InventorySize);
            this._myInventoryEquipped = new Inventory(this, (sbyte)InventoryType.COUNT);
            this._myBitmap = generator.creatureBitmaps;

            this._inhabitedMap = currentMap;
            this._inhabitedMap.MenagerieAddCreatureTo(ID, this);

            this._myVisibilityTracker = _inhabitedMap.MyVisibilityTracker;
            //this._myPathfinder = _inhabitedMap.MyPathfinder;

            this._creatureBrain = creatureBrain;
            _creatureBrain.MyCreature = this;

            this._fieldOfView = new BitArray[currentMap.BoundX];
            for (int i = 0; i < currentMap.BoundX; ++i)
            {
                _fieldOfView[i] = new BitArray(currentMap.BoundY);
            }

            GenerateStats(generator.MainStatsMeans, generator.MainStatsDeviations);

            _APMoveCosts = generator.APMoveCosts;
            _APActionCosts = generator.APActionCosts;

            this._positionTile = startPos;
            this._inhabitedMap.TenancyMap[startPos.X, startPos.Y] = this;
            _myVisibilityTracker.VisibilityResidentAdd(startPos, this);
            //this._inhabitedMap.VacancyMap[startPos.X][startPos.Y] = false;

            this._myMoveRangeCalculator = new MoveRangeCalculator(this);
            _myMoveRangeCalculator.Update();
            this.FieldOfViewUpdate();
            //this._inhabitedMap.MyCollider.RegisterCreature(this);
        }

        #endregion

    }

    public class Band : Unit
    {
        private List<Creature> _members = new List<Creature>();
        public List<Creature> Members
        {
            get
            {
                return _members;
            }
        }
        public override ushort GetSightRange()
        {
            return Constants.DefaultBandSightRange;
        }

        public Band(Map myMap, Coords startPos, UInt16 ID, Team team, Brain brain, List<Creature> members, SpriteBatchCreature myBitmap)
            : base(myMap, startPos, ID, team, brain)
        {
            _members = members;
            _myBitmap = myBitmap;
        }
    }



    public enum EffectType : sbyte
    {
        ChangeStatBasic = 0
    }

    // Skill/spell (and not only) effect class.
    public abstract class Effect
    {
        private Int32 _durationMax;
        public Int32 DurationMax
        {
            get
            {
                return _durationMax;
            }
        }
        private Int32 _durationElapsed;
        public Int32 DurationElapsed
        {
            get
            {
                return _durationElapsed;
            }
        }

        protected Creature _author;

        public void IncremementDuration()
        {
            ++_durationElapsed;
        }
        public bool Expired()
        {
            return _durationElapsed >= _durationMax;
        }

        protected abstract void Initialize();

        public abstract void Update();

        public Effect(Creature author, Int32 duration)
        {
            _author = author;
            _durationElapsed = 0;
            _durationMax = duration;
        }

    }

    public class EffectChangeStatBasic : Effect
    {
        private Creature _target;
        private Creature.StatBasic _stat;
        private Int32 _changePerTurn;

        private void CheckEnemyVanquished()
        {
            if (_target.Dead)
            {
                // target vanquished; apply XP gain to effect author (if appropriate).
                _author.AddToStatBasic(Creature.StatBasic.XP, StaticMathFunctions.XPFormula(_target));
            }
        }

        protected override void Initialize()
        {
            _target.AddToStatBasic(_stat, _changePerTurn);
            CheckEnemyVanquished();
        }
        public override void Update()
        {
            _target.AddToStatBasic(_stat, _changePerTurn);
            CheckEnemyVanquished();
            IncremementDuration();
        }

        public EffectChangeStatBasic(Creature author, Int32 duration, Creature target, Creature.StatBasic stat, Int32 changePerTurn)
            : base(author, duration)
        {
            _target = target;
            _stat = stat;
            _changePerTurn = changePerTurn;
            Initialize();
        }
    }
}
