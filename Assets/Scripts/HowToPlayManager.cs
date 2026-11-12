
using UnityEngine;
using TMPro;

public class HowToPlayManager : MonoBehaviour {
    public GameObject tutorialPanel;

    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text pageNumberText;

    public GameObject previousButton;
    public GameObject nextButton;

    private int currentPage = 0;

    private string[] titles = {
        "Welcome to Café La Esquina",
        "Moving Around",
        "Interacting with Objects",
        "Learning Spanish",
        "Completing Lessons",
        "Saving and Pausing"
    };

    private string[] descriptions = {
        "Welcome to Café La Esquina! Explore a virtual Spanish café and practice your Spanish through interactive lessons.",

        "Use your keyboard to move around the café. Explore the environment and find different activities to complete.",

        "Approach objects and characters in the café to interact with them. Follow the instructions shown during each activity.",

        "Practice Spanish vocabulary and conversations through café activities. Read the prompts and choose the correct responses.",

        "Complete the activities in each lesson to make progress. Your completed lessons can be tracked and saved.",

        "Press ESC during gameplay to open the pause menu. From there, you can resume, save your progress, change settings, or return to the main menu."
    };

    public void OpenTutorial() {
        currentPage = 0;

        tutorialPanel.SetActive(true);

        UpdatePage();
    }

    public void CloseTutorial() {
        tutorialPanel.SetActive(false);
    }

    public void NextPage() {
        if (currentPage < titles.Length - 1) {
            currentPage++;

            UpdatePage();
        }
    }

    public void PreviousPage() {
        if (currentPage > 0) {
            currentPage--;

            UpdatePage();
        }
    }

    public void UpdatePage() {
        titleText.text = titles[currentPage];

        descriptionText.text = descriptions[currentPage];

        pageNumberText.text = (currentPage + 1) + " / " + titles.Length;

        previousButton.SetActive(currentPage > 0);

        nextButton.SetActive(currentPage < titles.Length - 1);
    }
}
