using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace XNATBS
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public enum GameState
        {
            StartMenu = 0,
            Battle
        }

        #region Properties

        GameState _state;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private InterfaceBattle _myInterface;
        private InterfaceStartMenu _myInterfaceMenu;
        private DrawerBattle _myDrawer;
        private DrawerStartMenu _myDrawerStartMenu;

        private Team _playerTeam;
        public Team PlayerTeam
        {
            get
            {
                return _playerTeam;
            }
        }
        private MapBattle _currentMap;
        public MapBattle CurrentMap
        {
            get
            {
                return _currentMap;
            }
        }
        private Scheduler _scheduler;
        public Scheduler MyScheduler
        {
            get
            {
                return _scheduler;
            }
        }
        private Ledger _ledger;

        private RandomStuff _randomator;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1024;

            _state = GameState.StartMenu;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            this.IsMouseVisible = true;

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _myInterfaceMenu = new InterfaceStartMenu(this);
            _myDrawerStartMenu = new DrawerStartMenu(this, Content, _myInterfaceMenu);

            _randomator = new RandomStuff(907);

            _myInterface = new InterfaceBattle(this);
            _myDrawer = new DrawerBattle(this, Content, _myInterface);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyState = Keyboard.GetState();

            switch (_state)
            {
                case GameState.StartMenu:
                    _myInterfaceMenu.Update(gameTime, mouseState, keyState);
                    break;
                case GameState.Battle:
                    _myInterface.Update(gameTime, mouseState, keyState);
                    _myDrawer.Update(gameTime);
                    this._scheduler.Update();
                    break;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (_state)
            {
                case GameState.StartMenu:
                    _myDrawerStartMenu.Draw(gameTime, spriteBatch);
                    break;
                case GameState.Battle:
                    _myDrawer.Draw(gameTime, spriteBatch);
                    break;
            }
            
            base.Draw(gameTime);
        }

        public void SwitchGameState(GameState newState)
        {
            _state = newState;
            if (_state == GameState.Battle)
            {
                this.IsMouseVisible = false;
                StartNewBattle();
            }
        }

        private void StartNewBattle()
        {
            MapGenerator mapGen = new MapGenerator(_randomator, _myInterface, _myDrawer);
            _currentMap = mapGen.GenerateBasicGrasslands(Constants.MapSize, Constants.MapSize);
            this._playerTeam = _currentMap.TeamRosterGetTeamFrom(0);

            _currentMap.SetTile(new Coords(0, 0), new Tile(_currentMap, new Coords(CoordsType.Tile, 0, 0), Constants.TileGeneratorGrass));
            _currentMap.SetTile(new Coords(0, 1), new Tile(_currentMap, new Coords(CoordsType.Tile, 0, 1), Constants.TileGeneratorGrass));
            _currentMap.SetTile(new Coords(3, 3), new Tile(_currentMap, new Coords(CoordsType.Tile, 3, 3), Constants.TileGeneratorGrass));
            _currentMap.AnalyzeTileAccessibility();

            _scheduler = new Scheduler(_currentMap);
            _ledger = new Ledger(_scheduler);

            _myInterface.MyDrawer = _myDrawer;

            Creature hero1 = _currentMap.CreateCreature(new Coords(CoordsType.Tile, 0, 0), _currentMap.TeamRosterGetTeamFrom(0),
                Constants.CreatureGeneratorHero, new BrainPlayerControlled(), _myDrawer, _myInterface);

            Creature hero2 = _currentMap.CreateCreature(new Coords(CoordsType.Tile, 0, 1), _currentMap.TeamRosterGetTeamFrom(0),
                Constants.CreatureGeneratorHero, new BrainPlayerControlled(), _myDrawer, _myInterface);
            hero2.SkillPointsAdd(5);

            _currentMap.CreateItem(hero1, Constants.ItemGeneratorSword);
            _currentMap.CreateItem(hero2, Constants.ItemGeneratorSword);
            _currentMap.CreateItem(hero2, Constants.ItemGeneratorBow);
        }

        public void EndBattle()
        {
            _state = GameState.StartMenu;
            this.IsMouseVisible = true;
        }

        public void EndTurn()
        {
            SortedDictionary<UInt32, Creature> menagerie = _currentMap.Menagerie;
            // end turn
            _scheduler.EndTurn();

            if (_myInterface.SelectedCreature != null)
            {
                _myInterface.SelectedCreature.MyMoveRangeCalculator.Update();
            }

            // this crap should be in the scheduler
            foreach(KeyValuePair<UInt32, Creature> kvp in menagerie)
            {
                //fix later
                Creature current = kvp.Value;
                current.SetStatBasic(Creature.StatBasic.AP, true, current.GetStatBasic(Creature.StatBasic.AP, false));
                current.MyMoveRangeCalculator.Update();
            }
            for (sbyte i = 0; i < _currentMap.TeamCount; ++i)
            {
                Team currentTeam = _currentMap.TeamRosterGetTeamFrom(i);
                if (currentTeam != _playerTeam)
                {
                    foreach (Creature c in currentTeam.Members)
                    {
                        c.CreatureBrain.Update();
                    }
                }
            }
        }

        /// <summary>
        /// Responsible for executing an action and recording its output.
        /// </summary>
        public void ExecuteAction(Action action)
        {
            Message output = action.Execute();
            _ledger.RecordMessage(output);
        }

    }
}
