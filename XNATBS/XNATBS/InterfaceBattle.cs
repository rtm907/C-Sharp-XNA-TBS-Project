using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace XNATBS
{
    public abstract class Interface
    {
        protected Game1 _myGame;
        public Game1 MyGame
        {
            get
            {
                return _myGame;
            }
        }

        protected DrawerBattle _myDrawer;
        public DrawerBattle MyDrawer
        {
            set
            {
                _myDrawer = value;
            }
        }

        protected KeyboardState _oldKeyboardState;
        protected MouseState _oldMouseState;

        protected Vector2 _mousePosition;
        public Vector2 MousePosition
        {
            get
            {
                return _mousePosition;
            }
        }

        public abstract void Update(GameTime gameTime, MouseState mouse, KeyboardState keys);

        protected abstract void HandlerMouse(MouseState mouse);

        protected abstract void HandlerKeyboard(KeyboardState keys);

        /// <summary>
        /// Checks if a given vector is in a given rectangle. 
        /// </summary>
        protected bool VectorInRectangle(Vector2 v, Rectangle r)
        {
            return (r.Left <= v.X && r.Right >= v.X && r.Top <= v.Y && r.Bottom >= v.Y);
        }

        public Interface(Game1 myGame)
        {
            _myGame = myGame;
        }
    }

    public class InterfaceBattle : Interface
    {
        #region Properties

        private MapBattle _currentMap;

        private Creature _selectedCreature;
        public Creature SelectedCreature
        {
            get
            {
                return _selectedCreature;
            }
        }
        private Item _itemSelected;
        public Item ItemSelected
        {
            get
            {
                return _itemSelected;
            }
        }
        private Item _itemHovered;
        public Item ItemHovered
        {
            get
            {
                return _itemHovered;
            }
        }


        private Coords _mouseHoverTile;
        public Coords MouseHoverTile
        {
            get
            {
                return _mouseHoverTile;
            }
        }
        private Vector2 _lastLeftButtonPress;

        public enum Buttons : sbyte
        {
            CharacterInfo = 0,
            Inventory,
            Spells,
            COUNT
        }
        private Rectangle[] _buttons = new Rectangle[(sbyte)Buttons.COUNT];
        public Rectangle GetButton(Buttons type)
        {
            return _buttons[(sbyte)type];
        }
        private Rectangle[] _boxes = new Rectangle[(sbyte)Buttons.COUNT];
        public Rectangle GetBox(Buttons type)
        {
            return _boxes[(sbyte)type];
        }
        private Rectangle[] _inventorySlotsBackPack = new Rectangle[Constants.InventorySize];
        public Rectangle GetInventorySlotBackpack(int i)
        {
            return _inventorySlotsBackPack[i];
        }
        private Rectangle[] _inventorySlotsEquipped = new Rectangle[(sbyte)InventoryType.COUNT];
        public Rectangle GetInventorySlotEquipped(int i)
        {
            return _inventorySlotsEquipped[i];
        }
        private Rectangle[] _spells = new Rectangle[Constants.MaximumNumberAllowedSpells];
        public Rectangle GetSpellButton(int i)
        {
            return _spells[i];
        }

        private bool[] _boxOpen = new bool[(sbyte)Buttons.COUNT];
        public bool IsBoxOpen(Buttons type)
        {
            return _boxOpen[(sbyte) type];
        }

        #endregion

        #region Methods

        public override void Update(GameTime gameTime, MouseState mouse, KeyboardState keys)
        {
            HandlerKeyboard(keys);
            _oldKeyboardState = keys;

            HandlerMouse(mouse);
            _oldMouseState = mouse;
        }

        #region Mouse Handler

        /// <summary>
        /// Handles mouse activity.
        /// </summary>
        protected override void HandlerMouse(MouseState mouse)
        {
            Int32 deltaWheel = mouse.ScrollWheelValue - _oldMouseState.ScrollWheelValue;
            if (deltaWheel > 0)
            {
                if (_selectedCreature != null && _selectedCreature.Team == _myGame.PlayerTeam)
                {
                    _selectedCreature.SpellSelectNext();
                }
            }
                            
            else if(deltaWheel < 0)
            {
                if (_selectedCreature != null && _selectedCreature.Team == _myGame.PlayerTeam)
                {
                    _selectedCreature.SpellSelectPrevious();
                }
            }


            _mousePosition = new Vector2(mouse.X, mouse.Y);
            _mouseHoverTile = ClickedHex(_mousePosition,_myDrawer.ScreenAnchor);

            #region Mouse Hover
            // check if the cursor is over an item.
            if (_selectedCreature != null && _selectedCreature.Team == _myGame.PlayerTeam)
            {
                int? i = DetermineInventorySlotClickedBackpack();
                int? j = DetermineInventorySlotClickedEquipped();

                if (i != null || j !=null)
                {
                    _itemHovered = (i != null) ? _selectedCreature.InventoryBackpack.GetItem(i.Value) : _selectedCreature.InventoryEquipped.GetItem(j.Value);
                }
                else                
                {
                    _itemHovered = null;
                }
            }

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                _lastLeftButtonPress = new Vector2(mouse.X - _myDrawer.ScreenAnchor.X, mouse.Y - _myDrawer.ScreenAnchor.Y);
                if (_oldMouseState.LeftButton == ButtonState.Released)
                {

                }
            }
            #endregion

            HandlerMouse_LeftButton(mouse);

            HandlerMouse_RightButton(mouse);
        }

        private void HandlerMouse_LeftButton(MouseState mouse)
        {
            // check if a mouse click occured
            if (mouse.LeftButton == ButtonState.Released && _oldMouseState.LeftButton == ButtonState.Pressed)
            {
                int? i = DetermineInventorySlotClickedBackpack();
                int? j = DetermineInventorySlotClickedEquipped();

                sbyte? k = HoveredSpell();

                Buttons? clickType = DetermineIfButtonIsClicked();
                // Button clicked
                if (clickType != null)
                {
                    HandlerMouse_LeftButton_Button(clickType.Value);
                }
                // Inventory area clicked
                else if ((i != null || j != null) && IsBoxOpen(Buttons.Inventory))
                {
                    HandlerMouse_LeftButton_InventoryArea(mouse, i, j);
                }
                // Spells area clicked
                else if ((k != null) && IsBoxOpen(Buttons.Spells))
                {
                    HandlerMouse_LeftButton_SpellsArea(k.Value);
                }
                // General area clicked
                else
                {
                    HandlerMouse_LeftButton_GeneralArea(mouse);
                }
            }
        }

        private void HandlerMouse_LeftButton_Button(Buttons button)
        {
            FlipButtonStatus(button);
        }

        private void HandlerMouse_LeftButton_InventoryArea(MouseState mouse, int? i, int? j)
        {
            if (_itemSelected != null)
            {
                if (i != null || ItemFitsInventoryType(_itemSelected, j.Value))
                {
                    Inventory clickedInventory = (i != null) ? _selectedCreature.InventoryBackpack : _selectedCreature.InventoryEquipped;
                    //Item itemClicked = clickedInventory.GetItem(inventorySlotClicked.Value.Y);
                    if (_itemHovered != null)
                    {
                        //Item temp = itemClicked;
                        clickedInventory.ItemRemoveFromList(_itemHovered);
                        clickedInventory.ItemAddToList(_itemSelected);
                        _itemSelected = _itemHovered;
                    }
                    else
                    {
                        if (i != null)
                        {
                            clickedInventory.ItemAddToList((sbyte)i.Value, _itemSelected);
                        }
                        else
                        {
                            clickedInventory.ItemAddToList((sbyte)j.Value, _itemSelected);
                        }
                        _itemSelected = null;
                    }
                }
            }
            else
            {
                //Item clicked = _selectedCreature.MyInventory[inventorySlotClicked.Value.X].GetItem(inventorySlotClicked.Value.Y);
                if (_itemHovered != null)
                {
                    _itemSelected = _itemHovered;
                    if (i != null)
                    {
                        _selectedCreature.InventoryBackpack.ItemRemoveFromList(_itemSelected);
                    }
                    else // here j!= null; this is inelegant, fix
                    {
                        _selectedCreature.InventoryEquipped.ItemRemoveFromList(_itemSelected);
                    }
                }
            }
        }

        private void HandlerMouse_LeftButton_SpellsArea(sbyte spell)
        {
            if (_selectedCreature.SkillPoints > 0)
            {
                _selectedCreature.SkillPointsSubstract(1);
                _selectedCreature.SpellAtIndex(spell).IncreaseMagnitude();
            }
        }

        private void HandlerMouse_LeftButton_GeneralArea(MouseState mouse)
        {
            // try to drop selected item
            if (_itemSelected != null)
            {
                Tile tileUnderAgent = _currentMap.GetTile(_selectedCreature.PositionGet());
                if (!tileUnderAgent.MyInventory.Full() && 
                    _selectedCreature.GetStatBasic(Creature.StatBasic.AP, true) >= _selectedCreature.GetAPActionCost(APCostTypes.ItemDrop))
                {
                    // drop item
                    //tileUnderAgent.MyInventory.ItemAddToList(_selectedItem);
                    ActionItemDrop action = new ActionItemDrop(_selectedCreature, _itemSelected);
                    _myGame.ExecuteAction(action);
                    _itemSelected = null;
                }
            }
            // try to select tenant
            else
            {
                Coords selectedHex = ClickedHex(new Vector2(mouse.X, mouse.Y), _myDrawer.ScreenAnchor);
                _selectedCreature = _currentMap.TenancyMap[selectedHex.X, selectedHex.Y];
                if (_selectedCreature != null)
                {
                    _selectedCreature.MyMoveRangeCalculator.Update();
                }
            }
        }

        private void HandlerMouse_RightButton(MouseState mouse)
        {
            if (mouse.RightButton == ButtonState.Released && _oldMouseState.RightButton == ButtonState.Pressed)
            {
                if (_selectedCreature != null && _selectedCreature.Team == _myGame.PlayerTeam)
                {
                    Coords selectedHex = ClickedHex(new Vector2(mouse.X, mouse.Y), _myDrawer.ScreenAnchor);

                    Creature hexTenant = _currentMap.TenancyMap[selectedHex.X, selectedHex.Y];
                    // Self Clicked
                    if (hexTenant == _selectedCreature)
                    {
                        HandlerMouse_RightButton_SelfClicked(mouse, selectedHex);
                    }
                    // ATTACK
                    else if (hexTenant != null && hexTenant.Team != _myGame.PlayerTeam)
                    {
                        HandlerMouse_RightButton_EnemyClicked(mouse, selectedHex, hexTenant);
                    }
                    // MOVEMENT
                    else
                    {
                        HandlerMouse_RightButton_TerrainClicked(mouse, selectedHex);
                    }
                }
            }
        }

        private void HandlerMouse_RightButton_SelfClicked(MouseState mouse, Coords selectedHex)
        {
            // NOTE: you could set up some kind of a (radial?) menu for this case

            // Item Pickup
            if (_currentMap.GetTile(selectedHex).MyInventory.Size() > 0
                        && _selectedCreature.GetAP() >= _selectedCreature.GetAPActionCost(APCostTypes.ItemPickUp))
            {
                ActionItemPick pickItem = new ActionItemPick(_selectedCreature);
                _myGame.ExecuteAction(pickItem);
            }
        }

        private void HandlerMouse_RightButton_EnemyClicked(MouseState mouse, Coords selectedHex, Creature hexTenant)
        {
            // Apply selected skill.
            if (_selectedCreature.GetAP() >= _selectedCreature.SpellCurrent.ExecutionTime)
            {
                ActionUseSpell useSpell = new ActionUseSpell(_selectedCreature, _selectedCreature.SpellCurrent, selectedHex);
                useSpell.Execute();
            }
        }

        private void HandlerMouse_RightButton_TerrainClicked(MouseState mouse, Coords selectedHex)
        {
            //List<Direction> route = _selectedCreature.MyPathfinder.PathfinderAStarCoarse
            //    (_selectedCreature.PositionTile, selectedHex, StaticMathFunctions.DistanceBetweenTwoCoordsEucledean);
            if (_selectedCreature.MyMoveRangeCalculator.Accessible(selectedHex))
            {
                List<Direction> route = _selectedCreature.MyMoveRangeCalculator.RetrieveRoute(selectedHex);
                ActionMove move = new ActionMove(_selectedCreature, route, _selectedCreature.MyMoveRangeCalculator.Cost(selectedHex),_myDrawer);
                //_myDrawer.AddMovementAnimation(_selectedCreature, route);
                _myGame.ExecuteAction(move);
            }
        }

        #endregion

        #region Keyboard Handler

        /// <summary>
        /// Handles keyboard activity.
        /// </summary>
        protected override void HandlerKeyboard(KeyboardState keys)
        {
            #region Arrow Keys

            if (keys.IsKeyDown(Keys.Left))
            {
                _myDrawer.ScreenAnchor = new Vector2(Math.Min(_myDrawer.ScreenAnchor.X + Constants.FreeScrollingSpeed,_myGame.GraphicsDevice.Viewport.Width), 
                    _myDrawer.ScreenAnchor.Y);
            }
            if (keys.IsKeyDown(Keys.Right))
            {
                _myDrawer.ScreenAnchor = new Vector2(Math.Max(_myDrawer.ScreenAnchor.X - Constants.FreeScrollingSpeed,-_currentMap.PixelBoundX), 
                    _myDrawer.ScreenAnchor.Y);
            }
            if (keys.IsKeyDown(Keys.Up))
            {
                _myDrawer.ScreenAnchor = new Vector2(_myDrawer.ScreenAnchor.X,
                    Math.Min(_myDrawer.ScreenAnchor.Y + Constants.FreeScrollingSpeed, _myGame.GraphicsDevice.Viewport.Height));
            }
            if (keys.IsKeyDown(Keys.Down))
            {
                _myDrawer.ScreenAnchor = new Vector2(_myDrawer.ScreenAnchor.X, Math.Max(_myDrawer.ScreenAnchor.Y - Constants.FreeScrollingSpeed, -_currentMap.PixelBoundY));
            }

            #endregion

            #region Hotkeys

            // Add customization support at some point
            if (HandlerKeyboard_KeyDepressed(keys, Keys.C))
            {
                FlipButtonStatus(Buttons.CharacterInfo);
            }

            if (HandlerKeyboard_KeyDepressed(keys, Keys.S))
            {
                FlipButtonStatus(Buttons.Spells);
            }

            if (HandlerKeyboard_KeyDepressed(keys, Keys.I))
            {
                FlipButtonStatus(Buttons.Inventory);
            }

            if (HandlerKeyboard_KeyDepressed(keys, Keys.PageDown))
            {
                if (Constants.ZoomingAllowed)
                {
                    _myDrawer.Zoom = Math.Min(Math.Max(Constants.ZoomMin, _myDrawer.Zoom - Constants.ZoomSpeed), Constants.ZoomMax);
                }
            }
            if (HandlerKeyboard_KeyDepressed(keys, Keys.PageUp))
            {
                if (Constants.ZoomingAllowed)
                {
                    _myDrawer.Zoom = Math.Min(Math.Max(Constants.ZoomMin, _myDrawer.Zoom + Constants.ZoomSpeed), Constants.ZoomMax);
                }
            }


            #endregion

            if (HandlerKeyboard_KeyDepressed(keys, Keys.Space))
            {
                _myGame.EndTurn();
            }

            if (HandlerKeyboard_KeyDepressed(keys, Keys.Q))
            {
                _myGame.EndBattle();
            }
        }

        private bool HandlerKeyboard_KeyDepressed(KeyboardState keys, Keys key)
        {
            return (_oldKeyboardState.IsKeyDown(key) && keys.IsKeyUp(key));
        }

        #endregion

        #region Helper Methods Private

        private void FlipButtonStatus(Buttons b)
        {
            _boxOpen[(sbyte)b] = !_boxOpen[(sbyte)b];
        }

        private Coords ClickedHex(Vector2 mousePos, Vector2 screenAnchor)
        {
            int tilesize = (int) (Constants.TileSize * _myDrawer.Zoom);
            int offset = tilesize / 4;

            Vector2 truePos = new Vector2(mousePos.X - screenAnchor.X * _myDrawer.Zoom, mousePos.Y - screenAnchor.Y * _myDrawer.Zoom);

            int j = (int)(truePos.Y / (tilesize - offset));
            int i = (int)((truePos.X - (j % 2) * (tilesize / 2)) / tilesize);


            return new Coords(Math.Min(Math.Max(i, 0), _currentMap.BoundX - 1), Math.Min(Math.Max(j, 0), _currentMap.BoundY - 1));
        }

        /// <summary>
        /// Returns true if item i fits the given slot.
        /// </summary>
        private bool ItemFitsInventoryType(Item i, InventoryType t)
        {
            return (t == InventoryType.General) || (t == InventoryType.HandWeapon && i.MyType == ItemType.Weapon)
                || (t == InventoryType.HandShield && i.MyType == ItemType.Armor);
        }

        private bool ItemFitsInventoryType(Item i, int j)
        {
            return (j == (sbyte)InventoryType.HandWeapon && i.MyType == ItemType.Weapon)
                || (j == (sbyte)InventoryType.HandShield && i.MyType == ItemType.Armor);
        }

        private int? DetermineInventorySlotClickedBackpack()
        {
            for (int i = 0; i < Constants.InventorySize; ++i)
            {
                if (VectorInRectangle(_mousePosition, _inventorySlotsBackPack[i]))
                {
                    return i;
                }
            }

            return null;
        }

        private int? DetermineInventorySlotClickedEquipped()
        {
            for (int i = 0; i < (sbyte)InventoryType.COUNT; ++i)
            {
                if (VectorInRectangle(_mousePosition, _inventorySlotsEquipped[i]))
                {
                    return i;
                }
            }

            return null;
        }

        #endregion

        #region Helper Methods Public

        public bool PlayableCreatureSelected()
        {
            return (_selectedCreature != null && _selectedCreature.Team == _myGame.PlayerTeam);
        }

        public Vector2 HexPosition(Coords hexCoords)
        {
            int tilesize = Constants.TileSize;
            int offset = tilesize / 4;

            return new Vector2(hexCoords.X * tilesize + (hexCoords.Y % 2) * (tilesize / 2), hexCoords.Y * tilesize - hexCoords.Y * offset);
        }

        /// <summary>
        /// Returns type of button clicked, or null if none is clicked.
        /// </summary>
        /// <returns></returns>
        public Buttons? DetermineIfButtonIsClicked()
        {
            for (int i = 0; i < (sbyte)Buttons.COUNT; ++i)
            {
                if (VectorInRectangle(_mousePosition, GetButton((Buttons)i)))
                {
                    return (Buttons)i;
                }
            }

            return null;
        }

        public Buttons? DetermineIfBoxIsClicked()
        {
            for (int i = 0; i < (sbyte)Buttons.COUNT; ++i)
            {
                if (VectorInRectangle(_mousePosition, GetBox((Buttons)i)))
                {
                    return (Buttons)i;
                }
            }

            return null;
        }

        public bool InventorySlotUnderCursor()
        {
            // this should be fixed so the check is done only once per update.
            return (DetermineInventorySlotClickedBackpack() != null || DetermineInventorySlotClickedEquipped() != null);
        }

        public sbyte? HoveredSpell()
        {
            if (_boxOpen[(sbyte)Buttons.Spells] && _selectedCreature != null)
            {
                if (VectorInRectangle(_mousePosition, _boxes[(sbyte)Buttons.Spells]))
                {
                    for (sbyte i = 0; i < _selectedCreature.Spells.Count; ++i)
                    {
                        if (VectorInRectangle(_mousePosition, _spells[i]))
                        {
                            return i;
                        }
                    }
                }
            }

            return null;
        }

        #endregion

        #endregion

        #region Constructor

        public InterfaceBattle(Game1 myGame)
            : base(myGame)
        {
            _currentMap = _myGame.CurrentMap;
            DeclareRectangles();
        }

        /// <summary>
        /// Declares the rectangles for the buttons and boxes of the interface.
        /// Call at construction and when the window size is updated().
        /// </summary>
        private void DeclareRectangles()
        {
            // Character info button
            _buttons[(sbyte)Buttons.CharacterInfo] = new Rectangle(_myGame.GraphicsDevice.Viewport.Width - Constants.ButtonDefaultWidth,
                (int)(0.5 * _myGame.GraphicsDevice.Viewport.Height) - Constants.ButtonDefaultHeight,
                Constants.ButtonDefaultWidth, Constants.ButtonDefaultHeight);

            // Skills info button
            _buttons[(sbyte)Buttons.Spells] = new Rectangle(_myGame.GraphicsDevice.Viewport.Width - Constants.ButtonDefaultWidth,
                (int)(0.5 * _myGame.GraphicsDevice.Viewport.Height), Constants.ButtonDefaultWidth, Constants.ButtonDefaultHeight);

            // Inventory info button
            _buttons[(sbyte)Buttons.Inventory] = new Rectangle(0, (int)(0.5 * (_myGame.GraphicsDevice.Viewport.Height - Constants.ButtonDefaultWidth)),
                Constants.ButtonDefaultWidth, Constants.ButtonDefaultHeight);

            // Character box
            _boxes[(sbyte)Buttons.CharacterInfo] = new Rectangle((int)(2 * _myGame.GraphicsDevice.Viewport.Width / 3), 0,
                (int)(_myGame.GraphicsDevice.Viewport.Width / 3), (int)(0.5 * _myGame.GraphicsDevice.Viewport.Height));

            // Skills Box
            _boxes[(sbyte)Buttons.Spells] = new Rectangle((int)(2 * _myGame.GraphicsDevice.Viewport.Width / 3),
                (int)(0.5 * _myGame.GraphicsDevice.Viewport.Height), (int)(_myGame.GraphicsDevice.Viewport.Width / 3),
                (int)(0.5 * _myGame.GraphicsDevice.Viewport.Height));

            // Inventory box
            _boxes[(sbyte)Buttons.Inventory] = new Rectangle(0, 0,
                Constants.InventoryRowSize * Constants.InventorySlotSpriteSize, (int)(_myGame.GraphicsDevice.Viewport.Height));

            DeclareInventorySlots();
            DeclareSpellsButtons();
        }

        private void DeclareInventorySlots()
        {
            for (int j = 0; j < Constants.InventoryRows; ++j)
            {
                for (int i = 0; i < Constants.InventoryRowSize; ++i)
                {
                    _inventorySlotsBackPack[j * Constants.InventoryRowSize + i] = new Rectangle(i * Constants.InventorySlotSpriteSize,
                        _myGame.GraphicsDevice.Viewport.Height - (j + 1) * Constants.InventorySlotSpriteSize,
                        Constants.InventorySlotSpriteSize, Constants.InventorySlotSpriteSize);
                }
            }
            for (int i = 0; i < (sbyte)InventoryType.COUNT; ++i)
            {
                _inventorySlotsEquipped[i] = new Rectangle(i * Constants.InventorySlotSpriteSize,
                        _myGame.GraphicsDevice.Viewport.Height - (Constants.InventoryRows + 1) * Constants.InventorySlotSpriteSize,
                        Constants.InventorySlotSpriteSize, Constants.InventorySlotSpriteSize);
            }
        }

        private void DeclareSpellsButtons()
        {
            Rectangle skillsBox = _boxes[(sbyte)Buttons.Spells];
            UInt16 iconSize = Constants.ButtonSpellPixelSize;
            int columns = skillsBox.Width/iconSize;

            for (int i = 0; i < _spells.Length; ++i)
            {
                int row = i / columns;
                int col = i % columns;

                _spells[i] = new Rectangle(skillsBox.Left + col * iconSize, skillsBox.Top + row * iconSize, iconSize, iconSize);
            }
        }

        public void SetCurrentMapBattle(MapBattle map)
        {
            _currentMap = map;
        }

        #endregion
    }
}
