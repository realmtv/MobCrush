using System;
using System.Collections.Generic;
using System.Drawing;

namespace TricksGame
{
    // --- Класс Частицы (эффект взрыва при смерти врага) ---
    public class Particle
    {
        public float X, Y;
        public float VX, VY;
        public int Life;
        private readonly Brush _brush;
        private static readonly Random _rng = new Random();

        public Particle(int x, int y, Brush brush)
        {
            X = x;
            Y = y;
            _brush = brush;
            // Случайный разлет во все стороны
            VX = (_rng.Next(-3, 4)) * 1.5f;
            VY = (_rng.Next(-3, 4)) * 1.5f;
            // Время жизни частицы (15-25 кадров)
            Life = 15 + _rng.Next(10, 10);
        }

        public void Update()
        {
            X += VX;
            Y += VY;
            Life--;
        }

        public void Draw(Graphics g)
        {
            // Рисуем маленький квадрат 4x4
            g.FillRectangle(_brush, X - 2, Y - 2, 4, 4);
        }
    }

    // --- Класс Врага ---
    public class Enemy
    {
        public const int Radius = 6; // Радиус врага для рисования
        public int X { get; set; }
        public int Y { get; set; }

        // Поля состояния
        private int _currentHp;
        private bool _isDead;
        private Brush _bodyBrush;
        private float _speedMultiplier; // Множитель скорости (быстрый/медленный)

        // Система частиц и визуальные эффекты
        private List<Particle> _particles = new List<Particle>();
        private int _flashTimer; // Таймер мигания белым при ударе

        // Свойство для GameScreen (проверка смерти)
        public bool IsDead => _isDead;

        // КОНСТРУКТОР
        public Enemy(int x, int y)
        {
            X = x;
            Y = y;
            Random rng = new Random();

            // Генерация типа врага
            // 0-5 (60%) - Обычный
            // 6-8 (30%) - Быстрый
            // 9 (10%) - Танк
            int type = rng.Next(0, 10);

            if (type < 6)
            {
                // Обычный враг (Красный)
                _currentHp = 5;
                _speedMultiplier = 1.0f;
                _bodyBrush = new SolidBrush(Color.Red);
            }
            else if (type < 9)
            {
                // Быстрый враг (Оранжевый)
                _currentHp = 3;
                _speedMultiplier = 1.5f;
                _bodyBrush = new SolidBrush(Color.Orange);
            }
            else
            {
                // Танк (Фиолетовый)
                _currentHp = 15;
                _speedMultiplier = 0.7f;
                _bodyBrush = new SolidBrush(Color.Purple);
            }
        }

        // Получение урона
        public void TakeDamage(int damage)
        {
            if (_isDead) return;

            _currentHp -= damage;
            _flashTimer = 5; // Включаем мигание на 5 кадров

            if (_currentHp <= 0)
            {
                _isDead = true;
                // Создаем взрыв частиц в центре тела
                for (int i = 0; i < 10; i++)
                {
                    _particles.Add(new Particle(X, Y, _bodyBrush));
                }
            }
        }

        // Обновление (движение и частицы)
        public void Update(int speed)
        {
            // Если враг жив, он движется вниз
            if (!_isDead)
            {
                Y += (int)(speed * _speedMultiplier);
                if (_flashTimer > 0) _flashTimer--;
            }

            // Обновляем частицы и удаляем те, что отжили свое
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (_particles[i].Life <= 0)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        // Отрисовка
        public void Draw(Graphics g)
        {
            // 1. Рисуем тело (если жив)
            if (!_isDead)
            {
                // Если мигаем - рисуем белым, иначе своим цветом
                Brush currentBrush = (_flashTimer > 0) ? Brushes.White : _bodyBrush;
                g.FillEllipse(currentBrush, X - Radius, Y - Radius, Radius * 2, Radius * 2);
            }

            // 2. Рисуем все активные частицы
            foreach (Particle p in _particles)
            {
                p.Draw(g);
            }
        }

        // Проверка выхода за экран (нужна для удаления из списка в GameScreen)
        public bool IsOffScreen(int screenHeight)
        {
            if (!_isDead)
                return Y > screenHeight + 10; // Живой ушел вниз за край

            // Мертвый (с частицами) удаляем, когда все частицы исчезнут
            return _particles.Count == 0;
        }

        // Проверка коллизии с игроком (касание)
        public bool CollidesWithPlayer(int pX, int pY)
        {
            if (_isDead) return false;

            int dx = X - pX;
            int dy = Y - pY;
            // Простая проверка дистанции (Манхэттенское расстояние)
            int distance = Math.Abs(dx) + Math.Abs(dy);
            return distance < 20; // Если близко - считаем касанием
        }
    }
}