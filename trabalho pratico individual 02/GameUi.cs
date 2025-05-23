using System;
using System.Collections.Generic;
using Microsoft.VisualBasic.Devices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace trabalho_pratico_individual_02
{
    public class GameUI
    {
        private SpriteFont _font;
        private Texture2D _buttonTexture;
        private Texture2D _soldierTexture;
        private Texture2D _tankTexture;
        private Texture2D _droneTexture;
        private Texture2D _turretTexture;
        private Texture2D _turretPreviewTexture;
        private bool _placingTurret = false;
        public int Points { get; set; } = 0;
        public int Money { get; set; } = 100;
        private double _moneyTimer = 0;
        public bool IsPlacingTurret => _placingTurret;

        public void CancelPlacingTurret()
        {
            _placingTurret = false;
        }

        public GameUI(SpriteFont font, Texture2D buttonTexture)
        {
            _font = font;
            _buttonTexture = buttonTexture;
            _unitButtons = new List<UnitButton>();
        }

        public class UnitButton
        {
            public Rectangle Bounds;
            public Texture2D Texture;
            public string UnitType;
            public Texture2D Icon;
        }

        private List<UnitButton> _unitButtons;
        public void AddMoney(int amount) => Money += amount;
        private MouseState _prevMouse;

        public void Update(GameTime gameTime)
        {
            _moneyTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (_moneyTimer >= 2)
            {
                Money++;
                _moneyTimer = 0;
            }



            Microsoft.Xna.Framework.Input.MouseState mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released)
            {
                foreach (var btn in _unitButtons)
                {
                    if (btn.Bounds.Contains(mouse.Position))
                    {
                        int cost = GetUnitCost(btn.UnitType);
                        if (Money >= cost)
                        {
                            Money -= cost;
                            Game1.Instance.SpawnUnit(btn.UnitType);
                            if (btn.UnitType == "Turret")
                            {
                                _placingTurret = true;
                            }
                        }
                        break;
                    }
                }
            }

            UpdateButtonPositions();
            _prevMouse = mouse;

        }

        private void UpdateButtonPositions()
        {
            var viewport = Game1.Instance.GraphicsDevice.Viewport;
            int margin = 10;

            int buttonWidth = Math.Clamp((int)(viewport.Width * 0.12f), 200, 200);
            int buttonHeight = buttonWidth / 3;

            for (int i = 0; i < _unitButtons.Count; i++)
            {
                int x = viewport.Width - margin - buttonWidth - (buttonWidth + margin) * i;
                int y = viewport.Height - margin - buttonHeight;

                _unitButtons[i].Bounds = new Rectangle(x, y, buttonWidth, buttonHeight);
            }
        }


        public void LoadContent()
        {
            _unitButtons = new List<UnitButton>();

            // Carrega as texturas dos ícones
            _soldierTexture = Game1.Instance.Content.Load<Texture2D>("soldier");
            _tankTexture = Game1.Instance.Content.Load<Texture2D>("tank");
            _droneTexture = Game1.Instance.Content.Load<Texture2D>("drone");
            _turretTexture = Game1.Instance.Content.Load<Texture2D>("Unit-Turret/Cannon4_color1/Cannon4_color1_000");

            string[] types = { "Soldier", "Tank", "Drone", "Turret" };
            Texture2D[] icons = { _soldierTexture, _tankTexture, _droneTexture, _turretTexture };

            int buttonWidth = 150;
            int buttonHeight = 150;
            int margin = 30;

            var viewport = Game1.Instance.GraphicsDevice.Viewport;

            for (int i = 0; i < types.Length; i++)
            {
                int x = viewport.Width - margin - buttonWidth - (buttonWidth + margin) * i;
                int y = viewport.Height - margin - buttonHeight;

                _unitButtons.Add(new UnitButton
                {
                    Bounds = new Rectangle(x, y, buttonWidth, buttonHeight),
                    Texture = _buttonTexture,
                    UnitType = types[i],
                    Icon = icons[i]
                });
            }
        }


        private int GetUnitCost(string unitType)
        {
            return unitType switch
            {
                "Soldier" => 25,
                "Tank" => 35,
                "Drone" => 15,
                "Turret" => 50,
                _ => 10
            };
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, $"Refound: {Money}", new Vector2(10, 10), Color.Yellow);
            spriteBatch.DrawString(_font, $"Points: {Points}", new Vector2(10, 50), Color.White);

            foreach (var btn in _unitButtons)
            {
                spriteBatch.Draw(btn.Texture, btn.Bounds, Color.White);

                int iconSize = btn.Bounds.Height + 50;
                iconSize = Math.Min(iconSize, 80);

                var iconPos = new Vector2(
                    btn.Bounds.X + (btn.Bounds.Width - iconSize) / 2,
                    btn.Bounds.Y -20
                );

                spriteBatch.Draw(btn.Icon, new Rectangle((int)iconPos.X, (int)iconPos.Y, iconSize, iconSize), Color.White);

                string label = $"{btn.UnitType} ({GetUnitCost(btn.UnitType)})";
                var labelPos = new Vector2(btn.Bounds.X + 10, btn.Bounds.Y + btn.Bounds.Height -15);
                spriteBatch.DrawString(_font, label, labelPos, Color.Blue);
            }
        }
    }
}
