using System.Collections.Generic;
using UnityEngine;

namespace SpanishCafe
{
    public enum ChoiceResult { Correct, Awkward, Wrong, Repeat }

    // Runs one conversation: which line Lupita is on, what the student can answer
    public class CafeDialogueManager
    {
        readonly Dictionary<string, DialogueTurn> turns = new Dictionary<string, DialogueTurn>();

        public LevelInfo Level { get; private set; }
        public Scenario Scenario { get; private set; }
        public DialogueTurn StaffTurn { get; private set; }
        public DialogueTurn StudentTurn { get; private set; }
        public bool IsFinished { get; private set; }
        public OrderTracker Order { get; } = new OrderTracker();
        public ConversationScore Score { get; } = new ConversationScore();

        public int HintsLeft => Level == null ? 0 : Mathf.Max(0, Level.maxHints - Score.HintsUsed);

        public bool Begin(LevelInfo level)
        {
            Level = level;
            Scenario = CafeContent.GetScenario(level);
            if (Scenario?.turns == null || Scenario.turns.Length == 0)
            {
                Debug.LogError($"[SpanishCafe] No dialogue found for level {level?.level}.");
                return false;
            }

            turns.Clear();
            foreach (var turn in Scenario.turns)
                if (!string.IsNullOrEmpty(turn.id)) turns[turn.id] = turn;

            Order.Clear();
            Score.Reset();
            IsFinished = false;
            StaffTurn = null;
            StudentTurn = null;

            GoTo(Scenario.turns[0].id);
            return true;
        }

        public ChoiceResult Choose(DialogueChoice choice)
        {
            var result = Parse(choice.result);
            switch (result)
            {
                case ChoiceResult.Correct:
                    Order.Apply(choice);
                    GoTo(choice.next);
                    break;
                case ChoiceResult.Awkward:
                    // She still understands you, so the conversation keeps going
                    Score.Awkward++;
                    Order.Apply(choice);
                    GoTo(choice.next);
                    break;
                case ChoiceResult.Wrong:
                    // Stay on the same question so the student can try again
                    Score.Wrong++;
                    break;
                case ChoiceResult.Repeat:
                    Score.Repeats++;
                    break;
            }
            return result;
        }

        // Only needed if two Lupita lines come in a row
        public void Continue()
        {
            if (!IsFinished && StudentTurn == null && StaffTurn != null)
                GoTo(StaffTurn.next);
        }

        public bool UseHint()
        {
            if (HintsLeft <= 0) return false;
            Score.HintsUsed++;
            return true;
        }

        void GoTo(string id)
        {
            if (string.IsNullOrEmpty(id) || !turns.TryGetValue(id, out var turn))
            {
                Debug.LogError($"[SpanishCafe] Turn '{id}' doesn't exist in {Scenario?.scenarioId}.");
                Finish();
                return;
            }

            if (!turn.IsStaff)
            {
                StudentTurn = turn;
                return;
            }

            StaffTurn = turn;
            StudentTurn = null;
            Order.Remove(turn.removeItem);

            if (turn.end)
            {
                Finish();
                return;
            }

            // Lupita talks, then the student answers on the next turn
            if (!string.IsNullOrEmpty(turn.next) && turns.TryGetValue(turn.next, out var next) && !next.IsStaff)
                StudentTurn = next;
        }

        void Finish()
        {
            IsFinished = true;
            Score.Finished = true;
            StudentTurn = null;
        }

        static ChoiceResult Parse(string result)
        {
            switch (result)
            {
                case "correct": return ChoiceResult.Correct;
                case "awkward": return ChoiceResult.Awkward;
                case "repeat": return ChoiceResult.Repeat;
                default: return ChoiceResult.Wrong;
            }
        }
    }
}
