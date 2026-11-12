
using UnityEngine;

public class MainMenuManager : MonoBehaviour {
    public GameObject mainPanel;
    public GameObject howToPlayPanel;
    public GameObject settingsPanel;
    public GameObject startPreviewPanel;
    public GameObject loadGamePanel;
    public GameObject lessonSelectionPanel;
    public GameObject characterSelectionPanel;

    void Start() {
        ShowMainMenu();
    }

    public void ShowMainMenu() {
        mainPanel.SetActive(true);
        howToPlayPanel.SetActive(false);
        settingsPanel.SetActive(false);
        startPreviewPanel.SetActive(false);
        loadGamePanel.SetActive(false);
        lessonSelectionPanel.SetActive(false);
        characterSelectionPanel.SetActive(false);
    }

    public void ShowNewGame() {
        ShowMainMenu();
        mainPanel.SetActive(false);
        startPreviewPanel.SetActive(true);
    }

    public void StartNewGame() {
        SceneTransitionManager.instance.StartNewGame();
    }

    public void ContinueGame() {
        SceneTransitionManager.instance.ContinueGame();
    }

    public void ShowLoadGame() {
        ShowMainMenu();
        mainPanel.SetActive(false);
        loadGamePanel.SetActive(true);
    }

    public void LoadGame(int slot) {
        SceneTransitionManager.instance.LoadSavedGame(slot);
    }

    public void ShowLessonSelection() {
        ShowMainMenu();
        mainPanel.SetActive(false);
        lessonSelectionPanel.SetActive(true);
    }

    public void SelectUnit(int unit) {
        LessonManager.instance.SelectUnit(unit);
    }

    public void SelectLesson(int lesson) {
        LessonManager.instance.SelectLesson(lesson);
    }

    public void StartSelectedLesson() {
        SceneTransitionManager.instance.StartSelectedLesson();
    }

    public void ShowCharacterSelection() {
        ShowMainMenu();
        mainPanel.SetActive(false);
        characterSelectionPanel.SetActive(true);
    }

    public void PreviousCharacter() {
        CharacterSelectionManager.instance.PreviousCharacter();
    }

    public void NextCharacter() {
        CharacterSelectionManager.instance.NextCharacter();
    }

    public void SaveCharacter() {
        CharacterSelectionManager.instance.SaveCharacter();
    }

    public void OpenCharacterCreator() {
        SceneTransitionManager.instance.OpenCharacterCreator();
    }

    public void ShowHowToPlay() {
        ShowMainMenu();
        mainPanel.SetActive(false);
        howToPlayPanel.SetActive(true);
    }

    public void ShowSettings() {
        ShowMainMenu();
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ExitGame() {
        SceneTransitionManager.instance.ExitGame();
    }
}
