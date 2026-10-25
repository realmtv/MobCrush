using System;
using System.Collections.Generic;
using System.Drawing;

namespace TricksGame
{
    // Добавлен интерфейс IDisposable для исправления ошибки вызова Dispose() в GameManager
    public class Enemy : IDisposable
    {
        public int X { get; set; }
        public int Y { get; set; }

        public int XPReward { get; private set; }

        private int _currentHp;
        private bool _isDead;
        private Brush _bodyBrush;
        private float _speedMultiplier;

        private List<Particle> _particles = new List<Particle>();
        private int _flashTimer;

        public bool IsDead => _isDead;

        public Enemy(int x, int y)
        {
            X = x;
            Y = y;
            Random rng = new Random();
            int type = rng.Next(0, 10);

            if (type < 6)
            {
                _currentHp = GameConfig.Enemy.NormalHP;
                _speedMultiplier = GameConfig.Enemy.NormalSpeed;
                _bodyBrush = new SolidBrush(Color.Red);
                XPReward = 2;
            }
            else if (type < 9)
            {
                _currentHp = GameConfig.Enemy.FastHP;
                _speedMultiplier = GameConfig.Enemy.FastSpeed;
                _bodyBrush = new SolidBrush(Color.Orange);
                XPReward = 1;
            }
            else
            {
                _currentHp = GameConfig.Enemy.TankHP;
                _speedMultiplier = GameConfig.Enemy.TankSpeed;
                _bodyBrush = new SolidBrush(Color.Purple);
                XPReward = 3;
            }
        }

        public void TakeDamage(int damage)
        {
            if (_isDead) return;

            _currentHp -= damage;
            _flashTimer = 5;

            if (_currentHp <= 0)
            {
                _isDead = true;
                for (int i = 0; i < 10; i++)
                {
                    _particles.Add(new Particle(X, Y, _bodyBrush));
                }
            }
        }

        public void Update(int speed)
        {
            if (!_isDead)
            {
                Y += (int)(speed * _speedMultiplier);
                if (_flashTimer > 0) _flashTimer--;
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (_particles[i].Life <= 0)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Draw(Graphics g)
        {
            if (!_isDead)
            {
                Brush currentBrush = (_flashTimer > 0) ? Brushes.White : _bodyBrush;
                int radius = GameConfig.Enemy.Radius;
                g.FillEllipse(currentBrush, X - radius, Y - radius, radius * 2, radius * 2);
            }

            foreach (Particle p in _particles)
            {
                p.Draw(g);
            }
        }

        public bool IsOffScreen(int screenHeight)
        {
            if (!_isDead)
                return Y > screenHeight + 10;
            return _particles.Count == 0;
        }

        public bool CollidesWithPlayer(int pX, int pY)
        {
            if (_isDead) return false;
            int dx = X - pX;
            int dy = Y - pY;
            return Math.Abs(dx) + Math.Abs(dy) < 20;
        }

        // Реализация IDisposable для корректного освобождения кисти
        public void Dispose()
        {
            _bodyBrush?.Dispose();
            _particles.Clear();
        }
    }
}