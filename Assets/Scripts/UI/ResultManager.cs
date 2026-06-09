using PushNotificationGod.Core;
using PushNotificationGod.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class ResultManager : MonoBehaviour
    {
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text maxComboText;
        [SerializeField] private Text playerNameText;
        [SerializeField] private Text rankTitleText;
        [SerializeField] private Text rankDescriptionText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button titleButton;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private Sprite roundedUiSprite;
        private Text successText;
        private Text missText;
        private Image resultPanelBackground;

        private const string SpecialScoreTitleId = "title_score_100000_plus";
        private static readonly Color DefaultPanelColor = new(0.94f, 0.99f, 1f, 0.92f);
        private static readonly Color GoldPanelColor = new(1f, 0.91f, 0.52f, 0.94f);

        private void Start()
        {
            UIJapaneseFont.ApplyToSceneTexts();
            Debug.Log($"[ResultController Show] FinalScore={GameResultData.FinalScore}, Success={GameResultData.SuccessCount}, Miss={GameResultData.MissCount}, MaxCombo={GameResultData.MaxCombo}, Rank={GameResultData.RankTitle}, Description={GameResultData.RankDescription}");
            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
            }

            audioManager?.StopGameplayBgm();
            Debug.Log($"[{BuildInfo.BuildId}] [BGM] Stop called from ResultManager.Start.");
            BuildRuntimeResultView();
            ApplyResultTexts();

            Debug.Log($"[ResultUI Target] scoreTextObject={finalScoreText?.gameObject.name}, active={finalScoreText?.gameObject.activeInHierarchy}");
            Debug.Log($"[ResultText Applied] scoreText={(finalScoreText != null ? finalScoreText.text : "null")}, successText={(successText != null ? successText.text : "null")}, missText={(missText != null ? missText.text : "null")}, maxComboText={(maxComboText != null ? maxComboText.text : "null")}, rankText={(rankTitleText != null ? rankTitleText.text : "null")}, rankDescriptionText={(rankDescriptionText != null ? rankDescriptionText.text : "null")}");

            UIJapaneseFont.ApplyToSceneTexts();
            audioManager?.PlayResult();
        }

        private void ApplyResultTexts()
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = GameResultData.FinalScore.ToString();
                ScoreColorUtility.ApplyScoreVisual(finalScoreText, GameResultData.FinalScore);
            }

            if (successText != null)
            {
                successText.text = GameResultData.SuccessCount.ToString();
            }

            if (missText != null)
            {
                missText.text = GameResultData.MissCount.ToString();
            }

            if (maxComboText != null)
            {
                maxComboText.text = $"{GameResultData.MaxCombo} COMBO";
                ScoreColorUtility.ApplyComboVisual(maxComboText, GameResultData.MaxCombo);
            }

            if (playerNameText != null)
            {
                playerNameText.text = GetDisplayPlayerName();
            }

            if (rankTitleText != null)
            {
                rankTitleText.text = GameResultData.RankTitle;
            }

            if (rankDescriptionText != null)
            {
                rankDescriptionText.text = GameResultData.RankDescription;
            }

            ApplyResultBackground();
        }

        public void OnRetryButton()
        {
            Debug.Log($"[{BuildInfo.BuildId}] Retry button pressed. Loading GameScene.");
            Time.timeScale = 1f;
            audioManager?.StopGameplayBgm();
            SceneManager.LoadScene("GameScene");
        }

        public void OnTitleButton()
        {
            Debug.Log($"[{BuildInfo.BuildId}] Title button pressed. Loading TitleScene.");
            Time.timeScale = 1f;
            audioManager?.StopGameplayBgm();
            SceneManager.LoadScene("TitleScene");
        }

        public void Retry()
        {
            OnRetryButton();
        }

        public void BackToTitle()
        {
            OnTitleButton();
        }

        private void BindButtons()
        {
            if (retryButton == null)
            {
                GameObject retryObject = GameObject.Find("RetryButton");
                if (retryObject != null)
                {
                    retryButton = retryObject.GetComponent<Button>();
                }
            }

            if (titleButton == null)
            {
                GameObject titleObject = GameObject.Find("BackToTitleButton");
                if (titleObject != null)
                {
                    titleButton = titleObject.GetComponent<Button>();
                }
            }

            ConfigureButton(retryButton, OnRetryButton);
            ConfigureButton(titleButton, OnTitleButton);
        }

        private void BuildRuntimeResultView()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("ResultManager could not find a Canvas. Falling back to serialized ResultScene UI.");
                BindButtons();
                return;
            }

            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform child = canvas.transform.GetChild(i);
                if (child.name != "BackgroundImage")
                {
                    child.gameObject.SetActive(false);
                }
            }

            Transform oldRuntimeView = canvas.transform.Find("RuntimeResultView");
            if (oldRuntimeView != null)
            {
                Destroy(oldRuntimeView.gameObject);
            }

            GameObject root = new("RuntimeResultView", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            root.transform.SetAsLastSibling();
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            resultPanelBackground = CreateRuntimePanel(root.transform, "ResultPanelBackground", new Vector2(0f, 0f), new Vector2(860f, 1460f), DefaultPanelColor);
            resultPanelBackground.transform.SetAsFirstSibling();

            retryButton = CreateRuntimeButton(root.transform, "RetryButton_Runtime", "もう一度", new Vector2(-188f, 548f), new Vector2(340f, 82f), OnRetryButton);
            titleButton = CreateRuntimeButton(root.transform, "BackToTitleButton_Runtime", "タイトルへ", new Vector2(188f, 548f), new Vector2(340f, 82f), OnTitleButton);

            CreateRuntimeText(root.transform, "HeadingText", "通知斬り完了！", 56, FontStyle.Bold, new Vector2(0f, 448f), new Vector2(780f, 90f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            playerNameText = CreateRuntimeText(root.transform, "PlayerNameText_Runtime", "ななしの通知人", 52, FontStyle.Bold, new Vector2(0f, 364f), new Vector2(800f, 76f), new Color(0.04f, 0.08f, 0.12f, 0.95f), false);
            ScoreColorUtility.ApplyReadableEffects(playerNameText);

            CreateRuntimeText(root.transform, "RankTitleLabelText", "今回の称号", 30, FontStyle.Bold, new Vector2(0f, 270f), new Vector2(740f, 42f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateRuntimePanel(root.transform, "RankTitleValueBackground", new Vector2(0f, 180f), new Vector2(740f, 140f), new Color(0.02f, 0.07f, 0.12f, 0.48f));
            rankTitleText = CreateRuntimeText(root.transform, "RankTitleText_Runtime", "通知に飲まれた人", 46, FontStyle.Bold, new Vector2(0f, 210f), new Vector2(780f, 62f), Color.white, true);
            rankDescriptionText = CreateRuntimeText(root.transform, "RankDescriptionText_Runtime", "気づいたら通知の波に流されていました。", 27, FontStyle.Bold, new Vector2(0f, 142f), new Vector2(760f, 54f), new Color(0.94f, 0.98f, 1f, 1f), true);

            CreateRuntimeText(root.transform, "FinalScoreLabelText", "最終スコア", 32, FontStyle.Bold, new Vector2(0f, 48f), new Vector2(740f, 44f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateRuntimePanel(root.transform, "FinalScoreValueBackground", new Vector2(0f, -30f), new Vector2(660f, 112f), new Color(0.02f, 0.07f, 0.12f, 0.52f));
            finalScoreText = CreateRuntimeText(root.transform, "FinalScoreText_Runtime", "0", 88, FontStyle.Bold, new Vector2(0f, -30f), new Vector2(760f, 96f), Color.white, true);

            CreateRuntimeText(root.transform, "SuccessLabelText", "処理数", 28, FontStyle.Bold, new Vector2(-190f, -166f), new Vector2(320f, 40f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateRuntimeText(root.transform, "MissLabelText", "ミス", 28, FontStyle.Bold, new Vector2(190f, -166f), new Vector2(320f, 40f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateRuntimePanel(root.transform, "SuccessValueBackground", new Vector2(-190f, -230f), new Vector2(290f, 78f), new Color(0.02f, 0.07f, 0.12f, 0.44f));
            CreateRuntimePanel(root.transform, "MissValueBackground", new Vector2(190f, -230f), new Vector2(290f, 78f), new Color(0.02f, 0.07f, 0.12f, 0.44f));
            successText = CreateRuntimeText(root.transform, "SuccessText_Runtime", "0", 48, FontStyle.Bold, new Vector2(-190f, -230f), new Vector2(320f, 70f), Color.white, true);
            missText = CreateRuntimeText(root.transform, "MissText_Runtime", "0", 48, FontStyle.Bold, new Vector2(190f, -230f), new Vector2(320f, 70f), Color.white, true);

            CreateRuntimeText(root.transform, "MaxComboLabelText", "最大コンボ", 28, FontStyle.Bold, new Vector2(0f, -326f), new Vector2(740f, 40f), new Color(0.04f, 0.08f, 0.12f, 1f), false);
            CreateRuntimePanel(root.transform, "MaxComboValueBackground", new Vector2(0f, -388f), new Vector2(650f, 84f), new Color(0.02f, 0.07f, 0.12f, 0.48f));
            maxComboText = CreateRuntimeText(root.transform, "MaxComboText_Runtime", "0 COMBO", 54, FontStyle.Bold, new Vector2(0f, -388f), new Vector2(760f, 72f), Color.white, true);
        }

        private void ApplyResultBackground()
        {
            if (resultPanelBackground == null)
            {
                Debug.LogWarning("ResultPanelBackground is missing. Cannot apply result background color.");
                return;
            }

            bool isSpecialScoreTitle = GameResultData.RankTitleId == SpecialScoreTitleId;
            resultPanelBackground.color = isSpecialScoreTitle ? GoldPanelColor : DefaultPanelColor;
            Debug.Log($"[ResultBackground] titleId={GameResultData.RankTitleId} gold={isSpecialScoreTitle}");
        }

        private static string GetDisplayPlayerName()
        {
            string playerName = LocalSaveManager.PlayerName;
            return string.IsNullOrWhiteSpace(playerName) ? "ななしの通知人" : playerName.Trim();
        }

        private Image CreateRuntimePanel(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject panel = new(objectName, typeof(RectTransform), typeof(Image), typeof(Shadow));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = panel.GetComponent<Image>();
            image.sprite = GetRoundedUiSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;

            Shadow shadow = panel.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.26f);
            shadow.effectDistance = new Vector2(0f, -5f);
            return image;
        }

        private Text CreateRuntimeText(Transform parent, string objectName, string value, int fontSize, FontStyle style, Vector2 anchoredPosition, Vector2 size, Color color, bool readableEffects)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            UIJapaneseFont.Apply(text);

            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            if (readableEffects)
            {
                ScoreColorUtility.ApplyReadableEffects(text);
            }

            text.transform.SetAsLastSibling();
            return text;
        }

        private Button CreateRuntimeButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
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
            image.sprite = GetRoundedUiSprite();
            image.type = Image.Type.Sliced;
            image.color = new Color(0.92f, 0.98f, 1f, 0.88f);

            Shadow shadow = buttonObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
            shadow.effectDistance = new Vector2(0f, -5f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);
            button.interactable = true;

            CreateRuntimeText(buttonObject.transform, "Label", label, 34, FontStyle.Bold, Vector2.zero, size, new Color(0.04f, 0.08f, 0.12f, 1f), false);
            return button;
        }

        private Sprite GetRoundedUiSprite()
        {
            return roundedUiSprite != null ? roundedUiSprite : RoundedUiSpriteFactory.Get();
        }

        private static void ConfigureButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
            button.interactable = true;

            foreach (Text text in button.GetComponentsInChildren<Text>(true))
            {
                text.raycastTarget = false;
            }
        }

    }
}
