using System;
using System.Windows.Forms;
using System.Drawing;

namespace TricksGame
{
    // --- ГЛАВНОЕ ОКНО (CONTROLLER) ---
    public class MainForm : Form
    {
        private Panel screenContainer;

        public MainForm()
        {
            this.Text = "Tricks";
            this.Size = new Size(480, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.BackColor = Color.Black; // Общий фон для страховки

            screenContainer = new Panel();
            screenContainer.Dock = DockStyle.Fill;
            this.Controls.Add(screenContainer);

            // Запуск первого экрана
            ShowScreen(new LoadingScreen(this));
        }

        /// <summary>
        /// Переключает экраны. Безопасно удаляет старый экран.
        /// </summary>
        public void ShowScreen(UserControl screen)
        {
            if (screenContainer.Controls.Count > 0)
            {
                // ⚠️ ВАЖНО: Сначала удаляем из коллекции!
                // Если вызвать Dispose() первым, он сам удалит контрол,
                // и вызов RemoveAt(0) упадет с ошибкой "индекс вне диапазона".
                Control oldScreen = screenContainer.Controls[0];
                screenContainer.Controls.RemoveAt(0);
                oldScreen.Dispose();
            }

            screen.Dock = DockStyle.Fill;
            screenContainer.Controls.Add(screen);
        }
    }

    // --- ЭКРАН ЗАГРУЗКИ ---
    public class LoadingScreen : UserControl
    {
        public LoadingScreen(MainForm form)
        {
            this.BackColor = Color.Black;
            
            Label label = new Label
            {
                Text = "Loading...",
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 16, FontStyle.Bold)
            };
            this.Controls.Add(label);

            // Таймер перехода в меню
            var timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1 секунда
            timer.Tick += (s, e) => 
            { 
                timer.Stop(); 
                form.ShowScreen(new MainMenu(form)); 
            };
            timer.Start();
        }
    }

    // --- ГЛАВНОЕ МЕНЮ ---
    public class MainMenu : UserControl
    {
        private FlowLayoutPanel panel;
        private MainForm mainForm;

        public MainMenu(MainForm form)
        {
            mainForm = form;
            this.BackColor = Color.FromArgb(40, 40, 40);

            // Панель для центрирования кнопок
            panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            this.Controls.Add(panel);

            // Логика центрирования при ресайзе
            this.Resize += (s, e) => 
            {
                panel.Location = new Point(
                    (this.Width - panel.Width) / 2, 
                    (this.Height - panel.Height) / 2
                );
            };
            panel.Location = new Point(140, 300); // Стартовая позиция

            // Кнопки меню
            AddButton("ИГРАТЬ", () => mainForm.ShowScreen(new GameScreen(mainForm)));
            AddButton("ОПЦИИ", () => mainForm.ShowScreen(new OptionsScreen(mainForm)));
            AddButton("ВЫХОД", () => Application.Exit());

            // Версия игры (внизу слева)
            Label lblVersion = new Label();
            lblVersion.Text = "Alpha 0.0.1";
            lblVersion.ForeColor = Color.Gray;
            lblVersion.Dock = DockStyle.Bottom;
            lblVersion.TextAlign = ContentAlignment.BottomLeft;
            lblVersion.Padding = new Padding(10, 0, 0, 10);
            this.Controls.Add(lblVersion);
        }

        /// <summary>
        /// Создает кнопку с эффектами наведения (Hover).
        /// </summary>
        private void AddButton(string text, Action onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Size = new Size(200, 50),
                Font = new Font("Arial", 14, FontStyle.Bold),
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Gray,
                ForeColor = Color.Black
            };

            // Добавляем интерактивности: меняем цвет при наведении
            btn.MouseEnter += (s, e) => btn.BackColor = Color.White;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Gray;

            btn.Click += (s, e) => onClick();
            panel.Controls.Add(btn);
        }
    }

    // --- ЭКРАН НАСТРОЕК ---
    public class OptionsScreen : UserControl
    {
        private FlowLayoutPanel panel;
        private MainForm mainForm;

        public OptionsScreen(MainForm form)
        {
            mainForm = form;
            this.BackColor = Color.FromArgb(40, 40, 40);

            panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            this.Controls.Add(panel);

            this.Resize += (s, e) =>
            {
                panel.Location = new Point(
                    (this.Width - panel.Width) / 2,
                    (this.Height - panel.Height) / 2
                );
            };
            panel.Location = new Point(140, 300);

            // Заглушка для будущих настроек
            Label infoLabel = new Label
            {
                Text = "Настройки в разработке...",
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = true
            };
            panel.Controls.Add(infoLabel);

            panel.Controls.Add(new Label { Height = 20 });

            // Кнопка Назад
            Button btnBack = new Button
            {
                Text = "НАЗАД",
                Size = new Size(200, 50),
                Font = new Font("Arial", 14, FontStyle.Bold),
                Margin = new Padding(10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Gray,
                ForeColor = Color.Black
            };

            btnBack.MouseEnter += (s, e) => btnBack.BackColor = Color.White;
            btnBack.MouseLeave += (s, e) => btnBack.BackColor = Color.Gray;
            
            btnBack.Click += (s, e) => mainForm.ShowScreen(new MainMenu(mainForm));
            panel.Controls.Add(btnBack);
        }
    }
}