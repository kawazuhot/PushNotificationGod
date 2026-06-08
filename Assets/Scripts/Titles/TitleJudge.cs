using System.Collections.Generic;
using PushNotificationGod.Core;
using PushNotificationGod.Tasks;
using UnityEngine;

namespace PushNotificationGod.Titles
{
    public class TitleJudge
    {
        public const string ConditionTagTapAtLeast = "tagTapAtLeast";
        public const string ConditionTagSwipeAtLeast = "tagSwipeAtLeast";
        public const string ConditionScoreRange = "scoreRange";
        public const string ConditionLifeZeroLastMistakeTag = "lifeZeroLastMistakeTag";
        public const string ConditionLifeZeroLastMistakeTagAndAction = "lifeZeroLastMistakeTagAndAction";

        private readonly List<TitleDefinition> definitions = new()
        {
            new TitleDefinition
            {
                titleId = "title_lover_swipe",
                titleName = "ハート右流しの民",
                description = "たまには、愛に気づいてあげよう。",
                conditionType = ConditionTagSwipeAtLeast,
                targetTag = "lover",
                threshold = 2,
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lover_lifezero",
                titleName = "ハート右流しの民",
                description = "たまには、愛に気づいてあげよう。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "lover",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_mother_lifezero",
                titleName = "親不孝スワイパー",
                description = "お母さんの通知、右に流しちゃったね。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "mother",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_scam_lifezero",
                titleName = "カモられ予備軍",
                description = "だいたい登録フォームの先に未来はありません。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "scam",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lazy_lifezero",
                titleName = "サボりの申し子",
                description = "休む才能だけ、やたら伸びています。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "lazy",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_video_lifezero",
                titleName = "動画沼の亡霊",
                description = "あと1本のつもりが、帰ってこられませんでした。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "video",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_sns_lifezero",
                titleName = "SNS吸引体質",
                description = "タイムラインを見る動体視力だけ異常です。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "sns",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_life_lifezero",
                titleName = "生活放棄モード",
                description = "生活タスクを右に流した先に、だいたい散らかった部屋があります。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "life",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_health_lifezero",
                titleName = "体に謝ってください",
                description = "体はずっと通知していました。あなたが見なかっただけです。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "health",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_furima_lifezero",
                titleName = "取引放置マン",
                description = "その通知、たぶん相手は返事を待っています。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "furima",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_hair",
                titleName = "前髪に敗れし者",
                description = "今日の敵は世界ではなく、前髪でした。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "hair",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_old_friend",
                titleName = "久しぶり警戒レベル0",
                description = "「久しぶり」の後ろには、だいたい何かあります。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "old_friend",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_fortune",
                titleName = "運命に丸投げした人",
                description = "人生を変える前に、通知を閉じましょう。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "fortune",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_fake_beauty",
                titleName = "美人投資家を信じた人",
                description = "その美人、中身はおっさんです。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "fake_beauty",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_diagnosis",
                titleName = "無料診断ホイホイ",
                description = "無料の先に、だいたい入力フォームがあります。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "diagnosis",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_lifehack",
                titleName = "人生ワンデイ勢",
                description = "1日で変えられるのは、髪型くらいです。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "lifehack",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_sale",
                titleName = "特別価格に弱い人",
                description = "「あなただけ」は、みんなに届いています。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "sale",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_like",
                titleName = "いいね通知みちゃう勢",
                description = "そんなにいいね！とはみんな思ってない。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "like",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_friend_dm",
                titleName = "友達DM見逃し隊",
                description = "それはたぶん、ちゃんと見た方がよかったやつです。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "friend_dm",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_friend",
                titleName = "友情あとまわし勢",
                description = "友達は大事。でも返信はなぜか後回しでした。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "friend",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_toilet",
                titleName = "膀胱との戦いに敗れし者",
                description = "通知より先に、行くべき場所がありました。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "toilet",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_game",
                titleName = "ログボに支配された人",
                description = "ログインするために生きていませんか？",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "game",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_work",
                titleName = "仕事通知から逃げた人",
                description = "仕事は右に流しても消えません。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "work",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_clickbait",
                titleName = "タイトル一本釣り",
                description = "見出しに引っかかる才能があります。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "clickbait",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_lifezero_diet",
                titleName = "未来のダイエット",
                description = "今日だけはノーカウントということにしました。",
                conditionType = ConditionLifeZeroLastMistakeTag,
                targetTag = "diet",
                priority = 80
            },
            new TitleDefinition
            {
                titleId = "title_score_0000_4999",
                titleName = "通知に飲まれた人",
                description = "気づいたら通知の波に流されていました。",
                conditionType = ConditionScoreRange,
                minScore = 0,
                maxScore = 4999,
                priority = 50
            },
            new TitleDefinition
            {
                titleId = "title_score_5000_9999",
                titleName = "未読ためがち",
                description = "読む気はある。たぶん。あとで。",
                conditionType = ConditionScoreRange,
                minScore = 5000,
                maxScore = 9999,
                priority = 50
            },
            new TitleDefinition
            {
                titleId = "title_score_10000_29999",
                titleName = "そこそこ既読マン",
                description = "通知に振り回されつつ、ちゃんと戦いました。",
                conditionType = ConditionScoreRange,
                minScore = 10000,
                maxScore = 29999,
                priority = 50
            },
            new TitleDefinition
            {
                titleId = "title_score_30000_59999",
                titleName = "通知さばき職人",
                description = "指先だけで今日の混乱をだいぶ片付けました。",
                conditionType = ConditionScoreRange,
                minScore = 30000,
                maxScore = 59999,
                priority = 100
            },
            new TitleDefinition
            {
                titleId = "title_score_60000_plus",
                titleName = "タスク処理の神様",
                description = "もはや通知の方があなたを怖がっています。",
                conditionType = ConditionScoreRange,
                minScore = 60000,
                maxScore = -1,
                priority = 100
            },
            new TitleDefinition
            {
                titleId = "title_score_100000_plus",
                titleName = "通知処理バグってる人",
                description = "その指、たぶん人間の速度ではありません。",
                conditionType = ConditionScoreRange,
                minScore = 100000,
                maxScore = -1,
                priority = 120
            }
        };

        public TitleDefinition JudgeMainTitle(Dictionary<string, TaskTagStats> tagStats)
        {
            return JudgeMainTitle(tagStats, 0);
        }

        public TitleDefinition JudgeMainTitle(Dictionary<string, TaskTagStats> tagStats, int finalScore)
        {
            return JudgeMainTitle(tagStats, finalScore, GameEndReason.Unknown, string.Empty, null);
        }

        public TitleDefinition JudgeMainTitle(
            Dictionary<string, TaskTagStats> tagStats,
            int finalScore,
            GameEndReason gameEndReason,
            string lastMistakeTaskTag,
            TaskAction? lastMistakeAction)
        {
            if (tagStats == null || tagStats.Count == 0)
            {
                Debug.Log("[TitleJudge] no tag stats.");
            }
            else
            {
                foreach (TaskTagStats stats in tagStats.Values)
                {
                    Debug.Log($"[TitleJudge] tag={stats.tag} tap={stats.tapCount} swipe={stats.swipeCount}");
                }
            }

            Debug.Log($"[TitleJudge] gameEndReason={gameEndReason} lastMistakeTag={lastMistakeTaskTag} lastMistakeAction={lastMistakeAction?.ToString() ?? string.Empty}");

            TitleDefinition bestTitle = null;
            int bestScore = 0;
            foreach (TitleDefinition definition in definitions)
            {
                if (!TryGetConditionScore(definition, tagStats, finalScore, gameEndReason, lastMistakeTaskTag, lastMistakeAction, out int conditionScore))
                {
                    LogConditionResult(definition, finalScore, gameEndReason, lastMistakeTaskTag, lastMistakeAction, 0, false);
                    continue;
                }

                LogConditionResult(definition, finalScore, gameEndReason, lastMistakeTaskTag, lastMistakeAction, conditionScore, true);
                if (bestTitle == null
                    || definition.priority > bestTitle.priority
                    || definition.priority == bestTitle.priority && conditionScore > bestScore
                    || definition.priority == bestTitle.priority && conditionScore == bestScore && string.CompareOrdinal(definition.titleId, bestTitle.titleId) < 0)
                {
                    Debug.Log($"[TitleJudge] candidate={definition.titleName}");
                    bestTitle = definition;
                    bestScore = conditionScore;
                }
            }

            TitleDefinition selected = bestTitle ?? definitions.Find(definition => definition.titleId == "title_score_0000_4999");
            Debug.Log($"[TitleJudge] selected={selected.titleName}");
            return selected;
        }

        private static bool TryGetConditionScore(
            TitleDefinition definition,
            Dictionary<string, TaskTagStats> tagStats,
            int finalScore,
            GameEndReason gameEndReason,
            string lastMistakeTaskTag,
            TaskAction? lastMistakeAction,
            out int conditionScore)
        {
            conditionScore = 0;
            if (definition == null)
            {
                return false;
            }

            if (definition.conditionType == ConditionScoreRange)
            {
                int safeScore = Mathf.Max(0, finalScore);
                if (safeScore < definition.minScore)
                {
                    return false;
                }

                if (definition.maxScore >= 0 && safeScore > definition.maxScore)
                {
                    return false;
                }

                conditionScore = safeScore;
                return true;
            }

            if (definition.conditionType == ConditionLifeZeroLastMistakeTag || definition.conditionType == ConditionLifeZeroLastMistakeTagAndAction)
            {
                if (gameEndReason != GameEndReason.LifeZero || IsEmptyTag(definition.targetTag) || IsEmptyTag(lastMistakeTaskTag))
                {
                    return false;
                }

                if (!string.Equals(definition.targetTag.Trim(), lastMistakeTaskTag.Trim(), System.StringComparison.Ordinal))
                {
                    return false;
                }

                if (definition.conditionType == ConditionLifeZeroLastMistakeTagAndAction)
                {
                    if (!lastMistakeAction.HasValue || string.IsNullOrWhiteSpace(definition.targetAction))
                    {
                        return false;
                    }

                    if (!string.Equals(definition.targetAction.Trim(), lastMistakeAction.Value.ToString(), System.StringComparison.Ordinal))
                    {
                        return false;
                    }
                }

                conditionScore = 1;
                return true;
            }

            if (definition.conditionType != ConditionTagTapAtLeast && definition.conditionType != ConditionTagSwipeAtLeast)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(definition.targetTag) || definition.threshold <= 0 || tagStats == null)
            {
                return false;
            }

            if (!tagStats.TryGetValue(definition.targetTag.Trim(), out TaskTagStats stats) || stats == null)
            {
                return false;
            }

            conditionScore = definition.conditionType == ConditionTagTapAtLeast ? stats.tapCount : stats.swipeCount;
            return conditionScore >= definition.threshold;
        }

        private static void LogConditionResult(
            TitleDefinition definition,
            int finalScore,
            GameEndReason gameEndReason,
            string lastMistakeTaskTag,
            TaskAction? lastMistakeAction,
            int conditionScore,
            bool result)
        {
            if (definition.conditionType == ConditionScoreRange)
            {
                string maxScore = definition.maxScore >= 0 ? definition.maxScore.ToString() : "plus";
                Debug.Log($"[TitleJudge] check {definition.titleId} condition=scoreRange finalScore={finalScore} range={definition.minScore}-{maxScore} result={result}");
                return;
            }

            if (definition.conditionType == ConditionLifeZeroLastMistakeTag || definition.conditionType == ConditionLifeZeroLastMistakeTagAndAction)
            {
                Debug.Log($"[TitleJudge] check {definition.conditionType} target={definition.targetTag} targetAction={definition.targetAction} gameEndReason={gameEndReason} lastMistakeTag={lastMistakeTaskTag} lastMistakeAction={lastMistakeAction?.ToString() ?? string.Empty} result={result}");
                return;
            }

            Debug.Log($"[TitleJudge] check {definition.titleId} target={definition.targetTag} condition={definition.conditionType} score={conditionScore} threshold={definition.threshold} result={result}");
        }

        private static bool IsEmptyTag(string value)
        {
            return string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "none", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
