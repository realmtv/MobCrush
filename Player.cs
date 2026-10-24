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
        
        // --- Параметры атаки ---
        // Ускорили удар (было 5f) для более отзывчивого управления
        private const float AttackSpeed = 12f; 
        private const float MaxSwingAngle = 45f;

        // --- I-Frames (Неуязвимость) ---
        private int _invulnerableTimer;
        public const int InvulnerableDuration = 30; // ~0.5 секунды при 60 FPS

        // --- Ресурсы (создаются 1 раз в конструкторе) ---
        private readonly SolidBrush _bodyBrush;
        private readonly SolidBrush _handBrush;
        private readonly SolidBrush _hitBrush; // Красная кисть для получения урона

        // --- Состояние игрока ---
        public int X { get; set; }
        public int Y { get; set; }
        
        public int CurrentHp { get; private set; }
        public int MaxHp { get; private set; }

        // Состояние атаки
        private float _handAngle;
        private bool _isAttacking;
        private float _swingDirection;

        public bool IsAttacking => _isAttacking;

        public Player(int screenWidth, int screenHeight)
        {
            // Инициализация ресурсов
            _bodyBrush = new SolidBrush(Color.Green);
            _handBrush = new SolidBrush(Color.Orange);
            _hitBrush = new SolidBrush(Color.Red); // Цвет при получении урона

            MaxHp = 100;
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
            if (!_isAttacking)
            {
                _isAttacking = true;
                _handAngle = 0f;
                _swingDirection = 1f;
            }
        }

        public void Update()
        {
            // 1. Логика I-Frames (таймер неуязвимости)
            if (_invulnerableTimer > 0) _invulnerableTimer--;

            // 2. Логика анимации руки
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
            // Если активны I-Frames — урон игнорируется
            if (_invulnerableTimer > 0) return;

            CurrentHp -= amount;
            if (CurrentHp < 0) CurrentHp = 0;

            // Запускаем таймер неуязвимости
            _invulnerableTimer = InvulnerableDuration;
        }

        public bool IsDead => CurrentHp <= 0;

        /// <summary>
        /// Возвращает прямоугольник хитбокса руки для проверки коллизий в GameScreen.
        /// </summary>
        public Rectangle GetAttackHitbox()
        {
            double angleRad = _handAngle * Math.PI / 180.0;
            int handX = (int)(X + HandOrbitRadius * Math.Cos(angleRad));
            int handY = (int)(Y - HandOrbitRadius * Math.Sin(angleRad));

            // Возвращаем прямоугольник вокруг руки
            return new Rectangle(handX - HandRadius, handY - HandRadius, HandRadius * 2, HandRadius * 2);
        }

        public void Draw(Graphics g)
        {
            // 1. Рисуем тело
            // Если есть таймер неуязвимости — рисуем красным, иначе зеленым
            Brush currentBodyBrush = (_invulnerableTimer > 0) ? _hitBrush : _bodyBrush;
            g.FillEllipse(currentBodyBrush, X - Radius, Y - Radius, Radius * 2, Radius * 2);

            // 2. Вычисляем и рисуем руку
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