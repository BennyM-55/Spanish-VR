
using UnityEngine;

public class LessonManager : MonoBehaviour {
    public static LessonManager instance;

    public int totalUnits = 4;
    public int lessonsPerUnit = 5;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void SelectUnit(int unit) {
        if (unit >= 1 && unit <= totalUnits) {
            GameManager.instance.SelectUnit(unit);
            Debug.Log("Unit " + unit + " selected");
        } else {
            Debug.Log("Invalid unit");
        }
    }

    public void SelectLesson(int lesson) {
        if (lesson >= 1 && lesson <= lessonsPerUnit) {
            GameManager.instance.SelectLesson(lesson);
            Debug.Log("Lesson " + lesson + " selected");
        } else {
            Debug.Log("Invalid lesson");
        }
    }

    public void CompleteLesson() {
        int unit = GameManager.instance.currentUnit;
        int lesson = GameManager.instance.currentLesson;

        string lessonID = unit + "-" + lesson;

        if (!SaveGameManager.instance.gameData.completedLessons.Contains(lessonID)) {
            SaveGameManager.instance.gameData.completedLessons.Add(lessonID);

            Debug.Log("Lesson completed: " + lessonID);
        }
    }

    public bool IsLessonCompleted(int unit, int lesson) {
        string lessonID = unit + "-" + lesson;

        return SaveGameManager.instance.gameData.completedLessons.Contains(lessonID);
    }

    public float GetCompletionPercentage() {
        int totalLessons = totalUnits * lessonsPerUnit;

        int completed = SaveGameManager.instance.gameData.completedLessons.Count;

        if (totalLessons == 0) {
            return 0;
        }

        return (float)completed / totalLessons * 100;
    }

    public void SaveLessonProgress() {
        GameManager.instance.SaveProgress();

        Debug.Log("Lesson progress saved");
    }
}

