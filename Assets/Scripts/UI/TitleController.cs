using PushNotificationGod.Core;
using PushNotificationGod.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PushNotificationGod.UI
{
    public class TitleController : MonoBehaviour
    {
        private const string DefaultPlayerName = "なまえ未設定";
        private const bool UseRuntimeTitleButtons = true;
        private static readonly Color TitleButtonLabelColor = new(0.04f, 0.08f, 0.12f, 1f);

        [SerializeField] private GameObject howToPanel;
        [SerializeField] private GameObject playerNamePanel;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private Text infoMessageText;
        [SerializeField] private Sprite titleBackgroundSprite;
        [SerializeField] private Sprite titleLogoSprite;
        [SerializeField] private Sprite howToSprite;
        [SerializeField] private Sprite roundedButtonSprite;
        [SerializeField] private Sprite startButtonLabelSprite;
        [SerializeField] private Sprite rankingButtonLabelSprite;
        [SerializeField] private Sprite howToButtonLabelSprite;
        [SerializeField] private Sprite settingsButtonLabelSprite;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AudioClip titleBgmClip;
        [SerializeField] private AudioSource titleBgmSource;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider seVolumeSlider;

        private Transform uiRoot;
        private Font uiFont;

        private void Start()
        {
            uiFont = UIJapaneseFont.Get();
            uiRoot = FindAnyObjectByType<Canvas>()?.transform;
            EnsureEventSystem();
            UIJapaneseFont.ApplyToSceneTexts();
            ForceHideModalPanels();
            ApplyTitleVisuals();
            EnsurePanels();
            if (UseRuntimeTitleButtons)
            {
                CreateRuntimeTitleButtons();
            }
            else
            {
                BindButtons();
            }
            ForceHideModalPanels();
            UIJapaneseFont.ApplyToSceneTexts();
            EnsureBuildText();
            EnsureTitleBgmSource();
            Debug.Log($"[{BuildInfo.BuildId}] TitleScene started. titleBgmClip={(titleBgmClip != null ? "OK" : "NULL")}");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.touchCount > 0)
            {
                Debug.Log($"[{BuildInfo.BuildId}] First title user input detected.");
                TryPlayTitleBgm();
            }
        }

        public void StartGame()
        {
            Debug.Log($"[{BuildInfo.BuildId}] Start button pressed. Showing player name input.");
            TryPlayTitleBgm();
            ShowPlayerNameInput();
        }

        public void ShowPlayerNameInput()
        {
            EnsurePanels();
            HideAllPanels();
            if (playerNameInput != null)
            {
                playerNameInput.text = string.IsNullOrWhiteSpace(LocalSaveManager.PlayerName) ? string.Empty : LocalSaveManager.PlayerName;
                playerNameInput.ActivateInputField();
            }

            playerNamePanel.SetActive(true);
        }

        public void ConfirmPlayerName()
        {
            Debug.Log("[NameDialog] Confirm clicked");
            string playerName = playerNameInput != null ? playerNameInput.text : string.Empty;
            string safePlayerName = string.IsNullOrWhiteSpace(playerName) ? DefaultPlayerName : playerName.Trim();

            try
            {
                Debug.Log("[NameDialog] Save player name start");
                LocalSaveManager.SavePlayerName(safePlayerName);
                Debug.Log("[NameDialog] Save player name success");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[NameDialog] Save player name failed. Continue with default. {ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                if (playerNameInput != null)
                {
                    playerNameInput.DeactivateInputField();
                }

                if (playerNamePanel != null)
                {
                    Debug.Log("[NameDialog] Close dialog");
                    playerNamePanel.SetActive(false);
                }
            }

            Debug.Log("[NameDialog] Confirm finished");
            Debug.Log($"[{BuildInfo.BuildId}] Player name confirmed. Loading GameScene.");
            StopTitleBgm();
            SceneManager.LoadScene("GameScene");
        }

        public void CancelPlayerNameInput()
        {
            if (playerNamePanel != null)
            {
                playerNamePanel.SetActive(false);
            }
        }

        public void ShowRanking()
        {
            ShowInfo("ランキングは準備中です");
        }

        public void ShowSettings()
        {
            Debug.Log($"[{BuildInfo.BuildId}] Settings button pressed.");
            EnsurePanels();
            HideAllPanels();
            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.SetValueWithoutNotify(LocalSaveManager.BgmVolume);
            }

            if (seVolumeSlider != null)
            {
                seVolumeSlider.SetValueWithoutNotify(LocalSaveManager.SeVolume);
            }

            settingsPanel.SetActive(true);
        }

        public void ToggleHowTo()
        {
            ShowHowTo();
        }

        public void ShowHowTo()
        {
            if (howToPanel == null)
            {
                Debug.Log("必要なメッセージはタップ、いらないメッセージは右スワイプで消そう。");
                return;
            }

            HideAllPanels();
            howToPanel.SetActive(true);
        }

        public void ClosePanels()
        {
            HideAllPanels();
        }

        private void ShowInfo(string message)
        {
            EnsurePanels();
            HideAllPanels();
            if (infoMessageText != null)
            {
                infoMessageText.text = message;
            }

            infoPanel.SetActive(true);
        }

        private void BindButtons()
        {
            ConfigureButton("StartButton", StartGame);
            ConfigureButton("RankingButton", ShowRanking);
            ConfigureButton("HowToButton", ShowHowTo);
            ConfigureButton("SettingsButton", ShowSettings);
        }

        private void CreateRuntimeTitleButtons()
        {
            Transform buttonParent = GetTitleButtonParent();
            HideTitleButton("StartButton");
            HideTitleButton("RankingButton");
            HideTitleButton("HowToButton");
            HideTitleButton("SettingsButton");
            HideTitleButton("RuntimeStartButton");
            HideTitleButton("RuntimeHowToButton");
            HideTitleButton("RuntimeSettingButton");

            CreateRuntimeButton("RuntimeStartButton", buttonParent, "ゲームスタート", null, new Vector2(0f, -245f), new Vector2(640f, 120f), StartGame);
            CreateRuntimeButton("RuntimeHowToButton", buttonParent, "あそびかた", null, new Vector2(0f, -395f), new Vector2(640f, 104f), ShowHowTo);
            CreateRuntimeButton("RuntimeSettingButton", buttonParent, "設定", null, new Vector2(0f, -525f), new Vector2(640f, 104f), ShowSettings);
        }

        private Transform GetTitleButtonParent()
        {
            GameObject startButton = GameObject.Find("StartButton");
            if (startButton != null && startButton.transform.parent != null)
            {
                return startButton.transform.parent;
            }

            return uiRoot;
        }

        private static void HideTitleButton(string objectName)
        {
            GameObject button = GameObject.Find(objectName);
            if (button != null)
            {
                button.SetActive(false);
            }
        }

        private Button CreateRuntimeButton(string objectName, Transform parent, string fallbackLabel, Sprite labelSprite, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            ApplyRoundedImage(image, new Color(0.92f, 0.98f, 1f, 0.92f), true);
            Shadow shadow = buttonObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
            shadow.effectDistance = new Vector2(0f, -5f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            button.targetGraphic = image;

            Text text = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(buttonObject.transform, false);
            text.text = fallbackLabel;
            text.font = uiFont;
            text.fontSize = fallbackLabel.Length > 8 ? 38 : 44;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = TitleButtonLabelColor;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            StretchToParent(text.rectTransform, 12f, 8f);
            text.gameObject.SetActive(labelSprite == null);

            if (labelSprite != null)
            {
                Image labelImage = new GameObject("LabelImage", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                labelImage.transform.SetParent(buttonObject.transform, false);
                labelImage.sprite = labelSprite;
                labelImage.color = Color.white;
                labelImage.preserveAspect = true;
                labelImage.raycastTarget = false;
                StretchToParent(labelImage.rectTransform, 34f, 20f);
                labelImage.transform.SetAsLastSibling();
            }
            else
            {
                text.transform.SetAsLastSibling();
            }

            buttonObject.transform.SetAsLastSibling();
            return button;
        }

        private void ConfigureButton(string objectName, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = GameObject.Find(objectName);
            if (buttonObject == null)
            {
                return;
            }

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            button.interactable = true;
            ApplyButtonStyle(button, objectName == "SettingsButton");
            EnsureButtonLabel(button, GetButtonLabel(objectName), GetButtonLabelSprite(objectName), objectName == "SettingsButton");
            DisableChildTextRaycasts(button.transform);
        }

        private void EnsurePanels()
        {
            if (uiRoot == null)
            {
                return;
            }

            if (playerNamePanel == null)
            {
                playerNamePanel = CreatePlayerNamePanel();
            }

            if (howToPanel == null || (howToPanel.transform.Find("AsobikataImage") == null && howToPanel.transform.Find("CloseButton") == null))
            {
                if (howToPanel != null)
                {
                    howToPanel.SetActive(false);
                }

                howToPanel = CreateHowToImagePanel();
            }

            if (infoPanel == null)
            {
                infoPanel = CreateMessagePanel("InfoPanel", string.Empty, out infoMessageText);
            }

            if (settingsPanel == null)
            {
                settingsPanel = CreateSettingsPanel();
            }
        }

        private GameObject CreatePlayerNamePanel()
        {
            GameObject panel = CreateModalPanel("PlayerNamePanel");
            GameObject card = CreateCard(panel.transform, new Vector2(840f, 460f), "NameInputPanelBackground");
            CreateText("PromptText", card.transform, "ユーザー名を入力してください", 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 135f), new Vector2(720f, 70f), Color.black);

            GameObject inputObject = CreatePanel("PlayerNameInput", card.transform, new Color(0.98f, 0.99f, 1f, 0.96f), new Vector2(0f, 35f), new Vector2(660f, 88f));
            ApplyRoundedImage(inputObject.GetComponent<Image>(), new Color(0.98f, 0.99f, 1f, 0.96f), true);
            Shadow inputShadow = inputObject.AddComponent<Shadow>();
            inputShadow.effectColor = new Color(0f, 0f, 0f, 0.12f);
            inputShadow.effectDistance = new Vector2(0f, -3f);
            playerNameInput = inputObject.AddComponent<InputField>();
            Text inputText = CreateText("Text", inputObject.transform, string.Empty, 34, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(610f, 70f), new Color(0.04f, 0.08f, 0.12f));
            inputText.rectTransform.anchorMin = Vector2.zero;
            inputText.rectTransform.anchorMax = Vector2.one;
            inputText.rectTransform.offsetMin = new Vector2(28f, 8f);
            inputText.rectTransform.offsetMax = new Vector2(-28f, -8f);
            Text placeholder = CreateText("Placeholder", inputObject.transform, "名前", 34, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(610f, 70f), new Color(0.25f, 0.3f, 0.36f, 0.55f));
            placeholder.rectTransform.anchorMin = Vector2.zero;
            placeholder.rectTransform.anchorMax = Vector2.one;
            placeholder.rectTransform.offsetMin = new Vector2(28f, 8f);
            placeholder.rectTransform.offsetMax = new Vector2(-28f, -8f);
            playerNameInput.textComponent = inputText;
            playerNameInput.placeholder = placeholder;

            Button ok = CreateButton("ConfirmButton", card.transform, "決定", new Vector2(-175f, -135f), new Vector2(280f, 92f));
            ok.GetComponent<Image>().color = new Color(0.84f, 0.97f, 1f, 0.92f);
            ok.onClick.AddListener(ConfirmPlayerName);
            Button cancel = CreateButton("CancelButton", card.transform, "キャンセル", new Vector2(175f, -135f), new Vector2(280f, 92f));
            cancel.onClick.AddListener(CancelPlayerNameInput);
            return panel;
        }

        private GameObject CreateMessagePanel(string objectName, string message)
        {
            return CreateMessagePanel(objectName, message, out _);
        }

        private GameObject CreateMessagePanel(string objectName, string message, out Text messageText)
        {
            GameObject panel = CreateModalPanel(objectName);
            GameObject card = CreateCard(panel.transform, new Vector2(820f, 360f));
            messageText = CreateText("MessageText", card.transform, message, 38, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 45f), new Vector2(720f, 160f), new Color(0.04f, 0.08f, 0.12f));
            Button close = CreateButton("CloseButton", card.transform, "閉じる", new Vector2(0f, -110f), new Vector2(360f, 86f));
            close.onClick.AddListener(ClosePanels);
            return panel;
        }

        private GameObject CreateSettingsPanel()
        {
            GameObject panel = CreateModalPanel("SettingsPanel");
            GameObject card = CreateCard(panel.transform, new Vector2(820f, 620f));
            CreateText("SettingsTitleText", card.transform, "設定", 50, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 230f), new Vector2(720f, 80f), new Color(0.04f, 0.08f, 0.12f));

            bgmVolumeSlider = CreateVolumeSlider(card.transform, "BGM音量", new Vector2(0f, 95f), LocalSaveManager.BgmVolume, OnBgmVolumeChanged);
            seVolumeSlider = CreateVolumeSlider(card.transform, "SE音量", new Vector2(0f, -70f), LocalSaveManager.SeVolume, OnSeVolumeChanged);

            Button close = CreateButton("CloseButton", card.transform, "閉じる", new Vector2(0f, -230f), new Vector2(360f, 88f));
            close.onClick.AddListener(ClosePanels);
            return panel;
        }

        private Slider CreateVolumeSlider(Transform parent, string label, Vector2 anchoredPosition, float value, UnityEngine.Events.UnityAction<float> onChanged)
        {
            GameObject root = new(label + "Group", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = new Vector2(660f, 130f);

            CreateText(label + "Label", root.transform, label, 34, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, 42f), new Vector2(660f, 50f), new Color(0.04f, 0.08f, 0.12f));

            GameObject sliderObject = new(label + "Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(root.transform, false);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.anchoredPosition = new Vector2(0f, -24f);
            sliderRect.sizeDelta = new Vector2(640f, 46f);

            GameObject backgroundObject = CreatePanel("Background", sliderObject.transform, new Color(0.08f, 0.12f, 0.16f, 0.18f), Vector2.zero, new Vector2(640f, 18f));
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = value;
            slider.targetGraphic = backgroundObject.GetComponent<Image>();

            GameObject fillArea = new("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(12f, 0f);
            fillAreaRect.offsetMax = new Vector2(-12f, 0f);

            GameObject fill = CreatePanel("Fill", fillArea.transform, new Color(0.38f, 0.78f, 1f, 0.78f), Vector2.zero, new Vector2(0f, 18f));
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0.5f);
            fillRect.anchorMax = new Vector2(1f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.sizeDelta = new Vector2(0f, 18f);
            slider.fillRect = fillRect;

            GameObject handleArea = new("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObject.transform, false);
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(12f, 0f);
            handleAreaRect.offsetMax = new Vector2(-12f, 0f);

            GameObject handle = CreatePanel("Handle", handleArea.transform, Color.white, Vector2.zero, new Vector2(54f, 54f));
            Shadow shadow = handle.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.25f);
            shadow.effectDistance = new Vector2(0f, -3f);
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.onValueChanged.AddListener(onChanged);
            return slider;
        }

        private void OnBgmVolumeChanged(float value)
        {
            LocalSaveManager.SaveBgmVolume(value);
        }

        private void OnSeVolumeChanged(float value)
        {
            LocalSaveManager.SaveSeVolume(value);
        }

        private void EnsureAudioManager()
        {
            if (audioManager == null)
            {
                audioManager = FindAnyObjectByType<AudioManager>();
            }
        }

        private void EnsureTitleBgmSource()
        {
            if (titleBgmSource == null)
            {
                titleBgmSource = gameObject.GetComponent<AudioSource>();
            }

            if (titleBgmSource == null)
            {
                titleBgmSource = gameObject.AddComponent<AudioSource>();
            }

            titleBgmSource.playOnAwake = false;
            titleBgmSource.loop = true;
            titleBgmSource.spatialBlend = 0f;
            titleBgmSource.volume = GetSafeVolume("bgm_volume", 0.35f);
        }

        private void TryPlayTitleBgm()
        {
            EnsureTitleBgmSource();
            if (titleBgmClip == null || titleBgmSource == null || titleBgmSource.isPlaying)
            {
                return;
            }

            titleBgmSource.clip = titleBgmClip;
            titleBgmSource.volume = GetSafeVolume("bgm_volume", 0.35f);
            titleBgmSource.Play();
            Debug.Log($"[{BuildInfo.BuildId}] [Audio] Title BGM started: {titleBgmClip.name}, volume={titleBgmSource.volume:F2}");
        }

        private void StopTitleBgm()
        {
            if (titleBgmSource != null)
            {
                titleBgmSource.Stop();
            }
        }

        private static float GetSafeVolume(string key, float fallback)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return fallback;
            }

            float value = PlayerPrefs.GetFloat(key, fallback);
            if (value <= 0.001f)
            {
                PlayerPrefs.SetFloat(key, fallback);
                return fallback;
            }

            return Mathf.Clamp01(value);
        }

        private GameObject CreateHowToImagePanel()
        {
            GameObject panel = CreateModalPanel("HowToPanel");
            Button closeByTap = panel.AddComponent<Button>();
            closeByTap.transition = Selectable.Transition.None;
            closeByTap.onClick.AddListener(ClosePanels);

            GameObject imageObject = new("AsobikataImage", typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(panel.transform, false);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = howToSprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(960f, 1580f);

            if (howToSprite == null)
            {
                CreateText("FallbackText", panel.transform, "必要なメッセージはタップ、\nいらないメッセージは右スワイプで消そう。", 42, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(850f, 260f), Color.white);
            }

            return panel;
        }

        private GameObject CreateModalPanel(string objectName)
        {
            GameObject panel = new(objectName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(uiRoot, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image image = panel.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.42f);
            image.raycastTarget = true;
            panel.SetActive(false);
            return panel;
        }

        private void ApplyTitleVisuals()
        {
            if (uiRoot == null)
            {
                return;
            }

            Image background = FindTitleBackgroundImage();
            if (background != null)
            {
                background.gameObject.name = "TitleBackgroundImage";
                background.sprite = titleBackgroundSprite;
                background.color = titleBackgroundSprite != null ? Color.white : background.color;
                background.preserveAspect = true;
                background.raycastTarget = false;
                background.transform.SetAsFirstSibling();
                StretchToParent(background.rectTransform);
            }

            GameObject titleText = GameObject.Find("Title");
            if (titleText != null)
            {
                titleText.SetActive(false);
            }

            Image logo = FindOrCreateTitleLogoImage();
            if (logo != null)
            {
                logo.sprite = titleLogoSprite;
                logo.color = Color.white;
                logo.preserveAspect = true;
                logo.raycastTarget = false;
                RectTransform rect = logo.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, 320f);
                rect.sizeDelta = new Vector2(990f, 590f);
                TitleLogoBreath breath = logo.GetComponent<TitleLogoBreath>();
                if (breath == null)
                {
                    breath = logo.gameObject.AddComponent<TitleLogoBreath>();
                }

                breath.Configure(1f, 1.045f, 0.8f);
            }

            MoveButton("StartButton", new Vector2(0f, -245f), new Vector2(640f, 120f));
            MoveButton("RankingButton", new Vector2(0f, -395f), new Vector2(640f, 104f));
            MoveButton("HowToButton", new Vector2(0f, -525f), new Vector2(640f, 104f));
            MoveSettingsButton();
            EnsureCopyrightText();
            EnsureBuildText();
        }

        private void EnsureBuildText()
        {
            Transform parent = GetTitleUiParent();
            Transform existing = parent.Find("BuildInfoText");
            Text buildText = existing != null ? existing.GetComponent<Text>() : null;
            if (buildText == null)
            {
                buildText = new GameObject("BuildInfoText", typeof(RectTransform), typeof(Text), typeof(Shadow)).GetComponent<Text>();
                buildText.transform.SetParent(parent, false);
            }

            buildText.text = $"Build: {BuildInfo.BuildId}";
            buildText.font = uiFont;
            buildText.fontSize = 20;
            buildText.fontStyle = FontStyle.Normal;
            buildText.alignment = TextAnchor.MiddleCenter;
            buildText.color = new Color(1f, 1f, 1f, 0.72f);
            buildText.raycastTarget = false;

            Shadow shadow = buildText.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(0f, -2f);

            RectTransform rect = buildText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 74f);
            rect.sizeDelta = new Vector2(820f, 34f);
            buildText.transform.SetAsLastSibling();
        }

        private Image FindTitleBackgroundImage()
        {
            GameObject backgroundObject = GameObject.Find("TitleBackgroundImage") ?? GameObject.Find("BackgroundImage");
            if (backgroundObject != null)
            {
                return backgroundObject.GetComponent<Image>();
            }

            GameObject created = new("TitleBackgroundImage", typeof(RectTransform), typeof(Image));
            created.transform.SetParent(uiRoot, false);
            created.transform.SetAsFirstSibling();
            return created.GetComponent<Image>();
        }

        private Image FindOrCreateTitleLogoImage()
        {
            GameObject logoObject = GameObject.Find("TitleLogoImage");
            if (logoObject == null)
            {
                logoObject = new GameObject("TitleLogoImage", typeof(RectTransform), typeof(Image));
                logoObject.transform.SetParent(GetTitleUiParent(), false);
            }

            return logoObject.GetComponent<Image>();
        }

        private void EnsureCopyrightText()
        {
            Transform parent = GetTitleUiParent();
            Transform existing = parent.Find("CopyrightText");
            Text copyright = existing != null ? existing.GetComponent<Text>() : null;
            if (copyright == null)
            {
                copyright = new GameObject("CopyrightText", typeof(RectTransform), typeof(Text), typeof(Shadow)).GetComponent<Text>();
                copyright.transform.SetParent(parent, false);
            }

            copyright.text = "© 2026 Hyorome";
            copyright.font = uiFont;
            copyright.fontSize = 22;
            copyright.fontStyle = FontStyle.Normal;
            copyright.alignment = TextAnchor.MiddleCenter;
            copyright.color = new Color(1f, 1f, 1f, 0.68f);
            copyright.raycastTarget = false;

            Shadow shadow = copyright.GetComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
            shadow.effectDistance = new Vector2(0f, -2f);

            RectTransform rect = copyright.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 36f);
            rect.sizeDelta = new Vector2(700f, 42f);
            copyright.transform.SetAsLastSibling();
        }

        private Transform GetTitleUiParent()
        {
            GameObject safeArea = GameObject.Find("SafeArea");
            return safeArea != null ? safeArea.transform : uiRoot;
        }

        private static void MoveButton(string objectName, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject buttonObject = GameObject.Find(objectName);
            if (buttonObject == null)
            {
                return;
            }

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void MoveSettingsButton()
        {
            GameObject buttonObject = GameObject.Find("SettingsButton");
            if (buttonObject == null)
            {
                return;
            }

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(44f, -128f);
            rect.sizeDelta = new Vector2(150f, 78f);
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchToParent(RectTransform rect, float horizontal, float vertical)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(horizontal, vertical);
            rect.offsetMax = new Vector2(-horizontal, -vertical);
        }

        private GameObject CreateCard(Transform parent, Vector2 size, string objectName = "Card")
        {
            GameObject card = CreatePanel(objectName, parent, new Color(0.94f, 0.99f, 1f, 0.9f), Vector2.zero, size);
            ApplyRoundedImage(card.GetComponent<Image>(), new Color(0.94f, 0.99f, 1f, 0.9f), false);
            Shadow shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
            shadow.effectDistance = new Vector2(0f, -8f);
            return card;
        }

        private Button CreateButton(string objectName, Transform parent, string label, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject buttonObject = CreatePanel(objectName, parent, new Color(0.92f, 0.98f, 1f, 0.82f), anchoredPosition, size);
            Button button = buttonObject.AddComponent<Button>();
            ApplyButtonStyle(button, false);
            Text labelText = CreateText("Label", buttonObject.transform, label, 32, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, size, new Color(0.04f, 0.08f, 0.12f));
            labelText.rectTransform.anchorMin = Vector2.zero;
            labelText.rectTransform.anchorMax = Vector2.one;
            labelText.rectTransform.offsetMin = new Vector2(10f, 10f);
            labelText.rectTransform.offsetMax = new Vector2(-10f, -10f);
            labelText.raycastTarget = false;
            return button;
        }

        private void ApplyButtonStyle(Button button, bool compact)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                ApplyRoundedImage(image, compact ? new Color(0.92f, 0.98f, 1f, 0.74f) : new Color(0.92f, 0.98f, 1f, 0.84f), true);
            }

            Shadow shadow = button.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = button.gameObject.AddComponent<Shadow>();
            }

            shadow.effectColor = new Color(0f, 0f, 0f, 0.22f);
            shadow.effectDistance = compact ? new Vector2(0f, -3f) : new Vector2(0f, -5f);

            foreach (Text text in button.GetComponentsInChildren<Text>(true))
            {
                text.font = uiFont;
                text.fontSize = compact ? 28 : 34;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(0.04f, 0.08f, 0.12f, 1f);
                text.raycastTarget = false;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }

        private void EnsureButtonLabel(Button button, string label, Sprite labelSprite, bool compact)
        {
            if (button == null || string.IsNullOrEmpty(label))
            {
                return;
            }

            Text labelText = button.transform.Find("Label")?.GetComponent<Text>();
            if (labelText == null)
            {
                labelText = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
                labelText.transform.SetParent(button.transform, false);
            }

            labelText.text = label;
            labelText.font = uiFont;
            labelText.fontSize = compact ? 32 : 42;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = UseRuntimeTitleButtons ? TitleButtonLabelColor : new Color(0.04f, 0.08f, 0.12f, 1f);
            labelText.raycastTarget = false;
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelText.verticalOverflow = VerticalWrapMode.Overflow;
            StretchToParent(labelText.rectTransform, 10f, 10f);
            labelText.gameObject.SetActive(true);

            Image labelImage = button.transform.Find("LabelImage")?.GetComponent<Image>();
            if (labelImage == null)
            {
                labelImage = new GameObject("LabelImage", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                labelImage.transform.SetParent(button.transform, false);
            }

            labelImage.sprite = labelSprite;
            labelImage.color = Color.white;
            labelImage.preserveAspect = true;
            labelImage.raycastTarget = false;
            labelImage.enabled = !UseRuntimeTitleButtons && labelSprite != null;
            Vector2 inset = compact ? new Vector2(18f, 14f) : new Vector2(24f, 18f);
            StretchToParent(labelImage.rectTransform, inset.x, inset.y);
            labelText.transform.SetAsLastSibling();

            if (UseRuntimeTitleButtons)
            {
                EnsureOverlayButtonLabel(button, label, compact);
            }
        }

        private void EnsureOverlayButtonLabel(Button button, string label, bool compact)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            Transform parent = button.transform.parent;
            if (buttonRect == null || parent == null)
            {
                return;
            }

            string objectName = "DebugLabel_" + button.gameObject.name;
            Text overlayText = parent.Find(objectName)?.GetComponent<Text>();
            if (overlayText == null)
            {
                overlayText = new GameObject(objectName, typeof(RectTransform), typeof(Text)).GetComponent<Text>();
                overlayText.transform.SetParent(parent, false);
            }

            overlayText.text = label;
            overlayText.font = uiFont;
            overlayText.fontSize = compact ? 32 : 42;
            overlayText.fontStyle = FontStyle.Bold;
            overlayText.alignment = TextAnchor.MiddleCenter;
            overlayText.color = TitleButtonLabelColor;
            overlayText.raycastTarget = false;
            overlayText.horizontalOverflow = HorizontalWrapMode.Overflow;
            overlayText.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform overlayRect = overlayText.rectTransform;
            overlayRect.anchorMin = buttonRect.anchorMin;
            overlayRect.anchorMax = buttonRect.anchorMax;
            overlayRect.pivot = buttonRect.pivot;
            overlayRect.anchoredPosition = buttonRect.anchoredPosition;
            overlayRect.sizeDelta = buttonRect.sizeDelta;
            overlayText.gameObject.SetActive(true);
            overlayText.transform.SetAsLastSibling();
        }

        private static string GetButtonLabel(string objectName)
        {
            return objectName switch
            {
                "StartButton" => "ゲームスタート",
                "RankingButton" => "ランキング",
                "HowToButton" => "あそびかた",
                "SettingsButton" => "設定",
                _ => string.Empty
            };
        }

        private Sprite GetButtonLabelSprite(string objectName)
        {
            return objectName switch
            {
                "StartButton" => startButtonLabelSprite,
                "RankingButton" => rankingButtonLabelSprite,
                "HowToButton" => howToButtonLabelSprite,
                "SettingsButton" => settingsButtonLabelSprite,
                _ => null
            };
        }

        private void ApplyRoundedImage(Image image, Color color, bool raycastTarget)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = roundedButtonSprite;
            image.type = roundedButtonSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = raycastTarget;
        }

        private GameObject CreatePanel(string objectName, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject panel = new(objectName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private Text CreateText(string objectName, Transform parent, string value, int size, FontStyle style, TextAnchor anchor, Vector2 anchoredPosition, Vector2 rectSize, Color color)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = uiFont;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.raycastTarget = false;
            RectTransform rect = text.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = rectSize;
            return text;
        }

        private void HideAllPanels()
        {
            if (playerNamePanel != null)
            {
                playerNamePanel.SetActive(false);
            }

            if (howToPanel != null)
            {
                howToPanel.SetActive(false);
            }

            if (infoPanel != null)
            {
                infoPanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void ForceHideModalPanels()
        {
            HideAllPanels();
            HideScenePanelsByName("PlayerNamePanel");
            HideScenePanelsByName("HowToPanel");
            HideScenePanelsByName("InfoPanel");
            HideScenePanelsByName("SettingsPanel");
        }

        private static void HideScenePanelsByName(string panelName)
        {
            foreach (GameObject panel in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (panel == null || panel.name != panelName || !panel.scene.IsValid())
                {
                    continue;
                }

                panel.SetActive(false);
            }
        }

        private static void DisableChildTextRaycasts(Transform root)
        {
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                text.raycastTarget = false;
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
