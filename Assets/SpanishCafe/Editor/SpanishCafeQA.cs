using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpanishCafe.EditorTools
{
    // QA bot that plays every unidad by itself and checks nothing is broken
    // Run it from the menu, or from the terminal with -executeMethod SpanishCafe.EditorTools.SpanishCafeQA.RunFromCommandLine
    public static class SpanishCafeQA
    {
        [UnityEditor.MenuItem("Spanish Café/Run QA Play-through")]
        static void RunFromMenu()
        {
            var problems = RunAll();
            EditorUtility.DisplayDialog("Spanish Café QA",
                problems.Count == 0 ? "Everything passed!" : $"{problems.Count} problems. Details are in the Console.", "OK");
        }

        public static void RunFromCommandLine()
        {
            var problems = RunAll();
            EditorApplication.Exit(problems.Count == 0 ? 0 : 1);
        }

        public static List<string> RunAll()
        {
            CafeContent.Reload();
            CafeContent.EnsureLoaded();
            var problems = new List<string>();

            if (CafeContent.Levels.Length == 0) problems.Add("No unidades loaded.");
            foreach (var level in CafeContent.Levels)
            {
                CheckContent(level, problems);
                PlayPath(level, "correct", 8, problems);
                PlayPath(level, "awkward", 6, problems);
                PlayWrongThenCorrect(level, problems);
                CheckRepeatAndHints(level, problems);
            }
            CheckPanel(problems);

            foreach (var problem in problems) Debug.LogError("[SpanishCafe QA] " + problem);
            Debug.Log(problems.Count == 0
                ? $"[SpanishCafe QA] PASS: {CafeContent.Levels.Length} unidades played on every path, panel checks passed."
                : $"[SpanishCafe QA] FAIL: {problems.Count} problems.");
            return problems;
        }

        // Every line has its English, feedback, hint and voice clip
        static void CheckContent(LevelInfo level, List<string> problems)
        {
            var scenario = CafeContent.GetScenario(level);
            if (scenario?.turns == null)
            {
                problems.Add($"{level.level}: dialogue file '{level.scenarioFile}' didn't load.");
                return;
            }

            foreach (var turn in scenario.turns)
            {
                if (turn.IsStaff)
                {
                    NeedsEnglishAndAudio(turn.id, problems);
                    if (!string.IsNullOrEmpty(turn.repeatEs)) NeedsEnglishAndAudio(turn.id + ".repeat", problems);
                    continue;
                }

                if (CafeContent.Feedback(turn.id + ".hint") == null) problems.Add($"{turn.id}: no hint.");
                foreach (var choice in turn.choices)
                {
                    NeedsEnglishAndAudio(choice.id, problems);
                    bool needsFeedback = choice.result == "awkward" || choice.result == "wrong";
                    if (needsFeedback && CafeContent.Feedback(choice.id) == null) problems.Add($"{choice.id}: no feedback for a {choice.result} answer.");
                }
            }
        }

        static void NeedsEnglishAndAudio(string id, List<string> problems)
        {
            if (string.IsNullOrEmpty(CafeContent.English(id))) problems.Add($"{id}: no English.");
            if (Resources.Load<AudioClip>("CafeAudio/" + id.Replace('.', '_')) == null) problems.Add($"{id}: no voice clip.");
        }

        // Answers every question the same way and checks it finishes with the right score and total
        static void PlayPath(LevelInfo level, string result, int expectedScore, List<string> problems)
        {
            var manager = new CafeDialogueManager();
            if (!manager.Begin(level))
            {
                problems.Add($"{level.level}: couldn't start.");
                return;
            }

            if (!PlayUntilEnd(manager, level, result, problems)) return;

            if (manager.Score.Total != expectedScore)
                problems.Add($"{level.level} ({result} path): score {manager.Score.Total}/8, expected {expectedScore}/8.");
            CheckSpokenTotal(manager, level, result, problems);
        }

        // A wrong answer keeps the same question, then the right one moves on
        static void PlayWrongThenCorrect(LevelInfo level, List<string> problems)
        {
            var manager = new CafeDialogueManager();
            manager.Begin(level);
            int guard = 0;
            while (!manager.IsFinished && guard++ < 100)
            {
                var turn = manager.StudentTurn;
                if (turn == null)
                {
                    manager.Continue();
                    continue;
                }

                var wrong = Find(turn, "wrong");
                if (wrong != null)
                {
                    manager.Choose(wrong);
                    if (manager.StudentTurn != turn) problems.Add($"{wrong.id}: a wrong answer moved the conversation forward.");
                }
                manager.Choose(Find(turn, "correct"));
            }

            if (!manager.IsFinished) problems.Add($"{level.level} (wrong-then-correct path): never finished.");
            else if (manager.Score.Total != 5) problems.Add($"{level.level} (wrong-then-correct path): score {manager.Score.Total}/8, expected 5/8.");
        }

        // Repeat stays on the same question, and only 3 hints work
        static void CheckRepeatAndHints(LevelInfo level, List<string> problems)
        {
            var manager = new CafeDialogueManager();
            manager.Begin(level);
            var turn = manager.StudentTurn;
            var repeat = turn == null ? null : Find(turn, "repeat");
            if (repeat == null)
            {
                problems.Add($"{level.level}: first question has no ¿Puede repetir? option.");
            }
            else
            {
                manager.Choose(repeat);
                if (manager.StudentTurn != turn) problems.Add($"{repeat.id}: repeat moved the conversation forward.");
            }

            if (level.maxHints != 3) problems.Add($"{level.level}: maxHints is {level.maxHints}, should be 3.");
            int worked = 0;
            for (int i = 0; i < 5; i++) if (manager.UseHint()) worked++;
            if (worked != level.maxHints) problems.Add($"{level.level}: {worked} hints worked, expected {level.maxHints}.");
        }

        static bool PlayUntilEnd(CafeDialogueManager manager, LevelInfo level, string result, List<string> problems)
        {
            int guard = 0;
            while (!manager.IsFinished && guard++ < 100)
            {
                var turn = manager.StudentTurn;
                if (turn == null)
                {
                    manager.Continue();
                    continue;
                }

                var choice = Find(turn, result) ?? Find(turn, "correct");
                if (choice == null)
                {
                    problems.Add($"{turn.id}: no {result} or correct answer to pick.");
                    return false;
                }
                manager.Choose(choice);
            }

            if (!manager.IsFinished) problems.Add($"{level.level} ({result} path): never finished.");
            return manager.IsFinished;
        }

        // Lupita's last line has to say the same total the order adds up to
        static void CheckSpokenTotal(CafeDialogueManager manager, LevelInfo level, string result, List<string> problems)
        {
            int total = manager.Order.Total;
            string last = manager.StaffTurn?.es ?? "";
            if (total == 0 || !last.Contains(" pesos")) return; // she doesn't say a price at the end, nothing to compare
            if (!last.ToLowerInvariant().Contains(NumberInSpanish(total) + " pesos"))
                problems.Add($"{level.level} ({result} path): order is {total} pesos but Lupita says \"{last}\".");
        }

        static DialogueChoice Find(DialogueTurn turn, string result)
        {
            if (turn.choices == null) return null;
            foreach (var choice in turn.choices)
                if (choice.result == result) return choice;
            return null;
        }

        static string NumberInSpanish(int n)
        {
            string[] ones = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
            string[] teens = { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
            string[] twenties = { "veinte", "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve" };
            string[] tens = { "", "", "", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            string[] hundreds = { "", "ciento", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

            if (n == 100) return "cien";
            string words = n >= 100 ? hundreds[n / 100] : "";
            int rest = n % 100;
            string small = rest == 0 ? "" : rest < 10 ? ones[rest] : rest < 20 ? teens[rest - 10] : rest < 30 ? twenties[rest - 20]
                : tens[rest / 10] + (rest % 10 == 0 ? "" : " y " + ones[rest % 10]);
            return (words + " " + small).Trim();
        }

        // Opens the real dialogue panel and checks the English switch, the translations and the hint limit
        static void CheckPanel(List<string> problems)
        {
            // Finishing a unidad saves progress, so remember the real progress and put it back after
            string level = CafeContent.Levels[0].level;
            string doneKey = "SpanishCafe.done." + level, bestKey = "SpanishCafe.best." + level;
            bool hadDone = PlayerPrefs.HasKey(doneKey), hadBest = PlayerPrefs.HasKey(bestKey);
            int done = PlayerPrefs.GetInt(doneKey), best = PlayerPrefs.GetInt(bestKey);

            var go = new GameObject("QA Spanish Café UI");
            try
            {
                var ui = go.AddComponent<CafeDialogueUI>();
                Call(ui, "Awake");
                ui.Open("Lupita", null);

                var subtitle = Get<TextMeshProUGUI>(ui, "subtitleText");
                var choicesRoot = Get<Transform>(ui, "choicesRoot");
                var feedback = Get<TextMeshProUGUI>(ui, "feedbackText");
                var firstLevel = CafeContent.Levels[0];

                if (Get<bool>(ui, "subtitlesOn")) problems.Add("Panel: English is on when it opens, should start off.");
                if (subtitle.gameObject.activeSelf) problems.Add("Panel: English subtitle shows on the unidad screen with English off.");
                if (Labels(choicesRoot).Contains(firstLevel.nameEn)) problems.Add("Panel: English unit names show with English off.");

                Call(ui, "ToggleSubtitles");
                if (!subtitle.gameObject.activeSelf) problems.Add("Panel: pressing T didn't show the English subtitle.");
                if (!Labels(choicesRoot).Contains(firstLevel.nameEn)) problems.Add("Panel: pressing T didn't show the English unit names.");
                Call(ui, "ToggleSubtitles");

                Call(ui, "StartLevel", firstLevel);
                var manager = Get<CafeDialogueManager>(ui, "manager");
                var turn = manager.StudentTurn;
                var correct = Find(turn, "correct");
                string english = CafeContent.English(correct.id);

                if (Labels(choicesRoot).Contains(english)) problems.Add("Panel: answer translations show with English off.");
                Call(ui, "ToggleSubtitles");
                if (!Labels(choicesRoot).Contains(english)) problems.Add("Panel: pressing T didn't show the translations under the answers.");
                if (!subtitle.gameObject.activeSelf) problems.Add("Panel: pressing T didn't show Lupita's subtitle while talking.");

                // A wrong answer stays grayed out after the English is switched, and its feedback follows the switch
                var wrong = Find(turn, "wrong");
                Call(ui, "Pick", wrong);
                var wrongFeedback = CafeContent.Feedback(wrong.id);
                if (!feedback.text.Contains(wrongFeedback.en)) problems.Add("Panel: feedback is missing its English with English on.");
                Call(ui, "ToggleSubtitles");
                if (feedback.text.Contains(wrongFeedback.en)) problems.Add("Panel: feedback kept its English after pressing T.");
                if (!feedback.text.Contains(wrongFeedback.es)) problems.Add("Panel: feedback disappeared after pressing T.");
                foreach (var button in choicesRoot.GetComponentsInChildren<Button>())
                    if (button.GetComponentInChildren<TextMeshProUGUI>().text.Contains(wrong.es) && button.interactable)
                        problems.Add("Panel: a wrong answer can be clicked again after pressing T.");

                // Only 3 hints, the 4th says there are none left
                var hintButton = Get<Button>(ui, "hintButton");
                for (int i = 0; i < 3; i++) Call(ui, "ShowHint");
                if (hintButton.interactable) problems.Add("Panel: hint button still works after 3 hints.");
                Call(ui, "ShowHint");
                string noHints = CafeContent.Feedback("general.noHints")?.es ?? "";
                if (!feedback.text.Contains(noHints)) problems.Add("Panel: the 4th hint didn't say there are no hints left.");

                // Finish the unidad and check the score screen follows the switch too
                while (!manager.IsFinished) Call(ui, "Pick", Find(manager.StudentTurn, "correct"));
                bool newRecordShown = feedback.text.Contains("¡Nuevo récord!") || !feedback.text.Contains("Tu mejor");
                Call(ui, "ToggleSubtitles");
                if (!feedback.text.Contains("(Score)")) problems.Add("Panel: score screen didn't show English after pressing T.");
                bool stillSame = feedback.text.Contains("¡Nuevo récord!") || !feedback.text.Contains("Tu mejor");
                if (newRecordShown != stillSame) problems.Add("Panel: pressing T on the score screen changed the best-score message.");
            }
            catch (System.Exception e)
            {
                problems.Add("Panel check crashed: " + e.GetBaseException().Message);
            }
            finally
            {
                Object.DestroyImmediate(go);
                if (hadDone) PlayerPrefs.SetInt(doneKey, done); else PlayerPrefs.DeleteKey(doneKey);
                if (hadBest) PlayerPrefs.SetInt(bestKey, best); else PlayerPrefs.DeleteKey(bestKey);
                PlayerPrefs.Save();
                var events = Object.FindAnyObjectByType<EventSystem>();
                if (events != null) Object.DestroyImmediate(events.gameObject);
            }
        }

        static string Labels(Transform root)
        {
            var all = new System.Text.StringBuilder();
            foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>()) all.Append(text.text).Append('\n');
            return all.ToString();
        }

        const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        static T Get<T>(object target, string field) => (T)target.GetType().GetField(field, Any).GetValue(target);

        static void Call(object target, string method, params object[] args) => target.GetType().GetMethod(method, Any).Invoke(target, args);
    }
}
