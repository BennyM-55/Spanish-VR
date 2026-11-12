
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public int currentUnit = 1;
    public int currentLesson = 1;
    public int selectedCharacter = 0;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void StartNewGame() {
        SaveGameManager.instance.NewGame();

        currentUnit = 1;
        currentLesson = 1;
        selectedCharacter = 0;

        Debug.Log("New game started");
    }

    public void ContinueGame() {
        int slot = SaveGameManager.instance.currentSlot;

        if (SaveGameManager.instance.SaveExists(slot)) {
            SaveGameManager.instance.LoadGame(slot);
            UpdateGameData();

            Debug.Log("Continuing saved game");
        } else {
            Debug.Log("No saved game found");
        }
    }

    public void SelectUnit(int unit) {
        currentUnit = unit;

        SaveGameManager.instance.gameData.currentUnit = unit;

        Debug.Log("Selected unit: " + unit);
    }

    public void SelectLesson(int lesson) {
        currentLesson = lesson;

        SaveGameManager.instance.gameData.currentLesson = lesson;

        Debug.Log("Selected lesson: " + lesson);
    }

    public void SelectCharacter(int character) {
        selectedCharacter = character;

        SaveGameManager.instance.gameData.selectedCharacter = character;

        Debug.Log("Selected character: " + character);
    }

    public void SaveProgress() {
        SaveGameManager.instance.gameData.currentUnit = currentUnit;
        SaveGameManager.instance.gameData.currentLesson = currentLesson;
        SaveGameManager.instance.gameData.selectedCharacter = selectedCharacter;

        SaveGameManager.instance.SaveGame();
    }

    public void UpdateGameData() {
        GameData data = SaveGameManager.instance.gameData;

        currentUnit = data.currentUnit;
        currentLesson = data.currentLesson;
        selectedCharacter = data.selectedCharacter;
    }
}

