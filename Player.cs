using System;
using System.Drawing;

namespace TricksGame
{
    public class Player : IDisposable
    {
        // --- Визуальные константы ---
        public const int Radius = 10;
        private const int HandRadius = 8;
        private const int HandOrbitRadius = 35;
        
        // --- Параметры анимации ---
        private const float AttackSpeed = 12f; 
        private const float MaxSwingAngle = 45f;

        // --- Параметры Меча (Хитбокс) ---
        public float AttackRange { get; set; } = 95f; // Дальность меча

        // Геометрия дороги (запоминаем при инициализации)
        private readonly int _roadWidth;

        // --- I-Frames (Неуязвимость) ---
        private int _invulnerableTimer;
        public const int InvulnerableDuration = 30; 

        // --- Ресурсы (создаются 1 раз) ---
        private readonly SolidBrush _bodyBrush;
        private readonly SolidBrush _handBrush;
        private readonly SolidBrush _hitBrush;

        // --- Состояние ---
        public int X { get; set; }
        public int Y { get; set; }
        
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }

        // Анимация
        private float _handAngle;
        private bool _isAttacking;
        private float _swingDirection;

        public bool IsAttacking => _isAttacking;

        // КОНСТРУКТОР
        public Player(int screenWidth, int screenHeight, int roadWidth)
        {
            _roadWidth = roadWidth;

            _bodyBrush = new SolidBrush(Color.Green);
            _handBrush = new SolidBrush(Color.Orange);
            _hitBrush = new SolidBrush(Color.Red);

            MaxHp = 100;
            CurrentHp = MaxHp;
            UpdatePosition(screenWidth, screenHeight);
        }

        public void UpdatePosition(int screenWidth, int screenHeight)
        {
            X = screenWidth / 2;
            Y = screenHeight - 60;
        }

        // Спам-атака: просто включаем флаг и толкаем вперед
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

                if (_handAngle >= MaxSwingAngle) 
                    _swingDirection = -1f;

                if (_handAngle <= 0f) 
                {
                    _handAngle = 0f;
                    _isAttacking = false;
                }
            }
        }

        public void TakeDamage(int amount)
        {
            if (_invulnerableTimer > 0) return;
            
            CurrentHp -= amount;
            if (CurrentHp < 0) CurrentHp = 0;
            
            _invulnerableTimer = InvulnerableDuration;
        }

        public bool IsDead => CurrentHp <= 0;

        /// <summary>
        /// Проверка попадания в "зону меча" (Верхний полукруг).
        /// </summary>
        public bool IsInAttackZone(int targetX, int targetY)
        {
            int dx = targetX - X;
            int dy = targetY - Y;

            // 1. Только вверх (не бить в пол)
            if (dy >= 0) return false; 

            // 2. Радиус (дальность меча)
            double distSq = (double)dx * dx + (double)dy * dy;
            if (distSq > (double)AttackRange * AttackRange) return false;

            // 3. Жестко по краям дороги (Edge-to-Edge)
            if (Math.Abs(dx) > _roadWidth / 2) return false;

            return true;
        }

        public void Draw(Graphics g)
        {
            Brush currentBodyBrush = (_invulnerableTimer > 0) ? _hitBrush : _bodyBrush;
            g.FillEllipse(currentBodyBrush, X - Radius, Y - Radius, Radius * 2, Radius * 2);

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