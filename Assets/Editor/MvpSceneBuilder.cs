using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VocabCardGame.Core;
using VocabCardGame.UI;

namespace VocabCardGame.Editor
{
    /// <summary>
    /// MVP 場景自動建立器
    /// </summary>
    public static class MvpSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/MvpScene.unity";
        private const string PrefabFolder = "Assets/Prefabs/MVP";

        [MenuItem("VocabCardGame/Build MVP Scene")]
        public static void BuildMvpScene()
        {
            EnsureFolders();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateGameBootstrap();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var (cardPrefab, enemyPrefab) = CreatePrefabs();

            var combatUI = CreateCombatUI(canvas.transform, cardPrefab, enemyPrefab);
            var quizUI = CreateQuizUI(canvas.transform);

            // 連結 CombatUI -> QuizUI
            var combatController = combatUI.GetComponent<CombatUIController>();
            var quizController = quizUI.GetComponent<QuizUIController>();
            if (combatController != null)
            {
                combatController.quizUI = quizController;
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[MVP] Scene created: " + ScenePath);
        }

        // 給 CLI -executeMethod 用
        public static void BuildMvpSceneFromCli()
        {
            BuildMvpScene();
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "MVP");
            }
        }

        private static void CreateGameBootstrap()
        {
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private static void CreateEventSystem()
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas()
        {
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            // 以手機直向為基準
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static (GameObject cardPrefab, GameObject enemyPrefab) CreatePrefabs()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Card Button Prefab
            var cardGo = new GameObject("CardButton", typeof(RectTransform));
            var cardImage = cardGo.AddComponent<Image>();
            cardImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            cardGo.AddComponent<Button>();
            var cardLayout = cardGo.AddComponent<LayoutElement>();
            cardLayout.preferredWidth = 140;
            cardLayout.preferredHeight = 180;
            var cardTextGo = new GameObject("Text", typeof(RectTransform));
            cardTextGo.transform.SetParent(cardGo.transform);
            var cardText = cardTextGo.AddComponent<Text>();
            cardText.font = font;
            cardText.text = "Card";
            cardText.alignment = TextAnchor.MiddleCenter;
            cardText.color = Color.black;
            StretchToParent(cardTextGo.GetComponent<RectTransform>());

            var cardPrefabPath = Path.Combine(PrefabFolder, "CardButton.prefab");
            var cardPrefab = PrefabUtility.SaveAsPrefabAsset(cardGo, cardPrefabPath);
            Object.DestroyImmediate(cardGo);

            // Enemy Item Prefab
            var enemyGo = new GameObject("EnemyItem", typeof(RectTransform));
            var enemyImage = enemyGo.AddComponent<Image>();
            enemyImage.color = new Color(0.95f, 0.95f, 1f, 1f);
            var enemyButton = enemyGo.AddComponent<Button>();
            var enemyLayout = enemyGo.AddComponent<LayoutElement>();
            enemyLayout.preferredWidth = 320;
            enemyLayout.preferredHeight = 110;

            var selectionGo = new GameObject("Selection", typeof(RectTransform));
            selectionGo.transform.SetParent(enemyGo.transform);
            var selectionImage = selectionGo.AddComponent<Image>();
            selectionImage.color = new Color(1f, 0.8f, 0.2f, 0.5f);
            StretchToParent(selectionGo.GetComponent<RectTransform>());

            var nameGo = CreateLabel(enemyGo.transform, font, "Name", new Vector2(0, 30));
            var hpGo = CreateLabel(enemyGo.transform, font, "HP", new Vector2(0, 0));
            var intentGo = CreateLabel(enemyGo.transform, font, "Intent", new Vector2(0, -30));

            var view = enemyGo.AddComponent<EnemyView>();
            view.nameText = nameGo.GetComponent<Text>();
            view.hpText = hpGo.GetComponent<Text>();
            view.intentText = intentGo.GetComponent<Text>();
            view.selectionFrame = selectionImage;
            view.selectButton = enemyButton;

            var enemyPrefabPath = Path.Combine(PrefabFolder, "EnemyItem.prefab");
            var enemyPrefab = PrefabUtility.SaveAsPrefabAsset(enemyGo, enemyPrefabPath);
            Object.DestroyImmediate(enemyGo);

            return (cardPrefab, enemyPrefab);
        }

        private static GameObject CreateCombatUI(Transform parent, GameObject cardPrefab, GameObject enemyPrefab)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = CreateUiObject("CombatUI", parent);

            var controller = root.AddComponent<CombatUIController>();

            StretchToParent(root.GetComponent<RectTransform>());

            // Top HUD (Slay the Spire-like)
            var topBar = CreateUiObject("TopBar", root.transform);
            var topLayout = topBar.AddComponent<HorizontalLayoutGroup>();
            topLayout.childAlignment = TextAnchor.MiddleCenter;
            topLayout.spacing = 12;
            topLayout.childForceExpandHeight = false;
            topLayout.childForceExpandWidth = false;
            SetRect(topBar.GetComponent<RectTransform>(), new Vector2(0.05f, 0.92f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);

            controller.playerHpText = CreateInlineText(topBar.transform, font, "HP");
            controller.playerBlockText = CreateInlineText(topBar.transform, font, "Block");
            controller.energyText = CreateInlineText(topBar.transform, font, "Energy");
            controller.turnText = CreateInlineText(topBar.transform, font, "Turn");
            controller.stateText = CreateInlineText(topBar.transform, font, "State");

            // Enemy area center
            var enemyArea = CreateUiObject("EnemyArea", root.transform);
            var enemyLayout = enemyArea.AddComponent<VerticalLayoutGroup>();
            enemyLayout.childAlignment = TextAnchor.UpperCenter;
            enemyLayout.spacing = 8;
            enemyLayout.childForceExpandWidth = false;
            SetRect(enemyArea.GetComponent<RectTransform>(), new Vector2(0.10f, 0.38f), new Vector2(0.90f, 0.86f), Vector2.zero, Vector2.zero);

            controller.enemyContainer = enemyArea.transform;
            controller.enemyItemPrefab = enemyPrefab;

            // Hand row bottom
            var handArea = CreateUiObject("HandArea", root.transform);
            var handLayout = handArea.AddComponent<HorizontalLayoutGroup>();
            handLayout.childAlignment = TextAnchor.MiddleCenter;
            handLayout.spacing = 12;
            handLayout.childForceExpandHeight = false;
            handLayout.childForceExpandWidth = false;
            SetRect(handArea.GetComponent<RectTransform>(), new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.28f), Vector2.zero, Vector2.zero);

            controller.handContainer = handArea.transform;
            controller.cardButtonPrefab = cardPrefab;

            // End turn button right-middle
            var endTurnGo = CreateButton(root.transform, font, "End Turn");
            var endLayout = endTurnGo.AddComponent<LayoutElement>();
            endLayout.preferredWidth = 240;
            endLayout.preferredHeight = 80;
            SetRect(endTurnGo.GetComponent<RectTransform>(), new Vector2(0.78f, 0.34f), new Vector2(0.98f, 0.44f), Vector2.zero, Vector2.zero);
            controller.endTurnButton = endTurnGo.GetComponent<Button>();

            return root;
        }

        private static GameObject CreateQuizUI(Transform parent)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var root = CreateUiObject("QuizUI", parent);

            var canvasGroup = root.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var panel = CreateUiObject("Panel", root.transform);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.7f);
            StretchToParent(panel.GetComponent<RectTransform>());

            var content = CreateUiObject("Content", panel.transform);
            SetRect(content.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 360));

            var modeText = CreateInlineText(content.transform, font, "Mode");
            var questionText = CreateInlineText(content.transform, font, "Question");
            var timerText = CreateInlineText(content.transform, font, "Time");

            SetRect(modeText.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(300, 30));
            SetRect(questionText.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(500, 40));
            SetRect(timerText.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(200, 30));

            var audioButtonGo = CreateButton(content.transform, font, "Play Audio");
            SetRect(audioButtonGo.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(200, 40));

            var options = CreateUiObject("Options", content.transform);
            var optionsLayout = options.AddComponent<VerticalLayoutGroup>();
            optionsLayout.childAlignment = TextAnchor.UpperCenter;
            optionsLayout.spacing = 6;
            SetRect(options.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(400, 160));

            var optionButtons = new Button[4];
            var optionTexts = new Text[4];
            for (int i = 0; i < 4; i++)
            {
                var btn = CreateButton(options.transform, font, "Option");
                optionButtons[i] = btn.GetComponent<Button>();
                optionTexts[i] = btn.GetComponentInChildren<Text>();
            }

            var spelling = CreateUiObject("Spelling", content.transform);
            SetRect(spelling.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(400, 80));

            var inputGo = CreateUiObject("Input", spelling.transform);
            var inputImage = inputGo.AddComponent<Image>();
            inputImage.color = Color.white;
            var input = inputGo.AddComponent<InputField>();
            var inputTextGo = CreateUiObject("Text", inputGo.transform);
            var inputText = inputTextGo.AddComponent<Text>();
            inputText.font = font;
            inputText.color = Color.black;
            inputText.alignment = TextAnchor.MiddleLeft;
            input.textComponent = inputText;
            StretchToParent(inputTextGo.GetComponent<RectTransform>());
            SetRect(inputGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-80, 0), new Vector2(240, 36));

            var submitGo = CreateButton(spelling.transform, font, "Submit");
            SetRect(submitGo.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(120, 0), new Vector2(120, 36));

            var quiz = root.AddComponent<QuizUIController>();
            quiz.root = canvasGroup;
            quiz.modeText = modeText.GetComponent<Text>();
            quiz.questionText = questionText.GetComponent<Text>();
            quiz.timerText = timerText.GetComponent<Text>();
            quiz.audioButton = audioButtonGo.GetComponent<Button>();
            quiz.optionsContainer = options;
            quiz.optionButtons = optionButtons;
            quiz.optionTexts = optionTexts;
            quiz.spellingContainer = spelling;
            quiz.spellingInput = input;
            quiz.spellingSubmitButton = submitGo.GetComponent<Button>();

            return root;
        }

        private static Text CreateInlineText(Transform parent, Font font, string text)
        {
            var go = CreateUiObject(text + "Text", parent);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            var rt = t.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(120, 30);
            return t;
        }

        private static GameObject CreateLabel(Transform parent, Font font, string text, Vector2 offset)
        {
            var go = CreateUiObject(text, parent);
            var t = go.AddComponent<Text>();
            t.font = font;
            t.text = text;
            t.color = Color.black;
            t.alignment = TextAnchor.MiddleCenter;
            SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), offset, new Vector2(180, 24));
            return go;
        }

        private static GameObject CreateButton(Transform parent, Font font, string text)
        {
            var go = CreateUiObject(text + "Button", parent);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            go.AddComponent<Button>();

            var labelGo = CreateUiObject("Text", go.transform);
            var label = labelGo.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.color = Color.black;
            label.alignment = TextAnchor.MiddleCenter;
            StretchToParent(labelGo.GetComponent<RectTransform>());

            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(200, 36);
            return go;
        }

        private static void StretchToParent(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent);
            return go;
        }

        private static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }
    }
}
