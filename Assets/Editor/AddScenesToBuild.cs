using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class AddScenesToBuild
{
    [MenuItem("Tools/Add 100 Scenes To Build")]
    public static void AddScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        for (int i = 1; i <= 100; i++)
        {
            string path = "Assets/Scenes/Level" + i + ".unity";
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("Successfully added 100 scenes to Build Settings!");
    }
}
