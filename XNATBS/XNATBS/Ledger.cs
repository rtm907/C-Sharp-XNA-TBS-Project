using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XNATBS
{
    /// <summary>
    /// Message structure for the ledger.
    /// </summary>
    public struct Message
    {
        private UInt64 _time;
        public UInt64 Time
        {
            get
            {
                return this._time;
            }
            set
            {
                this._time = value;
            }
        }
        private String _text;
        public String Text
        {
            get
            {
                return _text;
            }
            set
            {
                this._text = value;
            }
        }

        public Message(String message)
        {
            this._time = 0;
            this._text = message;
        }

        public Message(UInt64 time, String message)
        {
            this._time = time;
            this._text = message;
        }
    }

    /*
    public abstract class PlayerInput
    {
        private UInt64 _time;

        public PlayerInput(UInt64 timeStamp)
        {
            this._time = timeStamp;
        }
    }

    public class PlayerInputMoveInDir : PlayerInput
    {
        UInt32 _unitID;
        Vector _dir;

        public PlayerInputMoveInDir(UInt64 timeStamp, UInt32 unitID, Vector dir)
            : base(timeStamp)
        {
            this._unitID = unitID;
            this._dir = dir;
        }
    }

    public class PlayerInputMoveTo : PlayerInput
    {
        UInt32 _unitID;
        Coords _point;

        public PlayerInputMoveTo(UInt64 timeStamp, UInt32 unitID, Coords point)
            : base(timeStamp)
        {
            this._unitID = unitID;
            this._point = point;
        }
    }
    */

    /// <summary>
    /// Keeps a record of the important events in the game.
    /// Class isn't finished.
    /// </summary>
    public class Ledger
    {
        // Scheduler ref so we can timestamp the message
        private Scheduler _schedulerReference;
        private List<String> _ledger;

        public Ledger(Scheduler scheduler)
        {
            this._schedulerReference = scheduler;
            this._ledger = new List<string>();
        }

        public void RecordMessage(Message message)
        {
            // timestamp
            message.Time = (message.Time == 0) ? this._schedulerReference.TurnCounter : message.Time;

            this._ledger.Add(message.Time + "; " + message.Text);
        }
    }
}
