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
            this.Resize += (s, e) => { 
                panel.Location = new Point((this.Width - panel.Width)/2, (this.Height - panel.Height)/2); 
            };
            panel.Location = new Point(140, 300);

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
        
        // Параметры игрока
        private int currentHp = 100;
        private int maxHp = 100;
        private int playerX, playerY;
        
        // Параметры мира
        private int roadWidth = 200;
        private int speed = 5;
        private bool isGameOver = false;

        // --- АНИМАЦИЯ РУКИ ---
        private float handAngle = 0f;       // Текущий угол руки (в градусах)
        private bool isAttacking = false;   // Сейчас ли мы бьем?
        private float attackSpeed = 5f;     // Скорость анимации
        private int swingDirection = -1;    // Направление взмаха (-1 = вверх, 1 = вниз)

        public GameScreen(MainForm form)
        {
            mainForm = form;
            
            // Убираем мерцание
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            this.BackColor = Color.DarkGray;
            
            // Обработка нажатия мыши
            this.MouseClick += (s, e) => {
                if (e.Button == MouseButtons.Left && !isGameOver)
                {
                    StartAttack();
                }
            };

            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; 
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        // Функция запуска атаки
        private void StartAttack()
        {
            if (!isAttacking)
            {
                isAttacking = true;
                handAngle = 0f;
                swingDirection = -1; // Начинаем взмах вверх (вперед)
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!isGameOver) Update();
            this.Invalidate(); // Перерисовка
        }

        private void Update()
        {
            playerX = this.Width / 2;
            playerY = this.Height - 60;

            // 1. Спавн врагов
            if (enemies.Count < 10)
            {
                int roadLeft = (this.Width - roadWidth) / 2;
                int mobX = random.Next(roadLeft + 20, roadLeft + roadWidth - 20);
                enemies.Add(new Mob(mobX, -20));
            }

            // 2. Движение и коллизии
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Mob m = enemies[i];
                m.Y += speed;

                // Проверка столкновения
                int distX = Math.Abs(m.X - playerX);
                int distY = Math.Abs(m.Y - playerY);

                if (distX < 20 && distY < 20)
                {
                    currentHp -= 10;
                    enemies.RemoveAt(i); 
                    if (currentHp <= 0) { currentHp = 0; GameOver(); }
                    continue;
                }

                if (m.Y > this.Height) enemies.RemoveAt(i);
            }

            // 3. Логика анимации руки
            if (isAttacking)
            {
                handAngle += attackSpeed * swingDirection;

                // Если рука ушла на 45 градусов вверх (отрицательные углы в нашей системе), разворачиваем
                if (handAngle <= -45f) 
                {
                    swingDirection = 1; // Теперь бьем вниз (возврат)
                }

                // Если рука вернулась в 0 градусов - стоп
                if (handAngle >= 0f)
                {
                    handAngle = 0f;
                    isAttacking = false;
                }
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop();

            var returnTimer = new System.Windows.Forms.Timer();
            returnTimer.Interval = 2000;
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

            g.Clear(Color.DarkGray); // Очищаем экран (убирает следы)

            // 1. Дорога
            int roadLeft = (this.Width - roadWidth) / 2;
            g.FillRectangle(Brushes.Gray, roadLeft, 0, roadWidth, this.Height);

            // 2. ГГ (Зеленая точка)
            Brush ggBrush = new SolidBrush(Color.Green);
            g.FillEllipse(ggBrush, playerX - 10, playerY - 10, 20, 20);

            // 3. РУКА (Оранжевая точка)
            // Рисуем её на определенном расстоянии (радиусе) от героя
            int handRadiusOrbit = 35; // Расстояние от центра героя
            // Переводим градусы в радианы для Math.Sin/Cos
            double angleRad = handAngle * Math.PI / 180.0;

            // Считаем координаты руки на окружности
            // 0 градусов = Справа от героя. Отрицательные = Вверх (вперед).
            int handX = (int)(playerX + handRadiusOrbit * Math.Cos(angleRad));
            int handY = (int)(playerY - handRadiusOrbit * Math.Sin(angleRad));

            Brush handBrush = new SolidBrush(Color.Orange);
            g.FillEllipse(handBrush, handX - 8, handY - 8, 16, 16);

            // 4. Враги (Красные точки)
            Brush mobBrush = new SolidBrush(Color.Red);
            foreach (var mob in enemies)
            {
                g.FillEllipse(mobBrush, mob.X - 6, mob.Y - 6, 12, 12);
            }

            // 5. HP BAR (Снизу слева)
            int barWidth = 150;
            int barHeight = 20;
            int x = 10;
            int y = this.Height - 50; // Прилепляем к низу

            g.FillRectangle(Brushes.LightGray, x, y, barWidth, barHeight);
            g.DrawRectangle(Pens.Black, x, y, barWidth, barHeight);

            int currentBarWidth = (int)((float)currentHp / maxHp * barWidth);
            g.FillRectangle(Brushes.Red, x, y, currentBarWidth, barHeight);

            g.DrawString($"{currentHp} / {maxHp}", new Font("Arial", 12), Brushes.Black, new Point(x + 5, y + 2));

            // 6. GAME OVER Оверлей
            if (isGameOver)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 0, 0)), 0, 0, this.Width, this.Height);
                Font bigFont = new Font("Arial", 40, FontStyle.Bold);
                StringFormat sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("GAME OVER", bigFont, Brushes.Red, new Rectangle(0, 0, this.Width, this.Height), sf);
                g.DrawString("Возврат в меню...", new Font("Arial", 20), Brushes.White, new Rectangle(0, 0, this.Width, this.Height), sf);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            gameTimer.Stop();
            base.OnHandleDestroyed(e);
        }
    }

    class Mob
    {
        public int X, Y;
        public Mob(int x, int y) {
            X = x; 
            Y = y; 
        }
    }
}