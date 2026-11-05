using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpanishCafe
{
    // The student's side of the conversation. When they pick an answer,
    // BoZo says it out loud and it shows up in a speech bubble over his head
    // This is its own object that follows BoZo around, so we never add anything to James's character
    public class StudentSpeech : MonoBehaviour
    {
        // 1.5 mm per pixel, so a 600 px bubble is about 90 cm wide over his head
        const float Scale = 0.0015f;
        const float MaxWidth = 600f;

        Transform student;
        float headHeight = 2.1f;
        AudioSource voice;
        GameObject bubble;
        TextMeshProUGUI bubbleText;
        float hideAt;

        public static StudentSpeech Create(Transform parent)
        {
            var go = new GameObject("Student Speech");
            go.transform.SetParent(parent, false);
            var speech = go.AddComponent<StudentSpeech>();
            speech.Build();
            return speech;
        }

        // Point it at the student. In VR the student is the camera, so there's no head to put a bubble over
        public void Follow(Transform target)
        {
            student = target;
            headHeight = FindHeadHeight(target);
        }

        // Says the line and shows the bubble. Returns how long it takes, so Lupita waits her turn
        public float Say(string spanish, AudioClip clip)
        {
            float length = clip != null ? clip.length : Mathf.Clamp(spanish.Length * 0.06f, 1.2f, 4f);

            voice.Stop();
            if (clip != null)
            {
                voice.clip = clip;
                voice.Play();
            }

            bool hasBody = student != null && (Camera.main == null || student != Camera.main.transform);
            bubbleText.text = spanish;
            bubble.SetActive(hasBody);
            hideAt = Time.time + length + 1.5f; // stays up a little after he finishes talking
            return length;
        }

        public void Hide()
        {
            voice.Stop();
            bubble.SetActive(false);
        }

        void LateUpdate()
        {
            if (student != null) transform.position = student.position + Vector3.up * headHeight;

            if (!bubble.activeSelf) return;
            if (Time.time > hideAt)
            {
                bubble.SetActive(false);
                return;
            }

            // Always face the camera so it's easy to read
            if (Camera.main != null) bubble.transform.rotation = Camera.main.transform.rotation;
        }

        // Puts the bubble just above the top of the character's model
        static float FindHeadHeight(Transform target)
        {
            if (target == null) return 2.1f;
            var renderers = target.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 2.1f;

            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            return bounds.max.y - target.position.y + 0.35f;
        }

        void Build()
        {
            voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 0.5f; // a bit 3D so it sounds like it comes from BoZo

            bubble = new GameObject("Bubble", typeof(RectTransform), typeof(Canvas), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            bubble.transform.SetParent(transform, false);
            bubble.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            bubble.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.92f);

            var rect = (RectTransform)bubble.transform;
            rect.localScale = Vector3.one * Scale;
            rect.pivot = new Vector2(0.5f, 0f); // grows up from his head

            var layout = bubble.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 14, 14);
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var fitter = bubble.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            textObject.transform.SetParent(bubble.transform, false);
            bubbleText = textObject.GetComponent<TextMeshProUGUI>();
            bubbleText.fontSize = 40;
            bubbleText.color = new Color(0.08f, 0.24f, 0.27f);
            bubbleText.alignment = TextAlignmentOptions.Center;
            bubbleText.raycastTarget = false;
            textObject.GetComponent<LayoutElement>().preferredWidth = MaxWidth; // long lines wrap instead of making a huge bubble

            bubble.SetActive(false);
        }
    }
}
