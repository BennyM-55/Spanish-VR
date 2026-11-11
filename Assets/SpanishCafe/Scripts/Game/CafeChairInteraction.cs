using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpanishCafe
{
    [DisallowMultipleComponent]
    public class CafeChairInteraction : MonoBehaviour
    {
        public Transform player;
        public Transform seatPoint;
        public Transform standPoint;
        public Animator playerAnimator;
        public float interactionDistance = 2f;
        public string sittingParameter = "IsSitting";

        private MonoBehaviour movementScript;
        private CharacterController characterController;
        private bool wasMovementEnabled;
        private bool wasControllerEnabled;
        private bool sitting;
        private Vector3 previousPosition;
        private Quaternion previousRotation;

        void Start()
        {
            if (player == null)
            {
                foreach (var component in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                {
                    if (component.GetType().Name == "BoZoPlayerMovement")
                    {
                        player = component.transform;
                        break;
                    }
                }
                if (player == null)
                {
                    var tagged = GameObject.FindWithTag("Player");
                    if (tagged != null) player = tagged.transform;
                }
            }

            if (player == null) return;
            characterController = player.GetComponent<CharacterController>();
            if (playerAnimator == null) playerAnimator = player.GetComponentInChildren<Animator>();
            foreach (var component in player.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component.GetType().Name == "BoZoPlayerMovement")
                {
                    movementScript = component;
                    break;
                }
            }
        }

        void Update()
        {
            if (player == null || seatPoint == null || !SitKeyPressed()) return;
            if (CafeDialogueUI.Instance.IsOpen) return;

            if (sitting)
            {
                Stand();
                return;
            }

            Vector3 delta = player.position - seatPoint.position;
            delta.y = 0;
            if (delta.magnitude <= interactionDistance) Sit();
        }

        void Sit()
        {
            previousPosition = player.position;
            previousRotation = player.rotation;
            wasMovementEnabled = movementScript != null && movementScript.enabled;
            wasControllerEnabled = characterController != null && characterController.enabled;

            if (movementScript != null) movementScript.enabled = false;
            if (characterController != null) characterController.enabled = false;

            player.SetPositionAndRotation(seatPoint.position, seatPoint.rotation);
            SetSittingAnimation(true);
            sitting = true;
        }

        void Stand()
        {
            SetSittingAnimation(false);
            player.SetPositionAndRotation(
                standPoint != null ? standPoint.position : previousPosition,
                standPoint != null ? standPoint.rotation : previousRotation
            );
            if (characterController != null) characterController.enabled = wasControllerEnabled;
            if (movementScript != null) movementScript.enabled = wasMovementEnabled;
            sitting = false;
        }

        void SetSittingAnimation(bool value)
        {
            if (playerAnimator == null || playerAnimator.runtimeAnimatorController == null) return;
            foreach (var parameter in playerAnimator.parameters)
            {
                if (parameter.name == sittingParameter && parameter.type == AnimatorControllerParameterType.Bool)
                {
                    playerAnimator.SetBool(sittingParameter, value);
                    return;
                }
            }
        }

        static bool SitKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.F);
#else
            return false;
#endif
        }
    }
}
