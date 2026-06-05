using PushNotificationGod.Audio;
using PushNotificationGod.Data;
using PushNotificationGod.Tasks;
using PushNotificationGod.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushNotificationGod.Core
{
    public class GameManager : MonoBehaviour
    {
        private enum GameState
        {
            WaitingForStart,
            Countdown,
            Playing,
            Result
        }

        [SerializeField] private TaskDatabase taskDatabase;
        [SerializeField] private TaskSpawner taskSpawner;
        [SerializeField] private TaskManager taskManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private LifeManager lifeManager;
        [SerializeField] private ComboManager comboManager;
        [SerializeField] private TimerManager timerManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private FeedbackManager feedbackManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private int maxVisibleTaskCount = 7;
        [SerializeField] private float deadlineY = 1870f;
        [SerializeField] private Text countdownText;
        [SerializeField] private Text countdownInstructionText;
        [SerializeField] private CanvasGroup countdownCanvasGroup;
        [SerializeField] private float countdownStepDuration = 1.15f;
        [SerializeField] private float countdownStartDuration = 0.75f;
        [SerializeField] private float gameOverResultDelaySeconds = 0.8f;

        private bool gameEnded;
        private GameState gameState = GameState.WaitingForStart;
        private int successCount;
        private int missCount;

        private void Start()
        {
            UIJapaneseFont.ApplyToSceneTexts();
            taskDatabase.Load();
            scoreManager.ResetScore();
            lifeManager.ResetLife();
            comboManager.ResetCombo();
            successCount = 0;
            missCount = 0;
            timerManager.PrepareTimer();
            uiManager.Bind(scoreManager, lifeManager, comboManager, timerManager);
            if (feedbackManager == null)
            {
                feedbackManager = gameObject.AddComponent<FeedbackManager>();
            }

            feedbackManager.Configure(uiManager.FeedbackParent, uiManager.ScoreText, uiManager.ComboText, audioManager);
            EnsureCountdownView();
            UIJapaneseFont.ApplyToSceneTexts();
            Debug.Log($"[{BuildInfo.BuildId}] GameScene started. GameManager={name}, TaskSpawner={taskSpawner?.name}, ResultScene target=ResultScene");

            lifeManager.OnLifeDepleted += EndGame;
            timerManager.OnTimeUp += EndGame;
            taskManager.OnCardSpawned += RegisterCard;

            StartCoroutine(StartCountdown());
        }

        private void Update()
        {
            if (gameState == GameState.Playing && !gameEnded && taskManager.IsOverflow(deadlineY, maxVisibleTaskCount))
            {
                EndGame();
            }
        }

        public void RegisterCard(TaskCard card)
        {
            card.OnAction += HandleTaskAction;
        }

        private void HandleTaskAction(TaskCard card, TaskAction action)
        {
            if (gameState != GameState.Playing || gameEnded)
            {
                return;
            }

            bool correct = card.Definition.correctAction == action;
            if (correct)
            {
                successCount++;
                float multiplier = comboManager.RegisterCorrect();
                int gainedScore = scoreManager.AddScore(card.Definition.baseScore, multiplier);
                if (action == TaskAction.Tap)
                {
                    audioManager?.PlayTapCorrect();
                }
                else
                {
                    audioManager?.PlaySwipeCorrect();
                }

                feedbackManager?.PlayCorrectFeedback(card, action, gainedScore, comboManager.CurrentCombo, multiplier);
            }
            else
            {
                missCount++;
                comboManager.RegisterMistake();
                lifeManager.ApplyMistakePenalty();
                VibrationManager.PlayLightVibration();
                audioManager?.PlayMiss();
                feedbackManager?.PlayMistakeFeedback();
            }

            if (action == TaskAction.Tap)
            {
                card.PlayTapDismiss(() => taskManager.Remove(card));
            }
            else
            {
                card.PlaySwipeDismiss(() => taskManager.Remove(card));
            }
        }

        private void EndGame()
        {
            if (gameEnded)
            {
                return;
            }

            gameEnded = true;
            gameState = GameState.Result;
            taskSpawner.Stop();
            timerManager.StopTimer();
            Debug.Log($"[GameEnd] score={scoreManager.Score}, success={successCount}, miss={missCount}, maxCombo={comboManager.MaxCombo}");
            GameResultData.Save(scoreManager.Score, successCount, missCount, comboManager.MaxCombo);
            StartCoroutine(EndGameRoutine());
        }

        private System.Collections.IEnumerator EndGameRoutine()
        {
            audioManager?.StopGameplayBgm();
            audioManager?.PlayGameOver();
            yield return new WaitForSecondsRealtime(gameOverResultDelaySeconds);

            Debug.Log($"[{BuildInfo.BuildId}] Loading ResultScene with FinalScore={GameResultData.FinalScore}, Success={GameResultData.SuccessCount}, Miss={GameResultData.MissCount}, MaxCombo={GameResultData.MaxCombo}, Rank={GameResultData.RankTitle}");
            SceneManager.LoadScene("ResultScene");
        }

        private System.Collections.IEnumerator StartCountdown()
        {
            gameState = GameState.Countdown;
            if (countdownCanvasGroup != null)
            {
                countdownCanvasGroup.gameObject.SetActive(true);
                countdownCanvasGroup.alpha = 1f;
            }

            string[] labels = { "3", "2", "1", "START!" };
            for (int i = 0; i < labels.Length; i++)
            {
                if (i == labels.Length - 1)
                {
                    audioManager?.PlayCountdownStart();
                    audioManager?.PlayGameplayBgm();
                }
                else
                {
                    audioManager?.PlayCountdownTick();
                }

                yield return PlayCountdownStep(labels[i], i == labels.Length - 1 ? countdownStartDuration : countdownStepDuration);
            }

            if (countdownCanvasGroup != null)
            {
                countdownCanvasGroup.alpha = 0f;
                countdownCanvasGroup.gameObject.SetActive(false);
            }

            gameState = GameState.Playing;
            timerManager.StartTimer();
            taskSpawner.Begin();
        }

        private System.Collections.IEnumerator PlayCountdownStep(string label, float duration)
        {
            if (countdownText == null)
            {
                yield return new WaitForSeconds(duration);
                yield break;
            }

            countdownText.text = label;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float pop = t < 0.28f
                    ? Mathf.Lerp(0.65f, 1.18f, t / 0.28f)
                    : Mathf.Lerp(1.18f, 1f, (t - 0.28f) / 0.72f);
                countdownText.transform.localScale = Vector3.one * pop;
                if (countdownCanvasGroup != null)
                {
                    countdownCanvasGroup.alpha = Mathf.Lerp(1f, 0.82f, t);
                }

                yield return null;
            }

            countdownText.transform.localScale = Vector3.one;
            if (countdownCanvasGroup != null)
            {
                countdownCanvasGroup.alpha = 1f;
            }
        }

        private void EnsureCountdownView()
        {
            if (countdownText != null && countdownInstructionText != null && countdownCanvasGroup != null)
            {
                countdownCanvasGroup.gameObject.SetActive(false);
                return;
            }

            RectTransform parent = uiManager.FeedbackParent;
            if (parent == null)
            {
                return;
            }

            GameObject overlay = new("CountdownOverlay", typeof(RectTransform), typeof(CanvasGroup));
            overlay.transform.SetParent(parent, false);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            countdownCanvasGroup = overlay.GetComponent<CanvasGroup>();
            countdownCanvasGroup.blocksRaycasts = false;

            GameObject textObject = new("CountdownText", typeof(RectTransform), typeof(Text), typeof(Shadow), typeof(Outline));
            textObject.transform.SetParent(overlay.transform, false);
            countdownText = textObject.GetComponent<Text>();
            countdownText.font = UIJapaneseFont.Get();
            countdownText.text = "3";
            countdownText.fontSize = 190;
            countdownText.fontStyle = FontStyle.Bold;
            countdownText.alignment = TextAnchor.MiddleCenter;
            countdownText.color = Color.white;
            countdownText.raycastTarget = false;
            textObject.GetComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.55f);
            textObject.GetComponent<Shadow>().effectDistance = new Vector2(0f, -6f);
            textObject.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.45f);
            textObject.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
            RectTransform textRect = countdownText.rectTransform;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, 96f);
            textRect.sizeDelta = new Vector2(760f, 260f);

            GameObject instructionPanel = new("CountdownInstructionPanel", typeof(RectTransform), typeof(Image));
            instructionPanel.transform.SetParent(overlay.transform, false);
            Image panelImage = instructionPanel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.42f);
            panelImage.raycastTarget = false;
            RectTransform panelRect = instructionPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, -164f);
            panelRect.sizeDelta = new Vector2(820f, 220f);

            GameObject instructionObject = new("CountdownInstructionText", typeof(RectTransform), typeof(Text), typeof(Shadow), typeof(Outline));
            instructionObject.transform.SetParent(instructionPanel.transform, false);
            countdownInstructionText = instructionObject.GetComponent<Text>();
            countdownInstructionText.font = UIJapaneseFont.Get();
            countdownInstructionText.text = "⭕ 必要なものは【タップ！】\n❌ いらないものは【右スワイプ！】";
            countdownInstructionText.fontSize = 48;
            countdownInstructionText.fontStyle = FontStyle.Bold;
            countdownInstructionText.alignment = TextAnchor.MiddleCenter;
            countdownInstructionText.color = Color.white;
            countdownInstructionText.raycastTarget = false;
            countdownInstructionText.horizontalOverflow = HorizontalWrapMode.Wrap;
            countdownInstructionText.verticalOverflow = VerticalWrapMode.Overflow;
            countdownInstructionText.lineSpacing = 1.08f;
            instructionObject.GetComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.72f);
            instructionObject.GetComponent<Shadow>().effectDistance = new Vector2(0f, -4f);
            instructionObject.GetComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.62f);
            instructionObject.GetComponent<Outline>().effectDistance = new Vector2(2.5f, -2.5f);
            RectTransform instructionRect = countdownInstructionText.rectTransform;
            instructionRect.anchorMin = Vector2.zero;
            instructionRect.anchorMax = Vector2.one;
            instructionRect.offsetMin = new Vector2(24f, 18f);
            instructionRect.offsetMax = new Vector2(-24f, -18f);
            overlay.SetActive(false);
        }
    }
}
