namespace SpanishCafe
{
    
    // It's meant for feedback inside a level, not for comparing levels
    public class ConversationScore
    {
        public int Wrong;
        public int Awkward;
        public int Repeats;
        public int HintsUsed;
        public bool Finished;

        public void Reset()
        {
            Wrong = 0;
            Awkward = 0;
            Repeats = 0;
            HintsUsed = 0;
            Finished = false;
        }

        // Completes the task done on your own, done with a few retries, or not finished
        public int Task => !Finished ? 0 : (Wrong <= 1 ? 2 : 1);

    
        public int Clarity => Wrong == 0 ? 2 : (Wrong <= 2 ? 1 : 0);

      
        public int Repair => !Finished ? 0 : (HintsUsed > 0 ? 1 : 2);

        // Uses suitable language: awkward answers work but sound blunt or unnatural
        public int Language => Awkward == 0 ? 2 : (Awkward <= 2 ? 1 : 0);

        public int Total => Task + Clarity + Repair + Language;

        public int ForRubric(string id)
        {
            switch (id)
            {
                case "task": return Task;
                case "clarity": return Clarity;
                case "repair": return Repair;
                case "language": return Language;
                default: return 0;
            }
        }
    }
}
