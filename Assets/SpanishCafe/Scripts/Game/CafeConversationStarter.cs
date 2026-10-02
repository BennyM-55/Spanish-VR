using System.Collections.Generic;
using UnityEngine;

namespace SpanishCafe
{
    // Put this on the café worker (Lupita) When the player walks close and presses E
    // the conversation opens and the player stops moving until it closes
    [DisallowMultipleComponent]
    public class CafeConversationStarter : MonoBehaviour
    {
        [Tooltip("Name shown in the conversation.")]
        public string staffName = "Lupita";

        [Tooltip("Leave this empty and it finds the player for you.")]
        public Transform player;

        [Tooltip("How close the player has to be to talk, in meters.")]
        public float talkDistance = 2.5f;

        [Tooltip("Turn Lupita to look at the player while you talk.")]
        public bool facePlayer = true;

        [Tooltip("Optional. Plays recorded lines from Resources/CafeAudio.")]
        public AudioSource voice;

        [Tooltip("Extra scripts to pause while talking, like a camera controller.")]
        public Behaviour[] pauseWhileTalking;

        // Scripts from the cafe scene that we pause by name
        static readonly string[] AutoPauseScripts =
        {
            "BoZoPlayerMovement",
            "RotateMoveCamera_InputSystem",
            "CinemachineInputAxisController",
        };

        readonly List<Behaviour> paused = new List<Behaviour>();
        CafeDialogueUI ui;
        bool talking;
        CursorLockMode savedLockState;
        bool savedCursorVisible;

        void Start()
        {
            CafeContent.EnsureLoaded();
            ui = CafeDialogueUI.Instance;
            ui.Closed += StopTalking;
            if (voice == null) voice = GetComponent<AudioSource>();
            if (player == null) player = FindPlayer();
        }

        void OnDestroy()
        {
            if (ui != null)
            {
                ui.Closed -= StopTalking;
                ui.ShowPrompt(this, null);
            }
        }

        void Update()
        {
            if (ui == null) return;
            if (player == null)
            {
                player = FindPlayer();
                if (player == null) return;
            }

            if (talking)
            {
                if (facePlayer) TurnTowardPlayer();
                return;
            }

            bool near = FlatDistance(player.position, transform.position) <= talkDistance;
            ui.ShowPrompt(this, near ? $"Presiona E para hablar con {staffName}  ·  Press E to talk" : null);

            if (near && CafeInput.TalkPressed) StartTalking();
        }

        void StartTalking()
        {
            talking = true;
            ui.ShowPrompt(this, null);

            // Stop walking and stop the camera so the mouse can click the choices
            paused.Clear();
            PauseByName(player);
            foreach (var script in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (script.enabled && IsAutoPause(script)) Pause(script);
            if (pauseWhileTalking != null)
                foreach (var script in pauseWhileTalking)
                    if (script != null && script.enabled) Pause(script);

            
            var animator = player.GetComponentInChildren<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null && HasBool(animator, "isWalk"))
                animator.SetBool("isWalk", false);

            savedLockState = Cursor.lockState;
            savedCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ui.Open(staffName, voice);
        }

        void StopTalking()
        {
            if (!talking) return;
            talking = false;

            foreach (var script in paused)
                if (script != null) script.enabled = true;
            paused.Clear();

            Cursor.lockState = savedLockState;
            Cursor.visible = savedCursorVisible;
        }

        void PauseByName(Transform target)
        {
            if (target == null) return;
            foreach (var script in target.GetComponentsInChildren<MonoBehaviour>())
                if (script.enabled && IsAutoPause(script)) Pause(script);
        }

        void Pause(Behaviour script)
        {
            if (paused.Contains(script)) return;
            script.enabled = false;
            paused.Add(script);
        }

        static bool IsAutoPause(MonoBehaviour script)
        {
            string typeName = script.GetType().Name;
            foreach (var name in AutoPauseScripts)
                if (typeName == name) return true;
            return false;
        }

        static bool HasBool(Animator animator, string name)
        {
            foreach (var parameter in animator.parameters)
                if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == name) return true;
            return false;
        }

        void TurnTowardPlayer()
        {
            var direction = player.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f) return;
            var target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, 5f * Time.deltaTime);
        }

        static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        // Finds the student's avatar: the café movement script first, then the usual fallbacks.
        static Transform FindPlayer()
        {
            foreach (var script in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (script.GetType().Name == "BoZoPlayerMovement") return script.transform;

            var tagged = GameObject.FindWithTag("Player");
            if (tagged != null) return tagged.transform;

            var controller = FindAnyObjectByType<CharacterController>();
            if (controller != null) return controller.transform;

            return Camera.main != null ? Camera.main.transform : null;
        }

        void OnDrawGizmosSelected()
        {
            // Shows the talk distance as a circle in the Scene view.
            Gizmos.color = new Color(0.86f, 0.47f, 0.31f);
            Gizmos.DrawWireSphere(transform.position, talkDistance);
        }
    }
}
