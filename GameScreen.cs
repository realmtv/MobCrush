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
        
        // Таймеры
        private System.Windows.Forms.Timer? gameTimer;
        private System.Windows.Forms.Timer? returnTimer;

        // Элементы интерфейса (прямоугольники для кнопок)
        private Rectangle _speedButtonRect;
        private Rectangle _resumeBtnRect, _optionsBtnRect, _exitBtnRect;
        private bool isMenuOpen = false;

        // Ресурсы для отрисовки
        private Font? hpBarFont, gameOverFont, saveTextFont, stageFont, pauseFont, menuBtnFont, levelFont;
        private Brush? saveTextBrush, gameOverOverlayBrush, xpFillBrush, xpBgBrush, speedButtonBrush, pauseOverlayBrush, btnBrushNormal, btnBrushHover;
        private Pen? speedButtonPen;
        private Brush? attackDebugBrush;
        private StringFormat? centerFormat;
        
        // Для подсветки кнопок меню
        private int _hoveredMenuBtnIndex = -1; 

        public GameScreen(MainForm form)
        {
            mainForm = form;
            
            var player = new Player(Width, Height, GameConfig.General.RoadWidth);
            gameManager = new GameManager(Width, Height, player);
            
            gameManager.UpdateDimensions(Width, Height);

            gameManager.OnGameOverTriggered += HandleGameOver;

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
            stageFont = new Font("Arial", 20, FontStyle.Bold);
            pauseFont = new Font("Arial", 36, FontStyle.Bold);
            menuBtnFont = new Font("Arial", 14, FontStyle.Bold);
            levelFont = new Font("Arial", 12, FontStyle.Italic | FontStyle.Bold);

            saveTextBrush = new SolidBrush(Color.FromArgb(150, 150, 150, 150));
            centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            gameOverOverlayBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));
            
            xpFillBrush = new SolidBrush(Color.FromArgb(255, 0, 255, 255));
            xpBgBrush = new SolidBrush(Color.FromArgb(255, 50, 50, 50));
            speedButtonBrush = new SolidBrush(Color.FromArgb(50, 255, 0, 0));
            speedButtonPen = new Pen(Color.Red, 2f);
            pauseOverlayBrush = new SolidBrush(Color.FromArgb(80, 0, 0, 0)); // Затемнение при паузе
            
            btnBrushNormal = new SolidBrush(Color.Gray);
            btnBrushHover = new SolidBrush(Color.White);

            attackDebugBrush = new SolidBrush(Color.FromArgb(50, 255, 0, 0));

            _speedButtonRect = new Rectangle(Width - 50, Height - 50, 40, 40);
        }

        private void InitEvents()
        {
            KeyDown += GameScreen_KeyDown;
            MouseClick += GameScreen_MouseClick;
            MouseMove += GameScreen_MouseMove; // Для ховера кнопок
            this.Focus();

            gameTimer = new System.Windows.Forms.Timer 
            { 
                Interval = (gameManager.IsSpeedMode) ? GameConfig.General.TimerInterval / 2 : GameConfig.General.TimerInterval 
            };
            gameTimer.Tick += (s, e) =>
            {
                if (!isMenuOpen && !gameManager.IsPaused) 
                {
                    gameManager.Update();
                    Invalidate();
                }
            };
            gameTimer.Start();
        }

        private void HandleGameOver()
        {
            isMenuOpen = false; // Скрываем меню паузы при смерти
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
            
            gameManager.Player.Draw(g);
            foreach (var enemy in gameManager.Enemies) enemy.Draw(g);

            DrawHUD(g);
            DrawXPBar(g);
            DrawStageAndLevel(g); // Теперь рисуем и уровень тоже
            DrawSpeedButton(g);

            if (isMenuOpen)
                DrawPauseMenuOverlay(g);
            else if (gameManager.IsPaused)
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
            
            float pct = gameManager.Progress.GetXPBarPercentage();
            int xpWidth = (int)(pct * GameConfig.UI.HPBarWidth);
            if (xpWidth > 0)
                g.FillRectangle(xpFillBrush, x, y, xpWidth, height);

            g.DrawRectangle(Pens.Cyan, x, y, GameConfig.UI.HPBarWidth, height);
        }

        private void DrawStageAndLevel(Graphics g)
        {
            if (stageFont == null || levelFont == null) return;
            
            // Стадия по центру сверху
            g.DrawString($"Stage {gameManager.Progress.CurrentStage}", stageFont, Brushes.LightGray, 
                new Point(Width / 2, 15), new StringFormat { Alignment = StringAlignment.Center });
                
            // Уровень слева под стадией или рядом с XP? Пусть будет над полоской опыта справа
            string lvlText = $"Lvl: {gameManager.Progress.PlayerLevel}";
            int x = GameConfig.UI.HPBarMarginX + GameConfig.UI.HPBarWidth - 60;
            int y = Height - GameConfig.UI.HPBarMarginBottom - 25; 
            
            g.DrawString(lvlText, levelFont, Brushes.Yellow, x, y);
        }

        private void DrawSpeedButton(Graphics g)
        {
            if (speedButtonBrush == null || speedButtonPen == null || hpBarFont == null) return;

            g.FillRectangle(speedButtonBrush, _speedButtonRect);
            g.DrawRectangle(speedButtonPen, _speedButtonRect);
            
            string text = gameManager.IsSpeedMode ? "x2" : "x1";
            g.DrawString(text, hpBarFont, Brushes.Red, new PointF(_speedButtonRect.X + 5, _speedButtonRect.Y + 5));
        }

        private void DrawPauseOverlay(Graphics g) // Для паузы через пробел
        {
            if (pauseOverlayBrush == null || pauseFont == null) return;
            g.FillRectangle(pauseOverlayBrush, 0, 0, Width, Height);
            g.DrawString("PAUSED", pauseFont, Brushes.White, 
                new Point(Width / 2, Height / 2), centerFormat);
        }

        private void DrawPauseMenuOverlay(Graphics g) // Новое меню при Escape
        {
             if (pauseOverlayBrush == null || menuBtnFont == null) return;
             
             // Фон - используем готовую кисть для оптимизации
             g.FillRectangle(pauseOverlayBrush, 0, 0, Width, Height);
             
             int btnW = 200;
             int btnH = 45;
             int margin = 30;
             int startY = Height / 2 - (btnH * 3 + margin * 2) / 2;

             // Рисуем кнопки и обновляем их координаты для обработки кликов
             _resumeBtnRect = new Rectangle((Width - btnW)/2, startY, btnW, btnH);
             DrawMenuButton(g, "ПРОДОЛЖИТЬ", _resumeBtnRect, 0);

             _optionsBtnRect = new Rectangle((Width - btnW)/2, startY + btnH + margin, btnW, btnH);
             DrawMenuButton(g, "НАСТРОЙКИ", _optionsBtnRect, 1);

             _exitBtnRect = new Rectangle((Width - btnW)/2, startY + (btnH + margin) * 2, btnW, btnH);
             DrawMenuButton(g, "В ГЛАВНОЕ МЕНЮ", _exitBtnRect, 2);
        }

        // Исправлено: Rect заменен на Rectangle
        private void DrawMenuButton(Graphics g, string text, Rectangle rect, int index)
        {
            Brush b = (_hoveredMenuBtnIndex == index) ? Brushes.White : Brushes.Gray;
            Pen p = Pens.Black;
            
            // Фон кнопки
            g.FillRectangle(b, rect);
            g.DrawRectangle(p, rect);
            
            // Текст
            g.DrawString(text, menuBtnFont, (Brush)((_hoveredMenuBtnIndex == index) ? Brushes.Black : Brushes.Black), 
                new Rectangle(rect.X + 5, rect.Y + 5, rect.Width - 10, rect.Height - 10), centerFormat);
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

        private void GameScreen_MouseMove(object? sender, MouseEventArgs e)
        {
             if (!isMenuOpen) return;
             
             _hoveredMenuBtnIndex = -1;
             Cursor = Cursors.Default;

             if (_resumeBtnRect.Contains(e.Location)) _hoveredMenuBtnIndex = 0;
             else if (_optionsBtnRect.Contains(e.Location)) _hoveredMenuBtnIndex = 1;
             else if (_exitBtnRect.Contains(e.Location)) _hoveredMenuBtnIndex = 2;

             if (_hoveredMenuBtnIndex != -1) Cursor = Cursors.Hand;
             
             Invalidate(); // Перерисовать для эффекта наведения
        }

        private void GameScreen_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                gameManager.TogglePause();
                if (!gameManager.IsPaused && !isMenuOpen) gameTimer?.Start();
                else gameTimer?.Stop();
                
                Invalidate();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                isMenuOpen = !isMenuOpen;
                if (isMenuOpen) 
                { 
                    gameManager.TogglePause(); // Ставим паузу в движке тоже
                    gameTimer?.Stop();
                } 
                else 
                {
                     gameManager.TogglePause(); // Снимаем паузу
                     gameTimer?.Start();
                }
                Invalidate();
            }
        }

        private void GameScreen_MouseClick(object? sender, MouseEventArgs e)
        {
            // Если открыто меню паузы - обрабатываем его кнопки
            if (isMenuOpen)
            {
                 if (_resumeBtnRect.Contains(e.Location))
                 {
                     isMenuOpen = false;
                     gameManager.TogglePause();
                     gameTimer?.Start();
                     Invalidate();
                     return;
                 }
                 
                 if (_optionsBtnRect.Contains(e.Location))
                 {
                     MessageBox.Show("Настройки пока в разработке", "Информация");
                     return;
                 }

                 if (_exitBtnRect.Contains(e.Location))
                 {
                    DisposeTimers();
                    gameManager.Player.Dispose();
                    mainForm.ShowScreen(new MainMenu(mainForm));
                    return;
                 }
            }

            // Обычная логика игры (если меню не открыто)
            if (!isMenuOpen && !gameManager.IsPaused && !gameManager.IsGameOver)
            {
                // Проверка клика по кнопке ускорения
                if (_speedButtonRect.Contains(e.Location))
                {
                    gameManager.ToggleSpeed();
                    if (gameTimer != null)
                        gameTimer.Interval = gameManager.IsSpeedMode ? GameConfig.General.TimerInterval / 2 : GameConfig.General.TimerInterval;
                    
                    Invalidate();
                    return;
                }

                // Атака (Левый клик) - теперь работает спам-атака
                if (e.Button == MouseButtons.Left)
                {
                    gameManager.Player.Attack();
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _speedButtonRect = new Rectangle(Width - 50, Height - 50, 40, 40);
            gameManager.Player.UpdatePosition(Width, Height);
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