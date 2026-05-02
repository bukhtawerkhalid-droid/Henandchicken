using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UpdateWinUI : EditorWindow
{
    [MenuItem("Tools/Update Win UI (100 Levels)")]
    public static void UpdateUI()
    {
        string youWinPath = "Assets/Sprites/you win.png";
        string nextBtnPath = "Assets/Sprites/Next button.png";

        // 1. Fix Sprite Import Settings
        FixSpriteImport(youWinPath);
        FixSpriteImport(nextBtnPath);

        // 2. Load the Sprites
        Sprite youWinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(youWinPath);
        Sprite nextBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(nextBtnPath);

        if (youWinSprite == null || nextBtnSprite == null)
        {
            Debug.LogError("Could not load sprites! Check paths.");
            return;
        }

        // 3. Process Levels
        int updatedCount = 0;
        for (int i = 1; i <= 100; i++)
        {
            string scenePath = $"Assets/Scenes/Level{i}.unity";
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            bool modified = ProcessScene(youWinSprite, nextBtnSprite);

            if (modified)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                updatedCount++;
                Debug.Log($"Successfully updated {scene.name}");
            }
        }

        Debug.Log($"Finished! Updated Win UI in {updatedCount} levels.");
    }

    private static void FixSpriteImport(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;
            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }
            
            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }

    private static bool ProcessScene(Sprite youWin, Sprite nextBtn)
    {
        // Find WinPanel (it might be inactive, so we search all Transforms)
        Transform winPanel = null;
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name == "WinPanel" && t.gameObject.scene.isLoaded)
            {
                winPanel = t;
                break;
            }
        }

        if (winPanel == null) return false;

        bool changed = false;

        // --- Process WinText ---
        Transform winTextT = winPanel.Find("WinText");
        if (winTextT != null)
        {
            // Remove text component if it exists
            TextMeshProUGUI tmp = winTextT.GetComponent<TextMeshProUGUI>();
            if (tmp != null) DestroyImmediate(tmp);
            
            Text legacyText = winTextT.GetComponent<Text>();
            if (legacyText != null) DestroyImmediate(legacyText);

            // Ensure there's an Image component
            Image img = winTextT.GetComponent<Image>();
            if (img == null) img = winTextT.gameObject.AddComponent<Image>();

            // Apply sprite
            img.sprite = youWin;
            img.preserveAspect = true;
            img.type = Image.Type.Simple;
            img.color = Color.white;

            // Fix RectTransform
            RectTransform rt = winTextT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(900, 500); // Scaled for portrait awesomeness
            rt.anchoredPosition = new Vector2(0, 300); // Place above the center

            changed = true;
        }

        // --- Process NextButton ---
        Transform nextBtnT = winPanel.Find("NextButton");
        if (nextBtnT != null)
        {
            Image img = nextBtnT.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = nextBtn;
                img.preserveAspect = true;
                img.type = Image.Type.Simple;
                img.color = Color.white;
            }

            // Fix RectTransform
            RectTransform rt = nextBtnT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(500, 190);
            rt.anchoredPosition = new Vector2(0, -300); // Place below the center

            // Hide the child text (usually named Text or Text (TMP))
            if (nextBtnT.childCount > 0)
            {
                Transform childText = nextBtnT.GetChild(0);
                if (childText.GetComponent<TextMeshProUGUI>() != null || childText.GetComponent<Text>() != null)
                {
                    childText.gameObject.SetActive(false);
                }
            }

            changed = true;
        }

        return changed;
    }
}
