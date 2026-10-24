using System;
using System.Drawing;

namespace TricksGame
{
    public class Player
    {
        public int X, Y;
        public int CurrentHp, MaxHp;

        // Параметры атаки
        private float handAngle = 0f;
        private bool isAttacking = false;
        private float attackSpeed = 5f;
        private float swingDirection = 1f;

        public Player(int screenWidth, int screenHeight)
        {
            MaxHp = 100;
            CurrentHp = MaxHp;
            UpdatePosition(screenWidth, screenHeight);
        }

        // Обновление позиции (герой всегда внизу по центру)
        public void UpdatePosition(int screenWidth, int screenHeight)
        {
            X = screenWidth / 2;
            Y = screenHeight - 60;
        }

        public void Attack()
        {
            if (!isAttacking)
            {
                isAttacking = true;
                handAngle = 0f;
                swingDirection = 1f; // Мах вперед
            }
        }

        public void Update()
        {
            // Логика анимации руки
            if (isAttacking)
            {
                handAngle += attackSpeed * swingDirection;
                if (handAngle >= 45f) swingDirection = -1f; // Возврат
                if (handAngle <= 0f) { handAngle = 0f; isAttacking = false; }
            }
        }

        public void TakeDamage(int amount)
        {
            CurrentHp -= amount;
            if (CurrentHp < 0) CurrentHp = 0;
        }

        public bool IsDead => CurrentHp <= 0;

        public void Draw(Graphics g)
        {
            // Рисуем героя
            Brush ggBrush = new SolidBrush(Color.Green);
            g.FillEllipse(ggBrush, X - 10, Y - 10, 20, 20);

            // Рисуем руку
            int handRadiusOrbit = 35;
            double angleRad = handAngle * Math.PI / 180.0;
            int handX = (int)(X + handRadiusOrbit * Math.Cos(angleRad));
            int handY = (int)(Y - handRadiusOrbit * Math.Sin(angleRad));

            Brush handBrush = new SolidBrush(Color.Orange);
            g.FillEllipse(handBrush, handX - 8, handY - 8, 16, 16);
        }
    }
}