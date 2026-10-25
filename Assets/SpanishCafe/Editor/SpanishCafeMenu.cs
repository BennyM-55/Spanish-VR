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
            // A simple stand in until the team has a real character behind the counter.
            var lupita = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            lupita.name = "Lupita (placeholder)";

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) lupita.transform.position = sceneView.pivot;

            lupita.AddComponent<CafeConversationStarter>();
            Undo.RegisterCreatedObjectUndo(lupita, "Create Placeholder Lupita");
            Selection.activeGameObject = lupita;
            Debug.Log("[SpanishCafe] Placed Lupita where the Scene view is looking. Move her behind the counter.");
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
