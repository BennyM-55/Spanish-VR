using System.Collections.Generic;
using UnityEngine;

namespace SpanishCafe
{
    // Loads all the café JSON files once and lets other scripts look things up by id.
    public static class CafeContent
    {
        const string Root = "CafeContent/";

        static bool loaded;
        static readonly Dictionary<string, string> english = new Dictionary<string, string>();
        static readonly Dictionary<string, FeedbackEntry> feedback = new Dictionary<string, FeedbackEntry>();
        static readonly Dictionary<string, MenuItem> menuItems = new Dictionary<string, MenuItem>();
        static readonly Dictionary<string, Scenario> scenarios = new Dictionary<string, Scenario>();

        public static MenuData Menu { get; private set; }
        public static LevelInfo[] Levels { get; private set; } = new LevelInfo[0];
        public static RubricRow[] Rubric { get; private set; } = new RubricRow[0];

        // Clears the cache when you press Play, so edited JSON files show up right away.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reload()
        {
            loaded = false;
            english.Clear();
            feedback.Clear();
            menuItems.Clear();
            scenarios.Clear();
        }

        public static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;

            Menu = Read<MenuData>("Vocabulary/menu_and_phrases") ?? new MenuData();
            if (Menu.menu != null)
                foreach (var item in Menu.menu)
                    if (!string.IsNullOrEmpty(item.id)) menuItems[item.id] = item;

            var translations = Read<TranslationFile>("Translations/translations_en");
            if (translations?.entries != null)
                foreach (var entry in translations.entries)
                    if (!string.IsNullOrEmpty(entry.id)) english[entry.id] = entry.en;

            var feedbackFile = Read<FeedbackFile>("Feedback/feedback");
            if (feedbackFile?.entries != null)
                foreach (var entry in feedbackFile.entries)
                    if (!string.IsNullOrEmpty(entry.id)) feedback[entry.id] = entry;
            Rubric = feedbackFile?.rubric ?? new RubricRow[0];

            Levels = Read<LevelFile>("Levels/levels")?.levels ?? new LevelInfo[0];
        }

        public static Scenario GetScenario(LevelInfo level)
        {
            EnsureLoaded();
            if (level == null || string.IsNullOrEmpty(level.scenarioFile)) return null;
            if (scenarios.TryGetValue(level.scenarioFile, out var cached)) return cached;

            var scenario = Read<Scenario>("Dialogue/" + level.scenarioFile);
            if (scenario != null) scenarios[level.scenarioFile] = scenario;
            return scenario;
        }

        // Returns "" when a line has no English yet, so the subtitle just stays empty.
        public static string English(string id)
        {
            EnsureLoaded();
            return id != null && english.TryGetValue(id, out var en) ? en : "";
        }

        public static FeedbackEntry Feedback(string id)
        {
            EnsureLoaded();
            return id != null && feedback.TryGetValue(id, out var entry) ? entry : null;
        }

        public static int Price(string itemId)
        {
            EnsureLoaded();
            return itemId != null && menuItems.TryGetValue(itemId, out var item) ? item.price : 0;
        }

        public static string ItemName(string itemId)
        {
            EnsureLoaded();
            return itemId != null && menuItems.TryGetValue(itemId, out var item) ? item.es : itemId;
        }

        public static string Currency
        {
            get
            {
                EnsureLoaded();
                return string.IsNullOrEmpty(Menu?.currency) ? "MXN" : Menu.currency;
            }
        }

        static T Read<T>(string path) where T : class
        {
            var asset = Resources.Load<TextAsset>(Root + path);
            if (asset == null)
            {
                Debug.LogError($"[SpanishCafe] Missing content file: Resources/{Root}{path}.json");
                return null;
            }

            try
            {
                return JsonUtility.FromJson<T>(asset.text);
            }
            catch (System.Exception e)
            {
                // Usually a missing comma. Run Tools/SpanishCafe/validate_content.py to find it.
                Debug.LogError($"[SpanishCafe] Couldn't read {path}.json: {e.Message}");
                return null;
            }
        }
    }
}
