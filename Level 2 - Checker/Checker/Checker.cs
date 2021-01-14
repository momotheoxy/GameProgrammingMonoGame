using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Checker
{
    public class Checker : Game
    {
        const int _TILESIZE = 75;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        enum PlayerTurn
        {
            RedTurn,
            YellowTurn
        }

        PlayerTurn _currentPlayerTurn;

        bool _isBeatedChipExist;
        Point _selectedTile;

        List<Point> _possibleClicked;

        enum GameState
        {
            TurnBeginning,
            WaitingForSelection,
            ChipSelected,
            TurnEnded,
            GameEnded
        }

        GameState _currentGameState;

        MouseState _mouseState, _previousMouseState;
        Point _clickedPos;

        Texture2D _chip, _horse, _rect;

        int[,] _gameTable;

        private SpriteFont _font;

        public Checker()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 600;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = 600;   // set this value to the desired height of your window
            _graphics.ApplyChanges();

            _currentPlayerTurn = PlayerTurn.RedTurn;

            _currentGameState = GameState.TurnBeginning;

            _gameTable = new int[8, 8]
            {
                { 1,0,1,0,1,0,1,0},
                { 0,1,0,1,0,1,0,1},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { -1,0,-1,0,-1,0,-1,0},
                { 0,-1,0,-1,0,-1,0,-1}
            };

            _possibleClicked = new List<Point>();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _chip = this.Content.Load<Texture2D>("Chip");
            _horse = this.Content.Load<Texture2D>("Horse");

            _rect = new Texture2D(_graphics.GraphicsDevice, _TILESIZE, _TILESIZE);

            Color[] data = new Color[_TILESIZE * _TILESIZE];
            for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
            _rect.SetData(data);

            _font = Content.Load<SpriteFont>("GameText");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousMouseState = _mouseState;


            switch (_currentGameState)
            {
                case GameState.TurnBeginning:
                    //Search for available move
                    _possibleClicked.Clear();
                    _isBeatedChipExist = false;

                    //force move
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (_currentPlayerTurn == PlayerTurn.RedTurn)
                            {
                                //Red turn
                                if (_gameTable[j, i] == -1 ||
                                    _gameTable[j, i] == -2)
                                {
                                    if (FindBeatableMoves(new Point(i, j)).Count != 0)
                                    {
                                        _possibleClicked.Add(new Point(i, j));
                                        _isBeatedChipExist = true;
                                    }
                                }
                            }
                            else
                            {
                                //Yellow turn
                                if (_gameTable[j, i] == 1 ||
                                    _gameTable[j, i] == 2)
                                {
                                    if (FindBeatableMoves(new Point(i, j)).Count != 0)
                                    {
                                        _possibleClicked.Add(new Point(i, j));
                                        _isBeatedChipExist = true;
                                    }
                                }
                            }
                        }
                    }

                    //if beatable move available, force to do so. else, find next available moves
                    if (_possibleClicked.Count == 0)

                    {
                        //normal move
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                if (_currentPlayerTurn == PlayerTurn.RedTurn)
                                {
                                    //Red turn
                                    if (_gameTable[j, i] == -1 ||
                                        _gameTable[j, i] == -2)
                                    {
                                        if (FindPossibleMoves(new Point(i, j)).Count != 0)
                                        {
                                            _possibleClicked.Add(new Point(i, j));
                                        }
                                    }
                                }
                                else
                                {
                                    //Yellow turn
                                    if (_gameTable[j, i] == 1 ||
                                        _gameTable[j, i] == 2)
                                    {
                                        if (FindPossibleMoves(new Point(i, j)).Count != 0)
                                        {
                                            _possibleClicked.Add(new Point(i, j));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //if no move available, this player lose the game
                    if (_possibleClicked.Count == 0)
                    {
                        _currentGameState = GameState.GameEnded;
                    }
                    else
                    {
                        _currentGameState = GameState.WaitingForSelection;
                    }

                    break;
                case GameState.WaitingForSelection:
                    _mouseState = Mouse.GetState();

                    if (_mouseState.LeftButton == ButtonState.Pressed &&
                        _previousMouseState.LeftButton == ButtonState.Released)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        _clickedPos = new Point(xPos, yPos);

                        if (_possibleClicked.Contains(_clickedPos))
                        {
                            _possibleClicked.Clear();
                            _selectedTile = _clickedPos;
                            List<Point> beatableMove = FindBeatableMoves(_selectedTile);
                            if (beatableMove.Count > 0)
                            {
                                _possibleClicked.AddRange(beatableMove);
                                _isBeatedChipExist = true;
                            }
                            else
                            {
                                _possibleClicked.AddRange(FindPossibleMoves(_selectedTile));
                            }
                            _currentGameState = GameState.ChipSelected;
                        }
                    }
                    break;
                case GameState.ChipSelected:
                    _mouseState = Mouse.GetState();

                    if (_mouseState.LeftButton == ButtonState.Pressed &&
                        _previousMouseState.LeftButton == ButtonState.Released)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        _clickedPos = new Point(xPos, yPos);

                        if (!_possibleClicked.Contains(_clickedPos) && _clickedPos != _selectedTile)
                        {
                            //invalid moves
                            _possibleClicked.Clear();
                            _currentGameState = GameState.TurnBeginning;
                        }
                        else if (_possibleClicked.Contains(_clickedPos))
                        {
                            //available move selected
                            //TODO: animation
                            _gameTable[_clickedPos.Y, _clickedPos.X] = _gameTable[_selectedTile.Y, _selectedTile.X];
                            _gameTable[_selectedTile.Y, _selectedTile.X] = 0;
                            //handle beated chip
                            if (_isBeatedChipExist)
                            {
                                Point beatedChip = new Point();
                                beatedChip.X = _clickedPos.X < _selectedTile.X ? _clickedPos.X + 1 : _clickedPos.X - 1;
                                beatedChip.Y = _clickedPos.Y < _selectedTile.Y ? _clickedPos.Y + 1 : _clickedPos.Y - 1;
                                _gameTable[beatedChip.Y, beatedChip.X] = 0;
                            }

                            _currentGameState = GameState.TurnEnded;
                        }
                    }
                    break;
                case GameState.TurnEnded:
                    //to clear up when turn is ended

                    //if a chip reaches the end of the board, it becomes a horse
                    if (_currentPlayerTurn == PlayerTurn.RedTurn)
                    {
                        if (_clickedPos.Y == 0)
                        {
                            _gameTable[_clickedPos.Y, _clickedPos.X] = -2;
                        }
                    }
                    else
                    {
                        if (_clickedPos.Y == 7)
                        {
                            _gameTable[_clickedPos.Y, _clickedPos.X] = 2;
                        }
                    }

                    //if a chip is beated, check for additional move

                    if (_isBeatedChipExist)
                    {
                        _selectedTile = _clickedPos;
                        List<Point> possibleBeatMove = FindBeatableMoves(_selectedTile);

                        if (possibleBeatMove.Count > 0)
                        {
                            //beatable chips exist
                            _possibleClicked.Clear();
                            _possibleClicked.AddRange(possibleBeatMove);
                            _currentGameState = GameState.ChipSelected;
                        }
                        else
                        {
                            _currentPlayerTurn = _currentPlayerTurn == PlayerTurn.RedTurn ? PlayerTurn.YellowTurn : PlayerTurn.RedTurn;
                            _currentGameState = GameState.TurnBeginning;
                        }
                    }
                    else
                    {
                        _currentPlayerTurn = _currentPlayerTurn == PlayerTurn.RedTurn ? PlayerTurn.YellowTurn : PlayerTurn.RedTurn;
                        _currentGameState = GameState.TurnBeginning;
                    }
                    break;
                case GameState.GameEnded:
                    break;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            _spriteBatch.Begin();

            //Draw board
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    //draw checker board
                    if ((i + j) % 2 == 0)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }
            }

            //draw chips
            switch (_currentGameState)
            {
                case GameState.TurnBeginning:
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                            }
                        }
                    }
                    break;
                case GameState.WaitingForSelection:
                    //draw possible clicked
                    foreach (Point p in _possibleClicked)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * p.X, _TILESIZE * p.Y), null, Color.DarkGreen, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                            }
                        }
                    }
                    break;
                case GameState.ChipSelected:
                    //draw selected tile
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * _selectedTile.X, _TILESIZE * _selectedTile.Y), null, Color.Blue, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                    //draw possible move
                    foreach (Point v in _possibleClicked)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * v.X, _TILESIZE * v.Y), null, Color.LimeGreen, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                            }
                        }
                    }
                    break;
                case GameState.GameEnded:
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            switch (_gameTable[i, j])
                            {
                                case 1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -1:
                                    _spriteBatch.Draw(_chip, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case 2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Yellow, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                                case -2:
                                    _spriteBatch.Draw(_horse, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                                    break;
                            }
                        }
                    }

                    //display winning text
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * (8 - 4) / 2, _TILESIZE * (8 - 2) / 2), null, Color.LightGray, 0f, Vector2.Zero, new Vector2(4, 2), SpriteEffects.None, 0f);



                    //if red found unmovable, yellow wins
                    if (_currentPlayerTurn == PlayerTurn.RedTurn)
                    {
                        Vector2 fontSize = _font.MeasureString("Yellow wins");
                        _spriteBatch.DrawString(_font, "Yellow wins", new Vector2((_TILESIZE * 8 - fontSize.X) / 2, (_TILESIZE * 8 - fontSize.Y) / 2), Color.YellowGreen);
                    }
                    //else, red wins
                    else
                    {
                        Vector2 fontSize = _font.MeasureString("Red wins");
                        _spriteBatch.DrawString(_font, "Red wins", new Vector2((_TILESIZE * 8 - fontSize.X) / 2, (_TILESIZE * 8 - fontSize.Y) / 2), Color.DarkRed);
                    }
                    break;
            }

            _spriteBatch.End();

            _graphics.BeginDraw();

            base.Draw(gameTime);
        }

        protected List<Point> FindBeatableMoves(Point currentTile)
        {
            List<Point> returnVectors = new List<Point>();

            if (_gameTable[currentTile.Y, currentTile.X] == -1)
            {
                //red normal chip
                //check up-left
                if (currentTile.X - 1 >= 0 && currentTile.Y - 1 >= 0)
                {
                    if ((_gameTable[currentTile.Y - 1, currentTile.X - 1] == 1 ||
                        _gameTable[currentTile.Y - 1, currentTile.X - 1] == 2) &&
                        currentTile.X - 2 >= 0 && currentTile.Y - 2 >= 0 &&
                        _gameTable[currentTile.Y - 2, currentTile.X - 2] == 0)
                    {
                        //opponent found and winnable
                        returnVectors.Add(new Point(currentTile.X - 2, currentTile.Y - 2));
                    }
                }
                //check up-right
                if (currentTile.X + 1 < 8 && currentTile.Y - 1 >= 0)
                {
                    if ((_gameTable[currentTile.Y - 1, currentTile.X + 1] == 1 ||
                        _gameTable[currentTile.Y - 1, currentTile.X + 1] == 2) &&
                        currentTile.X + 2 < 8 && currentTile.Y - 2 >= 0 &&
                        _gameTable[currentTile.Y - 2, currentTile.X + 2] == 0)
                    {
                        //opponent found and winnable
                        returnVectors.Add(new Point(currentTile.X + 2, currentTile.Y - 2));
                    }
                }
            }
            else if (_gameTable[currentTile.Y, currentTile.X] == -2)
            {
                Point checkingPoint = currentTile;
                bool beatableChipFound;
                //red horse chip
                //check up-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y--;
                beatableChipFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y >= 0 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == 2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X - 1 >= 0 && checkingPoint.Y - 1 >= 0 &&
                        _gameTable[checkingPoint.Y - 1, checkingPoint.X - 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X - 1, checkingPoint.Y - 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X--;
                    checkingPoint.Y--;
                }
                //check up-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y--;
                beatableChipFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y >= 0 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == 2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X + 1 < 8 && checkingPoint.Y - 1 >= 0 &&
                        _gameTable[checkingPoint.Y - 1, checkingPoint.X + 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X + 1, checkingPoint.Y - 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X++;
                    checkingPoint.Y--;
                }
                //check down-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y++;
                beatableChipFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y < 8 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == 2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X - 1 >= 0 && checkingPoint.Y + 1 < 8 &&
                        _gameTable[checkingPoint.Y + 1, checkingPoint.X - 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X - 1, checkingPoint.Y + 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X--;
                    checkingPoint.Y++;
                }
                //check down-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y++;
                beatableChipFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y < 8 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == 2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X + 1 < 8 && checkingPoint.Y + 1 < 8 &&
                        _gameTable[checkingPoint.Y + 1, checkingPoint.X + 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X + 1, checkingPoint.Y + 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X++;
                    checkingPoint.Y++;
                }
            }
            else if (_gameTable[currentTile.Y, currentTile.X] == 1)
            {
                //yellow normal chip
                //check down-left
                if (currentTile.X - 1 >= 0 && currentTile.Y + 1 < 8)
                {
                    if ((_gameTable[currentTile.Y + 1, currentTile.X - 1] == -1 ||
                        _gameTable[currentTile.Y + 1, currentTile.X - 1] == -2) &&
                        currentTile.X - 2 >= 0 && currentTile.Y + 2 < 8 &&
                        _gameTable[currentTile.Y + 2, currentTile.X - 2] == 0)
                    {
                        //opponent found and winnable
                        returnVectors.Add(new Point(currentTile.X - 2, currentTile.Y + 2));
                    }
                }
                //check down-right
                if (currentTile.X + 1 < 8 && currentTile.Y + 1 < 8)
                {
                    if ((_gameTable[currentTile.Y + 1, currentTile.X + 1] == -1 ||
                        _gameTable[currentTile.Y + 1, currentTile.X + 1] == -2) &&
                        currentTile.X + 2 < 8 && currentTile.Y + 2 < 8 &&
                        _gameTable[currentTile.Y + 2, currentTile.X + 2] == 0)
                    {
                        //opponent found and winnable
                        returnVectors.Add(new Point(currentTile.X + 2, currentTile.Y + 2));
                    }
                }
            }
            else if (_gameTable[currentTile.Y, currentTile.X] == 2)
            {
                Point checkingPoint = currentTile;
                bool beatableChipFound;
                //yellow horse chip
                //check up-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y--;
                beatableChipFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y >= 0 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == -1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == -2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X - 1 >= 0 && checkingPoint.Y - 1 >= 0 &&
                        _gameTable[checkingPoint.Y - 1, checkingPoint.X - 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X - 1, checkingPoint.Y - 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X--;
                    checkingPoint.Y--;
                }
                //check up-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y--;
                beatableChipFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y >= 0 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == -1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == -2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X + 1 < 8 && checkingPoint.Y - 1 >= 0 &&
                        _gameTable[checkingPoint.Y - 1, checkingPoint.X + 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X + 1, checkingPoint.Y - 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X++;
                    checkingPoint.Y--;
                }
                //check down-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y++;
                beatableChipFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y < 8 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == -1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == -2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X - 1 >= 0 && checkingPoint.Y + 1 < 8 &&
                        _gameTable[checkingPoint.Y + 1, checkingPoint.X - 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X - 1, checkingPoint.Y + 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X--;
                    checkingPoint.Y++;
                }
                //check down-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y++;
                beatableChipFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y < 8 &&
                    !beatableChipFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == -1 ||
                        _gameTable[checkingPoint.Y, checkingPoint.X] == -2)
                    {
                        //found opponent chip
                        //check for space
                        if (checkingPoint.X + 1 < 8 && checkingPoint.Y + 1 < 8 &&
                        _gameTable[checkingPoint.Y + 1, checkingPoint.X + 1] == 0)
                        {
                            returnVectors.Add(new Point(checkingPoint.X + 1, checkingPoint.Y + 1));
                            beatableChipFound = true;
                        }
                    }
                    checkingPoint.X++;
                    checkingPoint.Y++;
                }
            }

            return returnVectors;
        }

        protected List<Point> FindPossibleMoves(Point currentTile)
        {
            List<Point> returnVectors = new List<Point>();

            if (_gameTable[currentTile.Y, currentTile.X] == -1)
            {
                //red normal chip
                //check up-left
                if (currentTile.X - 1 >= 0 && currentTile.Y - 1 >= 0)
                {
                    if (_gameTable[currentTile.Y - 1, currentTile.X - 1] == 0)
                    {
                        returnVectors.Add(new Point(currentTile.X - 1, currentTile.Y - 1));
                    }
                }
                //check up-right
                if (currentTile.X + 1 < 8 && currentTile.Y - 1 >= 0)
                {
                    if (_gameTable[currentTile.Y - 1, currentTile.X + 1] == 0)
                    {
                        returnVectors.Add(new Point(currentTile.X + 1, currentTile.Y - 1));
                    }
                }
            }
            else if (_gameTable[currentTile.Y, currentTile.X] == -2)
            {
                //red horse chip

                Point checkingPoint = currentTile;
                bool notEmptyFound;
                //red horse chip
                //check up-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y--;
                notEmptyFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y >= 0 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X--;
                    checkingPoint.Y--;
                }
                //check up-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y--;
                notEmptyFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y >= 0 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X++;
                    checkingPoint.Y--;
                }
                //check down-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y++;
                notEmptyFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y < 8 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X--;
                    checkingPoint.Y++;
                }
                //check down-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y++;
                notEmptyFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y < 8 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X++;
                    checkingPoint.Y++;
                }
            }
            else if (_gameTable[currentTile.Y, currentTile.X] == 1)
            {
                //yellow normal chip
                //check down-left
                if (currentTile.X - 1 >= 0 && currentTile.Y + 1 < 8)
                {
                    if (_gameTable[currentTile.Y + 1, currentTile.X - 1] == 0)
                    {
                        returnVectors.Add(new Point(currentTile.X - 1, currentTile.Y + 1));
                    }
                }
                //check down-right
                if (currentTile.X + 1 < 8 && currentTile.Y + 1 < 8)
                {
                    if (_gameTable[currentTile.Y + 1, currentTile.X + 1] == 0)
                    {
                        returnVectors.Add(new Point(currentTile.X + 1, currentTile.Y + 1));
                    }
                }
            }
            else if (_gameTable[currentTile.Y, currentTile.X] == 2)
            {
                //yellow horse chip

                Point checkingPoint = currentTile;
                bool notEmptyFound;
                //red horse chip
                //check up-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y--;
                notEmptyFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y >= 0 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X--;
                    checkingPoint.Y--;
                }
                //check up-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y--;
                notEmptyFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y >= 0 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X++;
                    checkingPoint.Y--;
                }
                //check down-left
                checkingPoint = currentTile;
                checkingPoint.X--;
                checkingPoint.Y++;
                notEmptyFound = false;
                while (checkingPoint.X >= 0 &&
                    checkingPoint.Y < 8 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X--;
                    checkingPoint.Y++;
                }
                //check down-right
                checkingPoint = currentTile;
                checkingPoint.X++;
                checkingPoint.Y++;
                notEmptyFound = false;
                while (checkingPoint.X < 8 &&
                    checkingPoint.Y < 8 &&
                    !notEmptyFound)
                {
                    if (_gameTable[checkingPoint.Y, checkingPoint.X] == 0)
                    {
                        returnVectors.Add(new Point(checkingPoint.X, checkingPoint.Y));
                    }
                    else
                    {
                        notEmptyFound = true;
                    }
                    checkingPoint.X++;
                    checkingPoint.Y++;
                }
            }

            return returnVectors;
        }
    }
}
