using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNATBS
{
    public class Team
    {
        private String _name;
        public String Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        private sbyte _uniqueID;
        public sbyte UniqueID
        {
            get
            {
                return _uniqueID;
            }
        }

        private Map _inhabittedMap;

        private Color _teamColor;
        public Color TeamColor
        {
            get
            {
                return _teamColor;
            }
        }

        private List<Unit> _members = new List<Unit>();
        public List<Unit> Members
        {
            get
            {
                return _members;
            }
        }

        public void MemberRegister(Creature newGuy)
        {
            _members.Add(newGuy);
        }
        public void MemberRemove(Creature oldGuy)
        {
            _members.Remove(oldGuy);
        }

        private SortedList<UInt32, Unit> _observedEnemies = new SortedList<uint, Unit>();
        public bool EnemyIsObserved(Unit enemy)
        {
            return _observedEnemies.ContainsKey(enemy.UniqueID);
        }
        public void ObservedEnemyAdd(Unit enemy)
        {
            if(!_observedEnemies.ContainsKey(enemy.UniqueID))
            {
                _observedEnemies.Add(enemy.UniqueID, enemy);
            }
        }
        public void ObservedEnemyRemove(Unit enemy)
        {
            _observedEnemies.Remove(enemy.UniqueID);
        }

        public Team(Map currentMap, sbyte id, Color teamColor)
        {
            _uniqueID = id;
            _teamColor = teamColor;
            _inhabittedMap = currentMap;

            this._inhabittedMap.TeamRosterAddTeamTo(id, this);
        }
    }
}
