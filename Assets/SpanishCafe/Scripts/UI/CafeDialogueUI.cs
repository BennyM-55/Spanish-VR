using System;
using System.Collections;
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

        // Auto picks World when a headset is on, and Screen on a normal computer
        public enum PanelMode { Auto, Screen, World }

        // 0.8 mm per pixel, so the panel is a little under 1 meter wide in VR
        const float WorldScale = 0.0008f;
        const float PanelDistance = 1.5f;

        // If Lupita is further than this off to the side, the panel opens where you're looking instead
        const float MaxSideAngle = 35f;

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

        Canvas canvas;
        bool inWorld;
        GameObject panel, promptBox;
        TextMeshProUGUI headerText, staffText, subtitleText, feedbackText, orderText, promptText;
        TextMeshProUGUI hintLabel, subtitleLabel;
        Button hintButton, subtitleButton;
        Transform choicesRoot;
        UnityEngine.Object promptOwner;
        Mode mode = Mode.Closed;
        string staffName = "Lupita";
        AudioSource voice;
        StudentSpeech studentSpeech;
        Coroutine talkTurn;
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
            studentSpeech = StudentSpeech.Create(transform);
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
            if (inWorld) FaceHead();

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

            // In VR the prompt floats over Lupita's head instead of the bottom of the screen
            if (inWorld && !IsOpen && owner is Component worker) PlaceAbove(worker.transform);
        }

        public void SetPanelMode(PanelMode wanted)
        {
            if (IsOpen) return; // don't flip it while someone is reading
            UseWorld(wanted == PanelMode.World || (wanted == PanelMode.Auto && UnityEngine.XR.XRSettings.isDeviceActive));
        }

        public void Open(string workerName, AudioSource voiceSource, Transform worker = null, Transform student = null)
        {
            staffName = string.IsNullOrEmpty(workerName) ? "Lupita" : workerName;
            voice = voiceSource;
            studentSpeech.Follow(student);
            EnsureEventSystem();
            promptBox.SetActive(false);
            panel.SetActive(true);
            if (inWorld) PlaceInFront(worker);
            ShowLevelSelect();
        }

        public void Close()
        {
            if (!IsOpen) return;
            StopTalkTurn();
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

                // Green "Hecho" and best score next to levels they already finished
                // (the default font doesn't have a check mark, it shows up as a box)
                string done = CafeProgress.Finished(level.level)
                    ? $"   <color={Green}><b>Hecho</b> · {Mathf.Max(0, CafeProgress.Best(level.level))}/8</color>"
                    : "";
                AddChoiceButton(i + 1, $"<b>{level.level}</b> · {level.nameEs} <color={Grey}>({level.nameEn})</color>{done}", title, () => StartLevel(level));
            }

            feedbackText.text = levels.Length == 0
                ? Colored(Red, "No levels found. Check Resources/CafeContent/Levels/levels.json.")
                : "";
            int count = Mathf.Max(1, levels.Length);
            orderText.text = inWorld
                ? $"Apunta y aprieta el gatillo, o usa 1–{count} · Point and pull the trigger, or keys 1–{count}"
                : $"Usa el ratón o las teclas 1–{count} · Use the mouse or number keys";
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

            StopTalkTurn();
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
            // Only save once, the first time this screen shows up
            bool firstTime = mode != Mode.Summary;
            mode = Mode.Summary;
            var level = manager.Level;
            var score = manager.Score;
            var staff = manager.StaffTurn;
            int bestBefore = CafeProgress.Best(level.level);
            bool newBest = firstTime && CafeProgress.Record(level.level, score.Total);

            headerText.text = $"¡Terminaste! · {level.level} {level.nameEs} · {manager.Scenario.titleEs}";
            if (staff != null)
                SetStaffLine($"<b>{staffName}:</b> {staff.es}", CafeContent.English(staff.id), subtitlesOn);

            var text = new StringBuilder();
            if (!string.IsNullOrEmpty(lastFeedback)) text.Append(lastFeedback).Append("\n\n");
            text.Append($"<b>Puntaje: {score.Total}/8</b> <size=85%><color={Grey}>(Score)</color></size>");
            if (newBest && bestBefore >= 0)
                text.Append($"   <color={Green}><b>¡Nuevo récord!</b></color> <size=85%><color={Grey}>(New best, before: {bestBefore}/8)</color></size>");
            else if (!newBest && bestBefore >= 0)
                text.Append($"   <size=85%><color={Grey}>Tu mejor: {bestBefore}/8 (Your best)</color></size>");
            text.Append("\n");
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

            // The student says their line first, then Lupita answers when they're done
            StopTalkTurn();
            talkTurn = StartCoroutine(StudentThenStaff(choice, result, staffBefore));
        }

        IEnumerator StudentThenStaff(DialogueChoice choice, ChoiceResult result, DialogueTurn staffBefore)
        {
            var clip = Resources.Load<AudioClip>("CafeAudio/" + choice.id.Replace('.', '_'));
            float wait = studentSpeech.Say(choice.es, clip);
            yield return new WaitForSeconds(wait + 0.3f); // a tiny pause like a real conversation

            if (result == ChoiceResult.Repeat) PlayLine(manager.StaffTurn, true);
            else if (manager.StaffTurn != staffBefore) PlayLine(manager.StaffTurn, false);
            talkTurn = null;
        }

        void StopTalkTurn()
        {
            if (talkTurn != null) StopCoroutine(talkTurn);
            talkTurn = null;
            studentSpeech.Hide();
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



        // VR panel

        void UseWorld(bool world)
        {
            if (inWorld == world) return;
            inWorld = world;

            var rect = (RectTransform)canvas.transform;
            if (!world)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                rect.localScale = Vector3.one;
                return;
            }

            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main; // the mouse still needs a camera to click on it
            rect.sizeDelta = new Vector2(1300f, 1000f);
            rect.pivot = new Vector2(0.5f, 0f); // bottom middle, so the panel grows up from the spot we pick
            rect.localScale = Vector3.one * WorldScale;

            // This lets the controller ray point at the buttons and click them
            if (canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>() == null)
                canvas.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
        }

        // Puts the panel between the player and Lupita, a little under eye level
        void PlaceInFront(Transform worker)
        {
            var head = Camera.main != null ? Camera.main.transform : null;
            if (head == null) return;

            var looking = new Vector3(head.forward.x, 0f, head.forward.z);
            if (looking.sqrMagnitude < 0.01f) looking = head.up; // looking straight down, use the top of the head instead
            looking.y = 0f;
            var toWorker = worker != null ? worker.position - head.position : looking;
            toWorker.y = 0f;

            // Only aim at Lupita if she's kind of in front of you, so the panel never opens half off screen
            bool useWorker = worker != null && toWorker.sqrMagnitude > 0.01f && Vector3.Angle(looking, toWorker) <= MaxSideAngle;
            float distance = useWorker ? Mathf.Clamp(toWorker.magnitude - 0.8f, 0.8f, PanelDistance) : PanelDistance;
            if (!useWorker) toWorker = looking;
            var spot = head.position + toWorker.normalized * distance;
            spot.y = head.position.y - 0.55f;
            canvas.transform.position = spot;
            FaceHead(true);
        }

        void PlaceAbove(Transform worker)
        {
            canvas.transform.position = worker.position + Vector3.up * 2.1f;
            FaceHead(true);
        }

        // Turns the panel toward the player but keeps it standing straight up
        void FaceHead(bool snap = false)
        {
            var head = Camera.main != null ? Camera.main.transform : null;
            if (head == null) return;

            var away = canvas.transform.position - head.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.01f) return;

            var target = Quaternion.LookRotation(away);
            canvas.transform.rotation = snap ? target : Quaternion.Slerp(canvas.transform.rotation, target, 4f * Time.deltaTime);
        }

        void Build()
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
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
            // The XR one handles the controller ray and the mouse, so it works both ways
            go.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
