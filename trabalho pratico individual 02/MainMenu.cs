using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Microsoft.Xna.Framework;

namespace trabalho_pratico_individual_02
{
    public class MainMenu
    {
        private Texture2D _backgroundTexture;
        private List<Enemy> _menuEnemies = new List<Enemy>();
        private SpriteFont _font;
        private Texture2D _buttonTexture;

        protected Button _startButton;
        private Button _highScoreButton;

        private Random _random = new Random();

        public MainMenu(Texture2D background, Texture2D buttonTex, SpriteFont font,
            Texture2D[] zombieIdle, Texture2D[] zombieWalk,
            Texture2D[] skeletonIdle, Texture2D[] skeletonWalk)
        {
            _backgroundTexture = background;
            _buttonTexture = buttonTex;
            _font = font;


            _startButton = new Button(Rectangle.Empty, "Start", _font, () =>
            {
                Game1.Instance._startSound.Play();
                Game1.Instance._backgroundSound.Play();
                Game1.Instance.CurrentGameState = Game1.GameState.Playing;
            });

            _highScoreButton = new Button(Rectangle.Empty, "Score", _font, () =>
            {
                Game1.Instance.CurrentGameState = Game1.GameState.HighScore;
            });
        }

        public void ResizeButtons(Viewport viewport)
        {
            int buttonWidth = 200;
            int buttonHeight = 60;

            int centerX = viewport.Width / 2;
            int centerY = viewport.Height / 2;

            _startButton.Bounds = new Rectangle(centerX - buttonWidth / 2, centerY - 40, buttonWidth, buttonHeight);
            _highScoreButton.Bounds = new Rectangle(centerX - buttonWidth / 2, centerY + 40, buttonWidth, buttonHeight);
        }
        public void DrawBackground(SpriteBatch spriteBatch)
        {
            // Fundo tileado
            for (int x = 0; x < 1600; x += _backgroundTexture.Width)
                for (int y = 0; y < 900; y += _backgroundTexture.Height)
                    spriteBatch.Draw(_backgroundTexture, new Vector2(x, y), Color.White);

            // inimigos de fundo (opcional)
            foreach (var enemy in _menuEnemies)
                enemy.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime, MouseState mouse, MouseState prevMouse)
        {
            ResizeButtons(Game1.Instance.GraphicsDevice.Viewport);  

            if (_menuEnemies.Any(e => e.ReachedBase))
            {
                string playerName = "Player";
                int score = Game1.Instance.UI.Points;
                HighScoreManager.SaveScore(playerName, score);
                Game1.Instance.CurrentGameState = Game1.GameState.HighScore;
            }

            // Spawn de inimigos no fundo
            if (_random.NextDouble() < 0.01)
            {
                var spawn = new Vector2(-100, _random.Next(0, 900));

                int tipoInimigo = _random.Next(2); // 0 ou 1

                if (tipoInimigo == 0)
                {
                    _menuEnemies.Add(new Zombie(spawn, Game1.Instance._zombieIdleFrames, Game1.Instance._zombieWalkFrames, null, new Vector2(1600, 800)));

                    if (_random.Next(2) == 0)
                    Game1.Instance._zombieSound.Play();
                }
                else
                {
                    _menuEnemies.Add(new Skeleton(
                    spawn,
                    Game1.Instance._skeletonIdleFrames,
                    Game1.Instance._skeletonWalkFrames,
                    Game1.Instance._bowFrames,
                    Game1.Instance._arrowTexture,
                    new Vector2(1600, 800)
));
                }
            }

            foreach (var enemy in _menuEnemies.ToList())
            {
                enemy.Update(gameTime, new Vector2(1600, 800), new List<Unit>());
                if (enemy.Position.X > 1700)
                    _menuEnemies.Remove(enemy);
            }

            _startButton.Update(mouse, prevMouse);
            _highScoreButton.Update(mouse, prevMouse);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Fundo (tile do chão como no jogo)
            for (int x = 0; x < 1600; x += _backgroundTexture.Width)
            {
                for (int y = 0; y < 900; y += _backgroundTexture.Height)
                {
                    spriteBatch.Draw(_backgroundTexture, new Vector2(x, y), Color.White);
                }
            }

            foreach (var enemy in _menuEnemies)
            {
                enemy.Draw(spriteBatch);
            }

            _startButton.Draw(spriteBatch);
            _highScoreButton.Draw(spriteBatch);
        }
    }

    public static class HighScoreManager
    {
        private static string FilePath => "highscores.txt";

        public static void SaveScore(string name, int score)
        {
            File.AppendAllLines(FilePath, new[] { $"{name}:{score}" });
        }

        public static List<(string Name, int Score)> LoadScores()
        {
            if (!File.Exists(FilePath)) return new();

            return File.ReadAllLines(FilePath)
                .Select(line => line.Split(':'))
                .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                .Select(parts => (parts[0], int.Parse(parts[1])))
                .OrderByDescending(s => s.Item2)
                .Take(10)
                .ToList();
        }
    }
    public class Button
    {
        public Rectangle Bounds;
        public string Text;
        public Action OnClick;

        private SpriteFont _font;

        public Button(Rectangle bounds, string text, SpriteFont font, Action onClick)
        {
            Bounds = bounds;
            Text = text;
            _font = font;
            OnClick = onClick;
        }

        public void Update(MouseState mouseState, MouseState prevMouse)
        {
            if (Bounds.Contains(mouseState.Position) &&
                mouseState.LeftButton == ButtonState.Pressed &&
                prevMouse.LeftButton == ButtonState.Released)
            {
                OnClick?.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Game1.Instance.Pixel, Bounds, Color.DarkGray);
            Vector2 textSize = _font.MeasureString(Text);
            Vector2 textPos = new Vector2(
                Bounds.X + (Bounds.Width - textSize.X) / 2,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(_font, Text, textPos, Color.White);
        }
    }
}
