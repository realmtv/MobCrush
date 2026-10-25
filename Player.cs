using System;
using System.Drawing;

namespace TricksGame
{
    public class Player : IDisposable
    {
        // Используем настройки из конфига
        private const int HandRadius = GameConfig.Player.HandRadius;
        private const int HandOrbitRadius = GameConfig.Player.HandOrbitRadius;
        private const float AttackSpeed = GameConfig.Player.AttackSpeed;
        private const float MaxSwingAngle = GameConfig.Player.MaxSwingAngle;

        public float AttackRange { get; set; } = GameConfig.Player.DefaultAttackRange;

        private readonly int _roadWidth;
        private int _invulnerableTimer;
        
        private readonly SolidBrush _bodyBrush;
        private readonly SolidBrush _handBrush;
        private readonly SolidBrush _hitBrush;

        public int X { get; set; }
        public int Y { get; set; }
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }

        private float _handAngle;
        private bool _isAttacking;
        private float _swingDirection;

        public bool IsAttacking => _isAttacking;

        public Player(int screenWidth, int screenHeight, int roadWidth)
        {
            _roadWidth = roadWidth;
            _bodyBrush = new SolidBrush(Color.Green);
            _handBrush = new SolidBrush(Color.Orange);
            _hitBrush = new SolidBrush(Color.Red);

            MaxHp = GameConfig.Player.MaxHP;
            CurrentHp = MaxHp;
            UpdatePosition(screenWidth, screenHeight);
        }

        public void UpdatePosition(int screenWidth, int screenHeight)
        {
            X = screenWidth / 2;
            Y = screenHeight - 60;
        }

        public void Attack()
        {
            _isAttacking = true;
            _swingDirection = 1f;
        }

        public void Update()
        {
            if (_invulnerableTimer > 0) _invulnerableTimer--;

            if (_isAttacking)
            {
                _handAngle += AttackSpeed * _swingDirection;

                if (_handAngle >= MaxSwingAngle) _swingDirection = -1f;
                if (_handAngle <= 0f) { _handAngle = 0f; _isAttacking = false; }
            }
        }

        public void TakeDamage(int amount)
        {
            if (_invulnerableTimer > 0) return;
            CurrentHp -= amount;
            if (CurrentHp < 0) CurrentHp = 0;
            _invulnerableTimer = GameConfig.Player.InvulnerableDuration;
        }

        public bool IsDead => CurrentHp <= 0;

        public bool IsInAttackZone(int targetX, int targetY)
        {
            int dx = targetX - X;
            int dy = targetY - Y;

            if (dy >= 0) return false;
            double distSq = (double)dx * dx + (double)dy * dy;
            if (distSq > (double)AttackRange * AttackRange) return false;
            if (Math.Abs(dx) > _roadWidth / 2) return false;

            return true;
        }

        public void Draw(Graphics g)
        {
            int radius = GameConfig.Player.Radius;
            Brush currentBodyBrush = (_invulnerableTimer > 0) ? _hitBrush : _bodyBrush;
            g.FillEllipse(currentBodyBrush, X - radius, Y - radius, radius * 2, radius * 2);

            double angleRad = _handAngle * Math.PI / 180.0;
            int handX = (int)(X + HandOrbitRadius * Math.Cos(angleRad));
            int handY = (int)(Y - HandOrbitRadius * Math.Sin(angleRad));

            g.FillEllipse(_handBrush, handX - HandRadius, handY - HandRadius, HandRadius * 2, HandRadius * 2);
        }

        public void Dispose()
        {
            _bodyBrush?.Dispose();
            _handBrush?.Dispose();
            _hitBrush?.Dispose();
        }
    }
}