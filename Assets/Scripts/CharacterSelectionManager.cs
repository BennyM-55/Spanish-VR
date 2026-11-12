
using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour {
    public static CharacterSelectionManager instance;

    public GameObject[] characters;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SelectCharacter(int characterIndex) {
        if (characterIndex >= 0 && characterIndex < characters.Length) {
            GameManager.instance.SelectCharacter(characterIndex);

            Debug.Log("Character selected: " + characterIndex);
        } else {
            Debug.Log("Invalid character selection");
        }
    }

    public void ShowSelectedCharacter() {
        int selected = GameManager.instance.selectedCharacter;

        for (int i = 0; i < characters.Length; i++) {
            if (characters[i] != null) {
                characters[i].SetActive(i == selected);
            }
        }
    }

    public void NextCharacter() {
        if (characters.Length == 0) {
            return;
        }

        int next = GameManager.instance.selectedCharacter + 1;

        if (next >= characters.Length) {
            next = 0;
        }

        SelectCharacter(next);
        ShowSelectedCharacter();
    }

    public void PreviousCharacter() {
        if (characters.Length == 0) {
            return;
        }

        int previous = GameManager.instance.selectedCharacter - 1;

        if (previous < 0) {
            previous = characters.Length - 1;
        }

        SelectCharacter(previous);
        ShowSelectedCharacter();
    }

    public void SaveCharacter() {
        GameManager.instance.SaveProgress();

        Debug.Log("Character selection saved");
    }
}

