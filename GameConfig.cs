using System;

namespace TricksGame
{
    public static class GameConfig
    {
        public static class General
        {
            public const int GameFPS = 60;
            public const int TimerInterval = 1000 / GameFPS;
            public const int RoadWidth = 200;
            public const int BaseSpeed = 2;
            public const int GameOverDelayMs = 2500;
        }

        public static class Enemy
        {
            public const int Damage = 10;
            public const int MaxOnScreen = 10;
            public const int SpawnMargin = 20;
            public const int SpawnY = -20;
            public const int Radius = 6;
            
            // Баланс типов врагов
            public const int NormalHP = 5;
            public const float NormalSpeed = 1.0f;
            
            public const int FastHP = 3;
            public const float FastSpeed = 1.5f;
            
            public const int TankHP = 15;
            public const float TankSpeed = 0.7f;
        }

        public static class Player
        {
            public const int Radius = 10;
            public const int MaxHP = 100;
            public const int InvulnerableDuration = 30;
            public const float DefaultAttackRange = 95f;
            
            // Параметры атаки
            public const int AttackDamage = 10;
            public const int AttackKnockback = 5;
            
            // Анимация
            public const float AttackSpeed = 12f;
            public const float MaxSwingAngle = 45f;
            public const int HandRadius = 8;
            public const int HandOrbitRadius = 35;
        }

        public static class UI
        {
            public const int HPBarWidth = 150;
            public const int HPBarHeight = 20;
            public const int HPBarMarginX = 10;
            public const int HPBarMarginBottom = 50;
            
            public const int GoOverFontSize = 40;
            public const int SaveTextFontSize = 14;
            public const int SaveTextMarginRight = 200;
            public const int SaveTextMarginBottom = 40;
        }
    }
}