using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData {
    public string playerName = "Player";

    public int selectedCharacter = 0;

    public int currentUnit = 1;
    public int currentLesson = 1;

    public List<string> completedLessons = new List<string>();

    public float positionX = 0;
    public float positionY = 0;
    public float positionZ = 0;

    public string lastSaved = "";
}

