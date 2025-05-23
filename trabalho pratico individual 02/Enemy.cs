using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace trabalho_pratico_individual_02
{
    public class Enemy
    {
        public enum State { Idle, Walk }
        protected State _currentState = State.Idle;

        private Texture2D[] _idleFrames;
        private Texture2D[] _walkFrames;
        private int _frameIndex;
        private float _frameTimer;
        private float _frameSpeed = 0.2f;
        protected bool _facingLeft;

        private float _actionTimer = 0f;
        private float _nextActionTime = 0f;
        private bool _isWandering = false;
        public float EnemyHealth { get; set; }
        private Random _random = new Random();
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 goal;
        private Vector2 _target;
        public Rectangle Bounds => new((int)Position.X, (int)Position.Y, 64, 64);
        public bool IsDead = false;
        protected bool _isAttacking = false;
        public bool IsAlive { get; set; } = true;
        public bool ReachedBase = false;

        protected Unit _chasingUnit;

        public Enemy(Vector2 position, Texture2D[] idleFrames, Texture2D[] walkFrames, Vector2 target)
        {
            _idleFrames = idleFrames;
            _walkFrames = walkFrames;
            Position = position;
            _target = Game1.Instance.WorldCenter;
            EnemyHealth = 100;
        }

        public virtual void Update(GameTime gameTime, Vector2 target, List<Unit> units) 
        {
            if (IsDead) return;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Escolher unit mais próxima se estiver dentro de range
            float chaseRange = 300f;
            float giveUpRange = 500f;
            float minDist = float.MaxValue;
            Unit closest = null;

            foreach (var unit in units)
            {
                float dist = Vector2.Distance(unit.Position, Position);
                if (dist < chaseRange && dist < minDist)
                {
                    minDist = dist;
                    closest = unit;
                }
            }

            if (_chasingUnit == null && closest != null)
                _chasingUnit = closest;

            if (_chasingUnit != null)
            {
                float dist = Vector2.Distance(_chasingUnit.Position, Position);
                if (dist > giveUpRange)
                    _chasingUnit = null;
            }


            if (_chasingUnit != null)
            {
                goal = _chasingUnit.Position + new Vector2(_chasingUnit.Bounds.Width / 2f, _chasingUnit.Bounds.Height / 2f);
            }
            else
            {
                goal = Game1.Instance.WorldCenter + new Vector2(32, 32); // ajusta se a base tiver tamanho diferente de 64x64
            }

            _facingLeft = (goal.X < Position.X);
            Vector2 direction = goal - Position;

            // Controlo aleatório de movimento (só se não estiver a perseguir Unit)
            if (_chasingUnit == null)
            {
                _actionTimer += deltaTime;

                if (_actionTimer >= _nextActionTime)
                {
                    _isWandering = !_isWandering;
                    _nextActionTime = _isWandering
                        ? _random.Next(1, 4)       // Andar entre 1 a 3 segundos
                        : _random.Next(1, 3);      // Parar entre 1 a 2 segundos

                    _actionTimer = 0f;
                }

                if (_isWandering && direction.Length() > 2)
                {
                    direction.Normalize();
                    Velocity = direction * 50f;
                    _currentState = State.Walk;
                }
                else
                {
                    Velocity = Vector2.Zero;
                    _currentState = State.Idle;
                }
            }
            else
            {
                // Persegue Unit normalmente
                if (direction.Length() > 2)
                {
                    direction.Normalize();
                    Velocity = direction * 50f;
                    _currentState = State.Walk;
                }
                else
                {
                    Velocity = Vector2.Zero;
                    _currentState = State.Idle;
                }
            }

            if (!_isAttacking)
                Position += Velocity * deltaTime;

            // Check base reach
            if (Vector2.Distance(Position, Game1.Instance.WorldCenter) < 30)
            {
                ReachedBase = true;
            }

            Animate(deltaTime);
        }

        private void Animate(float deltaTime)
        {
            _frameTimer += deltaTime;
            if (_frameTimer >= _frameSpeed)
            {
                _frameIndex = (_frameIndex + 1) % (_currentState == State.Idle ? _idleFrames.Length : _walkFrames.Length);
                _frameTimer = 0f;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead) return;

            Texture2D currentFrame;

            if (_currentState == State.Idle)
            {
                if (_idleFrames != null && _idleFrames.Length > 0)
                {
                    int safeIndex = Math.Clamp(_frameIndex, 0, _idleFrames.Length - 1);
                    currentFrame = _idleFrames[safeIndex];
                }
                else
                {
                    return;
                }
            }
            else
            {
                if (_walkFrames != null && _walkFrames.Length > 0)
                {
                    int safeIndex = Math.Clamp(_frameIndex, 0, _walkFrames.Length - 1);
                    currentFrame = _walkFrames[safeIndex];
                }
                else
                {
                    return;
                }
            }

            SpriteEffects flip = _facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Alinha ao centro inferior do sprite
            Vector2 origin = new Vector2(currentFrame.Width / 2f, currentFrame.Height);

            spriteBatch.Draw(
                currentFrame,
                Position,
                null,
                Color.White,
                0f,
                origin,
                3f, // Escala
                flip,
                0f
            );
        }

        public void Die(List<Coin> coins, Texture2D[] coinFrames)
        {

            IsDead = true;
            IsAlive = false;
        }
    }

    public class Zombie : Enemy
    {
        private Texture2D[] _attackFrames;
        private float _attackFrameIndex = 0f;
        private float _attackFrameSpeed = 0.4f;
        private float _attackCooldown = 2f;  // segundos entre ataques
        private float _attackTimer = 0f;
        private float _attackFrameTimer = 0f;
        public Zombie(Vector2 position, Texture2D[] idleFrames, Texture2D[] walkFrames, Texture2D[] attackFrames, Vector2 target)
            : base(position, idleFrames, walkFrames, target)
        {
            _attackFrames = attackFrames;
        }

        public override void Update(GameTime gameTime, Vector2 target, List<Unit> units)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _attackTimer += deltaTime;

            _isAttacking = false;

            foreach (var unit in units)
            {
                if (this.Bounds.Intersects(unit.Bounds))
                {
                    if (_attackTimer >= _attackCooldown)
                    {
                        _isAttacking = true;
                        _attackTimer = 0f;
                        // Aqui poderias aplicar dano à Unit se quiseres
                    }

                    Velocity = Vector2.Zero;
                    _currentState = State.Idle;
                    break;
                }
            }

            if (!_isAttacking)
                base.Update(gameTime, target, units);

            if (_isAttacking)
                AnimateAttack(gameTime);
        }

        private void AnimateAttack(GameTime gameTime)
        {
            _attackFrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_attackFrameTimer >= _attackFrameSpeed)
            {
                _attackFrameIndex++;
                if (_attackFrameIndex >= _attackFrames.Length)
                    _attackFrameIndex = 0f;

                _attackFrameTimer = 0f;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (_isAttacking)
            {
                int frame = (int)_attackFrameIndex;
                Vector2 attackPosition = Position + new Vector2(_facingLeft ? -20 : 20, -10); // Ajusta pra ficar na mão

                spriteBatch.Draw(_attackFrames[frame], attackPosition, null, Color.White, 0f,
                    new Vector2(_attackFrames[frame].Width / 2f, _attackFrames[frame].Height),
                    0.5f,
                    _facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0f);
            }
        }
    }

    public class Skeleton : Enemy
    {
        private Texture2D[] _bowFrames;
        private float _bowFrameIndex = 0f;
        private float _bowFrameSpeed = 0.5f;
        private Texture2D _arrowTexture;

        private float _shootCooldown = 2f; // tempo entre tiros
        private float _shootTimer = 0f;

        public List<Arrow> Arrows = new();

        public Skeleton(Vector2 position, Texture2D[] idleFrames, Texture2D[] walkFrames, Texture2D[] bowFrames, Texture2D arrowTexture, Vector2 target)
        : base(position, idleFrames, walkFrames, target)
        {
            _bowFrames = bowFrames;
            _arrowTexture = arrowTexture;
        }

        public override void Update(GameTime gameTime, Vector2 target, List<Unit> units)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _shootTimer += deltaTime;

            // Procurar unit mais próxima no alcance
            Unit closest = null;
            float minDist = float.MaxValue;
            foreach (var unit in units)
            {
                float dist = Vector2.Distance(Position, unit.Position);
                if (dist < 250 && dist < minDist)
                {
                    minDist = dist;
                    closest = unit;
                }
            }

            if (closest != null)
            {
                // Está dentro do range de ataque
                _isAttacking = true;
                Velocity = Vector2.Zero;
                _currentState = State.Idle;
                _facingLeft = (closest.Position.X < Position.X);

                if (_shootTimer >= _shootCooldown)
                {
                    ShootArrow(closest.Position);
                    _shootTimer = 0f;
                }

                AnimateBow(gameTime);
            }
            else
            {
                // Fora de alcance → movimento normal
                _isAttacking = false;
                base.Update(gameTime, target, units);
            }

            // Atualizar flechas
            foreach (var arrow in Arrows.ToList())
            {
                arrow.Update(gameTime);
                if (arrow.IsOffScreen())
                    Arrows.Remove(arrow);
            }
        }


        private void ShootArrow(Vector2 target)
        {
            Vector2 arrowStartPos = Position + new Vector2(_facingLeft ? -20 : 20, -10);
            Vector2 direction = Vector2.Normalize(target - arrowStartPos);
            Arrows.Add(new Arrow(arrowStartPos, direction, _arrowTexture)); // usa a textura carregada
        }

        private void AnimateBow(GameTime gameTime)
        {
            _bowFrameIndex += (float)gameTime.ElapsedGameTime.TotalSeconds / _bowFrameSpeed;
            if (_bowFrameIndex >= _bowFrames.Length)
                _bowFrameIndex = 0f;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            // Sempre desenha o arco
            int frame = _isAttacking ? (int)_bowFrameIndex : 0;  // frame 0 se não estiver atirando
            Vector2 bowPosition = Position + new Vector2(_facingLeft ? -20 : 20, -10);

            spriteBatch.Draw(_bowFrames[frame], bowPosition, null, Color.White, 0f,
                new Vector2(_bowFrames[frame].Width / 2f, _bowFrames[frame].Height),
                3f,
                _facingLeft ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                0f);

            foreach (var arrow in Arrows)
                arrow.Draw(spriteBatch);
        }
    }

    public class Arrow
    {
        private Vector2 _position;
        private Vector2 _direction;
        private float _speed = 300f;
        private Texture2D _texture;

        public Arrow(Vector2 position, Vector2 direction, Texture2D texture)
        {
            _position = position;
            _direction = direction;
            _texture = texture;
        }

        public void Update(GameTime gameTime)
        {
            _position += _direction * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            float rotation = (float)Math.Atan2(_direction.Y, _direction.X);

            spriteBatch.Draw(_texture, _position, null, Color.White, rotation,
                new Vector2(_texture.Width / 2f, _texture.Height / 2f), 2f, SpriteEffects.None, 0f);
        }

        public bool IsOffScreen()
        {
            return _position.X < 0 || _position.X > Game1.Instance.GraphicsDevice.Viewport.Width
                || _position.Y < 0 || _position.Y > Game1.Instance.GraphicsDevice.Viewport.Height;
        }
    }


    public class Coin
    {
        private Texture2D[] _frames;
        private int _frameIndex = 0;
        private float _frameTimer;
        private float _frameSpeed = 0.2f;
        public bool IsCollected => Collected;
        public int Value { get; set; } = 10;

        public Vector2 Position;
        private float _scale = 3f; // escala da coin
        public bool Collected = false;

        public Coin(Vector2 pos, Texture2D[] frames)
        {
            Position = pos;
            _frames = frames;
        }

        public Rectangle Bounds
        {
            get
            {
                var width = (int)(_frames[0].Width * _scale);
                var height = (int)(_frames[0].Height * _scale);
                return new Rectangle(
                    (int)(Position.X - width / 2),
                    (int)(Position.Y - height / 2),
                    width,
                    height
                );
            }
        }

        public void Update(GameTime gameTime, List<Unit> units)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameTimer += deltaTime;

            if (_frameTimer >= _frameSpeed)
            {
                _frameIndex = (_frameIndex + 1) % _frames.Length;
                _frameTimer = 0f;
            }

            foreach (var unit in units)
            {
                if (unit.Bounds.Intersects(this.Bounds))
                {
                    Collected = true;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var origin = new Vector2(_frames[0].Width / 2f, _frames[0].Height / 2f);
            spriteBatch.Draw(_frames[_frameIndex], Position, null, Color.White, 0f, origin, _scale, SpriteEffects.None, 0f);
        }
    }

}
