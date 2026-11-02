using System;

namespace SpanishCafe
{
    // These classes match the JSON files in Resources CafeContent
   

    [Serializable]
    public class MenuItem
    {
        public string id;
        public string es;
        public string en;
        public int price;
    }

    [Serializable]
    public class PhrasePair
    {
        public string es;
        public string en;
    }

    [Serializable]
    public class MenuData
    {
        public string cafeName;
        public string currency;
        public MenuItem[] menu;
        public PhrasePair[] phrases;
        public PhrasePair[] distinctions;
    }

    [Serializable]
    public class OrderLine
    {
        public string item;
        public int qty;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string id;
        public string es;
        public string result;   // correct, awkward, wrong, or repeat
        public string next;
        public string addItem;  // short way to add one menu item
        public int qty;
        public OrderLine[] adds; // long way to add several items at once
    }

    [Serializable]
    public class DialogueTurn
    {
        public string id;
        public string speaker;  // staff or student
        public string es;
        public string repeatEs; // simpler line Lupita says when you ask her to repeat
        public string next;
        public bool end;
        public string removeItem; // used when something sells out
        public DialogueChoice[] choices;

        public bool IsStaff => speaker == "staff";
    }

    [Serializable]
    public class Scenario
    {
        public string scenarioId;
        public string level;
        public string staffName;
        public string titleEs;
        public string goalEs;
        public DialogueTurn[] turns;
    }

    [Serializable]
    public class TranslationEntry
    {
        public string id;
        public string en;
    }

    [Serializable]
    public class TranslationFile
    {
        public string language;
        public TranslationEntry[] entries;
    }

    [Serializable]
    public class FeedbackEntry
    {
        public string id;
        public string es;
        public string en;
    }

    [Serializable]
    public class RubricRow
    {
        public string id;
        public string es;
        public string en;
        public string level0;
        public string level1;
        public string level2;
    }

    [Serializable]
    public class FeedbackFile
    {
        public RubricRow[] rubric;
        public FeedbackEntry[] entries;
    }

    [Serializable]
    public class LevelInfo
    {
        public string level;
        public string nameEn;
        public string nameEs;
        public string scenarioFile;
        public bool showSubtitles;
        public bool showChoiceTranslations;
        public int maxHints;
        public string challengeEs;
        public string challengeEn;
        public string successEn;
        public string sampleAnswerEs;
        public string sampleAnswerEn;
    }

    [Serializable]
    public class LevelFile
    {
        public LevelInfo[] levels;
    }
}
