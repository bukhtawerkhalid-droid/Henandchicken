using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetBg : EditorWindow
{
    [MenuItem("Tools/Set Background")]
    public static void DoIt()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Start image.png");
        Sprite newSprite = null;
        foreach (Object a in assets)
        {
            if (a is Sprite)
            {
                newSprite = a as Sprite;
                break;
            }
        }
        
        if (newSprite == null)
        {
            Debug.LogError("No sprite found in Start image.png");
            return;
        }

        GameObject bg = GameObject.Find("BackgroundObject");
        if (bg != null)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = newSprite;
                EditorUtility.SetDirty(bg);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("Set sprite successfully to " + sr.sprite.name);
            }
        }
        else
        {
            Debug.LogError("Could not find BackgroundObject in scene");
        }
    }
}
