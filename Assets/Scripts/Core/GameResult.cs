namespace PushNotificationGod.Core
{
    public static class GameResult
    {
        public static int LastScore { get; private set; }
        public static int LastMaxCombo { get; private set; }
        public static bool LastWasNewRecord { get; private set; }

        public static void Set(int score, int maxCombo)
        {
            LastScore = score;
            LastMaxCombo = maxCombo;
            LastWasNewRecord = score > LocalSaveManager.BestScore;
            LocalSaveManager.SaveBestScore(score);
        }
    }
}
