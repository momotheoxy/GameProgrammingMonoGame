using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TicTacToe
{
    public class TicTacToe : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D _line, _circle, _cross;

        bool _isCircleTurn;
        bool _isGameEnded;
        bool _isOWon;

        int[,] _gameTable;

        Point _win1, _win2;

        public TicTacToe()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.PreferredBackBufferWidth = 600;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = 600;   // set this value to the desired height of your window
            _graphics.ApplyChanges();

            _gameTable = new int[3, 3];

            _isCircleTurn = true;
            _isGameEnded = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load a Texture2D from a file
            _line = this.Content.Load<Texture2D>("Line");
            _circle = this.Content.Load<Texture2D>("Circle");
            _cross = this.Content.Load<Texture2D>("Cross");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState state = Mouse.GetState();

            if (state.LeftButton == ButtonState.Pressed && !_isGameEnded)
            {
                int iPos = state.X / 200;
                int jPos = state.Y / 200;

                if (iPos >= 0 && iPos < 3 && jPos >= 0 && jPos < 3)
                {
                    //check feasibility
                    if (_gameTable[jPos, iPos] == 0)
                    {
                        if (_isCircleTurn)
                        {
                            _gameTable[jPos, iPos] = 1;
                        }
                        else
                        {
                            _gameTable[jPos, iPos] = -1;
                        }

                        //flip turn
                        _isCircleTurn = !_isCircleTurn;
                    }
                }
            }

            //check winning condition
            if (_gameTable[0, 0] == _gameTable[1, 0] &&
                _gameTable[1, 0] == _gameTable[2, 0] &&
                _gameTable[0, 0] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(0, 0);
                _win2 = new Point(0, 2);
                _isOWon = _gameTable[0, 0] > 0 ? true : false;
            }
            else if (_gameTable[0, 1] == _gameTable[1, 1] &&
                _gameTable[1, 1] == _gameTable[2, 1] &&
                _gameTable[0, 1] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(1, 0);
                _win2 = new Point(1, 2);
                _isOWon = _gameTable[1, 0] > 0 ? true : false;
            }
            else if (_gameTable[0, 2] == _gameTable[1, 2] &&
               _gameTable[1, 2] == _gameTable[2, 2] &&
                _gameTable[0, 2] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(2, 0);
                _win2 = new Point(2, 2);
                _isOWon = _gameTable[2, 0] > 0 ? true : false;
            }
            else if (_gameTable[0, 0] == _gameTable[0, 1] &&
               _gameTable[0, 1] == _gameTable[0, 2] &&
                _gameTable[0, 0] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(0, 0);
                _win2 = new Point(2, 0);
                _isOWon = _gameTable[0, 0] > 0 ? true : false;
            }
            else if (_gameTable[1, 0] == _gameTable[1, 1] &&
               _gameTable[1, 1] == _gameTable[1, 2] &&
                _gameTable[1, 0] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(0, 1);
                _win2 = new Point(2, 1);
                _isOWon = _gameTable[1, 0] > 0 ? true : false;
            }
            else if (_gameTable[2, 0] == _gameTable[2, 1] &&
              _gameTable[2, 1] == _gameTable[2, 2] &&
                _gameTable[2, 0] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(0, 2);
                _win2 = new Point(2, 2);
                _isOWon = _gameTable[2, 0] > 0 ? true : false;
            }
            else if (_gameTable[0, 0] == _gameTable[1, 1] &&
             _gameTable[1, 1] == _gameTable[2, 2] &&
                _gameTable[0, 0] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(0, 0);
                _win2 = new Point(2, 2);
                _isOWon = _gameTable[0, 0] > 0 ? true : false;
            }
            else if (_gameTable[2, 0] == _gameTable[1, 1] &&
            _gameTable[1, 1] == _gameTable[0, 2] &&
                _gameTable[2, 0] != 0)
            {
                _isGameEnded = true;
                _win1 = new Point(0, 2);
                _win2 = new Point(2, 0);
                _isOWon = _gameTable[2, 0] > 0 ? true : false;
            }


            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();


            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (_gameTable[i, j] == 0)
                    {
                        //nothing
                    }
                    else if (_gameTable[i, j] == 1)
                    {
                        //circle
                        _spriteBatch.Draw(_circle, new Vector2(200 * j, 200 * i), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                    else if (_gameTable[i, j] == -1)
                    {
                        //cross
                        _spriteBatch.Draw(_cross, new Vector2(200 * j, 200 * i), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }
            }

            //Lines
            _spriteBatch.Draw(_line, new Vector2(0, 200), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_line, new Vector2(0, 400), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);


            _spriteBatch.Draw(_line, new Vector2(200, 0), null, Color.White, MathHelper.Pi / 2, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.Draw(_line, new Vector2(400, 0), null, Color.White, MathHelper.Pi / 2, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            //Finish line
            if (_isGameEnded)
            {
                if (_win1.X == _win2.X)
                {
                    _spriteBatch.Draw(_line, new Vector2(_win1.X * 200 + 100, 0), null, Color.Red, MathHelper.Pi / 2, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                else if (_win1.Y == _win2.Y)
                {
                    _spriteBatch.Draw(_line, new Vector2(0, _win1.Y * 200 + 100), null, Color.Red, 0, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
                else if (_win1.X == _win1.Y)
                {
                    _spriteBatch.Draw(_line, new Vector2(0, 0), null, Color.Red, MathHelper.Pi / 4, Vector2.Zero, 1.414f, SpriteEffects.None, 0f);
                }
                else if (_win2.X == _win1.Y)
                {
                    _spriteBatch.Draw(_line, new Vector2(0, 600), null, Color.Red, -MathHelper.Pi / 4, Vector2.Zero, 1.414f, SpriteEffects.None, 0f);
                }
            }


            _spriteBatch.End();

            _graphics.BeginDraw();

            base.Draw(gameTime);
        }
    }
}
