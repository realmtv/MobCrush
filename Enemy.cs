using System;
using System.Drawing;

namespace TricksGame
{
    public class Enemy
    {
        public int X, Y;

        public Enemy(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Update(int speed)
        {
            Y += speed;
        }

        public bool IsOffScreen(int screenHeight)
        {
            return Y > screenHeight;
        }

        // Проверка столкновения передается координаты игрока
        public bool CollidesWithPlayer(int playerX, int playerY)
        {
            // Та же простая проверка расстояния, что и раньше
            return Math.Abs(X - playerX) < 20 && Math.Abs(Y - playerY) < 20;
        }

        public void Draw(Graphics g)
        {
            Brush brush = new SolidBrush(Color.Red);
            g.FillEllipse(brush, X - 6, Y - 6, 12, 12);
        }
    }
}