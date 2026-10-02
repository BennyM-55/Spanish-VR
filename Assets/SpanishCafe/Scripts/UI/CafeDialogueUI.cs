using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpanishCafe
{
    // The conversation pop up. It builds itself in code
    
    public class CafeDialogueUI : MonoBehaviour
    {
        enum Mode { Closed, LevelSelect, Talking, Summary }

        static CafeDialogueUI instance;

        public static CafeDialogueUI Instance
        {
            get
            {
                if (instance == null) instance = FindAnyObjectByType<CafeDialogueUI>();
                if (instance == null) instance = new GameObject("Spanish Café UI").AddComponent<CafeDialogueUI>();
                return instance;
            }
        }

        public event Action Closed;
        public bool IsOpen => panel != null && panel.activeSelf;

        
        static readonly Color PanelColor = new Color(0.08f, 0.24f, 0.27f, 0.95f);
        static readonly Color Accent = new Color(0.86f, 0.47f, 0.31f);
        static readonly Color Soft = new Color(0.78f, 0.86f, 0.86f);
        const string Green = "#8FD694";
        const string Amber = "#F2C46D";
        const string Red = "#F28B82";
        const string Blue = "#9CC9F5";
        const string Grey = "#B7C9C9";

        readonly CafeDialogueManager manager = new CafeDialogueManager();
        readonly List<DialogueChoice> shownChoices = new List<DialogueChoice>();
        readonly HashSet<string> missedChoices = new HashSet<string>();
        readonly List<Action> numberActions = new List<Action>();

        GameObject panel, promptBox;
        TextMeshProUGUI headerText, staffText, subtitleText, feedbackText, orderText, promptText;
        TextMeshProUGUI hintLabel, subtitleLabel;
        Button hintButton, subtitleButton;
        Transform choicesRoot;
        UnityEngine.Object promptOwner;
        Mode mode = Mode.Closed;
        string staffName = "Lupita";
        AudioSource voice;
        bool subtitlesOn;
        string shuffledFor;
        string repeatShownFor;

        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            CafeContent.EnsureLoaded();
            Build();
            panel.SetActive(false);
            promptBox.SetActive(false);
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        void Update()
        {
            if (!IsOpen) return;

            if (CafeInput.ClosePressed)
            {
                Close();
                return;
            }

            int number = CafeInput.NumberPressed();
            if (number >= 0 && number < numberActions.Count)
            {
                numberActions[number]();
                return;
            }

            if (mode == Mode.Talking && CafeInput.HintPressed && hintButton.interactable) ShowHint();
            if (mode != Mode.LevelSelect && CafeInput.SubtitlesPressed) ToggleSubtitles();
            if (mode == Mode.Talking && CafeInput.ContinuePressed && manager.StudentTurn == null) ContinueTalking();
        }

        //  CafeConversationStarter

        public void ShowPrompt(UnityEngine.Object owner, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (promptOwner == owner)
                {
                    promptOwner = null;
                    promptBox.SetActive(false);
                }
                return;
            }

            promptOwner = owner;
            promptText.text = text;
            promptBox.SetActive(!IsOpen);
        }

        public void Open(string workerName, AudioSource voiceSource)
        {
            staffName = string.IsNullOrEmpty(workerName) ? "Lupita" : workerName;
            voice = voiceSource;
            EnsureEventSystem();
            promptBox.SetActive(false);
            panel.SetActive(true);
            ShowLevelSelect();
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (voice != null) voice.Stop();
            panel.SetActive(false);
            mode = Mode.Closed;
            Closed?.Invoke();
        }

        // Screens

        void ShowLevelSelect()
        {
            mode = Mode.LevelSelect;
            var levels = CafeContent.Levels;

            headerText.text = CafeContent.Menu?.cafeName ?? "Café La Esquina";
            SetStaffLine($"<b>{staffName}:</b> ¡Hola! ¿Qué nivel quieres practicar hoy?",
                         "Hi! Which level do you want to practice today?", true);

            ClearChoices();
            for (int i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                string title = CafeContent.GetScenario(level)?.titleEs ?? "";
                AddChoiceButton(i + 1, $"<b>{level.level}</b> · {level.nameEs} <color={Grey}>({level.nameEn})</color>", title, () => StartLevel(level));
            }

            feedbackText.text = levels.Length == 0
                ? Colored(Red, "No levels found. Check Resources/CafeContent/Levels/levels.json.")
                : "";
            orderText.text = $"Usa el ratón o las teclas 1–{Mathf.Max(1, levels.Length)} · Use the mouse or number keys";
            hintButton.gameObject.SetActive(false);
            subtitleButton.gameObject.SetActive(false);
        }

        void StartLevel(LevelInfo level)
        {
            if (!manager.Begin(level))
            {
                feedbackText.text = Colored(Red, "Couldn't load this level. Check the Console for details.");
                return;
            }

            mode = Mode.Talking;
            subtitlesOn = level.showSubtitles;
            shuffledFor = null;
            repeatShownFor = null;
            missedChoices.Clear();
            hintButton.gameObject.SetActive(true);
            subtitleButton.gameObject.SetActive(true);

            ShowTurn("");
            PlayLine(manager.StaffTurn, false);
        }

        void ShowTurn(string feedback)
        {
            if (manager.IsFinished)
            {
                ShowSummary(feedback);
                return;
            }

            var level = manager.Level;
            headerText.text = $"{staffName} · {level.level} {level.nameEs} · {manager.Scenario.titleEs}";

            var staff = manager.StaffTurn;
            if (staff != null)
            {
                // After "¿Puede repetir?", she says the simpler version of the same line.
                bool useRepeat = repeatShownFor == staff.id && !string.IsNullOrEmpty(staff.repeatEs);
                string spanish = useRepeat ? staff.repeatEs : staff.es;
                string english = CafeContent.English(useRepeat ? staff.id + ".repeat" : staff.id);
                SetStaffLine($"<b>{staffName}:</b> {spanish}", english, subtitlesOn);
            }

            BuildChoices(level);
            feedbackText.text = feedback ?? "";
            orderText.text = OrderSummary();
            RefreshButtons();
        }

        void ShowSummary(string lastFeedback)
        {
            mode = Mode.Summary;
            var level = manager.Level;
            var score = manager.Score;
            var staff = manager.StaffTurn;

            headerText.text = $"¡Terminaste! · {level.level} {level.nameEs} · {manager.Scenario.titleEs}";
            if (staff != null)
                SetStaffLine($"<b>{staffName}:</b> {staff.es}", CafeContent.English(staff.id), subtitlesOn);

            var text = new StringBuilder();
            if (!string.IsNullOrEmpty(lastFeedback)) text.Append(lastFeedback).Append("\n\n");
            text.Append($"<b>Puntaje: {score.Total}/8</b> <size=85%><color={Grey}>(Score)</color></size>\n");
            foreach (var row in CafeContent.Rubric)
            {
                int points = score.ForRubric(row.id);
                string meaning = points == 2 ? row.level2 : points == 1 ? row.level1 : row.level0;
                text.Append($"<size=85%>{row.es} <color={Grey}>/ {row.en}</color>: <b>{points}</b> · {meaning}</size>\n");
            }
            text.Append($"\n<b>Tu turno, sin guion:</b> {level.challengeEs}\n");
            text.Append($"<size=85%><color={Grey}><i>Try it without the script: {level.challengeEn}</i></color></size>");
            feedbackText.text = text.ToString();
            orderText.text = OrderSummary();

            ClearChoices();
            AddChoiceButton(1, "Jugar otra vez", "Play again", () => StartLevel(level));
            AddChoiceButton(2, "Elegir otro nivel", "Choose another level", ShowLevelSelect);
            AddChoiceButton(3, "Cerrar", "Close", Close);
            hintButton.gameObject.SetActive(false);
            subtitleButton.gameObject.SetActive(true);
            RefreshButtons();
        }

        // Choices 

        void BuildChoices(LevelInfo level)
        {
            ClearChoices();

            var turn = manager.StudentTurn;
            if (turn == null)
            {
                AddChoiceButton(1, "Continuar", "Continue", ContinueTalking);
                return;
            }

            // Shuffle once per question, so the right answer isn't always first
            if (shuffledFor != turn.id)
            {
                ShuffleChoices(turn);
                shuffledFor = turn.id;
                missedChoices.Clear();
            }

            for (int i = 0; i < shownChoices.Count; i++)
            {
                var choice = shownChoices[i];
                string english = level.showChoiceTranslations ? CafeContent.English(choice.id) : null;
                var button = AddChoiceButton(i + 1, choice.es, english, () => Pick(choice));

                // Gray out answers they already tried, so they don't click them twice?
                if (missedChoices.Contains(choice.id))
                {
                    button.interactable = false;
                    button.GetComponentInChildren<TextMeshProUGUI>().alpha = 0.45f;
                }
            }
        }

        void ShuffleChoices(DialogueTurn turn)
        {
            shownChoices.Clear();
            var repeats = new List<DialogueChoice>();
            if (turn.choices != null)
            {
                foreach (var choice in turn.choices)
                {
                    if (choice.result == "repeat") repeats.Add(choice);
                    else shownChoices.Add(choice);
                }
            }

            for (int i = shownChoices.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = shownChoices[i];
                shownChoices[i] = shownChoices[j];
                shownChoices[j] = temp;
            }

            // ¿Puede repetir? always goes last so students know where to find it translation in english can you repeat
            shownChoices.AddRange(repeats);
        }

        void Pick(DialogueChoice choice)
        {
            var staffBefore = manager.StaffTurn;
            var result = manager.Choose(choice);

            string feedback;
            switch (result)
            {
                case ChoiceResult.Correct:
                    feedback = Colored(Green, Praise());
                    break;
                case ChoiceResult.Awkward:
                    feedback = Colored(Amber, FeedbackText(choice.id, "general.awkward"));
                    break;
                case ChoiceResult.Wrong:
                    missedChoices.Add(choice.id);
                    feedback = Colored(Red, FeedbackText(choice.id, "general.wrong"));
                    break;
                default:
                    repeatShownFor = staffBefore?.id;
                    feedback = Colored(Blue, FeedbackText(null, "general.repeat"));
                    break;
            }

            ShowTurn(feedback);

            if (result == ChoiceResult.Repeat) PlayLine(manager.StaffTurn, true);
            else if (manager.StaffTurn != staffBefore) PlayLine(manager.StaffTurn, false);
        }

        void ContinueTalking()
        {
            var staffBefore = manager.StaffTurn;
            manager.Continue();
            ShowTurn("");
            if (manager.StaffTurn != staffBefore) PlayLine(manager.StaffTurn, false);
        }

        void ShowHint()
        {
            var turn = manager.StudentTurn;
            if (turn == null) return;

            if (!manager.UseHint())
            {
                feedbackText.text = Colored(Blue, FeedbackText(null, "general.noHints"));
                RefreshButtons();
                return;
            }

            var hint = CafeContent.Feedback(turn.id + ".hint");
            feedbackText.text = hint == null
                ? Colored(Blue, "Pista: lee otra vez lo que dijo " + staffName + ".")
                : Colored(Blue, $"<b>Pista:</b> {hint.es}\n<size=85%>Hint: {hint.en}</size>");
            RefreshButtons();
        }

        void ToggleSubtitles()
        {
            subtitlesOn = !subtitlesOn;
            subtitleText.gameObject.SetActive(subtitlesOn && !string.IsNullOrEmpty(subtitleText.text));
            RefreshButtons();
        }

        void RefreshButtons()
        {
            int left = manager.HintsLeft;
            bool unlimited = manager.Level != null && manager.Level.maxHints >= 99;
            hintLabel.text = unlimited ? "Pista (H)" : $"Pista (H) · {left}";
            hintButton.interactable = left > 0 && manager.StudentTurn != null;
            hintLabel.alpha = hintButton.interactable ? 1f : 0.45f;
            subtitleLabel.text = subtitlesOn ? "Subtítulos (T): sí" : "Subtítulos (T): no";
        }


        void SetStaffLine(string spanish, string english, bool showEnglish)
        {
            staffText.text = spanish;
            subtitleText.text = english ?? "";
            subtitleText.gameObject.SetActive(showEnglish && !string.IsNullOrEmpty(english));
        }

        string OrderSummary()
        {
            return $"Pedido: {manager.Order.Describe()}   ·   Total: ${manager.Order.Total} {CafeContent.Currency}";
        }

        static string Praise()
        {
            var entry = CafeContent.Feedback("general.correct." + UnityEngine.Random.Range(1, 4));
            return entry == null ? "¡Muy bien!" : $"{entry.es} <size=85%>{entry.en}</size>";
        }

        static string FeedbackText(string id, string fallbackId)
        {
            var entry = CafeContent.Feedback(id) ?? CafeContent.Feedback(fallbackId);
            return entry == null ? "" : $"{entry.es}\n<size=85%>{entry.en}</size>";
        }

        static string Colored(string hex, string text) => $"<color={hex}>{text}</color>";

        void PlayLine(DialogueTurn turn, bool repeat)
        {
            if (voice == null || turn == null) return;

            
            string fileName = (repeat ? turn.id + "_repeat" : turn.id).Replace('.', '_');
            var clip = Resources.Load<AudioClip>("CafeAudio/" + fileName);
            if (clip == null) return;

            voice.Stop();
            voice.clip = clip;
            voice.Play();
        }



        void Build()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            promptBox = MakePanel("Prompt", canvasObject.transform, new Color(0f, 0f, 0f, 0.65f), new RectOffset(28, 28, 14, 14));
            PlaceAtBottom(promptBox, 70f, 0f);
            promptText = MakeText(promptBox.transform, 28, FontStyles.Bold, Color.white);
            promptText.alignment = TextAlignmentOptions.Center;

            panel = MakePanel("Conversation", canvasObject.transform, PanelColor, new RectOffset(36, 36, 26, 26));
            PlaceAtBottom(panel, 30f, 1180f);

            headerText = MakeText(panel.transform, 22, FontStyles.Bold, Accent);
            staffText = MakeText(panel.transform, 34, FontStyles.Normal, Color.white);
            subtitleText = MakeText(panel.transform, 24, FontStyles.Italic, Soft);

            var choices = new GameObject("Choices", typeof(RectTransform), typeof(VerticalLayoutGroup));
            choices.transform.SetParent(panel.transform, false);
            var choiceLayout = choices.GetComponent<VerticalLayoutGroup>();
            choiceLayout.spacing = 8;
            choiceLayout.childControlWidth = true;
            choiceLayout.childControlHeight = true;
            choiceLayout.childForceExpandWidth = true;
            choiceLayout.childForceExpandHeight = false;
            choicesRoot = choices.transform;

            feedbackText = MakeText(panel.transform, 24, FontStyles.Normal, Color.white);

            var footer = new GameObject("Footer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            footer.transform.SetParent(panel.transform, false);
            var footerLayout = footer.GetComponent<HorizontalLayoutGroup>();
            footerLayout.spacing = 10;
            footerLayout.childAlignment = TextAnchor.MiddleLeft;
            footerLayout.childControlWidth = true;
            footerLayout.childControlHeight = true;
            footerLayout.childForceExpandWidth = false;
            footerLayout.childForceExpandHeight = false;

            orderText = MakeText(footer.transform, 22, FontStyles.Normal, Soft);
            orderText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            hintButton = MakeButton(footer.transform, out hintLabel, ShowHint, 200f);
            subtitleButton = MakeButton(footer.transform, out subtitleLabel, ToggleSubtitles, 250f);
            MakeButton(footer.transform, out var closeLabel, Close, 170f);
            closeLabel.text = "Cerrar (Esc)";
            hintLabel.alignment = TextAlignmentOptions.Center;
            subtitleLabel.alignment = TextAlignmentOptions.Center;
            closeLabel.alignment = TextAlignmentOptions.Center;
        }

        Button AddChoiceButton(int number, string spanish, string english, Action onPick)
        {
            var button = MakeButton(choicesRoot, out var label, onPick);
            label.text = string.IsNullOrEmpty(english)
                ? $"<b>{number}</b>   {spanish}"
                : $"<b>{number}</b>   {spanish}\n<size=80%><color={Grey}><i>{english}</i></color></size>";

            numberActions.Add(() =>
            {
                if (button != null && button.interactable) onPick();
            });
            return button;
        }

        void ClearChoices()
        {
            numberActions.Clear();
            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
            {
                var child = choicesRoot.GetChild(i).gameObject;
                child.SetActive(false); // hide it now so the layout ignores it
                Destroy(child);
            }
        }

        static GameObject MakePanel(string name, Transform parent, Color color, RectOffset padding)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return go;
        }

        // A width of 0 means "as wide as the text needs".
        static void PlaceAtBottom(GameObject go, float bottomOffset, float width)
        {
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, bottomOffset);

            var fitter = go.GetComponent<ContentSizeFitter>();
            if (width > 0f)
            {
                rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else
            {
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        static TextMeshProUGUI MakeText(Transform parent, float size, FontStyles style, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.richText = true;
            text.raycastTarget = false; // let clicks go through to the buttons
            return text;
        }

        static Button MakeButton(Transform parent, out TextMeshProUGUI label, Action onClick, float width = 0f)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = Color.white;

            // The image is white, and these tints make it see-through until you hover.
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.08f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.22f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.32f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0.08f);
            colors.disabledColor = new Color(1f, 1f, 1f, 0.03f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.onClick.AddListener(() => onClick());

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 12, 12);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var element = go.GetComponent<LayoutElement>();
            element.minHeight = 52f;
            if (width > 0f)
            {
                // Fixed width so a long order line can't squish the buttons.
                element.minWidth = width;
                element.preferredWidth = width;
            }

            label = MakeText(go.transform, 26, FontStyles.Normal, Color.white);
            return button;
        }

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null || FindAnyObjectByType<EventSystem>() != null) return;

            // Clicking buttons needs an EventSystem, and the cafe scene doesn't have one yet
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
