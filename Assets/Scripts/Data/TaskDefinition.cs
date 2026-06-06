using PushNotificationGod.Tasks;

namespace PushNotificationGod.Data
{
    [System.Serializable]
    public class TaskDefinition
    {
        public string taskId;
        public string appName;
        public string iconId;
        public string messageText;
        public TaskAction correctAction;
        public int baseScore;
        public string backgroundStyleId;
        public int spawnWeight;
        public string tag;
    }
}
