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
        public int threshold;
    }
}
