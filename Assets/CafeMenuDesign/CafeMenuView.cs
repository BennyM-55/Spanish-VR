using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CafeMenuView : MonoBehaviour
{
    public MainMenuManager mainMenu;
    public PauseMenuManager pauseMenu;
    public HowToPlayManager tutorial;
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public TMP_Text volumeText;
    public TMP_Text fullscreenMark;
    public TMP_Text nextLabel;
    public TMP_Text unitLabel;
    public Button previousButton;
    public Button nextButton;
    public Button saveButton;
    public Image[] pageBars;
    public Button[] unitButtons;
    bool settingsWasOpen, tutorialWasOpen, wasPaused;
    CursorLockMode previousCursorLock;
    bool previousCursorVisible;
    static readonly Color Gold = new Color32(221, 173, 89, 255);
    static readonly Color Green = new Color32(39, 72, 61, 255);

    void LateUpdate()
    {
        bool settingsOpen = settingsPanel != null && settingsPanel.activeInHierarchy;
        if (settingsOpen && !settingsWasOpen) RefreshSettings();
        settingsWasOpen = settingsOpen;
        bool tutorialOpen = tutorial != null && tutorial.tutorialPanel.activeInHierarchy;
        if (tutorialOpen && !tutorialWasOpen) tutorial.OpenTutorial();
        tutorialWasOpen = tutorialOpen;
        if (tutorialOpen) RefreshTutorial();
        if (pauseMenu != null && pauseMenu.isPaused != wasPaused)
        {
            if (pauseMenu.isPaused)
            {
                previousCursorLock = Cursor.lockState;
                previousCursorVisible = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = previousCursorLock;
                Cursor.visible = previousCursorVisible;
            }
            wasPaused = pauseMenu.isPaused;
        }
        if (saveButton != null) saveButton.interactable = GameManager.instance != null && SaveGameManager.instance != null;
        if (unitLabel != null && GameManager.instance != null)
        {
            int selected = GameManager.instance.currentUnit;
            unitLabel.text = "UNIT " + selected + " SELECTED";
            for (int i = 0; i < unitButtons.Length; i++)
            {
                var colors = unitButtons[i].colors;
                bool active = i + 1 == selected;
                colors.normalColor = active ? Gold : Green;
                colors.highlightedColor = active ? new Color32(237, 191, 112, 255) : new Color32(53, 92, 76, 255);
                colors.selectedColor = colors.highlightedColor;
                colors.pressedColor = active ? new Color32(202, 152, 71, 255) : new Color32(29, 59, 48, 255);
                unitButtons[i].colors = colors;
                unitButtons[i].GetComponentInChildren<TMP_Text>().color = i + 1 == selected ? new Color32(8, 32, 25, 255) : new Color32(255, 238, 210, 255);
            }
        }
    }

    public void RefreshSettings()
    {
        float volume = SettingsManager.instance != null ? SettingsManager.instance.masterVolume : PlayerPrefs.GetFloat("MasterVolume", 1);
        bool fullscreen = SettingsManager.instance != null ? SettingsManager.instance.isFullscreen : Screen.fullScreen;
        volumeSlider.SetValueWithoutNotify(volume);
        volumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        fullscreenMark.text = fullscreen ? "ON" : "OFF";
    }
    public void SetVolume(float value)
    {
        if (SettingsManager.instance != null) SettingsManager.instance.SetVolume(value);
        else { AudioListener.volume = value; PlayerPrefs.SetFloat("MasterVolume", value); PlayerPrefs.Save(); }
        volumeText.text = Mathf.RoundToInt(value * 100) + "%";
    }
    public void ToggleFullscreen()
    {
        if (SettingsManager.instance != null) SettingsManager.instance.ToggleFullscreen();
        else { Screen.fullScreen = !Screen.fullScreen; PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0); PlayerPrefs.Save(); }
        RefreshSettings();
    }
    public void ResetSettings()
    {
        if (SettingsManager.instance != null) SettingsManager.instance.ResetSettings();
        else { AudioListener.volume = 1; Screen.fullScreen = true; PlayerPrefs.SetFloat("MasterVolume", 1); PlayerPrefs.SetInt("Fullscreen", 1); PlayerPrefs.Save(); }
        RefreshSettings();
    }
    public void RefreshTutorial()
    {
        int page = 1;
        int.TryParse(tutorial.pageNumberText.text.Split('/')[0].Trim(), out page);
        page = Mathf.Clamp(page, 1, 6);
        if (page == 1) tutorial.descriptionText.text = "Explore a virtual Spanish café and practice your Spanish through interactive lessons.";
        previousButton.gameObject.SetActive(true);
        previousButton.interactable = page > 1;
        nextButton.gameObject.SetActive(true);
        nextLabel.text = page == 6 ? "Done" : "Next  >";
        for (int i = 0; i < pageBars.Length; i++) pageBars[i].color = i < page ? Gold : new Color32(68, 86, 77, 255);
    }
    public void NextTutorial()
    {
        if (tutorial.pageNumberText.text.StartsWith("6")) Back();
        else { tutorial.NextPage(); RefreshTutorial(); }
    }
    public void PreviousTutorial() { tutorial.PreviousPage(); RefreshTutorial(); }
    public void Back()
    {
        if (mainMenu != null) mainMenu.ShowMainMenu();
        else if (pauseMenu != null) pauseMenu.BackToPauseMenu();
    }
    public void ReturnToMainMenu()
    {
        if (pauseMenu != null) pauseMenu.ResumeGame();
        if (SceneTransitionManager.instance != null) SceneTransitionManager.instance.ReturnToMainMenu();
        else SceneManager.LoadScene("MainMenu");
    }
}
