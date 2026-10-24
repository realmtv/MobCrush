using System;
using System.Windows.Forms;
using System.Drawing;

namespace TricksGame
{
    // --- ГЛАВНОЕ ОКНО ---
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

    // --- ЭКРАН ЗАГРУЗКИ ---
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

    // --- ГЛАВНОЕ МЕНЮ ---
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

            // Версия игры
            Label lblVersion = new Label();
            lblVersion.Text = "Alpha 0.0.1";
            lblVersion.ForeColor = Color.Gray;
            lblVersion.Dock = DockStyle.Bottom;
            lblVersion.TextAlign = ContentAlignment.BottomLeft;
            lblVersion.Padding = new Padding(10, 0, 0, 10);
            this.Controls.Add(lblVersion);
        }

        private void AddButton(string text, Action onClick)
        {
            Button btn = new Button { Text = text, Size = new Size(200, 50), Font = new Font("Arial", 14), Margin = new Padding(10), FlatStyle = FlatStyle.Flat, BackColor = Color.Gray, ForeColor = Color.Black };
            btn.Click += (s, e) => onClick();
            panel.Controls.Add(btn);
        }
    }
}