using PushNotificationGod.Audio;
using PushNotificationGod.Data;
using PushNotificationGod.Tasks;
using PushNotificationGod.Titles;
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
        [SerializeField] private TaskTagStatsManager tagStatsManager;
        [SerializeField] private int maxVisibleTaskCount = 7;
        [SerializeField] private float deadlineY = 1870f;
        [SerializeField] private bool taskOverflowCheckEnabled;
        [SerializeField] private Text countdownText;
        [SerializeField] private Text countdownInstructionText;
        [SerializeField] private CanvasGroup countdownCanvasGroup;
        [SerializeField] private float countdownStepDuration = 1f;
        [SerializeField] private float countdownStartDuration = 0.8f;
        [SerializeField] private Button restartButton;
        [SerializeField] private LastSecondsWarningController lastSecondsWarningController;

        private bool gameEnded;
        private GameState gameState = GameState.WaitingForStart;
        private int successCount;
        private int missCount;
        private string lastMistakeTaskId = string.Empty;
        private string lastMistakeTaskMessage = string.Empty;
        private string lastMistakeTaskTag = string.Empty;
        private TaskAction? lastMistakeAction;
        private GameObject resultOverlay;
        private readonly TitleJudge titleJudge = new();
        private Coroutine countdownRoutine;
        private bool isCountingDown;
        private int countdownRunId;
        private Sprite restartButtonSprite;

        private void Start()
        {
            UIJapaneseFont.ApplyToSceneTexts();
            taskDatabase.Load();
            if (tagStatsManager == null)
            {
                tagStatsManager = gameObject.AddComponent<TaskTagStatsManager>();
            }

            uiManager.Bind(scoreManager, lifeManager, comboManager, timerManager);
            if (feedbackManager == null)
            {
                feedbackManager = gameObject.AddComponent<FeedbackManager>();
            }

            feedbackManager.Configure(uiManager.FeedbackParent, uiManager.ScoreText, uiManager.ComboText, audioManager);
            EnsureCountdownView();
            EnsureRestartButton();
            EnsureLastSecondsWarningView();
            UIJapaneseFont.ApplyToSceneTexts();
            Debug.Log($"[{BuildInfo.BuildId}] GameScene started. GameManager={name}, TaskSpawner={taskSpawner?.name}, ResultMode=InScenePanel");

            lifeManager.OnLifeDepleted += () => EndGame(GameEndReason.LifeZero);
            timerManager.OnTimeUp += () => EndGame(GameEndReason.TimeUp);
            taskManager.OnCardSpawned += RegisterCard;

            ResetGameState();
            StartCountdown();
        }

        private void Update()
        {
            UpdateLastSecondsWarning();
            if (gameState == GameState.Playing && !gameEnded)
            {
                audioManager?.RetryGameplayBgmIfNeeded();
            }

            if (taskOverflowCheckEnabled && gameState == GameState.Playing && !gameEnded && taskManager.IsOverflow(deadlineY, maxVisibleTaskCount))
            {
                EndGame(GameEndReason.TaskOverflow);
            }
        }

        public void RegisterCard(TaskCard card)
        {
            card.OnAction += HandleTaskAction;
        }

        public void RestartGame()
        {
            Debug.Log($"[{BuildInfo.BuildId}] Restart button pressed.");
            StopAllCoroutines();
            countdownRoutine = null;
            isCountingDown = false;
            ResetGameState();
            StartCountdown();
        }

        private void ResetGameState()
        {
            Time.timeScale = 1f;
            taskSpawner.Stop();
            timerManager.StopTimer();
            audioManager?.StopGameplayBgm();
            lastSecondsWarningController?.StopWarning();
            taskManager.ClearAllForRestart();
            taskDatabase.Load();

            if (resultOverlay != null)
            {
                resultOverlay.SetActive(false);
            }

            scoreManager.ResetScore();
            lifeManager.ResetLife();
            comboManager.ResetCombo();
            tagStatsManager?.ResetStats();
            successCount = 0;
            missCount = 0;
            ClearLastMistake();
            gameEnded = false;
            gameState = GameState.WaitingForStart;
            isCountingDown = false;
            timerManager.PrepareTimer();
            if (uiManager.ScoreText != null)
            {
                uiManager.ScoreText.text = "0";
            }

            if (uiManager.ComboText != null)
            {
                uiManager.ComboText.text = string.Empty;
                uiManager.ComboText.transform.localScale = Vector3.one;
            }

            if (countdownCanvasGroup != null)
            {
                countdownCanvasGroup.alpha = 0f;
                countdownCanvasGroup.gameObject.SetActive(false);
            }
        }

        private void HandleTaskAction(TaskCard card, TaskAction action)
        {
            if (gameState != GameState.Playing || gameEnded)
            {
                return;
            }

            bool correct = card.Definition.correctAction == action;
            tagStatsManager?.RecordAction(card.Definition, action);
            if (correct)
            {
                successCount++;
                float multiplier = comboManager.RegisterCorrect();
                int gainedScore = scoreManager.AddScore(card.Definition.baseScore, multiplier);
                Debug.Log($"[ScoreAdd] currentScore={scoreManager.Score}, success={successCount}, miss={missCount}, combo={comboManager.CurrentCombo}, maxCombo={comboManager.MaxCombo}");
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
                RecordLastMistake(card.Definition, action);
                comboManager.RegisterMistake();
                lifeManager.ApplyMistakePenalty();
                VibrationManager.PlayLightVibration();
                audioManager?.PlayMiss();
                feedbackManager?.PlayMistakeFeedback();
            }

            if (gameEnded || gameState != GameState.Playing)
            {
                Debug.Log("[GameOverFlow] Dismiss skipped because game already ended.");
                return;
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

        private void RecordLastMistake(TaskDefinition task, TaskAction action)
        {
            lastMistakeTaskId = task != null ? task.taskId : string.Empty;
            lastMistakeTaskMessage = task != null ? task.messageText : string.Empty;
            lastMistakeTaskTag = task != null ? task.tag : string.Empty;
            lastMistakeAction = action;
            Debug.Log($"[LastMistake] taskId={lastMistakeTaskId} message={lastMistakeTaskMessage} tag={lastMistakeTaskTag} action={lastMistakeAction}");
        }

        private void ClearLastMistake()
        {
            lastMistakeTaskId = string.Empty;
            lastMistakeTaskMessage = string.Empty;
            lastMistakeTaskTag = string.Empty;
            lastMistakeAction = null;
        }

        private void EndGame(GameEndReason reason)
        {
            Debug.Log("[GameOverFlow] 1 GameOver called");
            Debug.Log($"[EndGame] reason={reason}");
            if (gameEnded)
            {
                return;
            }

            gameEnded = true;
            gameState = GameState.Result;
            Debug.Log("[GameOverFlow] 2 isPlaying false");
            Debug.Log("[GameOverFlow] 3 Stop spawn");
            taskSpawner.Stop();
            timerManager.StopTimer();
            lastSecondsWarningController?.StopWarning();
            audioManager?.StopGameplayBgm();
            Debug.Log("[BGM] Stop called from EndGame");
            Debug.Log("[GameOverFlow] Gameplay BGM stopped");
            Debug.Log($"[GameEnd BeforeSave] score={scoreManager.Score}, success={successCount}, miss={missCount}, maxCombo={comboManager.MaxCombo}");
            Debug.Log("[GameOverFlow] 4 Save result start");
            tagStatsManager?.LogStats();
            if (reason == GameEndReason.LifeZero)
            {
                Debug.Log($"[LifeZero] lastMistakeTaskId={lastMistakeTaskId} message={lastMistakeTaskMessage} tag={lastMistakeTaskTag} action={lastMistakeAction?.ToString() ?? string.Empty}");
            }

            TitleDefinition resultTitle = titleJudge.JudgeMainTitle(
                tagStatsManager != null ? tagStatsManager.GetAllStats() : null,
                scoreManager.Score,
                reason,
                lastMistakeTaskTag,
                lastMistakeAction);
            GameResultData.Save(
                scoreManager.Score,
                successCount,
                missCount,
                comboManager.MaxCombo,
                resultTitle,
                reason,
                lastMistakeTaskId,
                lastMistakeTaskMessage,
                lastMistakeTaskTag,
                lastMistakeAction);
            Debug.Log("[GameOverFlow] 5 Save result done");
            Debug.Log($"[GameEnd AfterSave] FinalScore={GameResultData.FinalScore}, Success={GameResultData.SuccessCount}, Miss={GameResultData.MissCount}, MaxCombo={GameResultData.MaxCombo}, Rank={GameResultData.RankTitle}, Description={GameResultData.RankDescription}");
            Debug.Log("[GameOverFlow] 6 Hide active tasks start");
            taskManager.HideAllForGameOver();
            Debug.Log("[GameOverFlow] 7 Hide active tasks done");
            Debug.Log("[GameOverFlow] 8 Show result panel start");
            ShowInSceneResultPanel();
        }

        private void ShowInSceneResultPanel()
        {
            Debug.Log("[GameOverFlow] 9 ResultController.Show start");
            RectTransform parent = uiManager != null ? uiManager.FeedbackParent : null;
            if (parent == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                parent = canvas != null ? (RectTransform)canvas.transform : null;
            }

            if (parent == null)
            {
                Debug.LogWarning("[GameOverFlow] Result parent not found. Cannot show result panel.");
                return;
            }

            if (resultOverlay != null)
            {
                resultOverlay.SetActive(false);
            }

            resultOverlay = new GameObject("InSceneResultOverlay", typeof(RectTransform), typeof(Image));
            resultOverlay.transform.SetParent(parent, false);
            resultOverlay.transform.SetAsLastSibling();
            RectTransform overlayRect = resultOverlay.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            Image overlayImage = resultOverlay.GetComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, 0.58f);
            overlayImage.raycastTarget = true;

            CreateResultPanel(resultOverlay.transform);
            Debug.Log($"[ResultText Applied] scoreText={GameResultData.FinalScore}, successText={GameResultData.SuccessCount}, missText={GameResultData.MissCount}, maxComboText={GameResultData.MaxCombo} COMBO, rankText={GameResultData.RankTitle}, rankDescriptionText={GameResultData.RankDescription}");
            Debug.Log("[GameOverFlow] 10 ResultController.Show done");
        }

        private void StartCountdown()
        {
            Debug.Log($"[{BuildInfo.BuildId}] StartCountdown requested at {Time.realtimeSinceStartup:F3}. existingCoroutine={(countdownRoutine != null)}, isCountingDown={isCountingDown}, state={gameState}");
            if (countdownRoutine != null)
            {
                Debug.Log($"[{BuildInfo.BuildId}] Existing CountdownRoutine stopped before starting a new one. previousRunId={countdownRunId}");
                StopCoroutine(countdownRoutine);
                countdownRoutine = null;
            }

            isCountingDown = false;
            gameState = GameState.Countdown;
            taskSpawner.Stop();
            timerManager.StopTimer();
            lastSecondsWarningController?.StopWarning();
            EnsureCountdownView();
            countdownRoutine = StartCoroutine(CountdownRoutine());
        }

        private void CreateResultPanel(Transform parent)
        {
            GameObject panel = new("MvpResultPanel", typeof(RectTransform), typeof(Image), typeof(Shadow));
            panel.transform.SetParent(parent, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(850f, 1280f);
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.94f, 0.99f, 1f, 0.94f);
            panelImage.raycastTarget = true;
            Shadow panelShadow = panel.GetComponent<Shadow>();
            panelShadow.effectColor = new Color(0f, 0f, 0f, 0.32f);
            panelShadow.effectDistance = new Vector2(0f, -8f);

            CreateResultButton(panel.transform, "RetryButton", "もう一度", new Vector2(-188f, 520f), new Vector2(340f, 78f), () => SceneManager.LoadScene("GameScene"));
            CreateResultButton(panel.transform, "TitleButton", "タイトルへ", new Vector2(188f, 520f), new Vector2(340f, 78f), () => SceneManager.LoadScene("TitleScene"));

            CreateResultText(panel.transform, "ResultHeadingText", "通知斬り完了！", 54, FontStyle.Bold, new Vector2(0f, 420f), new Vector2(760f, 82f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateResultText(panel.transform, "PlayerNameText", GetDisplayPlayerName(), 50, FontStyle.Bold, new Vector2(0f, 348f), new Vector2(780f, 74f), new Color(0.04f, 0.08f, 0.12f, 0.95f), false);

            CreateResultText(panel.transform, "RankLabelText", "今回の称号", 28, FontStyle.Bold, new Vector2(0f, 254f), new Vector2(720f, 40f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateValueBand(panel.transform, new Vector2(0f, 168f), new Vector2(720f, 134f));
            CreateResultText(panel.transform, "RankText", GameResultData.RankTitle, 44, FontStyle.Bold, new Vector2(0f, 198f), new Vector2(760f, 62f), Color.white, true);
            CreateResultText(panel.transform, "RankDescriptionText", GameResultData.RankDescription, 27, FontStyle.Bold, new Vector2(0f, 136f), new Vector2(740f, 54f), new Color(0.94f, 0.98f, 1f, 1f), true);

            CreateResultText(panel.transform, "FinalScoreLabelText", "最終スコア", 30, FontStyle.Bold, new Vector2(0f, 34f), new Vector2(720f, 42f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateValueBand(panel.transform, new Vector2(0f, -32f), new Vector2(650f, 96f));
            Text finalScoreText = CreateResultText(panel.transform, "FinalScoreText", GameResultData.FinalScore.ToString(), 78, FontStyle.Bold, new Vector2(0f, -32f), new Vector2(740f, 88f), Color.white, true);
            ScoreColorUtility.ApplyScoreVisual(finalScoreText, GameResultData.FinalScore);

            CreateResultText(panel.transform, "SuccessLabelText", "処理数", 27, FontStyle.Bold, new Vector2(-190f, -156f), new Vector2(320f, 38f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateResultText(panel.transform, "MissLabelText", "ミス", 27, FontStyle.Bold, new Vector2(190f, -156f), new Vector2(320f, 38f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateValueBand(panel.transform, new Vector2(-190f, -220f), new Vector2(280f, 74f));
            CreateValueBand(panel.transform, new Vector2(190f, -220f), new Vector2(280f, 74f));
            CreateResultText(panel.transform, "SuccessText", GameResultData.SuccessCount.ToString(), 46, FontStyle.Bold, new Vector2(-190f, -220f), new Vector2(320f, 68f), Color.white, true);
            CreateResultText(panel.transform, "MissText", GameResultData.MissCount.ToString(), 46, FontStyle.Bold, new Vector2(190f, -220f), new Vector2(320f, 68f), Color.white, true);

            CreateResultText(panel.transform, "MaxComboLabelText", "最大コンボ", 28, FontStyle.Bold, new Vector2(0f, -314f), new Vector2(720f, 40f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateValueBand(panel.transform, new Vector2(0f, -378f), new Vector2(640f, 82f));
            Text maxComboText = CreateResultText(panel.transform, "MaxComboText", $"{GameResultData.MaxCombo} COMBO", 52, FontStyle.Bold, new Vector2(0f, -378f), new Vector2(740f, 72f), Color.white, true);
            ScoreColorUtility.ApplyComboVisual(maxComboText, GameResultData.MaxCombo);
        }

        private static string GetDisplayPlayerName()
        {
            string playerName = LocalSaveManager.PlayerName;
            return string.IsNullOrWhiteSpace(playerName) ? "ななしの通知人" : playerName.Trim();
        }

        private Image CreateValueBand(Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject band = new("ValueBand", typeof(RectTransform), typeof(Image));
            band.transform.SetParent(parent, false);
            RectTransform rect = band.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = band.GetComponent<Image>();
            image.color = new Color(0.02f, 0.07f, 0.12f, 0.48f);
            image.raycastTarget = false;
            return image;
        }

        private Text CreateResultText(Transform parent, string objectName, string value, int fontSize, FontStyle style, Vector2 anchoredPosition, Vector2 size, Color color, bool readable)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = UIJapaneseFont.Get();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            if (readable)
            {
                Shadow shadow = textObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.58f);
                shadow.effectDistance = new Vector2(0f, -4f);
                Outline outline = textObject.AddComponent<Outline>();
                outline.effectColor = new Color(0f, 0f, 0f, 0.32f);
                outline.effectDistance = new Vector2(2f, -2f);
            }

            text.transform.SetAsLastSibling();
            return text;
        }

        private Button CreateResultButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Shadow));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.92f, 0.98f, 1f, 0.9f);
            Shadow shadow = buttonObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
            shadow.effectDistance = new Vector2(0f, -4f);
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);
            CreateResultText(buttonObject.transform, "Label", label, 32, FontStyle.Bold, Vector2.zero, size, new Color(0.04f, 0.08f, 0.12f, 1f), false);
            return button;
        }

        private System.Collections.IEnumerator CountdownRoutine()
        {
            Time.timeScale = 1f;
            isCountingDown = true;
            int runId = ++countdownRunId;
            gameState = GameState.Countdown;
            Debug.Log($"[{BuildInfo.BuildId}] CountdownRoutine started. runId={runId}, step={countdownStepDuration:F2}, start={countdownStartDuration:F2}, realtime={Time.realtimeSinceStartup:F3}");
            EnsureCountdownView();
            if (countdownCanvasGroup == null)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] Countdown overlay is missing. Countdown will still delay gameplay.");
            }

            audioManager?.PlayCountdownTick();
            ShowCountdownLabelImmediately("3", 190);
            Debug.Log($"[Countdown] 3 at {Time.realtimeSinceStartup:F3} runId={runId}");
            yield return new WaitForSecondsRealtime(1f);

            audioManager?.PlayCountdownTick();
            ShowCountdownLabelImmediately("2", 190);
            Debug.Log($"[Countdown] 2 at {Time.realtimeSinceStartup:F3} runId={runId}");
            yield return new WaitForSecondsRealtime(1f);

            audioManager?.PlayCountdownTick();
            ShowCountdownLabelImmediately("1", 190);
            Debug.Log($"[Countdown] 1 at {Time.realtimeSinceStartup:F3} runId={runId}");
            yield return new WaitForSecondsRealtime(1f);

            audioManager?.PlayCountdownStart();
            ShowCountdownLabelImmediately("START!", 160);
            Debug.Log($"[Countdown] START at {Time.realtimeSinceStartup:F3} runId={runId}");
            yield return new WaitForSecondsRealtime(0.8f);

            if (countdownCanvasGroup != null)
            {
                countdownCanvasGroup.alpha = 0f;
                countdownCanvasGroup.gameObject.SetActive(false);
            }

            countdownRoutine = null;
            isCountingDown = false;
            Debug.Log($"[Countdown] StartGameplay at {Time.realtimeSinceStartup:F3} runId={runId}");
            StartGameplay();
        }

        private void StartGameplay()
        {
            Debug.Log($"[{BuildInfo.BuildId}] StartGameplay requested at {Time.realtimeSinceStartup:F3}. isCountingDown={isCountingDown}, state={gameState}, gameEnded={gameEnded}");
            if (gameEnded)
            {
                Debug.Log($"[{BuildInfo.BuildId}] StartGameplay skipped because game already ended.");
                return;
            }

            if (isCountingDown)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] StartGameplay blocked because countdown is still running.");
                return;
            }

            gameState = GameState.Playing;
            Debug.Log($"[{BuildInfo.BuildId}] StartGameplay called. Timer, tasks and BGM start now.");
            lastSecondsWarningController?.StopWarning();
            timerManager.StartTimer();
            taskSpawner.Begin();
            audioManager?.PlayGameplayBgm();
        }

        private void UpdateLastSecondsWarning()
        {
            if (lastSecondsWarningController == null)
            {
                return;
            }

            if (gameState == GameState.Playing && !gameEnded && timerManager != null && timerManager.RemainingSeconds <= 10f)
            {
                lastSecondsWarningController.StartWarning();
            }
            else
            {
                lastSecondsWarningController.StopWarning();
            }
        }

        private void EnsureLastSecondsWarningView()
        {
            if (lastSecondsWarningController != null)
            {
                lastSecondsWarningController.StopWarning();
                return;
            }

            Transform safeParent = uiManager != null ? uiManager.FeedbackParent : null;
            if (safeParent == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                safeParent = canvas != null && canvas.transform.Find("SafeArea") != null
                    ? canvas.transform.Find("SafeArea")
                    : canvas != null ? canvas.transform : null;
            }

            if (safeParent == null)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] Last seconds warning parent not found.");
                return;
            }

            GameObject overlayObject = new("LastSecondsWarningOverlay", typeof(RectTransform), typeof(Image), typeof(LastSecondsWarningController));
            overlayObject.transform.SetParent(safeParent, false);
            overlayObject.transform.SetAsFirstSibling();

            RectTransform rect = overlayObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.color = new Color(1f, 0f, 0f, 0f);
            overlayImage.raycastTarget = false;

            lastSecondsWarningController = overlayObject.GetComponent<LastSecondsWarningController>();
            lastSecondsWarningController.Configure(overlayImage);
        }

        private void ShowCountdownLabelImmediately(string label, int fontSize = 190)
        {
            EnsureCountdownView();
            if (countdownCanvasGroup != null)
            {
                countdownCanvasGroup.gameObject.SetActive(true);
                countdownCanvasGroup.alpha = 1f;
                countdownCanvasGroup.transform.SetAsLastSibling();
            }

            if (countdownText != null)
            {
                countdownText.text = label;
                countdownText.fontSize = fontSize;
                countdownText.transform.localScale = Vector3.one;
            }
        }

        private void EnsureCountdownView()
        {
            if (countdownText != null && countdownInstructionText != null && countdownCanvasGroup != null)
            {
                countdownCanvasGroup.gameObject.SetActive(false);
                return;
            }

            RectTransform parent = GetOverlayParent();
            if (parent == null)
            {
                Debug.LogWarning($"[{BuildInfo.BuildId}] Countdown parent not found.");
                return;
            }

            GameObject overlay = new("CountdownOverlay", typeof(RectTransform), typeof(CanvasGroup));
            overlay.transform.SetParent(parent, false);
            overlay.transform.SetAsLastSibling();
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

        private RectTransform GetOverlayParent()
        {
            if (uiManager != null && uiManager.FeedbackParent != null)
            {
                return uiManager.FeedbackParent;
            }

            Canvas canvas = FindAnyObjectByType<Canvas>();
            return canvas != null ? canvas.transform as RectTransform : null;
        }

        private void EnsureRestartButton()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(RestartGame);
                restartButton.onClick.AddListener(RestartGame);
                return;
            }

            RectTransform parent = uiManager != null ? uiManager.FeedbackParent : null;
            if (parent == null)
            {
                return;
            }

            GameObject buttonObject = new("RestartButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Shadow));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.transform.SetAsLastSibling();

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(48f, -214f);
            rect.sizeDelta = new Vector2(270f, 74f);

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = GetRestartButtonSprite();
            image.type = Image.Type.Sliced;
            image.color = new Color(0.94f, 0.99f, 1f, 0.82f);
            image.raycastTarget = true;

            Shadow shadow = buttonObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
            shadow.effectDistance = new Vector2(0f, -4f);

            restartButton = buttonObject.GetComponent<Button>();
            restartButton.onClick.AddListener(RestartGame);

            GameObject labelObject = new("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            Text label = labelObject.GetComponent<Text>();
            label.font = UIJapaneseFont.Get();
            label.text = "リスタート";
            label.fontSize = 30;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.04f, 0.08f, 0.12f, 1f);
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
        }

        private Sprite GetRestartButtonSprite()
        {
            if (restartButtonSprite != null)
            {
                return restartButtonSprite;
            }

            const int size = 64;
            const int radius = 18;
            Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
            Color fill = Color.white;
            Color clear = new(1f, 1f, 1f, 0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int dx = x < radius ? radius - x : x >= size - radius ? x - (size - radius - 1) : 0;
                    int dy = y < radius ? radius - y : y >= size - radius ? y - (size - radius - 1) : 0;
                    bool inside = dx == 0 && dy == 0 || dx * dx + dy * dy <= radius * radius;
                    texture.SetPixel(x, y, inside ? fill : clear);
                }
            }

            texture.Apply();
            texture.name = "RestartButtonRoundedSpriteTexture";
            restartButtonSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            restartButtonSprite.name = "RestartButtonRoundedSprite";
            return restartButtonSprite;
        }
    }
}
