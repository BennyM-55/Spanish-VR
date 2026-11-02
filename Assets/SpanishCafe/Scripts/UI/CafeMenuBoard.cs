using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpanishCafe
{
    // The menu board on the café wall. It reads the same menu file as the dialogue,
    // so if we change a price in the JSON the board changes too
    // It builds itself when you press Play, the orange box in the Scene view shows where it goes
    public class CafeMenuBoard : MonoBehaviour
    {
        [Tooltip("Show the small English line under each item. Turn it off for harder practice.")]
        public bool showEnglish = true;

        [Tooltip("Show a few useful phrases at the bottom of the board.")]
        public bool showPhrases = true;

        [Tooltip("How many phrases to show at the bottom.")]
        public int phraseCount = 4;

        // 1 pixel = 1 millimeter, so the board is 0.9 m wide and 1.3 m tall
        const float Scale = 0.001f;
        static readonly Vector2 Size = new Vector2(900f, 1300f);

        static readonly Color Board = new Color(0.10f, 0.20f, 0.17f, 1f);
        static readonly Color Frame = new Color(0.45f, 0.30f, 0.18f, 1f);
        static readonly Color Accent = new Color(0.86f, 0.47f, 0.31f);
        static readonly Color Chalk = new Color(0.95f, 0.94f, 0.88f);
        static readonly Color Soft = new Color(0.72f, 0.80f, 0.76f);

        void Start()
        {
            CafeContent.EnsureLoaded();
            var menu = CafeContent.Menu;
            if (menu?.menu == null)
            {
                Debug.LogWarning("[SpanishCafe] Menu board couldn't find the menu. Check Vocabulary/menu_and_phrases.json.");
                return;
            }
            Build(menu);
        }

        void Build(MenuData menu)
        {
            var canvasObject = new GameObject("Menu Board Canvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;

            var rect = (RectTransform)canvasObject.transform;
            rect.sizeDelta = Size;
            rect.localScale = Vector3.one * Scale;

            // Wood frame in the back, chalkboard on top of it
            var frame = MakeImage("Frame", rect, Frame);
            Stretch(frame, 0f);
            var board = MakeImage("Board", rect, Board);
            Stretch(board, 18f);

            var layout = board.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(56, 56, 40, 40);
            layout.spacing = 10;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var title = MakeText(board, menu.cafeName ?? "Café La Esquina", 60, FontStyles.Bold, Accent);
            title.alignment = TextAlignmentOptions.Center;
            var subtitle = MakeText(board, showEnglish ? "Menú  <size=70%><color=#B8CCC2>(Menu)</color></size>" : "Menú", 36, FontStyles.Italic, Chalk);
            subtitle.alignment = TextAlignmentOptions.Center;
            MakeLine(board);

            foreach (var item in menu.menu)
                MakeItemRow(board, item);

            string currency = string.IsNullOrEmpty(menu.currency) ? "MXN" : menu.currency;
            var prices = MakeText(board, $"Precios en pesos ({currency})" + (showEnglish ? "  <color=#B8CCC2>· Prices in pesos</color>" : ""), 22, FontStyles.Italic, Soft);
            prices.alignment = TextAlignmentOptions.Center;

            if (showPhrases && menu.phrases != null && menu.phrases.Length > 0)
            {
                MakeLine(board);
                MakeText(board, "Frases útiles" + (showEnglish ? "  <size=70%><color=#B8CCC2>(Useful phrases)</color></size>" : ""), 32, FontStyles.Bold, Accent);
                int count = Mathf.Min(phraseCount, menu.phrases.Length);
                for (int i = 0; i < count; i++)
                {
                    var phrase = menu.phrases[i];
                    string line = showEnglish
                        ? $"{phrase.es}\n<size=75%><color=#B8CCC2><i>{phrase.en}</i></color></size>"
                        : phrase.es;
                    MakeText(board, line, 26, FontStyles.Normal, Chalk);
                }
            }
        }

        // One menu line: name on the left, price on the right, English under the name
        void MakeItemRow(RectTransform parent, MenuItem item)
        {
            var row = new GameObject(item.id, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            row.transform.SetParent(parent, false);
            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.UpperLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            string name = showEnglish
                ? $"{item.es}\n<size=70%><color=#B8CCC2><i>{item.en}</i></color></size>"
                : item.es;
            var nameText = MakeText((RectTransform)row.transform, name, 34, FontStyles.Normal, Chalk);
            nameText.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var price = MakeText((RectTransform)row.transform, $"${item.price}", 34, FontStyles.Bold, Accent);
            price.alignment = TextAlignmentOptions.TopRight;
            price.gameObject.AddComponent<LayoutElement>().minWidth = 120f;
        }

        static void MakeLine(RectTransform parent)
        {
            var line = MakeImage("Line", parent, new Color(0.95f, 0.94f, 0.88f, 0.35f));
            line.gameObject.AddComponent<LayoutElement>().preferredHeight = 3f;
        }

        static RectTransform MakeImage(string name, RectTransform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false; // it's just to look at, nothing to click
            return (RectTransform)go.transform;
        }

        static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        static TextMeshProUGUI MakeText(RectTransform parent, string value, float size, FontStyles style, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.richText = true;
            text.raycastTarget = false;
            return text;
        }

        void OnDrawGizmos()
        {
            // Draws the board's outline so you can line it up on a wall before pressing Play
            Gizmos.color = Accent;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(Size.x * Scale, Size.y * Scale, 0.02f));
        }
    }
}
