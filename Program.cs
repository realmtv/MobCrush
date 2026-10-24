using System;
using System.Windows.Forms;
using System.Drawing;

namespace TricksGame
{
    // --- 1. ТОЧКА ВХОДА (Запуск программы) ---
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // --- 2. ГЛАВНОЕ ОКНО ---
    public class MainForm : Form
    {
        private Panel screenContainer;

        public MainForm()
        {
            this.Text = "Tricks (Roguelike)";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // Окно фиксированного размера

            screenContainer = new Panel();
            screenContainer.Dock = DockStyle.Fill;
            screenContainer.BackColor = Color.FromArgb(30, 30, 30);

            this.Controls.Add(screenContainer);

            // Запускаем сначала экран загрузки
            ShowScreen(new LoadingScreen(this));
        }

        // Смена экранов
        public void ShowScreen(UserControl screen)
        {
            if (screenContainer.Controls.Count > 0)
            {
                screenContainer.Controls[0].Dispose();
            }
            screen.Dock = DockStyle.Fill;
            screenContainer.Controls.Add(screen);
        }
    }

    // --- 3. ЭКРАН ЗАГРУЗКИ ---
    public class LoadingScreen : UserControl
    {
        private MainForm mainForm;

        public LoadingScreen(MainForm form)
        {
            mainForm = form;
            this.BackColor = Color.Black;

            Label label = new Label();
            label.Text = "Hello world";
            label.Font = new Font("Arial", 40, FontStyle.Bold);
            label.ForeColor = Color.White;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(label);

            // Таймер на 1 секунду
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (sender, e) => {
                timer.Stop();
                mainForm.ShowScreen(new MainMenu(mainForm));
            };
            timer.Start();
        }
    }

    // --- 4. ГЛАВНОЕ МЕНЮ ---
    public class MainMenu : UserControl
    {
        private FlowLayoutPanel panel; // Поле с кнопками, которое мы будем центрировать

        public MainMenu(MainForm form)
        {
            this.BackColor = Color.FromArgb(40, 40, 40);

            // Создаем панель для кнопок
            panel = new FlowLayoutPanel();
            panel.FlowDirection = FlowDirection.TopDown;
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // Создаем кнопки
            panel.Controls.Add(CreateButton("ИГРАТЬ"));
            panel.Controls.Add(CreateButton("ОПЦИИ"));
            panel.Controls.Add(CreateButton("ВЫХОД"));

            // Добавляем панель в основной контроль
            this.Controls.Add(panel);

            // Подписываемся на изменение размера окна, чтобы кнопки всегда были по центру
            this.Resize += (s, e) => CenterPanel(panel);

            // Запускаем центрирование сразу при старте
            CenterPanel(panel);

            // Привязываем события кликов
            this.Controls[0].Controls[0].Click += (s, e) => {
                // Здесь потом будет запуск игры
                // MessageBox.Show("Запуск игры...");
            };

            this.Controls[0].Controls[1].Click += (s, e) => {
                // Опции
            };

            this.Controls[0].Controls[2].Click += (s, e) => {
                Application.Exit(); // Выход
            };
        }

        // Метод для создания стильных кнопок
        private Button CreateButton(string text)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(200, 50);
            btn.Font = new Font("Arial", 16);
            btn.Margin = new Padding(10);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = Color.FromArgb(60, 60, 60);
            btn.ForeColor = Color.White;
            return btn;
        }

        // Метод, который вычисляет центр и ставит туда панель
        private void CenterPanel(Control control)
        {
            // (Ширина экрана - Ширина панели) / 2 = Отступ слева до центра
            int x = (this.ClientSize.Width - control.Width) / 2;
            // (Высота экрана - Высота панели) / 2 = Отступ сверху до центра
            int y = (this.ClientSize.Height - control.Height) / 2;
            
            control.Location = new Point(x, y);
        }
    }
}