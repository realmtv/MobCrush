using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TricksGame
{
    public class GameScreen : UserControl
    {
        // === Константы ===
        private const int GAME_FPS = 60;
        private const int TIMER_INTERVAL = 1000 / GAME_FPS; 
        private const int MAX_ENEMIES_ON_SCREEN = 10;
        private const int BASE_SPEED = 2;
        private const int GAME_OVER_DELAY_MS = 2500;

        private const int ROAD_WIDTH = 200;
        private const int ENEMY_SPAWN_MARGIN = 20;
        private const int ENEMY_SPAWN_Y = -20;

        private const int HP_BAR_WIDTH = 150;
        private const int HP_BAR_HEIGHT = 20;
        private const int HP_BAR_MARGIN_X = 10;
        private const int HP_BAR_MARGIN_BOTTOM = 50;

        private const int GAME_OVER_FONT_SIZE = 40;
        private const int SAVE_TEXT_FONT_SIZE = 14;
        private const int SAVE_TEXT_MARGIN_RIGHT = 200;
        private const int SAVE_TEXT_MARGIN_BOTTOM = 40;

        // === Параметры атаки ===
        private const int ATK_DAMAGE = 10;
        private const float ATK_RANGE_SQ = 25 * 25; 
        private const int ATK_KNOCKBACK = 5;
        
        // === Поля ===
        private readonly MainForm mainForm;
        private readonly Player player;
        private readonly List<Enemy> enemies = new List<Enemy>();
        private readonly Random random = new Random();

        private System.Windows.Forms.Timer? gameTimer;
        private System.Windows.Forms.Timer? returnTimer;

        private int speed = BASE_SPEED;
        private bool isGameOver = false;

        // === Система прогрессии ===
        private int _currentXP = 0;
        private int _xpToNextLevel = 100;
        private int _playerLevel = 1;
        private int _currentStage = 1;

        // === Управление игрой ===
        private bool _isPaused;
        private bool _isSpeedMode;

        // === Координаты кнопки ускорения ===
        private Rectangle _speedButtonRect;

        // Кэшированные ресурсы
        private readonly Font hpBarFont;
        private readonly Font gameOverFont;
        private readonly Font saveTextFont;
        private readonly SolidBrush saveTextBrush;
        private readonly StringFormat centerFormat;
        private readonly SolidBrush gameOverOverlayBrush;

        // === Новые ресурсы GDI+ ===
        private readonly Font _stageFont;
        private readonly Font _pauseFont;
        private readonly Brush _xpFillBrush;
        private readonly Brush _xpBgBrush;
        private readonly Brush _speedButtonBrush;
        private readonly Pen _speedButtonPen;
        private readonly Brush _pauseOverlayBrush;

        public GameScreen(MainForm form)
        {
            mainForm = form;
            player = new Player(Width, Height);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
            BackColor = Color.DarkGray;

            hpBarFont = new Font("Arial", 12);
            gameOverFont = new Font("Arial", GAME_OVER_FONT_SIZE, FontStyle.Bold);
            saveTextFont = new Font("Arial", SAVE_TEXT_FONT_SIZE);
            saveTextBrush = new SolidBrush(Color.FromArgb(150, 150, 150, 150));
            centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            gameOverOverlayBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));

            // Новые шрифты и кисти
            _stageFont = new Font("Arial", 20, FontStyle.Bold);
            _pauseFont = new Font("Arial", 36, FontStyle.Bold);
            _xpFillBrush = new SolidBrush(Color.FromArgb(255, 0, 255, 255));
            _xpBgBrush = new SolidBrush(Color.FromArgb(255, 50, 50, 50));
            _speedButtonBrush = new SolidBrush(Color.FromArgb(50, 255, 0, 0));
            _speedButtonPen = new Pen(Color.Red, 2f);
            _pauseOverlayBrush = new SolidBrush(Color.FromArgb(150, 0, 0, 0));

            // Инициализация кнопки ускорения (правый нижний угол, 40x40)
            _speedButtonRect = new Rectangle(Width - 50, Height - 50, 40, 40);

            // Подписка на события
            KeyDown += GameScreen_KeyDown;
            MouseClick += GameScreen_MouseClick;
            this.Focus();

            gameTimer = new System.Windows.Forms.Timer { Interval = TIMER_INTERVAL };
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (!isGameOver && !_isPaused)
                Update();

            Invalidate();
        }

        private new void Update()
        {
            player.UpdatePosition(Width, Height);
            player.Update();

            SpawnEnemies();
            UpdateEnemies();
        }

        private void SpawnEnemies()
        {
            int aliveCount = enemies.Count(e => !e.IsDead);
            
            if (aliveCount >= MAX_ENEMIES_ON_SCREEN)
                return;

            int roadLeft = GetRoadLeft();
            int mobX = random.Next(roadLeft + ENEMY_SPAWN_MARGIN, roadLeft + ROAD_WIDTH - ENEMY_SPAWN_MARGIN);
            enemies.Add(new Enemy(mobX, ENEMY_SPAWN_Y));
        }

        private void UpdateEnemies()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                enemy.Update(speed);

                // 1. Враг коснулся игрока
                if (!enemy.IsDead && enemy.CollidesWithPlayer(player.X, player.Y))
                {
                    player.TakeDamage(10);
                    enemies.RemoveAt(i);
                    
                    if (player.IsDead)
                    {
                        GameOver();
                    }
                    continue;
                }

                // 2. Атака игрока по врагу
                if (!enemy.IsDead && player.IsAttacking)
                {
                    float hitboxX = player.X;
                    float hitboxY = player.Y - 35; 

                    float distX = enemy.X - hitboxX;
                    float distY = enemy.Y - hitboxY;
                    float distSq = distX * distX + distY * distY;

                    if (distSq < ATK_RANGE_SQ)
                    {
                        bool wasAlive = !enemy.IsDead;
                        enemy.TakeDamage(ATK_DAMAGE);
                        enemy.Y -= ATK_KNOCKBACK; 

                        // Если враг умер от этого удара, начисляем опыт
                        if (wasAlive && enemy.IsDead)
                        {
                            AddXPForEnemy(enemy);
                        }
                    }
                }

                // 3. Удаление: Ушел за экран ИЛИ (Мертв и частицы исчезли)
                if (enemy.IsOffScreen(Height))
                {
                    enemies.RemoveAt(i);
                }
            }
        }

        private void AddXPForEnemy(Enemy enemy)
        {
            int xpReward = enemy.GetType().Name switch
            {
                "FastEnemy" => 1,   // Быстрый
                "TankEnemy" => 3,   // Живучий
                "Enemy"     => 2,   // Простой
                _           => 1    // Фолбэк
            };

            AddXP(xpReward);
        }

        private void AddXP(int amount)
        {
            _currentXP += amount;
            
            if (_currentXP >= _xpToNextLevel)
            {
                _playerLevel++;
                _currentXP -= _xpToNextLevel;
                _xpToNextLevel = (int)(_xpToNextLevel * 1.5);
            }
            
            Invalidate();
        }

        private void GameScreen_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                _isPaused = !_isPaused;
                
                if (_isPaused)
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
                    isGameOver = true;
                    ReturnToMenu(sender, EventArgs.Empty);
                }
            }
        }

        private void GameScreen_MouseClick(object? sender, MouseEventArgs e)
        {
            // Проверка клика по кнопке ускорения
            if (_speedButtonRect.Contains(e.Location))
            {
                _isSpeedMode = !_isSpeedMode;
                
                if (gameTimer != null)
                {
                    gameTimer.Interval = _isSpeedMode ? TIMER_INTERVAL / 2 : TIMER_INTERVAL;
                }
                
                Invalidate();
                return;
            }

            // Атака
            if (e.Button == MouseButtons.Left && !isGameOver && !_isPaused)
            {
                player.Attack();
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer?.Stop();

            returnTimer = new System.Windows.Forms.Timer { Interval = GAME_OVER_DELAY_MS };
            returnTimer.Tick += ReturnToMenu;
            returnTimer.Start();
        }

        private void ReturnToMenu(object? sender, EventArgs e)
        {
            if (returnTimer != null)
            {
                returnTimer.Stop();
                returnTimer.Dispose();
                returnTimer = null;
            }

            mainForm.ShowScreen(new MainMenu(mainForm));
        }

        private int GetRoadLeft()
        {
            return (Width - ROAD_WIDTH) / 2;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            Graphics g = pe.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.DarkGray);

            DrawRoad(g);
            DrawEntities(g);
            DrawHUD(g);
            DrawXPBar(g);
            DrawStage(g);

            if (_isPaused)
            {
                DrawPauseOverlay(g);
            }
            else if (isGameOver)
            {
                DrawGameOverOverlay(g);
            }

            DrawSpeedButton(g);
        }

        private void DrawRoad(Graphics g)
        {
            int roadLeft = GetRoadLeft();
            g.FillRectangle(Brushes.Gray, roadLeft, 0, ROAD_WIDTH, Height);
        }

        private void DrawEntities(Graphics g)
        {
            player.Draw(g);

            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(g); 
            }
        }

        private void DrawHUD(Graphics g)
        {
            int x = HP_BAR_MARGIN_X;
            int y = Height - HP_BAR_MARGIN_BOTTOM;

            g.FillRectangle(Brushes.LightGray, x, y, HP_BAR_WIDTH, HP_BAR_HEIGHT);
            g.DrawRectangle(Pens.Black, x, y, HP_BAR_WIDTH, HP_BAR_HEIGHT);

            int currentBarWidth = (int)((float)player.CurrentHp / player.MaxHp * HP_BAR_WIDTH);
            g.FillRectangle(Brushes.Red, x, y, currentBarWidth, HP_BAR_HEIGHT);

            string hpText = $"{player.CurrentHp} / {player.MaxHp}";
            g.DrawString(hpText, hpBarFont, Brushes.Black, new Point(x + 5, y + 2));
        }

        private void DrawXPBar(Graphics g)
        {
            int x = HP_BAR_MARGIN_X;
            int y = Height - HP_BAR_MARGIN_BOTTOM + HP_BAR_HEIGHT + 5;
            int height = 8;

            g.FillRectangle(_xpBgBrush, x, y, HP_BAR_WIDTH, height);
            
            int xpWidth = (int)((float)_currentXP / _xpToNextLevel * HP_BAR_WIDTH);
            if (xpWidth > 0)
            {
                g.FillRectangle(_xpFillBrush, x, y, xpWidth, height);
            }

            g.DrawRectangle(Pens.Cyan, x, y, HP_BAR_WIDTH, height);
        }

        private void DrawStage(Graphics g)
        {
            g.DrawString($"Stage {_currentStage}", _stageFont, Brushes.LightGray, 
                new Point(Width / 2, 20), new StringFormat { Alignment = StringAlignment.Center });
        }

        private void DrawSpeedButton(Graphics g)
        {
            g.FillRectangle(_speedButtonBrush, _speedButtonRect);
            g.DrawRectangle(_speedButtonPen, _speedButtonRect);
            
            string text = _isSpeedMode ? "x2" : "x1";
            g.DrawString(text, hpBarFont, Brushes.Red, new PointF(_speedButtonRect.X + 5, _speedButtonRect.Y + 5));
        }

        private void DrawPauseOverlay(Graphics g)
        {
            g.FillRectangle(_pauseOverlayBrush, 0, 0, Width, Height);
            g.DrawString("PAUSED", _pauseFont, Brushes.White, 
                new Point(Width / 2, Height / 2), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        private void DrawGameOverOverlay(Graphics g)
        {
            g.FillRectangle(gameOverOverlayBrush, 0, 0, Width, Height);
            g.DrawString("GAME OVER", gameOverFont, Brushes.Red, new Rectangle(0, 0, Width, Height), centerFormat);
            
            Point savePoint = new Point(Width - SAVE_TEXT_MARGIN_RIGHT, Height - SAVE_TEXT_MARGIN_BOTTOM);
            g.DrawString("игра сохраняется", saveTextFont, saveTextBrush, savePoint);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            _speedButtonRect = new Rectangle(Width - 50, Height - 50, 40, 40);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            DisposeTimers();
            base.OnHandleDestroyed(e);
        }

        private void DisposeTimers()
        {
            gameTimer?.Dispose();
            returnTimer?.Dispose();
        }
    }
}