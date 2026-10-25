using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TricksGame
{
    public class GameScreen : UserControl
    {
        private readonly MainForm mainForm;
        private readonly GameManager gameManager;
        
        // Исправлено: используем полный путь к классу и nullable
        private System.Windows.Forms.Timer? gameTimer;
        private System.Windows.Forms.Timer? returnTimer;

        private Rectangle _speedButtonRect;

        // Ресурсы для отрисовки (nullable для устранения ошибок компилятора)
        private Font? hpBarFont, gameOverFont, saveTextFont, stageFont, pauseFont;
        private Brush? saveTextBrush, gameOverOverlayBrush, xpFillBrush, xpBgBrush, speedButtonBrush, pauseOverlayBrush;
        private Pen? speedButtonPen;
        private Brush? attackDebugBrush;
        private StringFormat? centerFormat;

        public GameScreen(MainForm form)
        {
            mainForm = form;
            // Создаем игрока и передаем его в менеджер
            var player = new Player(Width, Height, GameConfig.General.RoadWidth);
            gameManager = new GameManager(Width, Height, player);
            
            // ОБНОВЛЕНИЕ РАЗМЕРОВ СРАЗУ ПРИ СОЗДАНИИ
            // Это гарантирует, что если контрол уже имеет размер, менеджер будет знать о нем
            gameManager.UpdateDimensions(Width, Height);

            // Подписка на событие Game Over
            gameManager.OnGameOverTriggered += HandleGameOver;

            // Настройка графики
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
            BackColor = Color.DarkGray;

            InitResources();
            InitEvents();
        }

        private void InitResources()
        {
            hpBarFont = new Font("Arial", 12);
            gameOverFont = new Font("Arial", GameConfig.UI.GoOverFontSize, FontStyle.Bold);
            saveTextFont = new Font("Arial", GameConfig.UI.SaveTextFontSize);
            saveTextBrush = new SolidBrush(Color.FromArgb(150, 150, 150, 150));
            centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            gameOverOverlayBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            
            stageFont = new Font("Arial", 20, FontStyle.Bold);
            pauseFont = new Font("Arial", 36, FontStyle.Bold);
            xpFillBrush = new SolidBrush(Color.FromArgb(255, 0, 255, 255));
            xpBgBrush = new SolidBrush(Color.FromArgb(255, 50, 50, 50));
            speedButtonBrush = new SolidBrush(Color.FromArgb(50, 255, 0, 0));
            speedButtonPen = new Pen(Color.Red, 2f);
            pauseOverlayBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            attackDebugBrush = new SolidBrush(Color.FromArgb(50, 255, 0, 0));

            _speedButtonRect = new Rectangle(Width - 50, Height - 50, 40, 40);
        }

        private void InitEvents()
        {
            KeyDown += GameScreen_KeyDown;
            MouseClick += GameScreen_MouseClick;
            this.Focus();

            gameTimer = new System.Windows.Forms.Timer 
            { 
                Interval = (gameManager.IsSpeedMode) ? GameConfig.General.TimerInterval / 2 : GameConfig.General.TimerInterval 
            };
            gameTimer.Tick += (s, e) =>
            {
                gameManager.Update();
                Invalidate();
            };
            gameTimer.Start();
        }

        private void HandleGameOver()
        {
            gameTimer?.Stop();
            returnTimer = new System.Windows.Forms.Timer { Interval = GameConfig.General.GameOverDelayMs };
            returnTimer.Tick += ReturnToMenu;
            returnTimer.Start();
        }

        private void ReturnToMenu(object? sender, EventArgs e)
        {
            returnTimer?.Stop();
            returnTimer?.Dispose();
            mainForm.ShowScreen(new MainMenu(mainForm));
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.DarkGray);

            DrawRoad(g);
            DrawAttackDebug(g);
            
            // Рисуем сущности прямо из списка менеджера
            gameManager.Player.Draw(g);
            foreach (var enemy in gameManager.Enemies) enemy.Draw(g);

            DrawHUD(g);
            DrawXPBar(g);
            DrawStage(g);
            DrawSpeedButton(g);

            if (gameManager.IsPaused)
                DrawPauseOverlay(g);
            else if (gameManager.IsGameOver)
                DrawGameOverOverlay(g);
        }

        private void DrawRoad(Graphics g)
        {
            int roadLeft = (Width - GameConfig.General.RoadWidth) / 2;
            g.FillRectangle(Brushes.Gray, roadLeft, 0, GameConfig.General.RoadWidth, Height);
        }

        private void DrawAttackDebug(Graphics g)
        {
            if (!gameManager.Player.IsAttacking) return;
            int range = (int)gameManager.Player.AttackRange;
            if (attackDebugBrush != null)
                g.FillEllipse(attackDebugBrush, gameManager.Player.X - range, gameManager.Player.Y - range, range * 2, range * 2);
        }

        private void DrawHUD(Graphics g)
        {
            if (hpBarFont == null) return;

            int x = GameConfig.UI.HPBarMarginX;
            int y = Height - GameConfig.UI.HPBarMarginBottom;
            int w = GameConfig.UI.HPBarWidth;
            int h = GameConfig.UI.HPBarHeight;

            g.FillRectangle(Brushes.LightGray, x, y, w, h);
            g.DrawRectangle(Pens.Black, x, y, w, h);

            float pct = (float)gameManager.Player.CurrentHp / gameManager.Player.MaxHp;
            g.FillRectangle(Brushes.Red, x, y, (int)(pct * w), h);
            
            // Текст HP внутри полоски
            string hpText = $"{gameManager.Player.CurrentHp} / {gameManager.Player.MaxHp}";
            g.DrawString(hpText, hpBarFont, Brushes.Black, new Point(x + 5, y + 2));
        }

        private void DrawXPBar(Graphics g)
        {
            if (xpBgBrush == null || xpFillBrush == null) return;

            int x = GameConfig.UI.HPBarMarginX;
            int y = Height - GameConfig.UI.HPBarMarginBottom + GameConfig.UI.HPBarHeight + 5;
            int height = 8;

            g.FillRectangle(xpBgBrush, x, y, GameConfig.UI.HPBarWidth, height);
            
            // Получаем процент заполненности от прогресса
            float pct = gameManager.Progress.GetXPBarPercentage();
            int xpWidth = (int)(pct * GameConfig.UI.HPBarWidth);
            if (xpWidth > 0)
            {
                g.FillRectangle(xpFillBrush, x, y, xpWidth, height);
            }

            g.DrawRectangle(Pens.Cyan, x, y, GameConfig.UI.HPBarWidth, height);
        }

        private void DrawStage(Graphics g)
        {
            if (stageFont == null) return;
            g.DrawString($"Stage {gameManager.Progress.CurrentStage}", stageFont, Brushes.LightGray, 
                new Point(Width / 2, 20), new StringFormat { Alignment = StringAlignment.Center });
        }

        private void DrawSpeedButton(Graphics g)
        {
            if (speedButtonBrush == null || speedButtonPen == null || hpBarFont == null) return;

            g.FillRectangle(speedButtonBrush, _speedButtonRect);
            g.DrawRectangle(speedButtonPen, _speedButtonRect);
            
            string text = gameManager.IsSpeedMode ? "x2" : "x1";
            g.DrawString(text, hpBarFont, Brushes.Red, new PointF(_speedButtonRect.X + 5, _speedButtonRect.Y + 5));
        }

        private void DrawPauseOverlay(Graphics g)
        {
            if (pauseOverlayBrush == null || pauseFont == null) return;

            g.FillRectangle(pauseOverlayBrush, 0, 0, Width, Height);
            g.DrawString("PAUSED", pauseFont, Brushes.White, 
                new Point(Width / 2, Height / 2), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DrawGameOverOverlay(Graphics g)
        {
            if (gameOverOverlayBrush == null || gameOverFont == null || centerFormat == null || saveTextFont == null || saveTextBrush == null) return;

            g.FillRectangle(gameOverOverlayBrush, 0, 0, Width, Height);
            g.DrawString("GAME OVER", gameOverFont, Brushes.Red, new Rectangle(0, 0, Width, Height), centerFormat);
            
            Point savePoint = new Point(Width - GameConfig.UI.SaveTextMarginRight, Height - GameConfig.UI.SaveTextMarginBottom);
            g.DrawString("игра сохраняется", saveTextFont, saveTextBrush, savePoint);
        }

        // --- СОБЫТИЯ ---

        private void GameScreen_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                gameManager.TogglePause();
                
                if (gameManager.IsPaused)
                {
                    gameTimer?.Stop();
                }
                else
                {
                    gameTimer?.Start();
                }
                
                Invalidate();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                var result = MessageBox.Show(
                    "Выйти в главное меню?\nПрогресс будет утерян.",
                    "Выход из игры",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    DisposeTimers();
                    gameManager.Player.Dispose();
                    mainForm.ShowScreen(new MainMenu(mainForm));
                }
            }
        }

        private void GameScreen_MouseClick(object? sender, MouseEventArgs e)
        {
            // Проверка клика по кнопке ускорения
            if (_speedButtonRect.Contains(e.Location))
            {
                gameManager.ToggleSpeed();
                
                // Меняем интервал таймера динамически
                if (gameTimer != null)
                {
                    gameTimer.Interval = gameManager.IsSpeedMode ? GameConfig.General.TimerInterval / 2 : GameConfig.General.TimerInterval;
                }
                
                Invalidate();
                return;
            }

            // Атака (Левый клик)
            if (e.Button == MouseButtons.Left && !gameManager.IsGameOver && !gameManager.IsPaused)
            {
                gameManager.Player.Attack();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _speedButtonRect = new Rectangle(Width - 50, Height - 50, 40, 40);
            // Обновляем позицию игрока при изменении размера окна
            gameManager.Player.UpdatePosition(Width, Height);
            // ВАЖНО: Обновляем размеры для спавна врагов
            gameManager.UpdateDimensions(Width, Height);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DisposeTimers();
            gameManager.Player.Dispose();
            base.OnHandleDestroyed(e);
        }

        private void DisposeTimers()
        {
            gameTimer?.Dispose();
            returnTimer?.Dispose();
        }
    }
}