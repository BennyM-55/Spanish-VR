using UnityEngine;

namespace SpanishCafe
{
    public class MateoQuestIndicator : MonoBehaviour
    {
        [Header("Assign in the Inspector")]
        public GameObject exclamationCanvas;
        public Transform player;
        public Camera playerCamera;

        [Header("Animation")]
        public float floatSpeed = 2f;
        public float floatHeight = 0.15f;

        private CafeConversationStarter mateo;
        private CafeDialogueUI dialogue;
        private Vector3 originalLocalPosition;
        private bool showing;
        private bool reachedMateo;
        private bool wasDialogueOpen;

        void Start()
        {
            mateo = GetComponent<CafeConversationStarter>();
            dialogue = CafeDialogueUI.Instance;
            if (playerCamera == null) playerCamera = Camera.main;
            if (player == null && mateo != null) player = mateo.player;
            if (exclamationCanvas == null)
            {
                Debug.LogWarning("MateoQuestIndicator: Assign Exclamation Canvas on Mateo.", this);
                enabled = false;
                return;
            }
            originalLocalPosition = exclamationCanvas.transform.localPosition;
            SetVisible(false);
            wasDialogueOpen = dialogue != null && dialogue.IsOpen;
        }

        void Update()
        {
            if (dialogue == null) dialogue = CafeDialogueUI.Instance;
            if (player == null && mateo != null) player = mateo.player;

            bool open = dialogue != null && dialogue.IsOpen;
            bool ready = LupitaFinishedAndMateoNotFinished();

            if (!reachedMateo && ready && !open && !showing)
                SetVisible(true);

            if (showing && open && !wasDialogueOpen && PlayerIsNearMateo())
            {
                reachedMateo = true;
                SetVisible(false);
            }

            if (!ready && showing) SetVisible(false);
            wasDialogueOpen = open;
        }

        void LateUpdate()
        {
            if (!showing || exclamationCanvas == null) return;
            Transform icon = exclamationCanvas.transform;
            icon.localPosition = originalLocalPosition + Vector3.up *
                (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            if (playerCamera == null) playerCamera = Camera.main;
            if (playerCamera != null) icon.rotation = playerCamera.transform.rotation;
        }

        bool LupitaFinishedAndMateoNotFinished()
        {
            CafeContent.EnsureLoaded();
            foreach (var level in CafeContent.Levels)
            {
                if (level.lesson != 1) continue;
                if (CafeFlow.HasMenu && level.unit != CafeFlow.Unit) continue;
                if (!CafeProgress.Finished(level.ProgressKey)) continue;
                var mateoLevel = CafeFlow.Find(level.unit, 2);
                if (mateoLevel != null && !CafeProgress.Finished(mateoLevel.ProgressKey))
                    return true;
            }
            return false;
        }

        bool PlayerIsNearMateo()
        {
            if (player == null) return false;
            float distance = Vector2.Distance(
                new Vector2(player.position.x, player.position.z),
                new Vector2(transform.position.x, transform.position.z));
            return distance <= (mateo != null ? mateo.talkDistance : 2.5f);
        }

        void SetVisible(bool visible)
        {
            showing = visible;
            if (exclamationCanvas != null) exclamationCanvas.SetActive(visible);
        }
    }
}
