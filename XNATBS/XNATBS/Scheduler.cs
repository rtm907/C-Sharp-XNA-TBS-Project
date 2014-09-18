using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNATBS
{
    /// <summary>
    /// Keeps track of game-time and takes care of having all the actors execute their actions.
    /// </summary>
    public class Scheduler
    {
        // Time since game started
        private UInt64 _turnCounter = 1;
        public UInt64 TurnCounter
        {
            get
            {
                return this._turnCounter;
            }
        }

        // Should be updated to a 'World' reference once there is a 'gameworld' class.
        private Map _gameMap;

        List<Creature> _deadCreaturesCleanUp = new List<Creature>();

        public void EndTurn()
        {
            ++this._turnCounter;
        }

        /// <summary>
        /// The scheduler updates its map's actors for the current tick.
        /// </summary>
        public void Update()
        {
            // this._turnCounter++;

            /*
            foreach (KeyValuePair<UInt32, Creature> kvp in _gameMap.Menagerie)
            {
                
                if (kvp.Value.Dead)
                {
                    _deadCreaturesCleanUp.Add(kvp.Value);
                    continue;
                }

                
                Brain currentBrain = kvp.Value.CreatureBrain;
                if (currentBrain != null)
                {
                    currentBrain.Update();
                }
            }

            
            for (int i = 0; i < _deadCreaturesCleanUp.Count; ++i)
            {
                _deadCreaturesCleanUp[i].Death();
            }
            _deadCreaturesCleanUp.Clear();
            */
        }

        public Scheduler(Map gamemap)
        {
            this._gameMap = gamemap;
        }
    }

}
