using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// ITEMS AND INVENTORY

namespace XNATBS
{
    public class Inventory
    {
        // Shouldn't this be a list of the item IDs? This way I do fewer look-ups.
        private Item[] _itemArray;
        public Item[] ItemArray
        {
            get
            {
                return this._itemArray;
            }
        }

        private sbyte _sizeMax;
        public sbyte SizeMax
        {
            get
            {
                return _sizeMax;
            }
        }

        private sbyte _sizeCurrent=0;

        public Item GetItem(Int32 index)
        {
            return _itemArray[index];
        }

        public Int32 Size()
        {
            return _sizeCurrent;
        }

        public bool Full()
        {
            return this.Size() >= _sizeMax;
        }

        public void ItemAddToList(sbyte index, Item newone)
        {
            if (this._itemArray[index] == null)
            {
                this._itemArray[index] = newone;
                newone.ParentInventory = this;
                _sizeCurrent++;
            }
        }

        public void ItemAddToList(Item newone)
        {
            if (_sizeCurrent < _sizeMax)
            {
                for (int i = 0; i < _sizeMax; ++i)
                {
                    if (_itemArray[i] == null)
                    {
                        _itemArray[i] = newone;
                        newone.ParentInventory = this;
                        _sizeCurrent++;
                        return;
                    }
                }
                throw new Exception("Couldn't find empty slot for item.");
            }
            else
            {
                throw new Exception("Failed to add item - inventory full." + " Item: "+newone.ToString()+"; Inventory: "+this.ToString());
            }
        }

        public void ItemRemoveFromList(Item toRemove)
        {
            for (int i = 0; i < _sizeMax; ++i)
            {
                if (_itemArray[i] == toRemove)
                {
                    _itemArray[i]=null;
                    toRemove.ParentInventory = null;
                    --_sizeCurrent;
                }
            }
        }

        // One of the following two should always be null. An inventory belongs to a tile OR
        // a creature. (later maybe also to a container?)
        private Tile _ownerTile;
        public Tile OwnerTile
        {
            get
            {
                return this._ownerTile;
            }
            set
            {
                this._ownerTile = value;
                this._ownerCreature = null;
            }
        }
        private Creature _ownerCreature;
        public Creature OwnerCreature
        {
            get
            {
                return this._ownerCreature;
            }
            set
            {
                this._ownerCreature = value;
                this._ownerTile = null;
            }
        }

        // returns the item's position (in Coords)
        public Nullable<Coords> Position()
        {
            Nullable<Coords> returnValue = null;
            if (this._ownerTile != null)
            {
                returnValue = _ownerTile.Position;
            }
            else if (this._ownerCreature != null)
            {
                returnValue = _ownerCreature.PositionGet();
            }

            return returnValue;
        }

        private Inventory(sbyte sizeMax)
        {
            this._sizeMax = sizeMax;
            this._itemArray = new Item[_sizeMax];
        }

        public Inventory(Creature owner, sbyte size)
            : this(size)
        {
            this._ownerTile = null;
            this._ownerCreature = owner;
        }

        public Inventory(Tile ownerTile, sbyte size)
            : this(size)
        {
            this._ownerTile = ownerTile;
            this._ownerCreature = null;
        }
    }

    public class Item : IComparable
    {
        private UInt32 _ID;
        public UInt32 ID
        {
            get
            {
                return this._ID;
            }
            set
            {
                this._ID = value;
            }
        }

        private String _itemName;
        public String Name
        {
            get
            {
                return _itemName;
            }
        }

        private SpriteItem _itemBitmap;
        public SpriteItem ItemBitmap
        {
            get
            {
                return this._itemBitmap;
            }
        }

        /*
        private UInt32 _weight;
        public UInt32 Weight
        {
            get
            {
                return _weight;
            }
            set
            {
                this._weight = value;
            }
        }

        private UInt32 _volume;
        public UInt32 Volume
        {
            get
            {
                return _volume;
            }
            set
            {
                this._volume = value;
            }
        }
        */

        private Inventory _parent;
        public Inventory ParentInventory
        {
            get
            {
                return this._parent;
            }
            set
            {
                this._parent = value;
            }
        }

        public Nullable<Coords> Position()
        {
            return (this._parent == null) ? null : this._parent.Position();
        }

        private ItemType _itemType;
        public ItemType MyType
        {
            get
            {
                return _itemType;
            }
        }

        public Int32 CompareTo(object obj)
        {
            if (!(obj is Item))
            {
                throw new Exception("Bad Items comparison.");
            }

            Item compared = (Item)obj;

            return (Int32)(this._ID - compared.ID);
        }

        private Dictionary<ItemProperty, float> _itemFunctions = new Dictionary<ItemProperty, float>();
        public Dictionary<ItemProperty, float> ItemFunctions
        {
            get
            {
                return _itemFunctions;
            }
        }

        public override string ToString()
        {
            return this._itemName;
        }

        #region constructors

        private Item(UInt32 itemID)
        {
            this._ID = itemID;
        }

        public Item(UInt32 itemID, ItemGenerator generator)
            : this(itemID)
        {
            this._itemName = generator.name;
            this._itemBitmap = generator.itemBitmap;
            this._itemFunctions = generator.functions;
            this._itemType = generator.typeOfItem;
        }

        public Item(UInt32 itemID, String name, SpriteItem myBitmap, Dictionary<ItemProperty, float> functions, ItemType itemType)
            : this(itemID)
        {
            this._itemName = name;
            this._itemBitmap = myBitmap;
            this._itemType = itemType;
            this._itemFunctions = functions;
        }

        public Item(UInt32 itemID, String name, SpriteItem myBitmap, Dictionary<ItemProperty, float> functions, ItemType itemType, Inventory parent)
            : this(itemID, name, myBitmap, functions, itemType)
        {
            this._parent = parent;
        }

        #endregion
        /*
        public Item(UInt32 itemID, String name, Dictionary<Stimulus, float> functions, ItemType itemType, Inventory parent, UInt32 itemWeight, UInt32 itemVolume)
            : this(itemID, name, functions, itemType, parent)
        {
            this._weight = itemWeight;
            this._volume = itemVolume;
        }*/
    }
}
