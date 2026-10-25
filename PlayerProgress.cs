using System;

namespace TricksGame
{
    public class PlayerProgress
    {
        public int CurrentXP { get; private set; }
        public int XpToNextLevel { get; private set; }
        public int PlayerLevel { get; private set; }
        public int CurrentStage { get; set; }

        // Исправлено: сделано nullable (Action?), чтобы убрать ошибку CS8618.
        // События в C# по умолчанию инициализируются null, но компилятор строгих проверок (Nullable) 
        // требует явного указания, что поле может быть null.
        public Action? OnMilestoneReached;

        public PlayerProgress()
        {
            CurrentXP = 0;
            XpToNextLevel = 100;
            PlayerLevel = 1;
            CurrentStage = 1;
        }

        public void AddXP(int amount)
        {
            CurrentXP += amount;

            while (CurrentXP >= XpToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            PlayerLevel++;
            CurrentXP -= XpToNextLevel;
            XpToNextLevel = (int)(XpToNextLevel * 1.5f);

            if (PlayerLevel % 5 == 0)
            {
                OnMilestoneReached?.Invoke();
            }
        }

        public float GetXPBarPercentage()
        {
            if (XpToNextLevel == 0) return 0;
            return (float)CurrentXP / XpToNextLevel;
        }
    }
}