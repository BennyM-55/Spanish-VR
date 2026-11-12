
using UnityEngine;

public class PauseMenuManager : MonoBehaviour {
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject howToPlayPanel;

    public bool isPaused = false;

    void Start() {
        ResumeGame();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (isPaused) {
                ResumeGame();
            } else {
                PauseGame();
            }
        }
    }

    public void PauseGame() {
        isPaused = true;

        Time.timeScale = 0;

        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);

        Debug.Log("Game paused");
    }

    public void ResumeGame() {
        isPaused = false;

        Time.timeScale = 1;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);

        Debug.Log("Game resumed");
    }

    public void OpenSettings() {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OpenHowToPlay() {
        pausePanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void BackToPauseMenu() {
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        howToPlayPanel.SetActive(false);
    }

    public void SaveGame() {
        GameManager.instance.SaveProgress();

        Debug.Log("Game saved from pause menu");
    }

    public void ReturnToMainMenu() {
        ResumeGame();

        SceneTransitionManager.instance.ReturnToMainMenu();
    }
}
