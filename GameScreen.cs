using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace TricksGame
{
    public class GameScreen : UserControl
    {
        private MainForm mainForm;
        private Player player;
        private List<Enemy> enemies = new List<Enemy>();
        private System.Windows.Forms.Timer gameTimer;
        private Random random = new Random();
        
        // Параметры мира
        private int roadWidth = 200;
        private int speed = 2; 
        private bool isGameOver = false;

        public GameScreen(MainForm form)
        {
            mainForm = form;
            
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.BackColor = Color.DarkGray;
            
            // Инициализируем игрока
            player = new Player(this.Width, this.Height);

            // Обработка клика
            this.MouseClick += (s, e) => {
                if (e.Button == MouseButtons.Left && !isGameOver)
                {
                    player.Attack();
                }
            };

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; 
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!isGameOver) Update();
            this.Invalidate(); 
        }

        private void Update()
        {
            // Обновляем позицию игрока (на всякий случай)
            player.UpdatePosition(this.Width, this.Height);
            player.Update();

            // 1. Спавн врагов
            if (enemies.Count < 10)
            {
                int roadLeft = (this.Width - roadWidth) / 2;
                int mobX = random.Next(roadLeft + 20, roadLeft + roadWidth - 20);
                enemies.Add(new Enemy(mobX, -20));
            }

            // 2. Движение и коллизии
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy m = enemies[i];
                m.Update(speed);

                // Используем метод самого врага для проверки
                if (m.CollidesWithPlayer(player.X, player.Y))
                {
                    player.TakeDamage(10);
                    enemies.RemoveAt(i); 
                    if (player.IsDead) { GameOver(); }
                    continue;
                }

                if (m.IsOffScreen(this.Height)) enemies.RemoveAt(i);
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop();

            var returnTimer = new System.Windows.Forms.Timer();
            returnTimer.Interval = 2500;
            returnTimer.Tick += (s, e) => {
                returnTimer.Stop();
                mainForm.ShowScreen(new MainMenu(mainForm));
            };
            returnTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            g.Clear(Color.DarkGray);

            // Дорога
            int roadLeft = (this.Width - roadWidth) / 2;
            g.FillRectangle(Brushes.Gray, roadLeft, 0, roadWidth, this.Height);

            // Рисуем игрока и врагов через их собственные методы
            player.Draw(g);

            foreach (var mob in enemies)
            {
                mob.Draw(g);
            }

            // HP BAR (Снизу слева)
            int barWidth = 150;
            int barHeight = 20;
            int x = 10;
            int y = this.Height - 50;

            g.FillRectangle(Brushes.LightGray, x, y, barWidth, barHeight);
            g.DrawRectangle(Pens.Black, x, y, barWidth, barHeight);
            int currentBarWidth = (int)((float)player.CurrentHp / player.MaxHp * barWidth);
            g.FillRectangle(Brushes.Red, x, y, currentBarWidth, barHeight);
            g.DrawString($"{player.CurrentHp} / {player.MaxHp}", new Font("Arial", 12), Brushes.Black, new Point(x + 5, y + 2));

            // Game Over Overlay
            if (isGameOver)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, this.Width, this.Height);
                
                Font bigFont = new Font("Arial", 40, FontStyle.Bold);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("GAME OVER", bigFont, Brushes.Red, new Rectangle(0, 0, this.Width, this.Height), sf);

                Font saveFont = new Font("Arial", 14);
                Brush saveBrush = new SolidBrush(Color.FromArgb(150, 150, 150, 150)); 
                Point savePoint = new Point(this.Width - 200, this.Height - 40);
                
                g.DrawString("игра сохраняется", saveFont, saveBrush, savePoint);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            gameTimer.Stop();
            base.OnHandleDestroyed(e);
        }
    }
}