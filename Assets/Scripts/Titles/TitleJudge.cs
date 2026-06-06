using System.Collections.Generic;
using PushNotificationGod.Tasks;
using UnityEngine;

namespace PushNotificationGod.Titles
{
    public class TitleJudge
    {
        public const string ConditionTagTapAtLeast = "tagTapAtLeast";
        public const string ConditionTagSwipeAtLeast = "tagSwipeAtLeast";

        private readonly List<TitleDefinition> definitions = new()
        {
            new TitleDefinition
            {
                titleId = "title_lover_swipe",
                titleName = "薄情なしの人",
                description = "大事な連絡ほど右へ流してしまうタイプ。",
                conditionType = ConditionTagSwipeAtLeast,
                targetTag = "lover",
                threshold = 2,
                priority = 90
            },
            new TitleDefinition
            {
                titleId = "title_scam_tap",
                titleName = "1000万円信じた人",
                description = "怪しい通知を信じてしまったピュアな人。",
                conditionType = ConditionTagTapAtLeast,
                targetTag = "scam",
                threshold = 1,
                priority = 100
            }
        };

        private readonly TitleDefinition defaultTitle = new()
        {
            titleId = "title_default",
            titleName = "普通のタスク人間",
            description = "今日もそれなりに通知をさばきました。",
            priority = 0
        };

        public TitleDefinition JudgeMainTitle(Dictionary<string, TaskTagStats> tagStats)
        {
            if (tagStats == null || tagStats.Count == 0)
            {
                Debug.Log("[TitleJudge] no tag stats. selected=普通のタスク人間");
                return defaultTitle;
            }

            foreach (TaskTagStats stats in tagStats.Values)
            {
                Debug.Log($"[TitleJudge] tag={stats.tag} tap={stats.tapCount} swipe={stats.swipeCount}");
            }

            TitleDefinition bestTitle = null;
            int bestScore = 0;
            foreach (TitleDefinition definition in definitions)
            {
                if (!TryGetConditionScore(definition, tagStats, out int conditionScore))
                {
                    Debug.Log($"[TitleJudge] check {definition.titleId} target={definition.targetTag} condition={definition.conditionType} threshold={definition.threshold} result=false");
                    continue;
                }

                Debug.Log($"[TitleJudge] check {definition.titleId} target={definition.targetTag} condition={definition.conditionType} score={conditionScore} threshold={definition.threshold} result=true");
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

            TitleDefinition selected = bestTitle ?? defaultTitle;
            Debug.Log($"[TitleJudge] selected={selected.titleName}");
            return selected;
        }

        private static bool TryGetConditionScore(TitleDefinition definition, Dictionary<string, TaskTagStats> tagStats, out int conditionScore)
        {
            conditionScore = 0;
            if (definition == null || string.IsNullOrWhiteSpace(definition.targetTag) || definition.threshold <= 0)
            {
                return false;
            }

            if (!tagStats.TryGetValue(definition.targetTag.Trim(), out TaskTagStats stats) || stats == null)
            {
                return false;
            }

            if (definition.conditionType == ConditionTagTapAtLeast)
            {
                conditionScore = stats.tapCount;
            }
            else if (definition.conditionType == ConditionTagSwipeAtLeast)
            {
                conditionScore = stats.swipeCount;
            }
            else
            {
                return false;
            }

            return conditionScore >= definition.threshold;
        }
    }
}
