namespace PushNotificationGod.Tasks
{
    [System.Serializable]
    public class TaskTagStats
    {
        public string tag;
        public int tapCount;
        public int swipeCount;

        public TaskTagStats(string tag)
        {
            this.tag = tag;
        }
    }
}
