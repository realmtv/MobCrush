using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace TricksGame
{
    // --- 1. ТОЧКА ВХОДА ---
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new MainForm());
        }
    }

    // --- 2. ГЛАВНОЕ ОКНО ---
    public class MainForm : Form
    {
        private Panel screenContainer;

        public MainForm()
        {
            this.Text = "Tricks";
            this.Size = new Size(480, 720); 
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            screenContainer = new Panel();
            screenContainer.Dock = DockStyle.Fill;
            this.Controls.Add(screenContainer);

            // Сначала показываем загрузку
            ShowScreen(new LoadingScreen(this));
        }

        public void ShowScreen(UserControl screen)
        {
            // Очищаем старый экран, если он есть
            if (screenContainer.Controls.Count > 0)
                screenContainer.Controls[0].Dispose();
                
            screen.Dock = DockStyle.Fill;
            screenContainer.Controls.Add(screen);
        }
    }

    // --- 3. ЗАГРУЗКА ---
    public class LoadingScreen : UserControl
    {
        public LoadingScreen(MainForm form)
        {
            this.BackColor = Color.Black;
            Label label = new Label { Text = "Loading...", ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Dock = DockStyle.Fill };
            this.Controls.Add(label);

            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; 
            timer.Tick += (s, e) => { timer.Stop(); form.ShowScreen(new MainMenu(form)); };
            timer.Start();
        }
    }

    // --- 4. МЕНЮ ---
    public class MainMenu : UserControl
    {
        private FlowLayoutPanel panel;
        private MainForm mainForm;

        public MainMenu(MainForm form)
        {
            mainForm = form;
            this.BackColor = Color.FromArgb(40, 40, 40);
            
            panel = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true };
            this.Controls.Add(panel);
            
            // Центрирование панели
            this.Resize += (s, e) => { 
                panel.Location = new Point((this.Width - panel.Width)/2, (this.Height - panel.Height)/2); 
            };
            panel.Location = new Point(140, 300);

            // Кнопка "Играть" запускает новый экран игры
            AddButton("ИГРАТЬ", () => mainForm.ShowScreen(new GameScreen(mainForm)));
            AddButton("ОПЦИИ", () => {});
            AddButton("ВЫХОД", () => Application.Exit());
        }

        private void AddButton(string text, Action onClick)
        {
            Button btn = new Button { Text = text, Size = new Size(200, 50), Font = new Font("Arial", 14), Margin = new Padding(10), FlatStyle = FlatStyle.Flat, BackColor = Color.Gray, ForeColor = Color.Black };
            btn.Click += (s, e) => onClick();
            panel.Controls.Add(btn);
        }
    }

    // --- 5. ИГРОВОЙ ЭКРАН ---
    public class GameScreen : UserControl
    {
        private MainForm mainForm;
        private List<Mob> enemies = new List<Mob>();
        private System.Windows.Forms.Timer gameTimer;
        private Random random = new Random();
        
        // Параметры игрока и мира
        private int currentHp = 100;
        private int maxHp = 100;
        private int roadWidth = 200;
        private int speed = 5;
        private bool isGameOver = false;

        // Координаты игрока (для удобства в одном месте)
        private int playerX, playerY;

        public GameScreen(MainForm form)
        {
            mainForm = form;
            
            // Двойная буферизация для плавности
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint, true);
            this.UpdateStyles();

            this.BackColor = Color.DarkGray;
            
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; 
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!isGameOver) Update();
            this.Invalidate(); // Перерисовка
        }

        private void Update()
        {
            // Координаты игрока (снизу по центру)
            playerX = this.Width / 2;
            playerY = this.Height - 60;

            // 1. Спавн врагов
            if (enemies.Count < 10)
            {
                int roadLeft = (this.Width - roadWidth) / 2;
                // Спавним в пределах дороги
                int mobX = random.Next(roadLeft + 20, roadLeft + roadWidth - 20);
                enemies.Add(new Mob(mobX, -20));
            }

            // 2. Движение и Коллизии
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Mob m = enemies[i];
                m.Y += speed;

                // --- ПРОВЕРКА СТОЛКНОВЕНИЯ ---
                // Формула: расстояние между центрами < суммы радиусов (примерно 20 пикселей)
                // Используем простое сравнение для скорости вычислений
                int distX = Math.Abs(m.X - playerX);
                int distY = Math.Abs(m.Y - playerY);

                // Если мобы очень близко по X и по Y - считаем, что они врезались
                if (distX < 20 && distY < 20)
                {
                    currentHp -= 10;
                    enemies.RemoveAt(i); // Удаляем врага при ударе
                    
                    // Проверка на смерть
                    if (currentHp <= 0)
                    {
                        currentHp = 0;
                        GameOver();
                    }
                    continue;
                }

                // Удаление врагов, ушедших за экран
                if (m.Y > this.Height)
                {
                    enemies.RemoveAt(i);
                }
            }
        }

        // Логика проигрыша
        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop(); // Останавливаем игровой цикл

            // Запускаем таймер возврата в меню (через 2 секунды)
            var returnTimer = new System.Windows.Forms.Timer();
            returnTimer.Interval = 2000;
            returnTimer.Tick += (s, e) => {
                returnTimer.Stop();
                mainForm.ShowScreen(new MainMenu(mainForm)); // Возврат в меню
            };
            returnTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Очистка экрана цветом фона (чтобы убрать следы)
            g.Clear(Color.DarkGray);

            // 1. Дорога
            int roadLeft = (this.Width - roadWidth) / 2;
            g.FillRectangle(Brushes.Gray, roadLeft, 0, roadWidth, this.Height);

            // 2. ГГ (Зеленая точка)
            // Rисуем игрока только если мы еще в игре (или рисуем мертвого, если хочешь)
            Brush ggBrush = new SolidBrush(Color.Green);
            g.FillEllipse(ggBrush, playerX - 10, playerY - 10, 20, 20);

            // 3. Враги (Красные точки)
            Brush mobBrush = new SolidBrush(Color.Red);
            foreach (var mob in enemies)
            {
                g.FillEllipse(mobBrush, mob.X - 6, mob.Y - 6, 12, 12);
            }

            // --- 4. РИСУЕМ HP BAR (Сверху слева) ---
            int barWidth = 150;
            int barHeight = 20;
            int x = 10;
            int y = 10;

            // Белый фон полоски
            g.FillRectangle(Brushes.LightGray, x, y, barWidth, barHeight);
            g.DrawRectangle(Pens.Black, x, y, barWidth, barHeight);

            // Красное здоровье (пропорционально текущему HP)
            int currentBarWidth = (int)((float)currentHp / maxHp * barWidth);
            g.FillRectangle(Brushes.Red, x, y, currentBarWidth, barHeight);

            // Надпись цифрами
            g.DrawString($"{currentHp} / {maxHp}", new Font("Arial", 12), Brushes.Black, new Point(x + 5, y + 2));


            // --- 5. РИСУЕМ GAME OVER ---
            if (isGameOver)
            {
                // Полупрозрачная маска (черный квадрат поверх всего)
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, this.Width, this.Height);
                
                // Текст
                Font bigFont = new Font("Arial", 40, FontStyle.Bold);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("GAME OVER", bigFont, Brushes.Red, new Rectangle(0, 0, this.Width, this.Height), sf);
                
                Font smallFont = new Font("Arial", 20);
                g.DrawString("Возврат в меню...", smallFont, Brushes.White, new Rectangle(0, 0, this.Width, this.Height), sf);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            gameTimer.Stop();
            base.OnHandleDestroyed(e);
        }
    }

    // --- КЛАСС ВРАГА ---
    class Mob
    {
        public int X, Y;
        public Mob(int x, int y) {
            X = x; 
            Y = y; 
        }
    }
}