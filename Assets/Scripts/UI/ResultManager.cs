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
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text rankTitleText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button titleButton;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private Sprite roundedUiSprite;

        private void Start()
        {
            UIJapaneseFont.ApplyToSceneTexts();
            GameResultData.RestoreFromSaveIfEmpty();
            Debug.Log($"[ResultController Show] FinalScore={GameResultData.FinalScore}, Success={GameResultData.SuccessCount}, Miss={GameResultData.MissCount}, MaxCombo={GameResultData.MaxCombo}, Rank={GameResultData.RankTitle}");
            BindButtons();
            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = GameResultData.FinalScore.ToString();
            }

            if (maxComboText != null)
            {
                maxComboText.text = $"{GameResultData.MaxCombo} COMBO";
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = LocalSaveManager.BestScore.ToString();
            }

            if (rankTitleText != null)
            {
                rankTitleText.text = GameResultData.RankTitle;
            }

            Debug.Log($"[ResultText Applied] scoreText={(finalScoreText != null ? finalScoreText.text : "null")}, successText={GameResultData.SuccessCount}, missText={GameResultData.MissCount}, maxComboText={(maxComboText != null ? maxComboText.text : "null")}");

            ApplyResultStyle();
            UIJapaneseFont.ApplyToSceneTexts();
            audioManager?.PlayResult();
        }

        public void OnRetryButton()
        {
            Debug.Log($"[{BuildInfo.BuildId}] Retry button pressed. Loading GameScene.");
            Time.timeScale = 1f;
            SceneManager.LoadScene("GameScene");
        }

        public void OnTitleButton()
        {
            Debug.Log($"[{BuildInfo.BuildId}] Title button pressed. Loading TitleScene.");
            Time.timeScale = 1f;
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

        private void ApplyResultStyle()
        {
            Transform parent = finalScoreText != null ? finalScoreText.transform.parent : null;
            if (parent == null)
            {
                return;
            }

            Image panelImage = EnsureResultPanel(parent);
            panelImage.transform.SetAsFirstSibling();

            ConfigureText(GameObject.Find("Heading")?.GetComponent<Text>(), 56, FontStyle.Bold, new Vector2(0f, 500f), new Vector2(760f, 92f), "通知さばき完了！");

            Text finalLabel = EnsureLabel(parent, "FinalScoreLabelText", "最終スコア");
            Text comboLabel = EnsureLabel(parent, "MaxComboLabelText", "最大コンボ");
            Text bestLabel = EnsureLabel(parent, "BestScoreLabelText", "最高スコア");
            Text newRecordText = EnsureLabel(parent, "NewRecordText", "新記録！");
            Text rankLabel = EnsureLabel(parent, "RankTitleLabelText", "今回の称号");
            Text processedLabel = EnsureLabel(parent, "ProcessedCountLabelText", "処理数 / ミス");
            Text processedText = EnsureLabel(parent, "ProcessedCountText", $"{GameResultData.SuccessCount} / {GameResultData.MissCount}");
            rankTitleText = EnsureLabel(parent, "RankTitleText", GameResultData.RankTitle);

            ConfigureText(finalLabel, 34, FontStyle.Bold, new Vector2(0f, 392f), new Vector2(740f, 50f));
            ConfigureText(finalScoreText, 88, FontStyle.Bold, new Vector2(0f, 318f), new Vector2(760f, 94f));
            ConfigureText(rankLabel, 30, FontStyle.Bold, new Vector2(0f, 188f), new Vector2(740f, 44f));
            ConfigureText(rankTitleText, 50, FontStyle.Bold, new Vector2(0f, 116f), new Vector2(760f, 76f));
            ConfigureText(processedLabel, 28, FontStyle.Bold, new Vector2(0f, 16f), new Vector2(740f, 40f));
            ConfigureText(processedText, 44, FontStyle.Bold, new Vector2(0f, -42f), new Vector2(760f, 62f));
            ConfigureText(comboLabel, 28, FontStyle.Bold, new Vector2(0f, -118f), new Vector2(740f, 40f));
            ConfigureText(maxComboText, 56, FontStyle.Bold, new Vector2(0f, -180f), new Vector2(760f, 68f));
            ConfigureText(bestLabel, 28, FontStyle.Bold, new Vector2(0f, -264f), new Vector2(740f, 40f));
            ConfigureText(bestScoreText, 56, FontStyle.Bold, new Vector2(0f, -322f), new Vector2(760f, 66f));
            ConfigureText(newRecordText, 34, FontStyle.Bold, new Vector2(0f, -374f), new Vector2(740f, 44f));

            Image finalScoreBackground = EnsureValueBackground(parent, "FinalScoreValueBackground", new Vector2(0f, 318f), new Vector2(650f, 116f));
            Image rankTitleBackground = EnsureValueBackground(parent, "RankTitleValueBackground", new Vector2(0f, 112f), new Vector2(720f, 112f));
            Image maxComboBackground = EnsureValueBackground(parent, "MaxComboValueBackground", new Vector2(0f, -180f), new Vector2(670f, 86f));
            Image bestScoreBackground = EnsureValueBackground(parent, "BestScoreValueBackground", new Vector2(0f, -322f), new Vector2(570f, 84f));

            newRecordText.gameObject.SetActive(GameResultData.WasNewRecord);
            newRecordText.color = new Color(1f, 0.72f, 0.08f, 1f);
            ScoreColorUtility.ApplyReadableEffects(newRecordText);

            rankTitleText.color = Color.white;
            ScoreColorUtility.ApplyReadableEffects(rankTitleText);
            ScoreColorUtility.ApplyReadableEffects(processedText);
            ScoreColorUtility.ApplyScoreVisual(finalScoreText, GameResultData.FinalScore);
            ScoreColorUtility.ApplyComboVisual(maxComboText, GameResultData.MaxCombo);
            ScoreColorUtility.ApplyScoreVisual(bestScoreText, LocalSaveManager.BestScore);
            ApplyResultHierarchy(
                finalScoreBackground,
                rankTitleBackground,
                maxComboBackground,
                bestScoreBackground,
                finalLabel,
                rankLabel,
                comboLabel,
                bestLabel,
                newRecordText);

            ConfigureButtonRect(retryButton, new Vector2(0f, -470f), new Vector2(560f, 96f));
            ConfigureButtonRect(titleButton, new Vector2(0f, -586f), new Vector2(560f, 96f));
            StartCoroutine(PlayRankTitlePop());
        }

        private Image EnsureResultPanel(Transform parent)
        {
            Transform existing = parent.Find("ResultPanelBackground");
            GameObject panel = existing != null ? existing.gameObject : new GameObject("ResultPanelBackground", typeof(RectTransform), typeof(Image), typeof(Shadow));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(850f, 1420f);

            Image image = panel.GetComponent<Image>();
            image.sprite = roundedUiSprite;
            image.type = roundedUiSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = new Color(0.94f, 0.99f, 1f, 0.9f);
            image.raycastTarget = false;

            Shadow shadow = panel.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
            shadow.effectDistance = new Vector2(0f, -8f);
            return image;
        }

        private Image EnsureValueBackground(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size)
        {
            Transform existing = parent.Find(objectName);
            GameObject background = existing != null
                ? existing.gameObject
                : new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Shadow));
            background.transform.SetParent(parent, false);

            RectTransform rect = background.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = background.GetComponent<Image>();
            image.sprite = roundedUiSprite;
            image.type = roundedUiSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = new Color(0.02f, 0.07f, 0.12f, 0.48f);
            image.raycastTarget = false;

            Shadow shadow = background.GetComponent<Shadow>();
            shadow.effectColor = new Color(1f, 1f, 1f, 0.18f);
            shadow.effectDistance = new Vector2(0f, 3f);

            return image;
        }

        private void ApplyResultHierarchy(
            Image finalScoreBackground,
            Image rankTitleBackground,
            Image maxComboBackground,
            Image bestScoreBackground,
            Text finalLabel,
            Text rankLabel,
            Text comboLabel,
            Text bestLabel,
            Text newRecordText)
        {
            finalScoreBackground?.transform.SetAsLastSibling();
            finalLabel?.transform.SetAsLastSibling();
            BringTextToFront(finalScoreText);

            rankTitleBackground?.transform.SetAsLastSibling();
            rankLabel?.transform.SetAsLastSibling();
            BringTextToFront(rankTitleText);

            maxComboBackground?.transform.SetAsLastSibling();
            comboLabel?.transform.SetAsLastSibling();
            BringTextToFront(maxComboText);

            bestScoreBackground?.transform.SetAsLastSibling();
            bestLabel?.transform.SetAsLastSibling();
            BringTextToFront(bestScoreText);

            newRecordText?.transform.SetAsLastSibling();
        }

        private static void BringTextToFront(Text text)
        {
            if (text == null)
            {
                return;
            }

            text.gameObject.SetActive(true);
            text.enabled = true;
            text.canvasRenderer.SetAlpha(1f);
            text.transform.SetAsLastSibling();
        }

        public static string GetRankTitle(int score)
        {
            if (score >= 12000)
            {
                return "タスクの神様";
            }

            if (score >= 9000)
            {
                return "タスク界の番人";
            }

            if (score >= 7000)
            {
                return "タスク処理マスター";
            }

            if (score >= 5000)
            {
                return "タップの申し子";
            }

            if (score >= 4000)
            {
                return "既読スナイパー";
            }

            if (score >= 3000)
            {
                return "タスクさばき職人";
            }

            if (score >= 2000)
            {
                return "右スワイプの民";
            }

            if (score >= 1000)
            {
                return "普通の人";
            }

            if (score >= 500)
            {
                return "未読ためがち";
            }

            return "スマホに負けた人";
        }

        private System.Collections.IEnumerator PlayRankTitlePop()
        {
            if (rankTitleText == null)
            {
                yield break;
            }

            Transform target = rankTitleText.transform;
            float elapsed = 0f;
            const float duration = 0.32f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = t < 0.45f
                    ? Mathf.Lerp(0.88f, 1.12f, t / 0.45f)
                    : Mathf.Lerp(1.12f, 1f, (t - 0.45f) / 0.55f);
                target.localScale = Vector3.one * scale;
                yield return null;
            }

            target.localScale = Vector3.one;
        }

        private Text EnsureLabel(Transform parent, string objectName, string textValue)
        {
            Transform existing = parent.Find(objectName);
            Text text = existing != null ? existing.GetComponent<Text>() : null;
            if (text == null)
            {
                text = new GameObject(objectName, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
                text.transform.SetParent(parent, false);
            }

            text.text = textValue;
            UIJapaneseFont.Apply(text);

            return text;
        }

        private static void ConfigureText(Text text, int fontSize, FontStyle style, Vector2 anchoredPosition, Vector2 size, string overrideText = null)
        {
            if (text == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(overrideText))
            {
                text.text = overrideText;
            }

            text.fontSize = fontSize;
            UIJapaneseFont.Apply(text);
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.04f, 0.08f, 0.12f, 1f);
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private void ConfigureButtonRect(Button button, Vector2 anchoredPosition, Vector2 size)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = roundedUiSprite;
                image.type = roundedUiSprite != null ? Image.Type.Sliced : Image.Type.Simple;
                image.color = new Color(0.92f, 0.98f, 1f, 0.86f);
            }

            Shadow shadow = button.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = button.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
            shadow.effectDistance = new Vector2(0f, -5f);

            foreach (Text text in button.GetComponentsInChildren<Text>(true))
            {
                text.fontSize = 34;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(0.04f, 0.08f, 0.12f, 1f);
                text.raycastTarget = false;
            }
        }
    }
}
