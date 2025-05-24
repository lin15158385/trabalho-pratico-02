using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using Microsoft.Xna.Framework.Audio;

namespace trabalho_pratico_individual_02
{
    public class Game1 : Game
    {
        //variaveis normais
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //----soms
        public SoundEffect _startSound;
        public SoundEffect _zombieSound;
        public SoundEffect _explosionSound;
        public SoundEffect _coinSound;
        public SoundEffect _selectSound;
        public SoundEffect _loseSound;

        private Camera _camera;
        private List<Unit> _units;
        private Texture2D _unitTexture;
        private Texture2D _tankTexture;
        private Texture2D _turretTexture;
        private Texture2D[] _turretAttackFrames;
        public Texture2D[] _explosionFrames;
        public Texture2D _projectileTexture;
        private List<Projectile> _projectiles = new List<Projectile>();
        public List<Projectile> Projectiles => _projectiles;
        public List<Explosion> Explosions { get; private set; } = new List<Explosion>();
        public List<Unit> Units => _units;//ponteiro
        public static Game1 Instance { get; private set; }
        public Point _worldSize = new Point(6000, 3000); // tamanho total do mundo em pixels
        public Vector2 WorldCenter => new Vector2(_worldSize.X / 2, _worldSize.Y / 2);
        public GameUI UI { get; private set; }
        //clasee inimigo
        private List<Enemy> _enemies = new List<Enemy>();
        private List<Coin> _coins = new List<Coin>();
        public List<Enemy> Enemies => _enemies;
        public List<Coin> Coins => _coins;
        public Texture2D[] CoinFrames => _coinFrames;
        public Texture2D[] _zombieIdleFrames;
        public Texture2D[] _zombieWalkFrames;
        public Texture2D[] _skeletonIdleFrames;
        public Texture2D[] _skeletonWalkFrames;

        public Texture2D[] _zombieAttackFrames;
        public Texture2D[] _bowFrames;
        public Texture2D _arrowTexture;
        public Texture2D[] _coinFrames;
        private Random _random = new Random();

        //gameui
        public GameUI _ui;
        private Texture2D _buttonTexture;
        private Texture2D _buttonMainTexture;
        private SpriteFont _font;
        private Texture2D _groundTexture;
        public Base PlayerBase => _playerBase; // torna acessível para UI

        //variavel para dragbox(para escolher unidades multiplas)
        private Vector2? _dragStart = null;
        private Rectangle _selectionRect;
        private bool _isDragging = false;
        private const int DragThreshold = 20; // pixels para considerar drag

        private MouseState _previousMouse;

        //base de jogador
        private Base _playerBase;
        private Texture2D _baseTexture;
        public Texture2D UnitTexture => _unitTexture; // expor textura para ser usada na Base

        public Texture2D Pixel { get; private set; }
        private MainMenu _mainMenu;
        public enum GameState
        {
            Menu,
            Playing,
            GameOver,
            HighScore
        }

        //-------------------------------------------------
        //este parte é da maximizar a ecra do jogo que copiei por um site qualquer
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x00010000;
        private const int WS_MINIMIZEBOX = 0x00020000;
        private const int WS_SYSMENU = 0x00080000;
        private void EnableMaximizeButton()
        {
            IntPtr hwnd = Window.Handle;

            int style = GetWindowLong(hwnd, GWL_STYLE);

            // Adiciona o estilo WS_MAXIMIZEBOX para ativar o botão de maximizar
            style |= WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SYSMENU;

            SetWindowLong(hwnd, GWL_STYLE, style);
        }
        //------------------------------------------------
        public GameState CurrentGameState { get; set; }
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            CurrentGameState = GameState.Menu;
        }

        protected override void Initialize()
        {
            Instance = this;
            _camera = new Camera(GraphicsDevice.Viewport, _worldSize);
            _units = new List<Unit>();
            EnableMaximizeButton();


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _groundTexture = Content.Load<Texture2D>("ground");
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });

            _startSound = Content.Load<SoundEffect>("Sound/game-start-6104");
            _zombieSound = Content.Load<SoundEffect>("Sound/growling-zombie-104988");
            _explosionSound = Content.Load<SoundEffect>("Sound/game-explosion-321700");
            _coinSound = Content.Load<SoundEffect>("Sound/coin-collision-sound-342335");
            _selectSound = Content.Load<SoundEffect>("Sound/select-sound-121244");
            _loseSound = Content.Load<SoundEffect>("Sound/game-over-classic-206486");

            _unitTexture = Content.Load<Texture2D>("unit");
            _baseTexture = Content.Load<Texture2D>("base");
            _buttonTexture = Content.Load<Texture2D>("button");
            _turretTexture = Content.Load<Texture2D>("Unit-Turret\\Cannon4_color1\\Cannon4_color1_001");
            _font = Content.Load<SpriteFont>("DefaultFont");
            _projectileTexture = Content.Load<Texture2D>("Unit-Tank/Effects/Exhaust_Fire");
            _buttonMainTexture = Content.Load<Texture2D>("Button_main");

            _arrowTexture = Content.Load<Texture2D>("Arrows/Arrow1");

            _turretAttackFrames = LoadFrames("Unit-Turret/Cannon4_color1/Cannon4_color1_", 3);
            _zombieIdleFrames = LoadFrames("Enemy_Zombie/Zombie_Idle/tile", 7);
            _zombieWalkFrames = LoadFrames("Enemy_Zombie/Zombie_Walk/tile", 4);
            _skeletonIdleFrames = LoadFrames("Enemy_Skeleton/Skeleton_Idle/tile", 6);
            _skeletonWalkFrames = LoadFrames("Enemy_Skeleton/Skeleton_Walk/tile", 4);

            _zombieAttackFrames = LoadFrames("Attack/tile", 4);
            _bowFrames = LoadFrames("Bow/tile", 4);

            _coinFrames = LoadFrames("Coin_sprite/tile", 9);
            _explosionFrames = LoadFrames("Unit-Tank/Effects/Explosion_", 8);

            Texture2D tankTracks = Content.Load<Texture2D>("Unit-Tank/Tracks/Track_1_A");
            Texture2D tankHullA = Content.Load<Texture2D>("Unit-Tank/Hulls_Color_A/Hull_01");
            Texture2D tankWeaponA = Content.Load<Texture2D>("Unit-Tank/Weapon_Color_A/Gun_01");

            _tankTexture = tankHullA; // usado apenas para UI ou ícone

            _playerBase = new Base(
                new Vector2(
                    (_worldSize.X / 2f) - (_baseTexture.Width / 2f),
                    (_worldSize.Y / 2f) - (_baseTexture.Height / 2f)
                ),
                _baseTexture
            );
            _ui = new GameUI(_font, _buttonTexture);
            _ui.LoadContent();
            UI = _ui;

            _mainMenu = new MainMenu(
                _groundTexture,
                _buttonMainTexture,
                _font,
                _zombieIdleFrames,
                _zombieWalkFrames,
                _skeletonIdleFrames,
                _skeletonWalkFrames
            );
        }
        private Texture2D[] LoadFrames(string basePath, int count)
        {
            Texture2D[] frames = new Texture2D[count];
            for (int i = 0; i < count; i++)
                frames[i] = Content.Load<Texture2D>($"{basePath}{i:000}");
            return frames;
        }
        private Rectangle CreateRectangle(Vector2 a, Vector2 b)
        {
            return new Rectangle(
                (int)Math.Min(a.X, b.X),
                (int)Math.Min(a.Y, b.Y),
                (int)Math.Abs(a.X - b.X),
                (int)Math.Abs(a.Y - b.Y)
            );
        }

        public void SpawnUnit(string type)
        {
            Vector2 spawnPosition = new Vector2((_worldSize.X / 2f),(_worldSize.Y / 2f));

            if (type == "Soldier")
                _units.Add(new Soldier(spawnPosition, _unitTexture));
            else if (type == "Tank")
            {
                var track = Content.Load<Texture2D>("Unit-Tank/Tracks/Track_1_A");
                var hull = Content.Load<Texture2D>("Unit-Tank/Hulls_Color_A/Hull_01");
                var weapon = Content.Load<Texture2D>("Unit-Tank/Weapon_Color_A/Gun_01");

                var tank = new Tank(spawnPosition, track, hull, weapon);

                // Define o ponto de origem correto para o weapon
                Vector2 weaponOrigin = new Vector2(50, 120); // ou (50, 160), depende do sprite
                tank.SetWeapon(weapon, weaponOrigin);

                tank.SetProjectileAssets(
                    _projectileTexture,
                    _explosionFrames
                );

                _units.Add(tank);
            }
            else if (type == "Drone")
                _units.Add(new Drone(spawnPosition, _unitTexture));
            else if (type == "Turret")
                _units.Add(new Turret(spawnPosition, _turretAttackFrames));
        }

        protected override void Update(GameTime gameTime)
        {
            // input de scroll da câmera (up down left right)
            _camera.Update(GraphicsDevice);

            _playerBase.Update(gameTime);

            if (CurrentGameState == GameState.Menu || CurrentGameState == GameState.HighScore)
            {
                _mainMenu.Update(gameTime, Mouse.GetState(), _previousMouse);
                _previousMouse = Mouse.GetState();
                return;
            }

            // mouse input
            MouseState mouse = Mouse.GetState();
            Vector2 mouseWorldPos = _camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));

            // início do drag
            if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
            {
                _dragStart = mouseWorldPos;
                _isDragging = false;
            }

            // durante o drag
            if (mouse.LeftButton == ButtonState.Pressed && _dragStart != null)
            {
                Vector2 dragEnd = mouseWorldPos;

                // verifica se arrastou o suficiente para considerar drag
                if (!_isDragging && Vector2.Distance(dragEnd, _dragStart.Value) > DragThreshold)
                    _isDragging = true;

                if (_isDragging)
                    _selectionRect = CreateRectangle(_dragStart.Value, dragEnd);
            }

            // fim do drag
            if (mouse.LeftButton == ButtonState.Released && _previousMouse.LeftButton == ButtonState.Pressed && _dragStart != null)
            {
                if (_isDragging)
                {
                    // seleção múltipla com drag box
                    foreach (var unit in _units)
                    {
                        unit.IsSelected = _selectionRect.Intersects(unit.Bounds);
                    }
                }
                else
                {
                    // clique simples: seleciona apenas a primeira unidade que contém o ponto
                    bool selectedAny = false;
                    foreach (var unit in _units)
                    {
                        if (unit.Bounds.Contains(mouseWorldPos))
                        {
                            unit.IsSelected = true;
                            selectedAny = true;
                        }
                        else
                        {
                            unit.IsSelected = false;
                        }
                    }

                    // se clicou no vazio, desmarca tudo
                    if (!selectedAny)
                    {
                        foreach (var unit in _units)
                            unit.IsSelected = false;
                    }
                }

                _dragStart = null;
                _isDragging = false;
            }

            if (_ui.IsPlacingTurret)
            {
                if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
                {
                    Vector2 worldPos = _camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));

                    // Verifica distância da base
                    float distanceToBase = Vector2.Distance(worldPos, _playerBase.Position);
                    bool nearBase = distanceToBase < 700f;

                    // Verifica colisão com outras turrets/unidades
                    Rectangle previewRect = new Rectangle((int)worldPos.X, (int)worldPos.Y, _turretTexture.Width, _turretTexture.Height);
                    bool collides = _units.Any(u => u.Bounds.Intersects(previewRect));

                    if (!collides && nearBase)
                    {
                        _units.Add(new Turret(worldPos, _turretAttackFrames));
                        _ui.CancelPlacingTurret();
                    }
                }
            }
            // atualiza unidades
            foreach (var unit in _units)
                unit.Update(gameTime, _camera);

            _previousMouse = mouse;
            _ui.Update(gameTime);
            base.Update(gameTime);

            // Spawning aleatório a cada poucos segundos (exemplo simples)
            if (_random.NextDouble() < 0.01) // ajuste para spawnar ou não
            {
                Vector2 spawn = GetRandomEdgeSpawn();

                // Escolhe aleatoriamente 0 ou 1
                int tipoInimigo = _random.Next(2); // 0 ou 1

                if (tipoInimigo == 0)
                {
                    _enemies.Add(new Zombie(spawn, _zombieIdleFrames, _zombieWalkFrames, _zombieAttackFrames, _playerBase.Position));

                }
                else
                {
                    _enemies.Add(new Skeleton(spawn, _skeletonIdleFrames, _skeletonWalkFrames, _bowFrames, _arrowTexture, _playerBase.Position));
                }
            }

            foreach (var enemy in _enemies.ToList())
            {
                enemy.Update(gameTime, _playerBase.Position, _units);

                if (enemy.ReachedBase) // <-- Verifica se chegou à base
                {
                    // Salva pontuação
                    string playerName = "Player"; // pode pedir nome depois se quiser
                    int score = _ui.Points; // ou de onde estiver guardando a pontuação
                    HighScoreManager.SaveScore(playerName, score);

                    // Muda o estado para HighScore
                    CurrentGameState = GameState.HighScore;

                    // Limpa inimigos e outros para resetar o jogo
                    _enemies.Clear();
                    _units.Clear();
                    _coins.Clear();
                    _projectiles.Clear();
                    Explosions.Clear();

                    break; // sai do loop para evitar erros após limpar listas
                }

                if (!enemy.IsAlive)
                {
                    _coins.Add(new Coin(enemy.Position, _coinFrames));
                    Game1.Instance._zombieSound.Play();
                    _enemies.Remove(enemy);
                }
            }

            foreach (var coin in _coins.ToList())
            {
                coin.Update(gameTime, _units);
                if (coin.IsCollected)
                {
                    _ui.AddMoney(coin.Value); // esse método já existe pelo seu comentário
                    Game1.Instance._coinSound.Play();
                    UI.Points += 5;
                    _coins.Remove(coin);
                }
            }
            foreach (var proj in _projectiles.ToList())
            {
                proj.Update(gameTime);
                if (proj.Hit)
                {
                    _projectiles.Remove(proj);
                    Explosions.Add(new Explosion(proj.Position, _explosionFrames));
                }
            }

            foreach (var explosion in Explosions.ToList())
            {
                explosion.Update(gameTime);
                if (explosion.Done)
                    Explosions.Remove(explosion);
            }

        }

        private Vector2 GetRandomEdgeSpawn()
        {
            int edge = _random.Next(4);
            int x = 0, y = 0;

            switch (edge)
            {
                case 0: x = 0; y = _random.Next(_worldSize.Y); break; // esquerda
                case 1: x = _worldSize.X; y = _random.Next(_worldSize.Y); break; // direita
                case 2: x = _random.Next(_worldSize.X); y = 0; break; // topo
                case 3: x = _random.Next(_worldSize.X); y = _worldSize.Y; break; // baixo
            }
            return new Vector2(x, y);

        }
        private void DrawHighScores()
        {
            var scores = HighScoreManager.LoadScores() ?? new List<(string, int)>();
            Vector2 pos = new Vector2(600, 200);
            _spriteBatch.DrawString(_font, "High Scores:", pos, Color.White);
            pos.Y += 40;

            foreach (var (name, score) in scores)
            {
                _spriteBatch.DrawString(_font, $"{name}: {score}", pos, Color.Yellow);
                pos.Y += 30;
            }
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(transformMatrix: _camera.Transform);

            int tileWidth = _groundTexture.Width;
            int tileHeight = _groundTexture.Height;

            for (int x = 0; x < _worldSize.X; x += tileWidth)
            {
                for (int y = 0; y < _worldSize.Y; y += tileHeight)
                {
                    _spriteBatch.Draw(_groundTexture, new Vector2(x, y), Color.White);
                }
            }

            foreach (var enemy in _enemies)
                enemy.Draw(_spriteBatch);

            foreach (var coin in _coins)
                coin.Draw(_spriteBatch);

            _playerBase.Draw(_spriteBatch);

            foreach (var unit in _units)
                unit.Draw(_spriteBatch);

            // desenha retângulo so se estiver realmente arrastando
            if (_dragStart != null && _isDragging)
            {
                Texture2D rectTexture = new Texture2D(GraphicsDevice, 1, 1);
                rectTexture.SetData(new[] { Color.White });

                _spriteBatch.Draw(rectTexture, _selectionRect, Color.White * 0.3f);
            }

            if (_ui.IsPlacingTurret)
            {
                Vector2 mouseWorld = _camera.ScreenToWorld(new Vector2(Mouse.GetState().X, Mouse.GetState().Y));
                Color color = Color.White;

                float distanceToBase = Vector2.Distance(mouseWorld, _playerBase.Position);
                bool nearBase = distanceToBase < 600f;

                Rectangle previewRect = new Rectangle((int)mouseWorld.X, (int)mouseWorld.Y, _turretTexture.Width, _turretTexture.Height);
                bool collides = _units.Any(u => u.Bounds.Intersects(previewRect));

                if (!nearBase || collides)
                    color = Color.Red * 0.6f;
                else
                    color = Color.White * 0.8f;

                _spriteBatch.Draw(_turretTexture, mouseWorld, color);
            }

            // Desenhar projéteis
            foreach (var proj in _projectiles)
                proj.Draw(_spriteBatch);

            // Desenhar explosões
            foreach (var explosion in Explosions)
                explosion.Draw(_spriteBatch);

            if (CurrentGameState == GameState.HighScore)
            {
                _spriteBatch.End();            // fecha o batch de jogo
                _spriteBatch.Begin();          // batch sem câmera

                // 1) desenha o mesmo fundo do menu
                _mainMenu.DrawBackground(_spriteBatch);

                // 2) desenha o painel de High Scores por cima
                DrawHighScores();

                _spriteBatch.End();
                return;
            }
            _spriteBatch.End();

            _spriteBatch.Begin(); // sem transformação da câmera
            _spriteBatch.Draw(_arrowTexture, new Vector2(100, 100), Color.White);
            _spriteBatch.End();

            if (CurrentGameState == GameState.Menu)
            {
                _spriteBatch.Begin();
                _mainMenu.Draw(_spriteBatch);
                _spriteBatch.End();
                return;
            }
            _spriteBatch.Begin(); // UI sem transformação da câmera
            _ui.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
