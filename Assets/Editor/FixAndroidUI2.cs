using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixAndroidUI2 : EditorWindow
{
    [MenuItem("Tools/Fix Android UI 2")]
    public static void DoIt()
    {
        bool fixedSomething = false;

        Image[] images = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image img in images)
        {
            img.preserveAspect = true;
            EditorUtility.SetDirty(img);
            fixedSomething = true;
            Debug.Log("Enabled Preserve Aspect on Image: " + img.gameObject.name);
        }

        if (fixedSomething)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("Android UI Fix 2 complete!");
        }
        else
        {
            Debug.LogWarning("Could not find any Image to fix.");
        }
    }
}
