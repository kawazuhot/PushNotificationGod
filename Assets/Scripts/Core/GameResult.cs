namespace PushNotificationGod.Core
{
    public static class GameResult
    {
        public static int LastScore { get; private set; }
        public static int LastMaxCombo { get; private set; }
        public static bool LastWasNewRecord { get; private set; }

        public static void Set(int score, int maxCombo)
        {
            GameResultData.Save(score, 0, 0, maxCombo);
            LastScore = GameResultData.FinalScore;
            LastMaxCombo = GameResultData.MaxCombo;
            LastWasNewRecord = GameResultData.WasNewRecord;
        }

        public static void RestoreFromSaveIfEmpty()
        {
            GameResultData.RestoreFromSaveIfEmpty();
            if (LastScore > 0 || LastMaxCombo > 0)
            {
                return;
            }

            LastScore = GameResultData.FinalScore;
            LastMaxCombo = GameResultData.MaxCombo;
            LastWasNewRecord = GameResultData.WasNewRecord;
        }
    }
}
