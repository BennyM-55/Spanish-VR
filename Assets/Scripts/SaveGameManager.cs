
using System;
using System.IO;
using UnityEngine;

public class SaveGameManager : MonoBehaviour {
    public static SaveGameManager instance;

    public GameData gameData = new GameData();

    public int currentSlot = 1;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void NewGame() {
        gameData = new GameData();
    }

    public void SaveGame() {
        string path = GetSavePath(currentSlot);

        gameData.lastSaved = DateTime.Now.ToString();

        string json = JsonUtility.ToJson(gameData, true);

        File.WriteAllText(path, json);

        Debug.Log("Game saved to slot " + currentSlot);
    }

    public void LoadGame(int slot) {
        string path = GetSavePath(slot);

        if (File.Exists(path)) {
            string json = File.ReadAllText(path);

            gameData = JsonUtility.FromJson<GameData>(json);

            currentSlot = slot;

            Debug.Log("Game loaded from slot " + slot);
        } else {
            Debug.Log("No save file found in slot " + slot);
        }
    }

    public bool SaveExists(int slot) {
        return File.Exists(GetSavePath(slot));
    }

    public void DeleteSave(int slot) {
        string path = GetSavePath(slot);

        if (File.Exists(path)) {
            File.Delete(path);

            Debug.Log("Save slot " + slot + " deleted");
        }
    }

    string GetSavePath(int slot) {
        return Path.Combine(Application.persistentDataPath,
                            "saveSlot" + slot + ".json");
    }
}
