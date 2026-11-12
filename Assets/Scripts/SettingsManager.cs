
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    public static SettingsManager instance;

    public float masterVolume = 1f;
    public bool isFullscreen = true;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
        } else {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float volume) {
        masterVolume = Mathf.Clamp01(volume);

        AudioListener.volume = masterVolume;

        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();

        Debug.Log("Volume set to " + masterVolume);
    }

    public void SetFullscreen(bool fullscreen) {
        isFullscreen = fullscreen;

        Screen.fullScreen = isFullscreen;

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Fullscreen set to " + isFullscreen);
    }

    public void ToggleFullscreen() {
        SetFullscreen(!isFullscreen);
    }

    public void LoadSettings() {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        AudioListener.volume = masterVolume;
        Screen.fullScreen = isFullscreen;

        Debug.Log("Settings loaded");
    }

    public void ResetSettings() {
        SetVolume(1f);
        SetFullscreen(true);

        Debug.Log("Settings reset to default");
    }
}
