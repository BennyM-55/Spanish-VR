# Developer Guide

## Project orientation

SpanishCafe is a Unity application built around a café scene, player navigation, NPC interaction, guided dialogue, and environmental audio. This guide focuses on **project organization and workflows**, not detailed C# script documentation.

## Important Unity content

* `Scenes/Cafe.unity` — main scene in the supplied archive
* `Scenes/BasicScene.unity` — additional supplied scene
* `Scenes/SampleScene.unity` — additional supplied scene
* Mexican Restaurant package — supplied restaurant environment assets

These are paths within the reviewed archives. Their locations within a complete Unity repository may differ.

## Editing the environment

1. Open the complete Unity project in its configured Editor version.
2. Open the café scene.
3. Use the Scene and Hierarchy views to inspect or position objects.
4. Save the scene and run Play mode to check changes.

## Extending conversations

When adding a new character or learning scenario, follow the existing conversation setup in the complete project, create appropriate Spanish dialogue and response choices, then check that the conversation starts, advances, and ends as intended.

## Maintaining documentation

Update the [Features](Features.md), [User Manual](User_Manual.md), and [Testing](Testing.md) files whenever player-facing behavior changes. Move future functionality into the implemented sections only after it is working.

## 

