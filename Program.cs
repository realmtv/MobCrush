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
            timer.Interval = 1500;
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

    // --- 5. ИГРОВОЙ ЭКРАН (ОСНОВНАЯ ЛОГИКА) ---
    public class GameScreen : UserControl
    {
        private MainForm mainForm;
        private List<Mob> enemies = new List<Mob>();
        private System.Windows.Forms.Timer gameTimer;
        private Random random = new Random();
        
        // Параметры мира
        private int roadWidth = 200;
        private int speed = 5;

        public GameScreen(MainForm form)
        {
            mainForm = form;
            
            // --- ВОТ ЭТО ИСПРАВЛЯЕТ МЕРЦАНИЕ ---
            // Включаем двойную буферизацию (рисуем в памяти, потом выкидываем на экран)
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer | 
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint, true);
            this.UpdateStyles();
            // -----------------------------

            this.BackColor = Color.DarkGray;
            
            // Запускаем игровой цикл
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; // ~60 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            Update();
            this.Invalidate(); // Перерисовка
        }

        private void Update()
        {
            // Спавн врагов
            if (enemies.Count < 10)
            {
                int roadLeft = (this.Width - roadWidth) / 2;
                int mobX = random.Next(roadLeft + 10, roadLeft + roadWidth - 10); // Чуть отступаем от краев дороги
                
                enemies.Add(new Mob(mobX, -20));
            }

            // Движение врагов
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Y += speed;
                if (enemies[i].Y > this.Height)
                {
                    enemies.RemoveAt(i);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 1. Дорога
            int roadLeft = (this.Width - roadWidth) / 2;
            g.FillRectangle(Brushes.Gray, roadLeft, 0, roadWidth, this.Height);

            // 2. ГГ (Зеленая точка)
            int ggX = this.Width / 2;
            int ggY = this.Height - 60;
            
            // Используем Brush (кисть) для залитого круга, это выглядит лучше линии
            Brush ggBrush = new SolidBrush(Color.Green);
            g.FillEllipse(ggBrush, ggX - 10, ggY - 10, 20, 20);

            // 3. Враги (Красные точки)
            Brush mobBrush = new SolidBrush(Color.Red);
            foreach (var mob in enemies)
            {
                // Рисуем врага. 10x10 - это диаметр (20px)
                // Ты просил 0.2 см -> примерно 8 пикселей.
                // Делаем 12x12, чтобы было чуть лучше видно.
                g.FillEllipse(mobBrush, mob.X - 6, mob.Y - 6, 12, 12);
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