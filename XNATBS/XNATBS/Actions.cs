using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNATBS
{

    /// <summary>
    /// This class creates Action objects that the creatures can own to 'remember' and queue.
    /// These are generally things that are related to the real-time logic of the game.
    /// </summary>
    public abstract class Action
    {
        // Reference to creature who owns this Action
        private Unit _actor;
        // Read-only. Changing an action's owner makes no sense. Maybe should be fully private?
        public Unit Actor
        {
            get
            {
                return _actor;
            }
        }

        // Update method. Executed every tick while action is active.
        // Returns true if Action is complete.
        public abstract Message Execute();

        public Action(Unit actor)
        {
            this._actor = actor;
        }
    }

    #region Actions

    /*
    /// <summary>
    /// Just waits the given amount of ticks.
    /// </summary>
    public class ActionWait : Action
    {
        public override void Update()
        {
            if (_finished)
            {
                return;
            }

            this.TicksIncrement();
            if (this.TicksRemaining() == 0)
            {
                this.WrapUp();
            }
        }

        protected override void WrapUp()
        {
            this._finished = true;
        }

        // Nothing to do here.
        public override void InterruptionCleanUp()
        {
            this.WrapUp();
        }

        public ActionWait(Creature actor, UInt16 ticksToWait)
            : base(actor)
        {
            this.ActionTotalDuration = ticksToWait;
        }
    }
    */

    public class ActionUseSpell : Action
    {
        private Spell _spell;
        private Coords? _target;

        public override Message Execute()
        {
            bool success = _spell.Execute(_target);
            if (success)
            {
                (Actor as Creature).AddToStatBasic(Creature.StatBasic.AP, -_spell.ExecutionTime);

                return new Message(Actor.ToString() + " used " + _spell.ToString() + ".");
            }
            return new Message(Actor.ToString() + " failed to use" + _spell.ToString() + "."); //fix this imbecility
        }

        public ActionUseSpell(Creature actor, Spell spell, Coords? target) : base(actor)
        {
            _spell = spell;
            _target = target;
        }
    }

    /// <summary>
    /// Has the actor say something.
    /// </summary>
    public class ActionSaySomething : Action
    {
        private String _text;

        public override Message Execute()
        {
            this.Actor.LabelUpper = this._text;

            return new Message(this.Actor.ToString() + " said: " + _text);
        }

        public ActionSaySomething(Unit actor, String wordsOfWisdom)
            : base(actor)
        {
            this._text = wordsOfWisdom; 
        }
    }

    public class ActionAttack : Action
    {
        Creature _target;
        UInt16 _damage;

        public override Message Execute()
        {
            //UInt16 targetHP = _target.GetStatBasic(Creature.StatBasic.HP, true);
            //_target.SetStatBasic(Creature.StatBasic.HP, true, (UInt16)Math.Max(0, targetHP - _damage));
            _target.AddToStatBasic(Creature.StatBasic.HP, -_damage); // casting problems?

            //Actor.AP -= APCost; //fix attack cost in AP later
            (Actor as Creature).AddToStatBasic(Creature.StatBasic.AP, -(Actor as Creature).GetAPActionCost(APCostTypes.AttackMelee));

            return new Message(this.Actor.ToString() + " hit " + _target.ToString() + " for " + _damage + ".");
        }

        public UInt16 Damage()
        {
            return _damage;
        }

        public ActionAttack(Creature actor, Creature target)
            : base(actor)
        {
            _target = target;

            _damage = (UInt16)Math.Max((Actor as Creature).GetAttackDamage() - _target.GetArmor(), 1);
        }
    }

    public class ActionMove : Action
    {
        private List<Direction> _route;
        private UInt16 _APCost;

        private DrawerBattle _drawer;

        public override Message Execute()
        {
            Coords original = Actor.PositionGet();
            Coords current = Actor.PositionGet();

            //Actor.MyMoveRangeCalculator.Cost(
            
            _drawer.AddMovementAnimation(Actor, _route);

            while (_route.Count > 0)
            {
                current = current.NeighborInDirection(_route[_route.Count - 1]);
                _route.RemoveAt(_route.Count - 1);
                Actor.PositionSet(current);
            }

            //this.Actor.AP -= this.APCost;
            (Actor as Creature).AddToStatBasic(Creature.StatBasic.AP, -_APCost);

            return new Message(this.Actor.ToString() + " went from " + original.ToString() + " to " + current.ToString() + ".");
        }

        public ActionMove(Unit actor, List<Direction> route, UInt16 APCost, DrawerBattle drawer)
            : base(actor)
        {
            _APCost = APCost;
            _route = route;
            _drawer = drawer;
        }
    }

    public class ActionItemPick : Action
    {
        public override Message Execute()
        {
            Item pickedUp = (Actor as Creature).ItemPick();
            (Actor as Creature).AddToStatBasic(Creature.StatBasic.AP, -(Actor as Creature).GetAPActionCost(APCostTypes.ItemPickUp));

            return new Message(this.Actor.ToString() + " picked up " + pickedUp.ToString() + ".");
        }

        public ActionItemPick(Creature actor)
            : base(actor)
        {
        }
    }

    public class ActionItemDrop : Action
    {
        private Item _toBeDropped;

        public override Message Execute()
        {
            Tile tileUnderAgent = (Actor as Creature).MapBattlemap.GetTile(Actor.PositionGet());
            tileUnderAgent.MyInventory.ItemAddToList(_toBeDropped);

            (Actor as Creature).AddToStatBasic(Creature.StatBasic.AP, -(Actor as Creature).GetAPActionCost(APCostTypes.ItemDrop));

            return new Message(this.Actor.ToString() + " dropped " + _toBeDropped.ToString() + " to " + Actor.PositionGet().ToString() + ".");
        }

        public ActionItemDrop(Creature actor, Item toBeDropped)
            : base(actor)
        {
            _toBeDropped = toBeDropped;
        }
    }

    public class ActionDie : Action
    {
        public override Message Execute()
        {
            (Actor as Creature).Death();
            return new Message(Actor.ToString() + " has perished.");
        }

        public ActionDie(Creature actor)
            : base(actor)
        {
        }
    }

    #endregion
}
