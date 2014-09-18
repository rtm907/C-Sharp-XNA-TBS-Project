using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNATBS
{
    public abstract class Anim
    {
        private Texture2D[] _sprites;
        public Texture2D[] Sprites
        {
            get
            {
                return _sprites;
            }
        }

        private Texture2D _currentSprite;
        public Texture2D CurrentSprite
        {
            get
            {
                return _currentSprite;
            }
        }


        private Vector2 _location;
        public Vector2 Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
            }
        }

        private Int32 _durationMax;
        public Int32 DurationMax
        {
            get
            {
                return _durationMax;
            }
        }
        private Int32 _durationElapsed;
        public Int32 DurationElapsed
        {
            get
            {
                return _durationElapsed;
            }
        }

        public void IncremementDuration()
        {
            ++_durationElapsed;
        }
        public bool Expired()
        {
            return _durationElapsed > _durationMax;
        }

        public abstract void Update();

        protected Rectangle GetDrawRectangle(Vector2 screenAnchor, float zoom)
        {
            Vector2 pos = this._location + screenAnchor;
            return new Rectangle((int)(pos.X * zoom), (int)(pos.Y * zoom), 
                (int)(_currentSprite.Width * zoom),(int)( _currentSprite.Height * zoom));
        }

        public abstract void Draw(SpriteBatch spriteBatch, Vector2 screenAnchor, Color drawColor, float zoom);



        public Anim(Int32 durationMax, Texture2D[] sprites, Vector2 location)
        {
            _durationMax = durationMax;
            _durationElapsed = 0;
            _sprites = sprites;
            _currentSprite = _sprites[0];
            _location = location;
        }
    }

    public class AnimUnitMove : Anim
    {
        private Unit _agent;
        public Unit Agent
        {
            get
            {
                return _agent;
            }
        }

        private Vector2 _goal;
        public Vector2 Goal
        {
            get
            {
                return Goal;
            }
        }

        private Vector2 _delta;

        public override void Update()
        {
            this.Location += _delta;
            this.IncremementDuration();
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 screenAnchor, Color drawColor, float zoom)
        {
            spriteBatch.Draw(this.CurrentSprite, base.GetDrawRectangle(screenAnchor, zoom), drawColor);
        }

        public AnimUnitMove(Unit agent, Int32 durationMax, Texture2D sprite, Vector2 location, Vector2 goal)
            : base(durationMax, new Texture2D[] {sprite}, location)
        {
            _agent = agent;
            _goal = goal;

            _delta = (goal - location);
            _delta.X = _delta.X / durationMax;
            _delta.Y = _delta.Y / durationMax;
        }
    }

    public class AnimWeaponSlash : Anim
    {
        //private Vector2 _attackDir;
        private float _angle = (float)Math.PI / 2;
        private Vector2 _origin;

        public override void Update()
        {
            _angle += (float)(2 * Math.PI / (this.DurationMax * 6));
            this.IncremementDuration();
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 screenAnchor, Color drawColor, float zoom)
        {
            spriteBatch.Draw(this.CurrentSprite, base.GetDrawRectangle(screenAnchor, zoom), null, drawColor, _angle, _origin, SpriteEffects.None, 0f);
        }

        public AnimWeaponSlash(Int32 durationMax, Texture2D sprite, Vector2 location)
            : base(durationMax, new Texture2D[] { sprite }, location)
        {
            //_attackDir = attackDir;
            //_angle = new Vector(attackDir.X, attackDir.Y).
            //_angle = (float)((((sbyte)attackDir + 5) % 6) * 2 * Math.PI / 6);
            _origin.X = sprite.Width / 2;
            _origin.Y = sprite.Height / 2;
        }
    }

    public class AnimProjectile : Anim
    {
        private Vector2 _goal;
        private Vector2 _delta;
        private float _angle;

        public override void Update()
        {
            this.Location += _delta;
            this.IncremementDuration();
        }

        public override void Draw(SpriteBatch spriteBatch, Vector2 screenAnchor, Color drawColor, float zoom)
        {
            spriteBatch.Draw(this.CurrentSprite, base.GetDrawRectangle(screenAnchor, zoom), null, drawColor, _angle, new Vector2(0, 0), SpriteEffects.None, 0);
        }

        public AnimProjectile(Int32 durationMax, Texture2D sprite, Vector2 origin, Vector2 goal)
            : base(durationMax, new Texture2D[] { sprite }, origin)
        {
            _goal = goal;
            _delta = (goal - origin);
            _delta.X = _delta.X / durationMax;
            _delta.Y = _delta.Y / durationMax;

            _angle = StaticMathFunctions.VectorToAngle(_delta);
        }
    }

    public class Particle
    {
        private Texture2D _sprite;
        private Vector2 _position;
        private float _speedMax;
        private Vector2 _speedCurrent = new Vector2();
        private Vector2 _acceleration;
        private Int32 _lifeElapsed=0;
        private Int32 _lifeMax;
        private Color _colorInitial;
        private Color _colorFinal;

        public Texture2D Sprite
        {
            get
            {
                return _sprite;
            }
        }
        public Vector2 Position
        {
            get
            {
                return _position;
            }
        }
        public bool Expired()
        {
            return _lifeElapsed > _lifeMax;
        }
        public Color ColorCurrent()
        {
            return Color.Lerp(_colorInitial, _colorFinal, (float)_lifeElapsed / _lifeMax);
        }

        public void Update()
        {
            _speedCurrent += _acceleration;
            if (_speedCurrent.Length() > _speedMax)
            {
                _speedCurrent.Normalize();
                _speedCurrent *= _speedMax;
            }
            _position += _speedCurrent;

            ++_lifeElapsed;
        }

        public Particle(Texture2D sprite, Vector2 position, float speedMax, Vector2 acceleration, Int32 lifeMax, Color initial, Color final)
        {
            _sprite = sprite;
            _position = position;
            _speedMax = speedMax;
            _acceleration = acceleration;
            _lifeMax = lifeMax;
            _colorInitial = initial;
            _colorFinal = final;
        }
    }

    public class ParticleEmitter
    {
        private List<Particle> _particleList = new List<Particle>();
        public List<Particle> ParticleList
        {
            get
            {
                return _particleList;
            }
        }

        private Texture2D _sprite;
        private Int32 _maxParticles;
        private Vector2 _source;
        //private Vector2 _direction;
        private float _speedMax;
        private float _acceleration;
        //private float _angle;
        private Int32 _durationMax;
        private Int32 _durationCurrent = 0;
        private Color _colorInitial;
        private Color _colorFinal;

        public void Update()
        {
            ++_durationCurrent;
            foreach (Particle p in _particleList)
            {
                p.Update();
            }
        }

        public bool Expired()
        {
            return _durationCurrent > _durationMax;
        }

        public ParticleEmitter(Texture2D sprite, Int32 maxParticles, Vector2 source, float acceleration, float speedMax, Int32 durationMax, Color initial, Color final)
        {
            _sprite = sprite;
            _maxParticles = maxParticles;
            _source = source;
            _acceleration = acceleration;
            _speedMax = speedMax;
            _durationMax = durationMax;
            _colorInitial = initial;
            _colorFinal = final;

            Random random = new Random();
            for (int i = 0; i < maxParticles; ++i)
            {
                double seed = random.NextDouble();
                Particle newOne = new Particle(_sprite, _source, _speedMax,
                    new Vector2((float)(random.NextDouble() - 0.5) * 2 * _acceleration, (float)(random.NextDouble() - 0.5) * 2 * _acceleration),
                    _durationMax, _colorInitial, _colorFinal);
                _particleList.Add(newOne);
            }
        }
    }
}
