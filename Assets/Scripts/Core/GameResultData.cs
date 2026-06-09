using UnityEngine;
using PushNotificationGod.Data;
using PushNotificationGod.Titles;
using PushNotificationGod.Tasks;

namespace PushNotificationGod.Core
{
    public enum GameMode
    {
        Normal,
        Easy
    }

    public static class GameModeSettings
    {
        public static GameMode SelectedMode { get; set; } = GameMode.Normal;
        public static bool IsEasyMode => SelectedMode == GameMode.Easy;
        public static string SelectedModeName => GetDisplayName(SelectedMode);

        public static string GetDisplayName(GameMode mode)
        {
            return mode == GameMode.Easy ? "かんたんモード" : "通常モード";
        }

        public static string GetHighScoreKey(GameMode mode)
        {
            return mode == GameMode.Easy ? "highScore_easy" : "highScore_normal";
        }

        public static int GetModeAdjustedBaseScore(TaskDefinition task)
        {
            if (task == null)
            {
                return 0;
            }

            if (SelectedMode != GameMode.Easy)
            {
                return Mathf.Max(0, task.baseScore);
            }

            return string.Equals(task.backgroundStyleId, "gold", System.StringComparison.OrdinalIgnoreCase) ? 300 : 100;
        }
    }

    public static class GameResultData
    {
        public static int FinalScore { get; private set; }
        public static int SuccessCount { get; private set; }
        public static int MissCount { get; private set; }
        public static int MaxCombo { get; private set; }
        public static int HighScore { get; private set; }
        public static string RankTitleId { get; private set; } = "title_score_0000_4999";
        public static string RankTitle { get; private set; } = "通知に飲まれた人";
        public static string RankDescription { get; private set; } = "気づいたら通知の波に流されていました。";
        public static bool WasNewRecord { get; private set; }
        public static GameMode Mode { get; private set; } = GameMode.Normal;
        public static string ModeName => GameModeSettings.GetDisplayName(Mode);
        public static GameEndReason EndReason { get; private set; } = GameEndReason.Unknown;
        public static string LastMistakeTaskId { get; private set; } = string.Empty;
        public static string LastMistakeTaskMessage { get; private set; } = string.Empty;
        public static string LastMistakeTaskTag { get; private set; } = string.Empty;
        public static string LastMistakeAction { get; private set; } = string.Empty;

        public static void Save(
            int finalScore,
            int successCount,
            int missCount,
            int maxCombo,
            TitleDefinition title = null,
            GameEndReason endReason = GameEndReason.Unknown,
            string lastMistakeTaskId = "",
            string lastMistakeTaskMessage = "",
            string lastMistakeTaskTag = "",
            TaskAction? lastMistakeAction = null,
            GameMode mode = GameMode.Normal)
        {
            FinalScore = Mathf.Max(0, finalScore);
            SuccessCount = Mathf.Max(0, successCount);
            MissCount = Mathf.Max(0, missCount);
            MaxCombo = Mathf.Max(0, maxCombo);
            Mode = mode;
            RankTitleId = string.IsNullOrWhiteSpace(title?.titleId) ? "title_score_0000_4999" : title.titleId;
            RankTitle = string.IsNullOrWhiteSpace(title?.titleName) ? "通知に飲まれた人" : title.titleName;
            RankDescription = string.IsNullOrWhiteSpace(title?.description) ? "気づいたら通知の波に流されていました。" : title.description;
            EndReason = endReason;
            LastMistakeTaskId = lastMistakeTaskId ?? string.Empty;
            LastMistakeTaskMessage = lastMistakeTaskMessage ?? string.Empty;
            LastMistakeTaskTag = lastMistakeTaskTag ?? string.Empty;
            LastMistakeAction = lastMistakeAction?.ToString() ?? string.Empty;
            UpdateHighScore(mode);

            Debug.Log($"[SaveResult] FinalScore={FinalScore}, Success={SuccessCount}, Miss={MissCount}, MaxCombo={MaxCombo}, Mode={ModeName}, HighScore={HighScore}, NewRecord={WasNewRecord}, RankId={RankTitleId}, Rank={RankTitle}, Description={RankDescription}, EndReason={EndReason}, LastMistakeTaskId={LastMistakeTaskId}, LastMistakeTag={LastMistakeTaskTag}, LastMistakeAction={LastMistakeAction}");
        }

        public static string GetRankTitle(int score)
        {
            if (score >= 100000)
            {
                return "通知処理バグってる人";
            }

            if (score >= 60000)
            {
                return "タスク処理の神様";
            }

            if (score >= 30000)
            {
                return "通知さばき職人";
            }

            if (score >= 10000)
            {
                return "そこそこ既読マン";
            }

            if (score >= 5000)
            {
                return "未読ためがち";
            }

            return "通知に飲まれた人";
        }

        private static void UpdateHighScore(GameMode mode)
        {
            string key = GameModeSettings.GetHighScoreKey(mode);
            int currentHighScore = 0;
            try
            {
                currentHighScore = Mathf.Max(0, PlayerPrefs.GetInt(key, 0));
                if (FinalScore > currentHighScore)
                {
                    HighScore = FinalScore;
                    WasNewRecord = true;
                    PlayerPrefs.SetInt(key, HighScore);
                    PlayerPrefs.Save();
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SaveResult] High score save failed. key={key} {ex.GetType().Name}: {ex.Message}");
            }

            HighScore = currentHighScore;
            WasNewRecord = false;
        }
    }
}
