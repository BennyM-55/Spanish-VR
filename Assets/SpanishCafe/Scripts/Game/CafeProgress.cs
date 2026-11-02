using UnityEngine;

namespace SpanishCafe
{
    // Remembers which levels you finished and your best score, even after you close the game
    // It uses PlayerPrefs, so it's saved on this computer or headset only
    public static class CafeProgress
    {
        const string Prefix = "SpanishCafe.";

        public static bool Finished(string level) => PlayerPrefs.GetInt(Prefix + "done." + level, 0) == 1;

        // -1 means they never finished it
        public static int Best(string level) => PlayerPrefs.GetInt(Prefix + "best." + level, -1);

        // Returns true if this was a new best score, so we can tell them
        public static bool Record(string level, int score)
        {
            if (string.IsNullOrEmpty(level)) return false;

            bool newBest = score > Best(level);
            PlayerPrefs.SetInt(Prefix + "done." + level, 1);
            if (newBest) PlayerPrefs.SetInt(Prefix + "best." + level, score);
            PlayerPrefs.Save();
            return newBest;
        }

        // Clears everything, good for starting fresh before a demo
        public static void ResetAll()
        {
            foreach (var level in CafeContent.Levels)
            {
                PlayerPrefs.DeleteKey(Prefix + "done." + level.level);
                PlayerPrefs.DeleteKey(Prefix + "best." + level.level);
            }
            PlayerPrefs.Save();
        }
    }
}
