using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpanishCafe
{
    // Connects the café to Ben's main menu (GameManager, LessonManager, SaveGameManager)
    // Each unidad has 2 lessons: 1 = order at the counter with Lupita, 2 = talk at the table with Mateo
    // If you play the café scene without the main menu, HasMenu is false and everything still works for testing
    public static class CafeFlow
    {
        public static bool HasMenu => GameManager.instance != null;

        public static int Unit => HasMenu ? Mathf.Max(1, GameManager.instance.currentUnit) : 0;
        public static int Lesson => HasMenu ? Mathf.Clamp(GameManager.instance.currentLesson, 1, 2) : 0;

        // The lesson the save says the student is on, or null if they finished everything
        public static LevelInfo Current => Find(Unit, Lesson);

        public static LevelInfo Find(int unit, int lesson)
        {
            foreach (var level in CafeContent.Levels)
                if (level.unit == unit && level.lesson == lesson) return level;
            return null;
        }

        // Lesson 1 goes to lesson 2 of the same unidad, lesson 2 goes to lesson 1 of the next unidad
        public static LevelInfo Next(LevelInfo level)
        {
            if (level == null) return null;
            return Find(level.unit, level.lesson + 1) ?? Find(level.unit + 1, 1);
        }

        // Marks the lesson done in the main menu's save, so Continue and unit selection know about it
        public static void Complete(LevelInfo level)
        {
            if (!HasMenu || level == null) return;
            SetCurrent(level);
            if (LessonManager.instance != null && SaveGameManager.instance != null) LessonManager.instance.CompleteLesson();
            else Debug.LogWarning("[SpanishCafe] No LessonManager or SaveGameManager, so this lesson wasn't marked done in the save.");

            // Save the next lesson right away, so leaving with Esc still continues from the right place
            var next = Next(level);
            if (next != null) SetCurrent(next);
            Save();
        }

        public static void MoveTo(LevelInfo level)
        {
            if (!HasMenu || level == null) return;
            SetCurrent(level);
            Save();
        }

        public static void SaveAndQuit()
        {
            Save();
            if (SceneTransitionManager.instance != null) SceneTransitionManager.instance.ReturnToMainMenu();
            else if (Application.CanStreamedLevelBeLoaded("MainMenu")) SceneManager.LoadScene("MainMenu");
            else Debug.LogWarning("[SpanishCafe] Saved, but the MainMenu scene isn't in the build settings.");
        }

        static void SetCurrent(LevelInfo level)
        {
            // SelectUnit also writes the save data, so only use it when the save system is there
            if (SaveGameManager.instance != null)
            {
                GameManager.instance.SelectUnit(level.unit);
                GameManager.instance.SelectLesson(level.lesson);
            }
            else
            {
                GameManager.instance.currentUnit = level.unit;
                GameManager.instance.currentLesson = level.lesson;
            }
        }

        static void Save()
        {
            if (HasMenu && SaveGameManager.instance != null) GameManager.instance.SaveProgress();
        }
    }
}
