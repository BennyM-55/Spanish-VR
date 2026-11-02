using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SpanishCafe
{
    // Keeps track of what the student ordered and adds up the total with the menu prices
    public class OrderTracker
    {
        readonly List<string> itemOrder = new List<string>();
        readonly Dictionary<string, int> counts = new Dictionary<string, int>();

        public bool IsEmpty => counts.Count == 0;

        public void Clear()
        {
            itemOrder.Clear();
            counts.Clear();
        }

        public void Add(string itemId, int qty)
        {
            if (string.IsNullOrEmpty(itemId) || qty <= 0) return;

            if (counts.TryGetValue(itemId, out var current))
            {
                counts[itemId] = current + qty;
            }
            else
            {
                counts[itemId] = qty;
                itemOrder.Add(itemId);
            }
        }

        // Used when something sells out, like the chicken in B1
        public void Remove(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return;
            counts.Remove(itemId);
            itemOrder.Remove(itemId);
        }

        public void Apply(DialogueChoice choice)
        {
            if (choice == null) return;

            if (choice.adds != null)
                foreach (var line in choice.adds)
                    Add(line.item, line.qty);

            // A qty of 0 means the JSON left it out, so count it as one
            if (!string.IsNullOrEmpty(choice.addItem))
                Add(choice.addItem, Mathf.Max(1, choice.qty));
        }

        public int Total
        {
            get
            {
                int total = 0;
                foreach (var pair in counts)
                    total += CafeContent.Price(pair.Key) * pair.Value;
                return total;
            }
        }

        // Example 2× Taco al pastor, 1× Agua de jamaica
        public string Describe()
        {
            if (IsEmpty) return "—";

            var text = new StringBuilder();
            foreach (var itemId in itemOrder)
            {
                if (text.Length > 0) text.Append(", ");
                text.Append(counts[itemId]).Append("× ").Append(CafeContent.ItemName(itemId));
            }
            return text.ToString();
        }
    }
}
