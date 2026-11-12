
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour {
    public static SceneTransitionManager instance;

    public string mainMenuScene = "MainMenu";
    public string gameScene = "Cafe";
    public string characterCreatorScene = "StylizedCharacterCreator";

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(this);
        }
    }

    public void StartNewGame() {
        GameManager.instance.StartNewGame();
        SceneManager.LoadScene(gameScene);
    }

    public void ContinueGame() {
        if (SaveGameManager.instance.SaveExists(
            SaveGameManager.instance.currentSlot)) {
            GameManager.instance.ContinueGame();
            SceneManager.LoadScene(gameScene);
        } else {
            Debug.Log("No saved game found.");
        }
    }

    public void LoadSavedGame(int slot) {
        if (SaveGameManager.instance.SaveExists(slot)) {
            SaveGameManager.instance.currentSlot = slot;
            ContinueGame();
        } else {
            Debug.Log("Save slot " + slot + " is empty.");
        }
    }

    public void StartSelectedLesson() {
        SceneManager.LoadScene(gameScene);
    }

    public void OpenCharacterCreator() {
        SceneManager.LoadScene(characterCreatorScene);
    }

    public void ReturnToMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ExitGame() {
        Application.Quit();
    }
}


