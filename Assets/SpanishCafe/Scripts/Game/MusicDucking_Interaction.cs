
using UnityEngine;

namespace SpanishCafe
{
    public class MusicDucking_Interaction : MonoBehaviour
    {
        public AudioSource[] backgroundAudio;

        [Range(0f, 1f)] public float normalVolume = 1f;
        [Range(0f, 1f)] public float interactionVolume = 0.05f;

        public float fadeSpeed = 2f;

        private float[] originalVolumes;

        void Start()
        {
            originalVolumes = new float[backgroundAudio.Length];

            for (int i = 0; i < backgroundAudio.Length; i++)
            {
                if (backgroundAudio[i] != null)
                    originalVolumes[i] = backgroundAudio[i].volume;
            }
        }

        void Update()
        {
            bool interacting = CafeDialogueUI.Instance != null
                && CafeDialogueUI.Instance.IsOpen;

            for (int i = 0; i < backgroundAudio.Length; i++)
            {
                if (backgroundAudio[i] == null) continue;

                float targetVolume = interacting
                    ? originalVolumes[i] * interactionVolume
                    : originalVolumes[i] * normalVolume;

                backgroundAudio[i].volume = Mathf.MoveTowards(
                    backgroundAudio[i].volume,
                    targetVolume,
                    fadeSpeed * Time.deltaTime
                );
            }
        }
    }
}
