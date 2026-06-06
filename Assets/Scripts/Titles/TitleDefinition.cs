namespace PushNotificationGod.Titles
{
    [System.Serializable]
    public class TitleDefinition
    {
        public string titleId;
        public string titleName;
        public string description;
        public int priority;
        public string conditionType;
        public string targetTag;
        public string targetAction;
        public int threshold;
        public int minScore;
        public int maxScore = -1;
    }
}
