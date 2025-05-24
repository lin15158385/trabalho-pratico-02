using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.Direct3D9;

namespace trabalho_pratico_individual_02
{
    public class Unit
    {
        public float Speed { get; protected set; }
        public float Damage { get; protected set; }
        public int Health { get; set; }
        public bool CanFly { get; protected set; }
        public int _selectedUnitIndex = 0;

        public bool IsDead { get; private set; } = false;

        public Vector2 Position;
        protected Texture2D Texture;
        protected float _angle = 0f;

        public bool IsSelected { get; set; }
        protected Vector2 Target;

        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);

        public Unit(Vector2 pos, Texture2D texture)
        {
            Position = pos;
            Texture = texture;
            Target = pos;
        }

        public void CheckSelection(Vector2 mouseWorld)
        {
            Rectangle bounds = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            if (bounds.Contains(mouseWorld))
            {
                // Desseleciona todas antes
                foreach (var unit in Game1.Instance.Units)
                    unit.IsSelected = false;

                this.IsSelected = true;
                Game1.Instance._selectSound.Play();
            }
        }

        public virtual void Die()
        {
            IsDead = true;
            Game1.Instance.Units.Remove(this);
            Game1.Instance._explosionSound.Play();
            Game1.Instance.Explosions.Add(new Explosion(Position, Game1.Instance._explosionFrames));
            // Ex: tocar som, criar efeito de explosão, etc，ainda nao pensei o que usar
        }
        public virtual void Update(GameTime gameTime, Camera camera)
        {
            if (IsDead) return;
            if (IsSelected && Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                Vector2 mouseWorld = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                mouseWorld = camera.ScreenToWorld(mouseWorld);
                Target = mouseWorld;
            }

            Vector2 toTarget = Target - Position;
            if (toTarget.LengthSquared() > 1f)
            {
                toTarget.Normalize();
                _angle = (float)Math.Atan2(toTarget.Y, toTarget.X);
                Position += toTarget * Speed;
            }

            // Checa colisão com outras unidades (simples separação)
            foreach (var other in Game1.Instance.Units)
            {
                if (other == this) continue;

                if (this.Bounds.Intersects(other.Bounds))
                {
                    Vector2 pushAway = Position - other.Position;
                    if (pushAway != Vector2.Zero)
                    {
                        pushAway.Normalize();
                        Position += pushAway * 1f; // afasta 1 pixel
                    }
                }
            }

        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead) return;
            Color color = IsSelected ? Color.Green : Color.White;
            spriteBatch.Draw(
                Texture,
                Position + new Vector2(Texture.Width / 2f, Texture.Height / 2f),
                null,
                color,
                _angle,
                new Vector2(Texture.Width / 2f, Texture.Height / 2f),
                1f,
                SpriteEffects.None,
                0f
            );
        }
    }

    public class Soldier : Unit
    {
        public Soldier(Vector2 position, Texture2D texture)
            : base(position, texture)
        {
            Speed = 2f;
            Health = 100;
        }

    }


    public class Tank : Unit
    {
        private Texture2D _tracksTexture;
        private Texture2D _hullTexture;
        private Texture2D _weaponTexture;
        private float _fireCooldown = 2.0f;
        private float _fireTimer = 0f;
        private Enemy _currentTarget;

        private Texture2D _projectileTexture;
        private Texture2D[] _explosionFrames;
        private Vector2 _weaponOrigin;



        private float tank_angle = -MathHelper.PiOver2;
        private float weapon_angle = -MathHelper.PiOver2;

        public Tank(Vector2 position, Texture2D tracks, Texture2D hull, Texture2D weapon)
                : base(position, tracks) // usa tracks como textura base apenas para o Unit
            {
                _tracksTexture = tracks;
                _hullTexture = hull;
                _weaponTexture = weapon;

                Speed = 1f;
                Health = 300;
            }

        public void SetProjectileAssets(Texture2D projectileTex, Texture2D[] explosionFrames)
        {
            _projectileTexture = projectileTex;
            _explosionFrames = explosionFrames;
        }

        public void SetWeapon(Texture2D weaponTexture, Vector2 origin)
        {
            _weaponTexture = weaponTexture;
            _weaponOrigin = origin;
        }
        private Enemy FindNearestEnemy(List<Enemy> enemies, float range)
        {
            Enemy nearest = null;
            float minDist = range;
            foreach (var e in enemies)
            {
                float dist = Vector2.Distance(Position, e.Position);
                if (e.IsAlive && dist < minDist)
                {
                    nearest = e;
                    minDist = dist;
                }
            }
            return nearest;
        }
        public override void Update(GameTime gameTime, Camera camera)
        {
            base.Update(gameTime, camera);

            Vector2 toTarget = Target - Position;

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _fireTimer += deltaTime;

            // Procurar alvo
            if (_currentTarget == null || _currentTarget.IsDead || !_currentTarget.IsAlive)
            {
                _currentTarget = FindNearestEnemy(Game1.Instance.Enemies, 350f); // raio de busca
            }

            // Atualizar torre
            if (_currentTarget != null)
            {
                Vector2 toEnemy = _currentTarget.Position - Position;
                float angleToEnemy = (float)Math.Atan2(toEnemy.Y, toEnemy.X) + MathHelper.PiOver2 + MathHelper.Pi;
                weapon_angle = MathHelper.Lerp(weapon_angle, angleToEnemy, 0.1f) + MathHelper.PiOver2 + MathHelper.Pi;

                if (_fireTimer >= _fireCooldown)
                {
                    _fireTimer = 0f;
                    var projectile = new Projectile(Position, _currentTarget, _projectileTexture);
                    Game1.Instance.Projectiles.Add(projectile);
                }
            }

            if (toTarget.LengthSquared() > 1f)
            {
                float targetAngle = (float)Math.Atan2(toTarget.Y, toTarget.X) + MathHelper.PiOver2 + MathHelper.Pi;
                tank_angle = MathHelper.Lerp(tank_angle, targetAngle, 0.1f) + MathHelper.PiOver2 + MathHelper.Pi;
                weapon_angle = MathHelper.Lerp(tank_angle, targetAngle, 0.1f) + MathHelper.PiOver2 + MathHelper.Pi;
            }
        }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Color color = IsSelected ? Color.Green : Color.White;

        int hullWidth = _hullTexture.Width;
        int hullHeight = _hullTexture.Height;
        int weaponWidth = _weaponTexture.Width;
        int weaponHeight = _weaponTexture.Height;

        float overlap = 20f;
        float horizontalOffset = hullWidth / 8f - overlap;
        float bodyYOffset = 90f;
        float weaponYOffset = bodyYOffset + 4f;

        Vector2 center = Position;

        Vector2 hullPos = new Vector2(center.X - hullWidth / 2f, center.Y - hullHeight / 2f + bodyYOffset);
        Vector2 weaponPos = center + new Vector2(0, weaponYOffset);

        spriteBatch.Draw(_hullTexture, hullPos + new Vector2(hullWidth / 2f, hullHeight / 2f), null, color, tank_angle, new Vector2(hullWidth / 2f, hullHeight / 2f), 1f, SpriteEffects.None, 0f);
        spriteBatch.Draw(_weaponTexture, weaponPos, null, color, weapon_angle, _weaponOrigin, 1f, SpriteEffects.None, 0f);
        
    }
}
    
    public class Drone : Unit
    {
        public Drone(Vector2 position, Texture2D texture)
            : base(position, texture)
        {
            Speed = 3.5f;
            Health = 50;
            CanFly = true; // caso tenhas lógica de voo
        }

    }
    public class Turret : Unit
    {
        private Texture2D[] _attackFrames;
        private float _attackTimer = 0f;
        private float _attackSpeed = 0.4f;
        private int _frameIndex = 0;
        private float _detectionRadius = 400f;
        private bool _attacking = false;
        private Texture2D _baseTexture;
        private Texture2D _cannonTexture;
        private float _cannonAngle = 0f;
        private float? _lastMouseAimAngle = null;  // Guarda o ângulo quando mira no mouse
        public Turret(Vector2 position, Texture2D[] attackFrames)
        : base(position, attackFrames[0]) // idle frame como base
        {
            _attackFrames = attackFrames;
            Speed = 0f;
            Health = 200;
            Damage = 15;
        }


        public override async void Update(GameTime gameTime, Camera camera)
        {
            MouseState mouseState = Mouse.GetState();

            if (this.IsSelected && mouseState.RightButton == ButtonState.Pressed)
            {
                Vector2 mouseWorld = camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                Vector2 dir = mouseWorld - this.Position;
                _lastMouseAimAngle = (float)Math.Atan2(dir.Y, dir.X) + MathHelper.PiOver2 + MathHelper.Pi;
                _cannonAngle = _lastMouseAimAngle.Value;
            }
            else if (!_lastMouseAimAngle.HasValue)
            {
                Enemy nearestEnemy = Game1.Instance.Enemies
                    .Where(e => e.IsAlive && Vector2.Distance(this.Position, e.Position) <= _detectionRadius)
                    .OrderBy(e => Vector2.Distance(e.Position, this.Position))
                    .FirstOrDefault();

                if (nearestEnemy != null)
                {
                    Vector2 dir = nearestEnemy.Position - this.Position;
                    _cannonAngle = (float)Math.Atan2(dir.Y, dir.X) + MathHelper.PiOver2 + MathHelper.Pi;
                }
            }

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _attacking = false;

            Enemy targetEnemy = Game1.Instance.Enemies
                .Where(e => e.IsAlive && Vector2.Distance(this.Position, e.Position) <= _detectionRadius)
                .FirstOrDefault();

            if (targetEnemy != null)
            {
                _attacking = true;
                _attackTimer += deltaTime;
                _lastMouseAimAngle = null;

                if (_attackTimer >= _attackSpeed)
                {
                    targetEnemy.EnemyHealth -= Damage;

                    if (targetEnemy.EnemyHealth <= 0)
                    {
                        targetEnemy.Die(Game1.Instance.Coins, Game1.Instance.CoinFrames);
                        _attackTimer = 0f; // apenas zera o timer para o próximo ataque
                        _frameIndex = 0;   // volta para idle pois inimigo morreu
                    }
                    else
                    {
                        _frameIndex = (_frameIndex + 1) % _attackFrames.Length;
                        _attackTimer = 0f;
                    }
                }
            }
            else
            {
                _attacking = false;
                _frameIndex = 0;
                _attackTimer = 0f;
            }

            base.Update(gameTime, camera);
        }



        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D currentFrame = _attacking ? _attackFrames[_frameIndex] : _attackFrames[0];
            spriteBatch.Draw(
              currentFrame,
            Position,
            null,
            Color.White,
            _cannonAngle,
            new Vector2(currentFrame.Width / 2f, currentFrame.Height / 2f),
            4.0f, // <-- AUMENTO do tamanho (escala dobrada)
            SpriteEffects.None,
            0f
            );
        }
    }
    public class Projectile
    {
        private Texture2D _texture;
        public Vector2 Position;
        private Vector2 _velocity;
        public bool Hit = false;
        private float _speed = 300f;
        public Enemy Target;

        public Projectile(Vector2 start, Enemy target, Texture2D texture)
        {
            Position = start;
            Target = target;
            _texture = texture;
            Vector2 direction = target.Position - start;
            direction.Normalize();
            _velocity = direction * _speed;
        }

        public void Update(GameTime gameTime)
        {
            if (Target == null || Target.IsDead)
            {
                Hit = true;
                return;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Position += _velocity * dt;

            if (Vector2.Distance(Position, Target.Position) < 20f)
            {
                Hit = true;
                Target.EnemyHealth -= 50; // dano direto
                Game1.Instance._explosionSound.Play();
                if (Target.EnemyHealth <= 0)
                {
                    Target.Die(Game1.Instance.Coins, Game1.Instance.CoinFrames); // ou como estiveres a chamar
                    Target.IsAlive = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);
            spriteBatch.Draw(_texture, Position, null, Color.White, 0f, origin, 2f, SpriteEffects.None, 0f);
        }
    }
    public class Explosion
    {
        private Texture2D[] _frames;
        private int _frameIndex = 0;
        private float _frameTimer = 0f;
        private float _frameSpeed = 0.2f;
        public Vector2 Position;
        public bool Done => _frameIndex >= _frames.Length - 1;

        public Explosion(Vector2 position, Texture2D[] frames)
        {
            Position = position;
            _frames = frames;
        }

        public void Update(GameTime gameTime)
        {
            _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_frameTimer >= _frameSpeed)
            {
                _frameIndex++;
                _frameTimer = 0f;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_frameIndex < _frames.Length)
            {
                Vector2 origin = new Vector2(_frames[_frameIndex].Width / 2f, _frames[_frameIndex].Height / 2f);
                spriteBatch.Draw(_frames[_frameIndex], Position, null, Color.White, 0f, origin, 2f, SpriteEffects.None, 0f);
            }
        }
    }
}


