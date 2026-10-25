using System;
using System.Collections.Generic;
using System.Drawing;

namespace TricksGame
{
    public class GameManager
    {
        public Player Player { get; }
        public List<Enemy> Enemies { get; } = new List<Enemy>();
        public PlayerProgress Progress { get; }

        private int speed;
        private bool isGameOver;
        private bool isPaused;
        private int speedMultiplier = 1;

        private readonly Random random = new Random();
        
        // Убраны readonly, чтобы можно было обновлять размеры при ресайзе окна
        private int screenWidth;
        private int screenHeight;

        // Событие, которое ловит экран, чтобы показать "Game Over"
        public Action? OnGameOverTriggered;

        public GameManager(int width, int height, Player player)
        {
            screenWidth = width;
            screenHeight = height;
            Player = player;
            Progress = new PlayerProgress();
            speed = GameConfig.General.BaseSpeed;

            // Если игрок достигает milestone, увеличиваем радиус атаки
            Progress.OnMilestoneReached += () => Player.AttackRange += 10;
        }

        // Метод для обновления размеров экрана
        public void UpdateDimensions(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
        }

        public bool IsGameOver => isGameOver;
        public bool IsPaused => isPaused;

        public void TogglePause()
        {
            isPaused = !isPaused;
        }

        public void ToggleSpeed()
        {
            speedMultiplier = (speedMultiplier == 1) ? 2 : 1;
        }

        public bool IsSpeedMode => speedMultiplier == 2;

        public void Update()
        {
            if (isPaused || isGameOver) return;

            // Защита от спавна, если окно ещё не появилось или имеет размер 0
            if (screenWidth <= 0 || screenHeight <= 0) return;

            Player.Update();
            SpawnEnemies();
            UpdateEnemies();
            CheckCollisions();
        }

        private void SpawnEnemies()
        {
            int aliveCount = Enemies.Count(e => !e.IsDead);
            if (aliveCount >= GameConfig.Enemy.MaxOnScreen) return;

            int roadLeft = (screenWidth - GameConfig.General.RoadWidth) / 2;
            int margin = GameConfig.Enemy.SpawnMargin;
            
            // Проверка на валидность координат
            if (roadLeft + margin >= roadLeft + GameConfig.General.RoadWidth - margin) return;

            int mobX = random.Next(roadLeft + margin, roadLeft + GameConfig.General.RoadWidth - margin);
            
            Enemies.Add(new Enemy(mobX, GameConfig.Enemy.SpawnY));
        }

        private void UpdateEnemies()
        {
            int currentSpeed = speed * speedMultiplier;

            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemies[i];
                enemy.Update(currentSpeed);

                if (enemy.IsOffScreen(screenHeight))
                {
                    enemy.Dispose(); // Освобождаем ресурсы при удалении за экраном
                    Enemies.RemoveAt(i);
                }
            }
        }

        private void CheckCollisions()
        {
            // Проходим в обратном порядке, чтобы безопасно удалять
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = Enemies[i];
                if (enemy.IsDead) continue;

                // 1. Враг коснулся игрока
                if (enemy.CollidesWithPlayer(Player.X, Player.Y))
                {
                    Player.TakeDamage(GameConfig.Enemy.Damage);
                    Enemies.RemoveAt(i);
                    enemy.Dispose(); // Освобождаем ресурсы

                    if (Player.IsDead)
                    {
                        isGameOver = true;
                        OnGameOverTriggered?.Invoke();
                    }
                    continue;
                }

                // 2. Атака игрока по врагу
                if (Player.IsAttacking && Player.IsInAttackZone(enemy.X, enemy.Y))
                {
                    enemy.TakeDamage(GameConfig.Player.AttackDamage);
                    enemy.Y -= GameConfig.Player.AttackKnockback;

                    if (enemy.IsDead)
                    {
                        Progress.AddXP(enemy.XPReward);
                    }
                }
            }
        }
    }
}