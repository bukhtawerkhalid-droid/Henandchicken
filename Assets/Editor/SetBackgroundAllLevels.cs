using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class SetBackgroundAllLevels : EditorWindow
{
    [MenuItem("Tools/Set Background All Levels")]
    public static void DoIt()
    {
        // 1. Load the Background sprite safely
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Background.png");
        Sprite bgSprite = null;
        foreach (Object a in assets)
        {
            if (a is Sprite)
            {
                bgSprite = a as Sprite;
                break;
            }
        }

        if (bgSprite == null)
        {
            Debug.LogError("Could not find Sprite at Assets/Sprites/Background.png");
            return;
        }

        // 2. Find all level scenes
        string[] scenePaths = Directory.GetFiles("Assets/Scenes", "Level*.unity");
        
        int changedCount = 0;
        foreach (string path in scenePaths)
        {
            // Skip the active scene for a moment, we will handle it at the end to avoid losing unsaved changes
            if (path.Replace('\\', '/') == EditorSceneManager.GetActiveScene().path.Replace('\\', '/'))
                continue;

            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if (ProcessScene(bgSprite))
            {
                EditorSceneManager.SaveScene(scene);
                changedCount++;
            }
        }

        // 3. Process the active scene (which is now the last one opened, or the original one)
        if (ProcessScene(bgSprite))
        {
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            changedCount++;
        }

        Debug.Log($"Successfully updated background in {changedCount} scenes.");
    }

    private static bool ProcessScene(Sprite bgSprite)
    {
        bool changed = false;
        GameObject bgObj = GameObject.Find("BackgroundObject");
        
        if (bgObj != null)
        {
            SpriteRenderer sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != bgSprite)
            {
                sr.sprite = bgSprite;
                EditorUtility.SetDirty(bgObj);
                changed = true;
            }
        }
        return changed;
    }
}
