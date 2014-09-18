using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace XNATBS
{
    public abstract class Drawer
    {
        protected Game1 _myGame;
        
        protected ContentManager _content;
        

        protected abstract void Load_content();
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime, SpriteBatch spriteBatch);

        public Drawer(Game1 myGame, ContentManager content)
        {
            _myGame = myGame;
            _content = content;
            
        }
    }


    public class DrawerBattle : Drawer
    {
        #region Properties
        protected InterfaceBattle _myInterface;

        protected MapBattle _currentMap;

        private Vector2 _screenAnchor = new Vector2();
        public Vector2 ScreenAnchor
        {
            get
            {
                return _screenAnchor;
            }
            set
            {
                _screenAnchor = value;
            }
        }

        private Texture2D[] _tiles = new Texture2D[(Int32)SpriteTile.COUNT];
        private Texture2D[] _creatures = new Texture2D[(Int32)SpriteBatchCreature.COUNT];
        // use a retriever function instead of get?
        public Texture2D[] Creatures
        {
            get
            {
                return _creatures;
            }
        }
        private Texture2D[] _items = new Texture2D[(Int32)SpriteItem.COUNT];
        public Texture2D[] Items
        {
            get
            {
                return _items;
            }
        }
        private Texture2D[] _mouseCursors = new Texture2D[(Int32)SpriteMouse.COUNT];
        private Texture2D[] _particles = new Texture2D[(sbyte)SpriteParticle.COUNT];
        public Texture2D[] Particles
        {
            get
            {
                return _particles;
            }
        }
        private Texture2D[] _spells = new Texture2D[(sbyte)Spells.COUNT];

        private Texture2D _menuBox;
        private Texture2D _buttonArrow;
        public Texture2D ButtonArrow
        {
            get
            {
                return _buttonArrow;
            }
        }
        private Texture2D _reddot;
        private Texture2D _InventorySlot;
        //private Texture2D _pixel;
        private SpriteFont _font;

        private float _zoom = 1f;
        public float Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value;
            }
        }

        private List<FloatingMessage> _floatingMessages = new List<FloatingMessage>();
        public List<FloatingMessage> FloatingMessages
        {
            get
            {
                return _floatingMessages;
            }
        }
        private Dictionary<UInt32, List<AnimUnitMove>> _creatureAnimations = new Dictionary<uint, List<AnimUnitMove>>();
        public Dictionary<UInt32, List<AnimUnitMove>> CreatureAnimations
        {
            get
            {
                return _creatureAnimations;
            }
        }
        private List<Anim> _animations = new List<Anim>();
        public List<Anim> Animations
        {
            get
            {
                return _animations;
            }
        }

        #endregion

        /// <summary>
        /// Load_content will be called once per game and is the place to load
        /// all of your _content.
        /// </summary>
        protected override void Load_content()
        {
            #region Texture Loading

            for (sbyte i = 0; i < this._tiles.Length; ++i)
            {
                Texture2D current = _content.Load<Texture2D>("Hexes\\" + ((SpriteTile)i).ToString());
                _tiles[i] = current;
            }

            for (sbyte i = 0; i < this._creatures.Length; ++i)
            {
                Texture2D current = _content.Load<Texture2D>("Creatures\\" + ((SpriteBatchCreature)i).ToString());
                _creatures[i] = current;
            }

            for (sbyte i = 0; i < this._items.Length; ++i)
            {
                Texture2D current = _content.Load<Texture2D>("Items\\" + ((SpriteItem)i).ToString());
                _items[i] = current;
            }

            for (sbyte i = 0; i < this._mouseCursors.Length; ++i)
            {
                Texture2D current = _content.Load<Texture2D>("Mouse\\" + ((SpriteMouse)i).ToString());
                _mouseCursors[i] = current;
            }

            for (sbyte i = 0; i < this._particles.Length; ++i)
            {
                Texture2D current = _content.Load<Texture2D>("Particles\\" + ((SpriteParticle)i).ToString());
                _particles[i] = current;
            }

            for (sbyte i = 0; i < this._spells.Length; ++i)
            {
                Texture2D current = _content.Load<Texture2D>("SpellIcons\\" + ((Spells)i).ToString());
                _spells[i] = current;
            }

            #endregion

            _reddot = _content.Load<Texture2D>("reddot");
            _InventorySlot = _content.Load<Texture2D>("InventorySlot");
            //_pixel = _content.Load<Texture2D>("pixel");
            _menuBox = _content.Load<Texture2D>("MenuBox");
            _buttonArrow = _content.Load<Texture2D>("ButtonArrow");

            _font = _content.Load<SpriteFont>("hudFont");
        }

        public override void Update(GameTime gameTime)
        {
            #region Animations update

            List<UInt32> pairsToBeRemoved = new List<UInt32>();
            foreach (KeyValuePair<UInt32, List<AnimUnitMove>> kvp in _creatureAnimations)
            {
                AnimUnitMove current = kvp.Value.Last();
                current.Update();
                if (current.Expired())
                {
                    kvp.Value.RemoveAt(kvp.Value.Count - 1);
                    if (kvp.Value.Count == 0)
                    {
                        pairsToBeRemoved.Add(kvp.Key);
                    }
                }
            }
            foreach (UInt32 key in pairsToBeRemoved)
            {
                _creatureAnimations.Remove(key);
            }

            for (int i = 0; i < _animations.Count; ++i)
            {
                _animations[i].Update();
                if (_animations[i].Expired())
                {
                    _animations.RemoveAt(i);
                    --i;
                }
            }

            #endregion
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            // Terrain
            DrawTerrain(spriteBatch);

            // Creatures
            DrawCreatures(spriteBatch);

            // Animations
            DrawAnimations(spriteBatch);

            // Floating Messages
            DrawFloatingMessages(spriteBatch);

            // Buttons and Stat Boxes
            DrawButtonsAndStatBoxes(spriteBatch);

            // Inventory
            DrawInventory(spriteBatch);

            // Spells
            DrawSpellsBox(spriteBatch);

            // MouseCursor
            DrawMouseCursor(spriteBatch);

            //spriteBatch.DrawString(_font, _myGame.TURN.ToString(), new Vector2(0, 0), Color.Red);

            spriteBatch.End();
        }

        #region Draw Subroutines

        public Vector2 ZoomTransform(Vector2 v)
        {
            Vector2 v2 = new Vector2(v.X * _zoom, v.Y * _zoom);
            return v2;
        }

        private Rectangle ZoomTransform(Rectangle rectangle1)
        {
            Rectangle r2 = new Rectangle((int)(rectangle1.X * _zoom), (int)(rectangle1.Y * _zoom), 
                (int)(rectangle1.Width * _zoom), (int)(rectangle1.Height * _zoom));
            return r2;
        }

        private void DrawTerrain(SpriteBatch spriteBatch)
        {
            int tilesize = Constants.TileSize;
            int offset = tilesize / 4;

            BitArray[] accessibilityMap = null;
            if (_myInterface.SelectedCreature != null)
            {
                accessibilityMap = _myInterface.SelectedCreature.MyMoveRangeCalculator.CurrentRangeMap;
            }

            for (int i = 0; i < _currentMap.BoundX; ++i)
            {
                for (int j = 0; j < _currentMap.BoundY; ++j)
                {
                    Vector2 pos = new Vector2(i * tilesize + (j % 2) * (tilesize / 2), j * tilesize - j * offset) + _screenAnchor;
                    Rectangle rect = ZoomTransform(new Rectangle((int)pos.X, (int)pos.Y, tilesize, tilesize));
                    Int16 visibilityTracker = _currentMap.MyVisibilityTracker.VisibilityCheck(new Coords(i, j), _myGame.PlayerTeam);

                    // Draw tile if explored; fogged if explored but unseen; otherwise draw unexplored graphic
                    if (visibilityTracker >= 0)
                    {
                        Tile currentTile = _currentMap.GetTile(i, j);
                        spriteBatch.Draw(_tiles[(sbyte)currentTile.MyBitmap], rect, Color.White);
                        if (visibilityTracker == 0) // fog
                        {
                            spriteBatch.Draw(_tiles[(sbyte)SpriteTile.HexFog], rect, Color.White);
                        }
                        else if (currentTile.MyInventory != null && currentTile.MyInventory.Size() > 0)
                        {
                            spriteBatch.Draw(_items[(sbyte)currentTile.MyInventory.GetItem(0).ItemBitmap], rect, Color.White);
                        }

                        // Draw green overlay if tile is accessible at current turn
                        if (accessibilityMap != null)
                        {
                            if (!accessibilityMap[i][j])
                            {
                                Color fillColor = Color.Gray;
                                fillColor.A = 200;
                                spriteBatch.Draw(_tiles[(sbyte)SpriteTile.HexFilled], rect, fillColor);
                            }
                        }
                    }
                    else // unexplored
                    {
                        spriteBatch.Draw(_tiles[(sbyte)SpriteTile.HexUnexplored], rect, Color.White);
                    }


                    // Draws grid
                    if (Constants.ShowGrid)
                    {
                        spriteBatch.Draw(_tiles[(sbyte)SpriteTile.Hex], rect, Color.White);
                    }

                    // Draws coordinate numbers
                    if (Constants.ShowCoordinates)
                    {
                        spriteBatch.DrawString(_font, "(" + i + "," + j + ")",
                            ZoomTransform(pos + new Vector2(Constants.TileSize / 3, Constants.TileSize / 3)), Color.Black);
                    }
                }
            }
        }

        private void DrawInventory(SpriteBatch spriteBatch)
        {
            // INVENTORY
            // draw only if selected creature is player's
            if (_myInterface.SelectedCreature != null && _myInterface.SelectedCreature.Team == _myGame.PlayerTeam 
                && _myInterface.IsBoxOpen(InterfaceBattle.Buttons.Inventory))
            {
                // Main inventory
                for (int i = 0; i < Constants.InventorySize; ++i)
                {
                    Rectangle drawLocation = _myInterface.GetInventorySlotBackpack(i);
                    spriteBatch.Draw(_InventorySlot, drawLocation, Color.White);
                    Item something = _myInterface.SelectedCreature.InventoryBackpack.GetItem(i);
                    if (something != null)
                    {
                        spriteBatch.Draw(this._items[(sbyte)something.ItemBitmap], drawLocation, Color.White);
                    }
                }
                // Equipped items
                for (int i = 0; i < (sbyte)InventoryType.COUNT; ++i)
                {
                    Item something = _myInterface.SelectedCreature.InventoryEquipped.GetItem(i);
                    Rectangle drawLocation = _myInterface.GetInventorySlotEquipped(i);
                    spriteBatch.Draw(_InventorySlot, drawLocation, Color.White);
                    if (something != null)
                    {
                        spriteBatch.Draw(this._items[(sbyte)something.ItemBitmap], drawLocation, Color.White);
                    }
                }

                DrawItemInfoBox(spriteBatch);
            }
        }

        private void DrawCreatures(SpriteBatch spriteBatch)
        {
            int tilesize = Constants.TileSize;
            int offset = tilesize / 4;

            // CREATURES
            foreach (KeyValuePair<UInt32, Creature> kvp in _currentMap.Menagerie)
            {
                Creature tenant = kvp.Value;
                Texture2D tenantBitmap = _creatures[(sbyte)tenant.MyBitmap];
                Int32 i = tenant.PositionGet().X;
                Int32 j = tenant.PositionGet().Y;
                Int16 visibilityTracker = _currentMap.MyVisibilityTracker.VisibilityCheck(tenant.PositionGet(), _myGame.PlayerTeam);
                Vector2 pos = new Vector2(i * tilesize + (j % 2) * (tilesize / 2), j * tilesize - j * offset) + _screenAnchor;
                Rectangle rectHex = ZoomTransform(new Rectangle((int)pos.X, (int)pos.Y, tilesize, tilesize));
                Rectangle rectCreature = ZoomTransform(new Rectangle((int)pos.X, (int)pos.Y, tenantBitmap.Width, tenantBitmap.Height));

                if (tenant != null && (tenant.Team == _myGame.PlayerTeam || visibilityTracker > 0))
                {
                    // if the tenant is selected, draw selection box
                    if (_myInterface.SelectedCreature == tenant)
                    {
                        spriteBatch.Draw(_tiles[(sbyte)SpriteTile.HexRed], rectHex, Color.White);
                    }

                    // if the tenant is an enemy and within range of a friendly selected creature, draw an indication
                    if (_myInterface.SelectedCreature != null && _myInterface.SelectedCreature.Team == _myGame.PlayerTeam
                        && tenant.Team != _myGame.PlayerTeam && _myInterface.SelectedCreature.GetAttackRange() >=
                        StaticMathFunctions.DistanceBetweenTwoCoordsHex(_myInterface.SelectedCreature.PositionGet(), tenant.PositionGet()))
                    {
                        spriteBatch.Draw(_tiles[(sbyte)SpriteTile.HexRed], rectHex, Color.Red);
                    }

                    // If there is an active animation for the creature, draw it.
                    if (_creatureAnimations.ContainsKey(tenant.UniqueID))
                    {
                        List<AnimUnitMove> currentStack = _creatureAnimations[tenant.UniqueID];
                        AnimUnitMove current = currentStack.Last();
                        current.Draw(spriteBatch, _screenAnchor, Color.White, _zoom);
                    }
                    // Otherwise, draw static sprite of the creature.
                    else
                    {
                        spriteBatch.Draw(_creatures[(sbyte)tenant.MyBitmap], rectCreature, Color.White);

                        //HP bar:
                        float hpRatio = (float)tenant.GetHP() / (float)tenant.GetHPMax();
                        Vector2 barLocation = pos;
                        barLocation.X += 3 * Constants.TileSize / 4;
                        barLocation.Y += (1 - hpRatio) * Constants.TileSize;
                        Color drawColor = Color.Lerp(Color.Red, Color.Green, hpRatio);
                        drawColor.A = 64;
                        spriteBatch.Draw(_particles[(sbyte)SpriteParticle.ParticlePixel], ZoomTransform(new Rectangle((int)barLocation.X,
                            (int)barLocation.Y, Constants.TileSize / 12, (int)(hpRatio * Constants.TileSize))), drawColor);
                    }
                }
            }
        }

        private void DrawFloatingMessages(SpriteBatch spriteBatch)
        {
            // FLOATING MESSAGES
            for (int i = 0; i < this._floatingMessages.Count; ++i)
            {
                FloatingMessage currentMessage = _floatingMessages[i];
                currentMessage.Update();
                spriteBatch.DrawString(this._font, currentMessage.Text, ZoomTransform(currentMessage.Location + _screenAnchor), Color.Red);
                if (currentMessage.Timer <= 0)
                {
                    _floatingMessages.RemoveAt(i);
                    --i;
                }
            }
        }

        private void DrawAnimations(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _animations.Count; ++i)
            {
                _animations[i].Draw(spriteBatch, _screenAnchor, Color.White,_zoom);
            }
        }

        private void DrawButtonsAndStatBoxes(SpriteBatch spriteBatch)
        {
            // Stat Boxes
            for (int q = 0; q < (sbyte)InterfaceBattle.Buttons.COUNT; ++q)
            {
                if (_myInterface.IsBoxOpen((InterfaceBattle.Buttons)q))
                {
                    spriteBatch.Draw(_menuBox, _myInterface.GetBox((InterfaceBattle.Buttons)q), Constants.ColorHalfTransparent);
                }
            }

            DrawCharInfo(spriteBatch);

            // Buttons
            for (int q = 0; q < (sbyte)InterfaceBattle.Buttons.COUNT; ++q)
            {
                spriteBatch.Draw(_buttonArrow,  _myInterface.GetButton((InterfaceBattle.Buttons)q), Color.White);
            }
        }

        /// <summary>
        /// Writes the character info in the character info box.
        /// </summary>
        private void DrawCharInfo(SpriteBatch spriteBatch)
        {
            if (_myInterface.IsBoxOpen(InterfaceBattle.Buttons.CharacterInfo) && _myInterface.SelectedCreature != null)
            {
                Rectangle box = _myInterface.GetBox(InterfaceBattle.Buttons.CharacterInfo);

                Vector2 loc = new Vector2(box.Left + Constants.TextDisplayItemBoxDefaultXOffset,
                    box.Top + Constants.TextDisplayDefaultVerticalSpacing);
                spriteBatch.DrawString(_font, ("Name: " + _myInterface.SelectedCreature.ToString()), loc, Constants.ColorCharacterName);

                Creature subject = _myInterface.SelectedCreature;
                int delta = Constants.FontDefaultSize + Constants.TextDisplayDefaultVerticalSpacing;

                loc.Y += delta;
                spriteBatch.DrawString(_font, ("Level: " + _myInterface.SelectedCreature.Level), loc, Constants.ColorCharacterProperties);

                for (int i = 0; i < (sbyte)Creature.StatBasic.COUNT; ++i)
                {
                    loc.Y += delta;
                    spriteBatch.DrawString(_font, (((Creature.StatBasic)i).ToString() + ": " + subject.GetStatBasic((Creature.StatBasic)i, true) + "/"
                        + subject.GetStatBasic((Creature.StatBasic)i, false)), loc, Constants.ColorCharacterProperties);
                }

                for (int i = 0; i < (sbyte)Creature.StatMain.COUNT; ++i)
                {
                    loc.Y += delta;
                    spriteBatch.DrawString(_font, (((Creature.StatMain)i).ToString() + ": " + subject.GetStatMain((Creature.StatMain)i, true) +
                        "(" + subject.GetStatMain((Creature.StatMain)i, false) + ")"), loc, Constants.ColorCharacterProperties);
                }
            }
        }

        /// <summary>
        /// Draws an item info box for a selected/hovered item.
        /// </summary>
        private void DrawItemInfoBox(SpriteBatch spriteBatch)
        {
            Item subject = _myInterface.ItemHovered == null ? _myInterface.ItemSelected : _myInterface.ItemHovered;

            if (subject != null)
            {
                // Figuring out the box dimensions and coordinates.
                int lines = 2 + subject.ItemFunctions.Count;

                int delta = Constants.FontDefaultSize + Constants.TextDisplayDefaultVerticalSpacing;
                int boxHeight = lines * delta + 2 * Constants.TextDisplayDefaultVerticalSpacing;
                int boxWidth = Constants.TextDisplayItemBoxDefaultWidth;

                int coordX = (int)(Math.Min(Math.Max(0, _myInterface.MousePosition.X), _myGame.GraphicsDevice.Viewport.Width - boxWidth)*_zoom);
                int coordY = (int)(Math.Max(0, Math.Min(_myInterface.MousePosition.Y, _myGame.GraphicsDevice.Viewport.Height) - boxHeight)*_zoom);

                Rectangle box = new Rectangle(coordX, coordY, boxWidth, boxHeight);
                // Draws box
                spriteBatch.Draw(_menuBox, box, Constants.ColorHalfTransparent);

                Vector2 loc = new Vector2(coordX + Constants.TextDisplayItemBoxDefaultXOffset, coordY);
                loc.Y += Constants.TextDisplayDefaultVerticalSpacing;

                // Item Name
                spriteBatch.DrawString(_font, ("Name: " + subject.ToString()), loc, Constants.ColorItemName);

                loc.Y += delta;
                // Item Type
                spriteBatch.DrawString(_font, ("Type: " + subject.MyType), loc, Constants.ColorItemProperties);
                // Item Properties
                foreach (KeyValuePair<ItemProperty, float> kvp in subject.ItemFunctions)
                {
                    loc.Y += delta;
                    spriteBatch.DrawString(_font, kvp.Key + ": " + kvp.Value, loc, Constants.ColorItemProperties);
                }
            }
        }

        private void DrawSpellsBox(SpriteBatch spriteBatch)
        {
            // Skill points
            if (_myInterface.PlayableCreatureSelected())
            {
                Rectangle r = _myInterface.GetButton(InterfaceBattle.Buttons.Spells);
                spriteBatch.DrawString(_font, _myInterface.SelectedCreature.SkillPoints.ToString(),
                    new Vector2(r.X + r.Width / 3, r.Y + r.Height / 3), Color.Black);
            }

            // Spell buttons
            if (_myInterface.IsBoxOpen(InterfaceBattle.Buttons.Spells))
            {
                if (_myInterface.SelectedCreature != null)
                {
                    Creature selected = _myInterface.SelectedCreature;
                    for (int q = 0; q < selected.Spells.Count; ++q)
                    {
                        spriteBatch.Draw(_spells[(sbyte)selected.Spells[q].Type], _myInterface.GetSpellButton(q), Color.White);
                    }
                }
                if (_myInterface.PlayableCreatureSelected())
                {
                    foreach (Spell spell in _myInterface.SelectedCreature.Spells)
                    {
                        Rectangle r = _myInterface.GetSpellButton((sbyte)spell.Type);
                        spriteBatch.DrawString(_font, spell.Magnitude.ToString(), new Vector2(r.X, r.Y), Color.Red);
                    }
                }
            }

            DrawSpellInfoBox(spriteBatch);
        }

        private void DrawSpellInfoBox(SpriteBatch spriteBatch)
        {
            sbyte? hoveredSpell = _myInterface.HoveredSpell();
            if(hoveredSpell == null)
            {
                return;
            }

            String displayString = Constants.SpellDescriptions[hoveredSpell.Value];
            int width = Math.Min(Constants.MaxLineLength, displayString.Length)*Constants.FontDefaultSize;
            int height = ((displayString.Length / Constants.MaxLineLength) + 2)*Constants.FontDefaultSize;
            int coordX = (int)(Math.Min(Math.Max(0, _myInterface.MousePosition.X), _myGame.GraphicsDevice.Viewport.Width - width));
            int coordY = (int)(Math.Max(0, Math.Min(_myInterface.MousePosition.Y, _myGame.GraphicsDevice.Viewport.Height) - height) );

            Rectangle box = new Rectangle(coordX, coordY, width, height);

            spriteBatch.Draw(_menuBox, box, Constants.ColorHalfTransparent);
            spriteBatch.DrawString(_font, displayString, new Vector2(coordX + Constants.TextDisplayItemBoxDefaultXOffset, coordY), Constants.ColorSpellInfo);
        }

        #region Draw Mouse Cursor

        private void DrawMouseCursor(SpriteBatch spriteBatch)
        {
            // own creature selected case
            if (_myInterface.SelectedCreature != null && _myInterface.SelectedCreature.Team == _myGame.PlayerTeam)
            {
                DrawMouseCursor_OwnCreatureSelected(spriteBatch);
            }
            // base case
            else
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBasic], _myInterface.MousePosition, Color.White);
            }
        }

        private void DrawMouseCursor_OwnCreatureSelected(SpriteBatch spriteBatch)
        {
            if (_myInterface.ItemSelected != null)
            {
                spriteBatch.Draw(_items[(sbyte)_myInterface.ItemSelected.ItemBitmap], _myInterface.MousePosition, Color.White);
            }

            Creature targetTenant = _currentMap.TenancyMap[_myInterface.MouseHoverTile.X, _myInterface.MouseHoverTile.Y];
            Int16 visibilityTracker = _currentMap.MyVisibilityTracker.VisibilityCheck(_myInterface.MouseHoverTile, _myGame.PlayerTeam);
            bool passability = (_myInterface.SelectedCreature.GetAPMoveCost(_currentMap.GetTile(_myInterface.MouseHoverTile).MyTerrainType) > 0)
                && _currentMap.TenancyMap[_myInterface.MouseHoverTile.X, _myInterface.MouseHoverTile.Y] == null;

            InterfaceBattle.Buttons? clickType = _myInterface.DetermineIfButtonIsClicked();

            sbyte? spellSeleced = _myInterface.HoveredSpell();

            // cursor on a button
            if (clickType != null)
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBasic], _myInterface.MousePosition, Color.White);
            }
            // inventory selected
            else if (_myInterface.InventorySlotUnderCursor() && _myInterface.IsBoxOpen(InterfaceBattle.Buttons.Inventory))
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBasic], _myInterface.MousePosition, Color.White);
            }
            // cursor over spell
            else if (spellSeleced != null)
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBasic], _myInterface.MousePosition, Color.White);
            }
            // cursor on gamespace
            else
            {
                if (targetTenant == null)
                {
                    DrawMouseCursor_OwnCreatureSelected_CursorOnTerrain(spriteBatch, visibilityTracker, passability, targetTenant);
                }
                else
                {
                    DrawMouseCursor_OwnCreatureSelected_CursorOnCreature(spriteBatch, targetTenant);
                }
            }
        }

        private void DrawMouseCursor_OwnCreatureSelected_CursorOnTerrain(SpriteBatch spriteBatch, Int16 visibilityTracker, 
            bool passability, Creature targetTenant)
        {
            // unexplored terrain
            if (visibilityTracker < 0)
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBoots], _myInterface.MousePosition, Color.White);
            }
            // fogged passable terrain
            else if (visibilityTracker == 0 && (passability || targetTenant != null))
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBoots], _myInterface.MousePosition, Color.White);
            }
            // impassable terrain
            else if (visibilityTracker >= 0 && !passability && targetTenant == null)
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseForbidden], _myInterface.MousePosition, Color.White);
            }
            // mouse hovers over free visible hex
            else if (targetTenant == null && passability)
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBoots], _myInterface.MousePosition, Color.White);
            }
        }

        private void DrawMouseCursor_OwnCreatureSelected_CursorOnCreature(SpriteBatch spriteBatch, Creature targetTenant)
        {
            // mouse over self; item on ground
            if (targetTenant == _myInterface.SelectedCreature &&
                _currentMap.GetTile(_myInterface.MouseHoverTile.X, _myInterface.MouseHoverTile.Y).MyInventory.Size() > 0)
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MousePickup], _myInterface.MousePosition, Color.White);
            }
            // mouse over friendly
            else if (targetTenant.Team == _myGame.PlayerTeam)
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseForbidden], _myInterface.MousePosition, Color.White);
            }
            // mouse over visible enemy
            else if (targetTenant.Team != _myGame.PlayerTeam)
            {
                // If enemy is visible draw attack cursor.
                if (_myGame.PlayerTeam.EnemyIsObserved(targetTenant))
                {
                    spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseAttack], _myInterface.MousePosition, Color.White);
                    // draw a spell-icon next to the cursor
                    int squareside = _mouseCursors[(sbyte)SpriteMouse.MouseAttack].Width;
                    Rectangle r = new Rectangle((int)(_myInterface.MousePosition.X + squareside), (int)(_myInterface.MousePosition.Y), squareside, squareside);
                    spriteBatch.Draw(_spells[(sbyte)_myInterface.SelectedCreature.SpellCurrent.Type], r, Color.White);
                }
                else // if not visible draw boots.
                {
                    spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBoots], _myInterface.MousePosition, Color.White);
                }
            }
            // general case (This should be unreachable!)
            else
            {
                spriteBatch.Draw(_mouseCursors[(sbyte)SpriteMouse.MouseBasic], _myInterface.MousePosition, Color.White);
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Enqueues the animation for a moving agent.
        /// </summary>
        public void AddMovementAnimation(Unit agent, List<Direction> route)
        {
            Coords current = agent.PositionGet();

            if (!_creatureAnimations.ContainsKey(agent.UniqueID))
            {
                _creatureAnimations.Add(agent.UniqueID, new List<AnimUnitMove>());
            }

            List<AnimUnitMove> moveStack = _creatureAnimations[agent.UniqueID];

            Coords nextGoal;
            for (int i = 0; i < route.Count; ++i)
            {
                nextGoal = current.NeighborInDirection(route[route.Count - 1 - i]);
                moveStack.Add(new AnimUnitMove(agent, Constants.AnimCreatureMoveBaseDuration, _creatures[(sbyte)agent.MyBitmap],
                    _myInterface.HexPosition(current), _myInterface.HexPosition(nextGoal)));
                current = nextGoal;
            }
            moveStack.Reverse();
        }

        public DrawerBattle(Game1 myGame, ContentManager content, InterfaceBattle myInterface) : base(myGame, content)
        {
            _myInterface = myInterface;

            _zoom = Constants.ZoomDefault;

            //_myMoveRangeCalculator = new MoveRangeCalculator(_currentMap);
            _currentMap = myGame.CurrentMap;

            Load_content();
        }

        public void SetCurrentMap(MapBattle map)
        {
            _currentMap = map;
        }
    }

    public class FloatingMessage
    {
        private UInt16 _timer;
        public UInt16 Timer
        {
            get
            {
                return _timer;
            }
        }
        public readonly String Text;
        private Vector2 _location;
        public Vector2 Location
        {
            get
            {
                return _location;
            }
        }

        public void Update()
        {
            _location.Y -= Constants.FloatingTextDefaultSpeed;
            --_timer;
        }

        public FloatingMessage(UInt16 time, String messageText, Vector2 messageLocation)
        {
            this.Text = messageText;
            this._timer = time;
            this._location = messageLocation;
        }
    }
}
