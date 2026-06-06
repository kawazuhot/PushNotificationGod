using PushNotificationGod.Audio;
using PushNotificationGod.Core;
using PushNotificationGod.Data;
using PushNotificationGod.Input;
using PushNotificationGod.Tasks;
using PushNotificationGod.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MvpSceneBuilder
{
    private const string SceneDir = "Assets/Scenes";
    private const string PrefabDir = "Assets/Prefabs/TaskCard";
    private const string CardPrefabPath = PrefabDir + "/TaskCard.prefab";
    private const string RoundedCardSpritePath = "Assets/Art/UI/notification_card_rounded.png";
    private const string GameplayBackgroundPath = "Assets/Art/Backgrounds/bg_gameplay.png";
    private const string TitleLogoPath = "Assets/Art/UI/title_logo.png";
    private const string HowToImagePath = "Assets/Art/UI/asobikata.png";
    private const string StartButtonLabelPath = "Assets/Art/UI/title_button_game_start.png";
    private const string RankingButtonLabelPath = "Assets/Art/UI/title_button_ranking.png";
    private const string HowToButtonLabelPath = "Assets/Art/UI/title_button_asobikata.png";
    private const string SettingsButtonLabelPath = "Assets/Art/UI/title_button_settings.png";
    private const string DefaultIconPath = "Assets/Art/Icons/icon_default.png";

    [MenuItem("Tools/Push Notification God/Build MVP Scenes")]
    public static void BuildAll()
    {
        CreateFolders();
        GameObject cardPrefab = CreateTaskCardPrefab();
        CreateTitleScene();
        CreateGameScene(cardPrefab);
        CreateResultScene();
        SetBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Push Notification God/Validate MVP Scenes")]
    public static void ValidateMvpScenes()
    {
        string[] scenePaths =
        {
            $"{SceneDir}/TitleScene.unity",
            $"{SceneDir}/GameScene.unity",
            $"{SceneDir}/ResultScene.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new System.Exception($"{scene.name}: MainCamera tagged camera is missing.");
            }

            if (!camera.orthographic)
            {
                throw new System.Exception($"{scene.name}: Main Camera is not orthographic.");
            }

            Canvas[] canvases = Object.FindObjectsByType<Canvas>();
            if (canvases.Length == 0)
            {
                throw new System.Exception($"{scene.name}: Canvas is missing.");
            }

            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    throw new System.Exception($"{scene.name}: {canvas.name} is not Screen Space - Overlay.");
                }
            }

            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                throw new System.Exception($"{scene.name}: EventSystem is missing.");
            }

            Debug.Log($"{scene.name}: camera/canvas/event system validation passed.");
        }
    }

    private static void CreateFolders()
    {
        string[] folders =
        {
            "Assets/Scenes", "Assets/Scripts", "Assets/Scripts/Core", "Assets/Scripts/UI", "Assets/Scripts/Tasks",
            "Assets/Scripts/Data", "Assets/Scripts/Input", "Assets/Scripts/Audio", "Assets/Prefabs",
            "Assets/Prefabs/UI", "Assets/Prefabs/TaskCard", "Assets/Art", "Assets/Art/Backgrounds",
            "Assets/Art/Icons", "Assets/Art/UI", "Assets/Audio", "Assets/Audio/SE", "Assets/Audio/BGM",
            "Assets/GameData"
        };

        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
                string name = System.IO.Path.GetFileName(folder);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }

    private static GameObject CreateTaskCardPrefab()
    {
        Sprite roundedCardSprite = CreateRoundedCardSprite();
        Sprite defaultIconSprite = LoadIconSprite("icon_self") ?? CreateDefaultIconSprite();
        GameObject root = new("TaskCard", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(SwipeInputHandler), typeof(TaskCard));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0f);
        rootRect.anchorMax = new Vector2(0.5f, 0f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(880f, 220f);
        Image background = root.GetComponent<Image>();
        background.sprite = roundedCardSprite;
        background.type = Image.Type.Sliced;
        background.color = new Color(0.96f, 0.99f, 1f, 0.94f);

        Image icon = CreateImage("IconFrame", root.transform, new Color(1f, 0.48f, 0.52f, 1f));
        icon.sprite = defaultIconSprite;
        icon.color = Color.white;
        icon.preserveAspect = true;
        RectTransform iconRect = icon.rectTransform;
        SetTopLeft(iconRect, new Vector2(32f, -44f), new Vector2(112f, 112f));

        Text appName = CreateText("AppName", root.transform, "自分", 34, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.12f, 0.12f, 0.14f));
        appName.horizontalOverflow = HorizontalWrapMode.Overflow;
        appName.verticalOverflow = VerticalWrapMode.Truncate;
        appName.lineSpacing = 1.05f;
        SetTopLeft(appName.rectTransform, new Vector2(168f, -30f), new Vector2(540f, 46f));

        Text time = CreateText("Now", root.transform, "今", 28, FontStyle.Bold, TextAnchor.UpperRight, new Color(0.28f, 0.28f, 0.31f, 0.92f));
        time.horizontalOverflow = HorizontalWrapMode.Overflow;
        time.verticalOverflow = VerticalWrapMode.Truncate;
        SetTopRight(time.rectTransform, new Vector2(-30f, -34f), new Vector2(100f, 40f));

        Text message = CreateText("Message", root.transform, "歯をみがく", 40, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.11f, 0.11f, 0.12f));
        message.horizontalOverflow = HorizontalWrapMode.Wrap;
        message.verticalOverflow = VerticalWrapMode.Truncate;
        message.lineSpacing = 1.08f;
        SetTopLeft(message.rectTransform, new Vector2(168f, -82f), new Vector2(700f, 128f));

        TaskCard taskCard = root.GetComponent<TaskCard>();
        SetSerialized(taskCard, "backgroundImage", background);
        SetSerialized(taskCard, "iconImage", icon);
        SetSerialized(taskCard, "appNameText", appName);
        SetSerialized(taskCard, "timeText", time);
        SetSerialized(taskCard, "messageText", message);
        SetSerialized(taskCard, "inputHandler", root.GetComponent<SwipeInputHandler>());

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static Sprite CreateRoundedCardSprite()
    {
        const int width = 256;
        const int height = 96;
        const int radius = 34;
        const int border = 36;

        Texture2D texture = new(width, height, TextureFormat.RGBA32, false);
        texture.name = "notification_card_rounded";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new(1f, 1f, 1f, 0f);
        Color fill = Color.white;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool inside = IsInsideRoundedRect(x + 0.5f, y + 0.5f, width, height, radius);
                texture.SetPixel(x, y, inside ? fill : clear);
            }
        }

        texture.Apply();
        byte[] png = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(RoundedCardSpritePath, png);
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(RoundedCardSpritePath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(RoundedCardSpritePath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = new Vector4(border, border, border, border);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(RoundedCardSpritePath);
    }

    private static bool IsInsideRoundedRect(float x, float y, int width, int height, int radius)
    {
        float left = radius;
        float right = width - radius;
        float bottom = radius;
        float top = height - radius;
        float closestX = Mathf.Clamp(x, left, right);
        float closestY = Mathf.Clamp(y, bottom, top);
        float dx = x - closestX;
        float dy = y - closestY;
        return dx * dx + dy * dy <= radius * radius;
    }

    private static void CreateTitleScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject safe = CreateBaseCanvas("TitleCanvas", new Color(0.06f, 0.08f, 0.12f), LoadGameplayBackgroundSprite());
        GameObject backgroundObject = GameObject.Find("BackgroundImage");
        if (backgroundObject != null)
        {
            backgroundObject.name = "TitleBackgroundImage";
            backgroundObject.GetComponent<Image>().raycastTarget = false;
        }

        TitleController controller = new GameObject("TitleController").AddComponent<TitleController>();

        Image titleLogo = CreateImage("TitleLogoImage", safe.transform, Color.white);
        titleLogo.sprite = LoadUiSprite(TitleLogoPath, true);
        titleLogo.preserveAspect = true;
        titleLogo.raycastTarget = false;
        titleLogo.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        titleLogo.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        titleLogo.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        titleLogo.rectTransform.anchoredPosition = new Vector2(0f, 320f);
        titleLogo.rectTransform.sizeDelta = new Vector2(990f, 590f);
        TitleLogoBreath titleLogoBreath = titleLogo.gameObject.AddComponent<TitleLogoBreath>();
        titleLogoBreath.Configure(1f, 1.045f, 0.8f);

        Button start = CreateButton("StartButton", safe.transform, "ゲームスタート", new Vector2(0f, -245f), new Vector2(640f, 120f), TextAnchor.MiddleCenter, LoadUiSprite(StartButtonLabelPath, true));
        start.onClick.AddListener(controller.StartGame);

        Button ranking = CreateButton("RankingButton", safe.transform, "ランキング", new Vector2(0f, -395f), new Vector2(640f, 104f), TextAnchor.MiddleCenter, LoadUiSprite(RankingButtonLabelPath, true));
        ranking.onClick.AddListener(controller.ShowRanking);

        Button howTo = CreateButton("HowToButton", safe.transform, "あそびかた", new Vector2(0f, -525f), new Vector2(640f, 104f), TextAnchor.MiddleCenter, LoadUiSprite(HowToButtonLabelPath, true));
        howTo.onClick.AddListener(controller.ToggleHowTo);

        Button settings = CreateButton("SettingsButton", safe.transform, "設定", new Vector2(44f, -128f), new Vector2(150f, 78f), TextAnchor.MiddleCenter, LoadUiSprite(SettingsButtonLabelPath, true));
        settings.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
        settings.GetComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
        settings.onClick.AddListener(controller.ShowSettings);

        Text copyright = CreateText("CopyrightText", safe.transform, "© 2026 Hyorome", 22, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.68f));
        copyright.raycastTarget = false;
        RectTransform copyrightRect = copyright.rectTransform;
        copyrightRect.anchorMin = new Vector2(0.5f, 0f);
        copyrightRect.anchorMax = new Vector2(0.5f, 0f);
        copyrightRect.pivot = new Vector2(0.5f, 0f);
        copyrightRect.anchoredPosition = new Vector2(0f, 36f);
        copyrightRect.sizeDelta = new Vector2(700f, 42f);
        Shadow copyrightShadow = copyright.gameObject.AddComponent<Shadow>();
        copyrightShadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
        copyrightShadow.effectDistance = new Vector2(0f, -2f);

        SetSerialized(controller, "titleBackgroundSprite", LoadGameplayBackgroundSprite());
        SetSerialized(controller, "titleLogoSprite", LoadUiSprite(TitleLogoPath, true));
        SetSerialized(controller, "howToSprite", LoadUiSprite(HowToImagePath, true));
        SetSerialized(controller, "roundedButtonSprite", CreateRoundedCardSprite());
        SetSerialized(controller, "startButtonLabelSprite", LoadUiSprite(StartButtonLabelPath, true));
        SetSerialized(controller, "rankingButtonLabelSprite", LoadUiSprite(RankingButtonLabelPath, true));
        SetSerialized(controller, "howToButtonLabelSprite", LoadUiSprite(HowToButtonLabelPath, true));
        SetSerialized(controller, "settingsButtonLabelSprite", LoadUiSprite(SettingsButtonLabelPath, true));

        EditorSceneManager.SaveScene(scene, $"{SceneDir}/TitleScene.unity");
    }

    private static void CreateGameScene(GameObject cardPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject safe = CreateBaseCanvas("GameCanvas", new Color(0.08f, 0.12f, 0.18f), LoadGameplayBackgroundSprite());

        Text date = CreateText("DateText", safe.transform, "6月5日（金）", 36, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.86f));
        StretchTop(date.rectTransform, 82f, 56f, 70f);
        Text remainingTime = CreateText("RemainingTimeText", safe.transform, "1:00", 178, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        remainingTime.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -5f);
        StretchTop(remainingTime.rectTransform, 124f, 210f, 70f);

        Text life = CreateText("LifeText", safe.transform, "LIFE 5", 30, FontStyle.Bold, TextAnchor.UpperLeft, Color.white);
        SetTopLeft(life.rectTransform, new Vector2(40f, -330f), new Vector2(220f, 50f));
        GameObject scoreGroup = new("ScoreGroup", typeof(RectTransform));
        scoreGroup.transform.SetParent(safe.transform, false);
        RectTransform scoreGroupRect = scoreGroup.GetComponent<RectTransform>();
        scoreGroupRect.anchorMin = new Vector2(0.5f, 0f);
        scoreGroupRect.anchorMax = new Vector2(0.5f, 0f);
        scoreGroupRect.pivot = new Vector2(0.5f, 0f);
        scoreGroupRect.anchoredPosition = new Vector2(0f, 28f);
        scoreGroupRect.sizeDelta = new Vector2(560f, 160f);

        Text scoreLabel = CreateText("ScoreLabelText", scoreGroup.transform, "SCORE", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.82f));
        scoreLabel.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -2f);
        StretchTop(scoreLabel.rectTransform, 0f, 42f, 0f);
        Text score = CreateText("ScoreValueText", scoreGroup.transform, "0", 82, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        score.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -4f);
        score.rectTransform.anchorMin = new Vector2(0f, 0f);
        score.rectTransform.anchorMax = new Vector2(1f, 0f);
        score.rectTransform.pivot = new Vector2(0.5f, 0f);
        score.rectTransform.offsetMin = new Vector2(0f, 0f);
        score.rectTransform.offsetMax = new Vector2(0f, 104f);
        Text combo = CreateText("ComboText", safe.transform, "", 36, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.36f));
        StretchTop(combo.rectTransform, 380f, 60f, 160f);
        combo.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -3f);

        GameObject taskParentObject = new("TaskCardArea", typeof(RectTransform));
        taskParentObject.transform.SetParent(safe.transform, false);
        RectTransform taskParent = taskParentObject.GetComponent<RectTransform>();
        taskParent.anchorMin = new Vector2(0.5f, 0f);
        taskParent.anchorMax = new Vector2(0.5f, 0f);
        taskParent.pivot = new Vector2(0.5f, 0f);
        taskParent.anchoredPosition = new Vector2(0f, 0f);
        taskParent.sizeDelta = new Vector2(900f, 1700f);

        GameObject feedbackLayerObject = new("FeedbackLayer", typeof(RectTransform));
        feedbackLayerObject.transform.SetParent(safe.transform, false);
        RectTransform feedbackLayer = feedbackLayerObject.GetComponent<RectTransform>();
        Stretch(feedbackLayer, 0f, 0f);

        Image screenFlash = CreateImage("CorrectScreenFlash", feedbackLayerObject.transform, new Color(1f, 1f, 1f, 0f));
        Stretch(screenFlash.rectTransform, 0f, 0f);
        screenFlash.raycastTarget = false;

        Image edgeFlash = CreateImage("ComboEdgeFlash", feedbackLayerObject.transform, new Color(1f, 0.86f, 0.25f, 0f));
        Stretch(edgeFlash.rectTransform, 0f, 0f);
        edgeFlash.raycastTarget = false;

        FloatingScoreText floatingScorePrefab = CreateFloatingScoreTemplate(feedbackLayerObject.transform);
        ComboEffectController comboEffect = combo.gameObject.AddComponent<ComboEffectController>();
        SetSerialized(comboEffect, "comboText", combo);
        SetSerialized(comboEffect, "screenFlashImage", screenFlash);
        SetSerialized(comboEffect, "edgeFlashImage", edgeFlash);

        GameObject system = new("GameSystem");
        TaskDatabase database = system.AddComponent<TaskDatabase>();
        ScoreManager scoreManager = system.AddComponent<ScoreManager>();
        LifeManager lifeManager = system.AddComponent<LifeManager>();
        ComboManager comboManager = system.AddComponent<ComboManager>();
        TimerManager timerManager = system.AddComponent<TimerManager>();
        AudioSource audioSource = system.AddComponent<AudioSource>();
        AudioSource bgmAudioSource = system.AddComponent<AudioSource>();
        AudioManager audioManager = system.AddComponent<AudioManager>();
        IconDatabase iconDatabase = system.AddComponent<IconDatabase>();
        TaskManager taskManager = system.AddComponent<TaskManager>();
        TaskSpawner spawner = system.AddComponent<TaskSpawner>();
        UIManager ui = system.AddComponent<UIManager>();
        FeedbackManager feedbackManager = system.AddComponent<FeedbackManager>();
        GameManager gameManager = system.AddComponent<GameManager>();

        SetSerialized(database, "taskCsv", AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/tasks.csv"));
        ConfigureAudioManager(audioManager, audioSource, bgmAudioSource);
        ConfigureIconDatabase(iconDatabase);
        SetSerialized(taskManager, "cardParent", taskParent);
        SetSerialized(taskManager, "taskCardPrefab", cardPrefab.GetComponent<TaskCard>());
        SetSerialized(taskManager, "iconDatabase", iconDatabase);
        SetSerialized(spawner, "taskDatabase", database);
        SetSerialized(spawner, "taskManager", taskManager);
        SetSerialized(spawner, "timerManager", timerManager);
        SetSerialized(spawner, "audioManager", audioManager);
        SetSerializedInt(spawner, "maxVisibleBeforeSpawn", 4);
        SetSerialized(ui, "dateText", date);
        SetSerialized(ui, "remainingTimeText", remainingTime);
        SetSerialized(ui, "scoreText", score);
        SetSerialized(ui, "lifeText", life);
        SetSerialized(ui, "comboText", combo);
        SetSerialized(feedbackManager, "feedbackRoot", feedbackLayer);
        SetSerialized(feedbackManager, "floatingScorePrefab", floatingScorePrefab);
        SetSerialized(feedbackManager, "scoreGroup", scoreGroupRect);
        SetSerialized(feedbackManager, "comboEffectController", comboEffect);
        SetSerialized(feedbackManager, "audioManager", audioManager);
        SetSerialized(gameManager, "taskDatabase", database);
        SetSerialized(gameManager, "taskSpawner", spawner);
        SetSerialized(gameManager, "taskManager", taskManager);
        SetSerialized(gameManager, "scoreManager", scoreManager);
        SetSerialized(gameManager, "lifeManager", lifeManager);
        SetSerialized(gameManager, "comboManager", comboManager);
        SetSerialized(gameManager, "timerManager", timerManager);
        SetSerialized(gameManager, "uiManager", ui);
        SetSerialized(gameManager, "feedbackManager", feedbackManager);
        SetSerialized(gameManager, "audioManager", audioManager);
        SetSerializedBool(gameManager, "taskOverflowCheckEnabled", false);
        SetSerializedFloat(gameManager, "countdownStepDuration", 1.35f);
        SetSerializedFloat(gameManager, "countdownStartDuration", 0.95f);
        SetSerializedFloat(gameManager, "countdownInitialHoldSeconds", 0.45f);

        EditorSceneManager.SaveScene(scene, $"{SceneDir}/GameScene.unity");
    }

    private static FloatingScoreText CreateFloatingScoreTemplate(Transform parent)
    {
        Text text = CreateText("FloatingScoreTextTemplate", parent, "+100", 92, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.93f, 0.18f, 1f));
        text.raycastTarget = false;
        text.gameObject.AddComponent<CanvasGroup>();
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(0f, -5f);
        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.15f, 0.08f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);
        FloatingScoreText floatingScoreText = text.gameObject.AddComponent<FloatingScoreText>();
        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(360f, 120f);
        rect.anchoredPosition = Vector2.zero;
        text.gameObject.SetActive(false);
        SetSerialized(floatingScoreText, "scoreText", text);
        SetSerialized(floatingScoreText, "canvasGroup", text.GetComponent<CanvasGroup>());
        return floatingScoreText;
    }

    private static void CreateResultScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject safe = CreateBaseCanvas("ResultCanvas", new Color(0.08f, 0.08f, 0.1f));
        GameObject resultObject = new("ResultManager");
        AudioSource audioSource = resultObject.AddComponent<AudioSource>();
        AudioManager audioManager = resultObject.AddComponent<AudioManager>();
        ResultManager result = resultObject.AddComponent<ResultManager>();
        ConfigureAudioManager(audioManager, audioSource);

        Image resultPanel = CreateImage("ResultPanelBackground", safe.transform, new Color(0.94f, 0.99f, 1f, 0.9f));
        resultPanel.sprite = CreateRoundedCardSprite();
        resultPanel.type = Image.Type.Sliced;
        resultPanel.raycastTarget = false;
        resultPanel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        resultPanel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        resultPanel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        resultPanel.rectTransform.anchoredPosition = Vector2.zero;
        resultPanel.rectTransform.sizeDelta = new Vector2(850f, 1420f);
        Shadow resultPanelShadow = resultPanel.gameObject.AddComponent<Shadow>();
        resultPanelShadow.effectColor = new Color(0f, 0f, 0f, 0.28f);
        resultPanelShadow.effectDistance = new Vector2(0f, -8f);

        Text heading = CreateText("Heading", safe.transform, "通知斬り完了！", 56, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.04f, 0.08f, 0.12f));
        SetCenter(heading.rectTransform, new Vector2(0f, 500f), new Vector2(760f, 92f));
        Text finalScoreLabel = CreateText("FinalScoreLabelText", safe.transform, "最終スコア", 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.04f, 0.08f, 0.12f));
        SetCenter(finalScoreLabel.rectTransform, new Vector2(0f, 392f), new Vector2(740f, 50f));
        CreateResultValueBackground(safe.transform, "FinalScoreValueBackground", new Vector2(0f, 318f), new Vector2(650f, 116f));
        Text finalScore = CreateText("FinalScoreText", safe.transform, "0", 92, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        SetCenter(finalScore.rectTransform, new Vector2(0f, 318f), new Vector2(760f, 94f));
        ScoreColorUtility.ApplyReadableEffects(finalScore);
        Text rankTitleLabel = CreateText("RankTitleLabelText", safe.transform, "今回の称号", 32, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.04f, 0.08f, 0.12f));
        SetCenter(rankTitleLabel.rectTransform, new Vector2(0f, 202f), new Vector2(740f, 46f));
        CreateResultValueBackground(safe.transform, "RankTitleValueBackground", new Vector2(0f, 112f), new Vector2(720f, 140f));
        Text rankTitle = CreateText("RankTitleText", safe.transform, "普通のタスク人間", 48, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        SetCenter(rankTitle.rectTransform, new Vector2(0f, 142f), new Vector2(760f, 70f));
        ScoreColorUtility.ApplyReadableEffects(rankTitle);
        Text rankDescription = CreateText("RankDescriptionText", safe.transform, "今日もそれなりに通知をさばきました。", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.94f, 0.98f, 1f, 1f));
        SetCenter(rankDescription.rectTransform, new Vector2(0f, 74f), new Vector2(740f, 56f));
        ScoreColorUtility.ApplyReadableEffects(rankDescription);
        Text maxComboLabel = CreateText("MaxComboLabelText", safe.transform, "最大コンボ", 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.04f, 0.08f, 0.12f));
        SetCenter(maxComboLabel.rectTransform, new Vector2(0f, -20f), new Vector2(740f, 46f));
        CreateResultValueBackground(safe.transform, "MaxComboValueBackground", new Vector2(0f, -90f), new Vector2(670f, 98f));
        Text maxCombo = CreateText("MaxComboText", safe.transform, "0 COMBO", 72, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        SetCenter(maxCombo.rectTransform, new Vector2(0f, -90f), new Vector2(760f, 80f));
        ScoreColorUtility.ApplyReadableEffects(maxCombo);
        Button retry = CreateButton("RetryButton", safe.transform, "もう一度", new Vector2(0f, -470f), new Vector2(560f, 96f));
        UnityEventTools.AddPersistentListener(retry.onClick, result.OnRetryButton);
        Button title = CreateButton("BackToTitleButton", safe.transform, "タイトルへ戻る", new Vector2(0f, -586f), new Vector2(560f, 96f));
        UnityEventTools.AddPersistentListener(title.onClick, result.OnTitleButton);

        SetSerialized(result, "finalScoreText", finalScore);
        SetSerialized(result, "maxComboText", maxCombo);
        SetSerialized(result, "rankTitleText", rankTitle);
        SetSerialized(result, "rankDescriptionText", rankDescription);
        SetSerialized(result, "retryButton", retry);
        SetSerialized(result, "titleButton", title);
        SetSerialized(result, "audioManager", audioManager);
        SetSerialized(result, "roundedUiSprite", CreateRoundedCardSprite());

        EditorSceneManager.SaveScene(scene, $"{SceneDir}/ResultScene.unity");
    }

    private static GameObject CreateBaseCanvas(string name, Color backgroundColor, Sprite backgroundSprite = null)
    {
        CreateMainCamera(backgroundColor);

        GameObject canvasObject = new(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(AspectSafeRootFitter));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject backgroundObject = new("BackgroundImage", typeof(RectTransform), typeof(Image));
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        backgroundObject.transform.SetAsFirstSibling();
        Image background = backgroundObject.GetComponent<Image>();
        background.sprite = backgroundSprite;
        background.color = backgroundSprite != null ? Color.white : backgroundColor;
        background.preserveAspect = true;
        Stretch(background.rectTransform, 0f, 0f);

        GameObject safe = new("SafeArea", typeof(RectTransform), typeof(SafeAreaFitter));
        safe.transform.SetParent(canvasObject.transform, false);
        Stretch(safe.GetComponent<RectTransform>(), 0f, 0f);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        return safe;
    }

    private static Sprite LoadGameplayBackgroundSprite()
    {
        if (!System.IO.File.Exists(GameplayBackgroundPath))
        {
            Debug.LogWarning($"{GameplayBackgroundPath} not found. GameScene will use the fallback color background.");
            return null;
        }

        AssetDatabase.ImportAsset(GameplayBackgroundPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(GameplayBackgroundPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(GameplayBackgroundPath);
    }

    private static Sprite LoadUiSprite(string path, bool alphaTransparency)
    {
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"{path} not found. UI sprite will be skipped.");
            return null;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = alphaTransparency;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite LoadIconSprite(string iconId)
    {
        string path = $"Assets/Art/Icons/{iconId}.png";
        if (!System.IO.File.Exists(path))
        {
            return null;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 512;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite CreateDefaultIconSprite()
    {
        const int size = 128;
        const int radius = 24;
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color clear = new(1f, 1f, 1f, 0f);
        Color fill = new(1f, 1f, 1f, 0.92f);
        Color mark = new(0.62f, 0.7f, 0.78f, 1f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = IsInsideRoundedRect(x + 0.5f, y + 0.5f, size, size, radius);
                Color color = inside ? fill : clear;
                if (inside && (Mathf.Abs(x - y) < 5 || Mathf.Abs(x + y - size) < 5))
                {
                    color = mark;
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        System.IO.File.WriteAllBytes(DefaultIconPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(DefaultIconPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(DefaultIconPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 512;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(DefaultIconPath);
    }

    private static void ConfigureIconDatabase(IconDatabase iconDatabase)
    {
        Sprite defaultIcon = LoadIconSprite("icon_self") ?? CreateDefaultIconSprite();
        SerializedObject serializedObject = new(iconDatabase);
        serializedObject.FindProperty("defaultIcon").objectReferenceValue = defaultIcon;
        SerializedProperty entries = serializedObject.FindProperty("entries");
        string[] iconIds = { "icon_self", "icon_message", "icon_furima", "icon_sns", "icon_life", "icon_video" };
        entries.arraySize = iconIds.Length;
        for (int i = 0; i < iconIds.Length; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("iconId").stringValue = iconIds[i];
            entry.FindPropertyRelative("sprite").objectReferenceValue = LoadIconSprite(iconIds[i]) ?? defaultIcon;
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureAudioManager(AudioManager audioManager, AudioSource audioSource, AudioSource bgmAudioSource = null)
    {
        SetSerialized(audioManager, "audioSource", audioSource);
        SetSerialized(audioManager, "bgmAudioSource", bgmAudioSource);
        SetSerialized(audioManager, "notificationPopSe", LoadSeClip("se_notification_pop.mp3"));
        SetSerialized(audioManager, "tapCorrectSe", LoadSeClip("se_tap_correct.mp3"));
        SetSerialized(audioManager, "swipeCorrectSe", LoadSeClip("se_swipe_correct.mp3"));
        SetSerialized(audioManager, "missSe", LoadSeClip("se_miss.mp3"));
        SetSerialized(audioManager, "scorePopSe", LoadSeClip("se_score_pop.mp3"));
        SetSerialized(audioManager, "comboSe", LoadSeClip("se_combo.mp3"));
        SetSerialized(audioManager, "combo5Se", LoadSeClip("se_combo_5.mp3"));
        SetSerialized(audioManager, "combo10Se", LoadSeClip("se_combo_10.mp3"));
        SetSerialized(audioManager, "combo20Se", LoadSeClip("se_combo_20.mp3"));
        SetSerialized(audioManager, "gameOverSe", LoadSeClip("se_game_over.mp3"));
        SetSerialized(audioManager, "resultSe", LoadSeClip("se_result.mp3"));
        SetSerialized(audioManager, "countdownTickSe", LoadSeClip("se_countdown_tick.mp3"));
        SetSerialized(audioManager, "countdownStartSe", LoadSeClip("se_countdown_start.mp3"));
        SetSerialized(audioManager, "gameplayBgm", LoadBgmClip("bgm_gameplay.mp3"));
        SetSerializedFloat(audioManager, "notificationPopVolume", 0.6f);
        SetSerializedFloat(audioManager, "correctVolume", 0.7f);
        SetSerializedFloat(audioManager, "swipeVolume", 0.7f);
        SetSerializedFloat(audioManager, "missVolume", 0.7f);
        SetSerializedFloat(audioManager, "scorePopVolume", 0.5f);
        SetSerializedFloat(audioManager, "comboVolume", 0.8f);
        SetSerializedFloat(audioManager, "gameOverVolume", 0.8f);
        SetSerializedFloat(audioManager, "resultVolume", 0.8f);
        SetSerializedFloat(audioManager, "countdownTickVolume", 0.8f);
        SetSerializedFloat(audioManager, "countdownStartVolume", 0.9f);
        SetSerializedFloat(audioManager, "bgmVolume", 0.35f);
        SetSerializedFloat(audioManager, "seVolume", 0.8f);
        audioSource.playOnAwake = false;
        if (bgmAudioSource != null)
        {
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = 0.35f;
        }
    }

    private static AudioClip LoadSeClip(string fileName)
    {
        string path = $"Assets/Audio/SE/{fileName}";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"{path} not found. The corresponding SE will be skipped.");
            return null;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static AudioClip LoadBgmClip(string fileName)
    {
        string path = $"Assets/Audio/BGM/{fileName}";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"{path} not found. BGM playback will be skipped.");
            return null;
        }

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static void CreateMainCamera(Color backgroundColor)
    {
        GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, TextAnchor labelAnchor = TextAnchor.MiddleCenter, Sprite labelSprite = null)
    {
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.92f, 0.98f, 1f, 0.84f), anchoredPosition, size);
        Image image = buttonObject.GetComponent<Image>();
        image.sprite = CreateRoundedCardSprite();
        image.type = Image.Type.Sliced;
        Shadow shadow = buttonObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.22f);
        shadow.effectDistance = new Vector2(0f, -5f);
        Button button = buttonObject.AddComponent<Button>();
        Text text = CreateText("Label", buttonObject.transform, label, 34, FontStyle.Bold, labelAnchor, new Color(0.04f, 0.08f, 0.12f));
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        Stretch(text.rectTransform, 10f, 10f);
        if (labelSprite != null)
        {
            Image labelImage = CreateImage("LabelImage", buttonObject.transform, Color.white);
            labelImage.sprite = labelSprite;
            labelImage.preserveAspect = true;
            labelImage.raycastTarget = false;
            Stretch(labelImage.rectTransform, name == "SettingsButton" ? 18f : 24f, name == "SettingsButton" ? 14f : 18f);
        }

        return button;
    }

    private static Image CreateResultValueBackground(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject background = CreatePanel(name, parent, new Color(0.02f, 0.07f, 0.12f, 0.48f), anchoredPosition, size);
        Image image = background.GetComponent<Image>();
        image.sprite = CreateRoundedCardSprite();
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
        Shadow shadow = background.AddComponent<Shadow>();
        shadow.effectColor = new Color(1f, 1f, 1f, 0.18f);
        shadow.effectDistance = new Vector2(0f, 3f);
        return image;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panel = new(name, typeof(RectTransform), typeof(Image));
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

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string name, Transform parent, string value, int size, FontStyle style, TextAnchor anchor, Color color)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = PushNotificationGod.UI.UIJapaneseFont.Get();
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private static void Stretch(RectTransform rect, float horizontal, float vertical)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(horizontal, vertical);
        rect.offsetMax = new Vector2(-horizontal, -vertical);
    }

    private static void StretchTop(RectTransform rect, float top, float height, float horizontal)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(horizontal, -top - height);
        rect.offsetMax = new Vector2(-horizontal, -top);
    }

    private static void SetCenter(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void SetTopLeft(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void SetTopRight(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void SetSerialized(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Serialized property not found: {target.name}.{propertyName}");
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedFloat(Object target, string propertyName, float value)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedInt(Object target, string propertyName, int value)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.intValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSerializedBool(Object target, string propertyName, bool value)
    {
        SerializedObject serializedObject = new(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        property.boolValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene($"{SceneDir}/TitleScene.unity", true),
            new EditorBuildSettingsScene($"{SceneDir}/GameScene.unity", true),
            new EditorBuildSettingsScene($"{SceneDir}/ResultScene.unity", true)
        };

        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.WebGL.template = "PROJECT:FixedAspect";
    }
}
