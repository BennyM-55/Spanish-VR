using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCreatorBackButton : MonoBehaviour {
    public void ReturnToMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
