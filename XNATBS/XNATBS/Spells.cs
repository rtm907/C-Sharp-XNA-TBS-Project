using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace XNATBS
{
    public enum SpellTarget : sbyte
    {
        Self = 0,
        ExpandingFromSelf,
        Creature,
        Hex,
        AreaCircular,
        AreaColumn
    }

    public abstract class Spell
    {
        protected Creature _agent;
        protected MapBattle _map;
        protected DrawerBattle _drawer; //link to drawer for passing of animations
        protected InterfaceBattle _interface;

        protected Spells _type;
        public Spells Type
        {
            get
            {
                return _type;
            }
        }

        //private Coords? _target;

        protected SpellTarget _targetType;
        //protected List<Effect> _spellEffects;

        protected UInt16 _range; //where applicable
        public UInt16 Range
        {
            get
            {
                return _range;
            }
        }
        protected UInt16 _magnitude; //where applicable
        public UInt16 Magnitude
        {
            get
            {
                return _magnitude;
            }
        }
        public void IncreaseMagnitude()
        {
            ++_magnitude;
        }
        protected UInt16 _executionTime;
        public UInt16 ExecutionTime
        {
            get
            {
                return _executionTime;
            }
        }

        /// <summary>
        /// Try to use skill. If successful, return 'true'.
        /// </summary>
        public abstract bool Execute(Coords? _target);

        public Spell(Spells type, Creature agent, DrawerBattle drawer, InterfaceBattle myInterface, 
        SpellTarget targetType, UInt16 range, UInt16 magnitude, UInt16 executionTime)
        {
            _type = type;
            _agent = agent;
            _map = _agent.MapBattlemap;
            _drawer = drawer;
            _interface = myInterface;
            _targetType = targetType;
            //_spellEffects = spellEffects;
            _range = range;
            _magnitude = magnitude;
            _executionTime = executionTime;
        }
    }

    public class SkillBasicAttackMelee : Spell
    {
        public override bool Execute(Coords? _target)
        {
            if (_target == null)
            {
                throw new Exception("Basic Attack no target hex passed.");
            }

            int distance = StaticMathFunctions.DistanceBetweenTwoCoordsHex(_agent.PositionGet(), _target.Value);
            if (distance > 1)
            {
                // target not in range; do nothing
                return false;
            }

            Creature quarry = _map.TenancyMap[_target.Value.X, _target.Value.Y];
            if (quarry == null)
            {
                // no one to hit: do nothing.
                return false;
            }

            Item weapon = _agent.InventoryEquipped.GetItem((sbyte)InventoryType.HandWeapon);
            if (weapon != null && (weapon.MyType != ItemType.Weapon || weapon.ItemFunctions[ItemProperty.Range] > 1))
            {
                // weapon not of proper type; do nothing.
                return false;
            }

            // determine damage
            int damage = (UInt16)Math.Max(_agent.GetAttackDamage() - quarry.GetArmor(), 1);
            // substract HP
            //quarry.AddToStatBasic(Creature.StatBasic.HP, -damage);
            quarry.EffectRegister(new EffectChangeStatBasic(_agent, 0, quarry, Creature.StatBasic.HP, -damage));

            // Check if target is dead.
            //if (quarry.Dead)
            //{
            //    _agent.AddToStatBasic(Creature.StatBasic.XP, StaticMathFunctions.XPFormula(quarry));
            //}

            // NOTE: AP substraction done at ActionUseSpell level.

            // Melee slash anim
            if (weapon != null)
            {
                _drawer.Animations.Add(new AnimWeaponSlash(Constants.AnimWeaponSlashBaseDuration, _drawer.Items[(sbyte)weapon.ItemBitmap],
                    _interface.HexPosition(_target.Value) + new Vector2(Constants.TileSize / 2, Constants.TileSize / 2)));
            }

            // Floating message addition
            _drawer.FloatingMessages.Add(new FloatingMessage(Constants.FloatingTextDefaultTimer, "-" + damage.ToString() + "HP",
                _interface.HexPosition(_target.Value) + new Vector2(Constants.TileSize / 2, Constants.TileSize / 2)));

            return true;
        }

        public SkillBasicAttackMelee(Creature agent, DrawerBattle drawer, InterfaceBattle myInterface, 
            SpellTarget targetType, UInt16 magnitude, UInt16 executionTime)
            : base(Spells.SkillMelee, agent, drawer, myInterface, targetType, 1, magnitude, executionTime)
        {

        }
    }

    public class SKillBasicAttackRanged : Spell
    {        
        public override bool Execute(Coords? _target)
        {
            if (_target == null)
            {
                throw new Exception("Basic Attack no target hex passed.");
            }

            _range = _agent.GetAttackRange();
            int distance = StaticMathFunctions.DistanceBetweenTwoCoordsHex(_agent.PositionGet(), _target.Value);
            if (distance > _range)
            {
                // target not in range; do nothing
                return false;
            }

            Creature quarry = _map.TenancyMap[_target.Value.X, _target.Value.Y];
            if (quarry == null)
            {
                // no one to hit: do nothing.
                return false;
            }

            Item weapon = _agent.InventoryEquipped.GetItem((sbyte)InventoryType.HandWeapon);
            if (weapon == null || (weapon.ItemFunctions[ItemProperty.Range] <= 1))
            {
                // weapon not of proper type; do nothing.
                return false;
            }

            // determine damage
            int damage = (UInt16)Math.Max(_agent.GetAttackDamage() - quarry.GetArmor(), 1);
            // substract HP
//            quarry.AddToStatBasic(Creature.StatBasic.HP, -damage);
            quarry.EffectRegister(new EffectChangeStatBasic(_agent, 0, quarry, Creature.StatBasic.HP, -damage));
 
            // NOTE: AP substraction done at ActionUseSpell level.

            // Check if target is dead.
            //if (quarry.Dead)
            //{
            //    _agent.AddToStatBasic(Creature.StatBasic.XP, StaticMathFunctions.XPFormula(quarry));
            //}

            // Ranged attack anim
            // for now use arrow by default; later associate particle sprites to the various ranged weapons (slings?)
            _drawer.Animations.Add(new AnimProjectile(Constants.AnimProjectileArrowBaseSpeed * distance,
                _drawer.Particles[(sbyte)SpriteParticle.ParticleArrow],
                _interface.HexPosition(_agent.PositionGet()) + new Vector2(Constants.TileSize / 2, Constants.TileSize / 2),
                _interface.HexPosition(_target.Value) + new Vector2(Constants.TileSize / 2, Constants.TileSize / 2)));

            // Floating message addition
            _drawer.FloatingMessages.Add(new FloatingMessage(Constants.FloatingTextDefaultTimer, "-" + damage.ToString() + "HP",
                _interface.HexPosition(_target.Value) + new Vector2(Constants.TileSize / 2, Constants.TileSize / 2)));

            return true;
        }

        public SKillBasicAttackRanged(Creature agent, DrawerBattle drawer, InterfaceBattle myInterface, 
            SpellTarget targetType, UInt16 magnitude, UInt16 executionTime)
            : base(Spells.SkillRanged, agent, drawer, myInterface, targetType, agent.GetAttackRange(), magnitude, executionTime)
        {

        }
    }
}
