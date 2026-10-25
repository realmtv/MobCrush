using System;
using System.Drawing;

namespace MobCrush
{
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
            VX = (_rng.Next(-3, 4)) * 1.5f;
            VY = (_rng.Next(-3, 4)) * 1.5f;
            // Исправлено: было Next(10,10) -> теперь Next(10) (0..9), итого жизнь 15..24 кадра
            Life = 15 + _rng.Next(10);
        }

        public void Update()
        {
            X += VX;
            Y += VY;
            Life--;
        }

        public void Draw(Graphics g)
        {
            g.FillRectangle(_brush, X - 2, Y - 2, 4, 4);
        }
    }
}