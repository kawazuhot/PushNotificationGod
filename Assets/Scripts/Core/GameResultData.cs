using UnityEngine;
using PushNotificationGod.Titles;

namespace PushNotificationGod.Core
{
    public static class GameResultData
    {
        public static int FinalScore { get; private set; }
        public static int SuccessCount { get; private set; }
        public static int MissCount { get; private set; }
        public static int MaxCombo { get; private set; }
        public static string RankTitle { get; private set; } = "普通のタスク人間";
        public static string RankDescription { get; private set; } = "今日もそれなりに通知をさばきました。";
        public static bool WasNewRecord { get; private set; }

        public static void Save(int finalScore, int successCount, int missCount, int maxCombo, TitleDefinition title = null)
        {
            FinalScore = Mathf.Max(0, finalScore);
            SuccessCount = Mathf.Max(0, successCount);
            MissCount = Mathf.Max(0, missCount);
            MaxCombo = Mathf.Max(0, maxCombo);
            RankTitle = string.IsNullOrWhiteSpace(title?.titleName) ? "普通のタスク人間" : title.titleName;
            RankDescription = string.IsNullOrWhiteSpace(title?.description) ? "今日もそれなりに通知をさばきました。" : title.description;
            WasNewRecord = false;

            Debug.Log($"[SaveResult] FinalScore={FinalScore}, Success={SuccessCount}, Miss={MissCount}, MaxCombo={MaxCombo}, Rank={RankTitle}, Description={RankDescription}");
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
