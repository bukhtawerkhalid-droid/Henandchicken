using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixAndroidUI : EditorWindow
{
    [MenuItem("Tools/Fix Android UI")]
    public static void DoIt()
    {
        bool fixedSomething = false;
        
        // 1. Configure the Canvas Scaler for Android Portrait
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(scaler);
                fixedSomething = true;
                Debug.Log("Set CanvasScaler to 1080x1920 Portrait mode.");
            }
        }

        // 2. Fix the Image component to not stretch
        Image[] images = Object.FindObjectsByType<Image>(FindObjectsSortMode.None);
        foreach (Image img in images)
        {
            if (img.gameObject.name == "Image" || (img.sprite != null && img.sprite.name.Contains("start image")))
            {
                img.preserveAspect = true;
                EditorUtility.SetDirty(img);
                fixedSomething = true;
                Debug.Log("Enabled Preserve Aspect on Image: " + img.gameObject.name);
            }
        }

        if (fixedSomething)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("Android UI Fix complete!");
        }
        else
        {
            Debug.LogWarning("Could not find Canvas or Image to fix.");
        }
    }
}
