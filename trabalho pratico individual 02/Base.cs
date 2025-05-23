using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace trabalho_pratico_individual_02
{
    public class Base
    {
        public Vector2 Position;
        private Texture2D _texture;
        private Rectangle _bounds;
        private float _productionCooldown = 2f; // segundos
        private float _timer = 0f;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);

        public Base(Vector2 position, Texture2D texture)
        {
            Position = position;
            _texture = texture;
        }

        public void Update(GameTime gameTime)
        {
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Pressiona ESPAÇO para produzir uma unidade
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && _timer >= _productionCooldown)
            {
                ProduceUnit();
                _timer = 0f;
            }
        }

        private void ProduceUnit()
        {
            var unitTexture = Game1.Instance.UnitTexture;
            var unitPosition = Position + new Vector2(_texture.Width + 10, 0); // ao lado da base

            Game1.Instance.Units.Add(new Unit(unitPosition, unitTexture));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Position, Color.White);
        }
    }
}
