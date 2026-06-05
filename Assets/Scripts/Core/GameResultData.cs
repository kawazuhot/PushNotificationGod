using UnityEngine;

namespace PushNotificationGod.Core
{
    public static class GameResultData
    {
        public static int FinalScore { get; private set; }
        public static int SuccessCount { get; private set; }
        public static int MissCount { get; private set; }
        public static int MaxCombo { get; private set; }
        public static string RankTitle { get; private set; } = "スマホに負けた人";
        public static bool WasNewRecord { get; private set; }

        public static void Save(int finalScore, int successCount, int missCount, int maxCombo)
        {
            FinalScore = Mathf.Max(0, finalScore);
            SuccessCount = Mathf.Max(0, successCount);
            MissCount = Mathf.Max(0, missCount);
            MaxCombo = Mathf.Max(0, maxCombo);
            RankTitle = GetRankTitle(FinalScore);
            WasNewRecord = FinalScore > LocalSaveManager.BestScore;

            LocalSaveManager.SaveLastResult(FinalScore, SuccessCount, MissCount, MaxCombo, RankTitle, WasNewRecord);
            LocalSaveManager.SaveBestScore(FinalScore);

            Debug.Log($"[SaveResult] FinalScore={FinalScore}, Success={SuccessCount}, Miss={MissCount}, MaxCombo={MaxCombo}, Rank={RankTitle}");
        }

        public static void RestoreFromSaveIfEmpty()
        {
            if (FinalScore > 0 || SuccessCount > 0 || MissCount > 0 || MaxCombo > 0)
            {
                return;
            }

            FinalScore = LocalSaveManager.LastScore;
            SuccessCount = LocalSaveManager.LastSuccessCount;
            MissCount = LocalSaveManager.LastMissCount;
            MaxCombo = LocalSaveManager.LastMaxCombo;
            RankTitle = string.IsNullOrWhiteSpace(LocalSaveManager.LastRankTitle)
                ? GetRankTitle(FinalScore)
                : LocalSaveManager.LastRankTitle;
            WasNewRecord = LocalSaveManager.LastWasNewRecord;
        }

        public static string GetRankTitle(int score)
        {
            if (score >= 12000)
            {
                return "タスクの神様";
            }

            if (score >= 9000)
            {
                return "タスク界の番人";
            }

            if (score >= 7000)
            {
                return "タスク処理マスター";
            }

            if (score >= 5000)
            {
                return "タップの申し子";
            }

            if (score >= 4000)
            {
                return "既読スナイパー";
            }

            if (score >= 3000)
            {
                return "タスクさばき職人";
            }

            if (score >= 2000)
            {
                return "右スワイプの民";
            }

            if (score >= 1000)
            {
                return "普通の人";
            }

            if (score >= 500)
            {
                return "未読ためがち";
            }

            return "スマホに負けた人";
        }
    }
}
