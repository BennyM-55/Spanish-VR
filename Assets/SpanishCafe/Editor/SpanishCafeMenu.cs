using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SpanishCafe.EditorTools
{
    // Adds a Spanish Café menu to the Unity menu bar with a few setup helpers
    public static class SpanishCafeMenu
    {
        [UnityEditor.MenuItem("Spanish Café/Add Conversation To Selected Character")]
        static void AddToSelected()
        {
            var target = Selection.activeGameObject;
            if (target == null)
            {
                EditorUtility.DisplayDialog("Spanish Café", "Click the café worker in the Hierarchy first, then try again.", "OK");
                return;
            }

            if (target.GetComponent<CafeConversationStarter>() == null)
                Undo.AddComponent<CafeConversationStarter>(target);

            Debug.Log($"[SpanishCafe] {target.name} can now talk. Walk up to it in Play mode and press E.");
        }

        [UnityEditor.MenuItem("Spanish Café/Create Placeholder Lupita")]
        static void CreatePlaceholder()
        {
            if (StopIfPlaying()) return;

            // A simple stand in until the team has a real character behind the counter.
            var lupita = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lupita.name = "Lupita (placeholder)";

            // 2 meters in front of the camera and a little to the left, so the menu board fits on the right
            var eye = FindEye();
            if (eye != null)
            {
                var forward = FlatForward(eye);
                var spot = eye.position + forward * 2f - Vector3.Cross(Vector3.up, forward) * 0.6f;
                spot.y = 1f; // the capsule is 2 m tall, so this puts her feet on the floor
                lupita.transform.position = spot;
            }

            lupita.AddComponent<CafeConversationStarter>();
            Undo.RegisterCreatedObjectUndo(lupita, "Create Placeholder Lupita");
            Selection.activeGameObject = lupita;
            Debug.Log("[SpanishCafe] Placed Lupita in front of the camera. Move her behind the counter.");
        }

        // The same character kit James used for BoZo. We only make a new copy, his BoZo stays the same
        const string CharacterPrefab = "Assets/BoZo_StylizedModularCharacters/Prefabs/BSMC_CharacterBase.prefab";

        [UnityEditor.MenuItem("Spanish Café/Add Lupita (BoZo character)")]
        static void AddBoZoLupita()
        {
            if (StopIfPlaying()) return;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefab);
            if (prefab == null)
            {
                EditorUtility.DisplayDialog("Spanish Café", "Couldn't find the BoZo character kit. Make sure you're on main with James's café merged in.", "OK");
                return;
            }

            // A new copy of the character, it doesn't have the walking script so she just stands there in idle
            var lupita = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            lupita.name = "Lupita";

            var eye = FindEye();
            if (eye != null)
            {
                var forward = FlatForward(eye);
                var spot = eye.position + forward * 2.5f;

                // Drop her down onto whatever floor is under that spot (start at eye height so we don't hit the ceiling)
                if (Physics.Raycast(spot, Vector3.down, out var hit, 20f)) spot.y = hit.point.y;
                lupita.transform.position = spot;
                lupita.transform.rotation = Quaternion.LookRotation(-forward); // facing the camera
            }

            var starter = lupita.AddComponent<CafeConversationStarter>();
            starter.panelMode = CafeDialogueUI.PanelMode.Auto;

            Undo.RegisterCreatedObjectUndo(lupita, "Add Lupita");
            Selection.activeGameObject = lupita;
            Debug.Log("[SpanishCafe] Added Lupita using the BoZo character kit. Move her behind the counter, BoZo walks up and presses E.");
        }

        [UnityEditor.MenuItem("Spanish Café/Create Menu Board")]
        static void CreateMenuBoard()
        {
            if (StopIfPlaying()) return;

            var board = new GameObject("Menu Board");
            board.AddComponent<CafeMenuBoard>();

            var eye = FindEye();
            if (eye != null)
            {
                var forward = FlatForward(eye);
                var right = Vector3.Cross(Vector3.up, forward);

                // If Lupita is in front of the camera, the board goes behind her and to her side
                // If she's off to the side or not there, it goes in front of the camera on the right
                var worker = Object.FindAnyObjectByType<CafeConversationStarter>();
                Vector3 spot = eye.position + forward * 2.5f + right * 1f;
                if (worker != null)
                {
                    var away = worker.transform.position - eye.position;
                    away.y = 0f;
                    if (away.sqrMagnitude > 0.01f && Vector3.Angle(forward, away) < 45f)
                    {
                        away.Normalize();
                        spot = worker.transform.position + away * 1.2f + Vector3.Cross(Vector3.up, away) * 1.1f;
                    }
                }
                spot.y = 1.7f;
                board.transform.position = spot;

                var facing = spot - eye.position;
                facing.y = 0f;
                if (facing.sqrMagnitude > 0.01f) board.transform.rotation = Quaternion.LookRotation(facing);
            }

            Undo.RegisterCreatedObjectUndo(board, "Create Menu Board");
            Selection.activeGameObject = board;
            Debug.Log("[SpanishCafe] Added the menu board. Move it onto a wall, it shows up when you press Play.");
        }

        // Stuff you add while Play is on gets deleted when you stop, so we stop them first
        static bool StopIfPlaying()
        {
            if (!EditorApplication.isPlaying) return false;
            EditorUtility.DisplayDialog("Spanish Café", "Stop Play first (the ▶ button). Anything you add during Play disappears when it stops.", "OK");
            return true;
        }

        // Use the player's camera if there is one, if not use the Scene view camera
        static Transform FindEye()
        {
            if (Camera.main != null) return Camera.main.transform;
            var sceneView = SceneView.lastActiveSceneView;
            return sceneView != null ? sceneView.camera.transform : null;
        }

        static Vector3 FlatForward(Transform eye)
        {
            var forward = new Vector3(eye.forward.x, 0f, eye.forward.z);
            return forward.sqrMagnitude < 0.01f ? Vector3.forward : forward.normalized;
        }

        [UnityEditor.MenuItem("Spanish Café/Reset Saved Progress")]
        static void ResetProgress()
        {
            if (!EditorUtility.DisplayDialog("Spanish Café", "Erase the finished levels and best scores on this computer?", "Erase", "Cancel")) return;

            CafeContent.EnsureLoaded();
            CafeProgress.ResetAll();
            Debug.Log("[SpanishCafe] Progress erased. Every level starts fresh.");
        }

        [UnityEditor.MenuItem("Spanish Café/Validate Content")]
        static void Validate()
        {
            CafeContent.Reload();
            CafeContent.EnsureLoaded();

            var problems = new List<string>();
            var warnings = new List<string>();

            if (CafeContent.Levels.Length == 0) problems.Add("No levels loaded. Check Levels/levels.json.");

            foreach (var level in CafeContent.Levels)
            {
                var scenario = CafeContent.GetScenario(level);
                if (scenario?.turns == null)
                {
                    problems.Add($"{level.level}: couldn't load Dialogue/{level.scenarioFile}.json");
                    continue;
                }

                var ids = new HashSet<string>();
                foreach (var turn in scenario.turns) ids.Add(turn.id);

                // Check every line points somewhere real and has English.
                foreach (var turn in scenario.turns)
                {
                    if (turn.IsStaff)
                    {
                        if (!turn.end && !ids.Contains(turn.next)) problems.Add($"{turn.id}: next '{turn.next}' doesn't exist");
                        if (string.IsNullOrEmpty(CafeContent.English(turn.id))) warnings.Add($"{turn.id}: no English yet");
                        continue;
                    }

                    if (turn.choices == null || turn.choices.Length == 0)
                    {
                        problems.Add($"{turn.id}: student turn has no choices");
                        continue;
                    }

                    foreach (var choice in turn.choices)
                    {
                        bool movesOn = choice.result == "correct" || choice.result == "awkward";
                        if (movesOn && !ids.Contains(choice.next)) problems.Add($"{choice.id}: next '{choice.next}' doesn't exist");
                        if (string.IsNullOrEmpty(CafeContent.English(choice.id))) warnings.Add($"{choice.id}: no English yet");
                    }
                }
            }

            foreach (var warning in warnings) Debug.LogWarning("[SpanishCafe] " + warning);
            foreach (var problem in problems) Debug.LogError("[SpanishCafe] " + problem);

            string summary = $"{CafeContent.Levels.Length} levels checked: {problems.Count} problems, {warnings.Count} warnings.";
            Debug.Log("[SpanishCafe] " + summary);
            EditorUtility.DisplayDialog("Spanish Café", summary + (problems.Count + warnings.Count > 0 ? "\nDetails are in the Console." : ""), "OK");
        }
    }
}
