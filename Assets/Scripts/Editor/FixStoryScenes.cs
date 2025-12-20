using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quick fix tool for story scenes: adds cameras and fixes text alpha
/// </summary>
public class FixStoryScenes : EditorWindow
{
    [MenuItem("Tools/Fix Story Scenes (Camera + Text Alpha)")]
    static void FixAllStoryScenes()
    {
        if (EditorUtility.DisplayDialog(
            "Fix Story Scenes",
            "This will:\n" +
            "• Add Camera to all dialogue/story scenes (fixes 'No cameras rendering')\n" +
            "• Fix text alpha to ensure proper fade-in animation\n\n" +
            "Continue?",
            "Yes, Fix",
            "Cancel"))
        {
            FixScenes();
        }
    }

    static void FixScenes()
    {
        string[] sceneNames = new string[]
        {
            "Assets/Scenes/Dialogue_Prologue.unity",
            "Assets/Scenes/Dialogue_Transition1.unity",
            "Assets/Scenes/Dialogue_Transition2.unity",
            "Assets/Scenes/Dialogue_FinalReveal.unity",
            "Assets/Scenes/StoryImage_PreDungeon.unity",
            "Assets/Scenes/StoryImage_PreIce.unity",
            "Assets/Scenes/StoryImage_PreGreen.unity",
            "Assets/Scenes/StoryImage_Reveal.unity"
        };

        foreach (string scenePath in sceneNames)
        {
            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogWarning($"Scene not found: {scenePath}");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            // Add camera if missing
            Camera cam = GameObject.FindObjectOfType<Camera>();
            if (cam == null)
            {
                GameObject cameraObj = new GameObject("Camera");
                cam = cameraObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                cam.orthographic = true;
                cam.orthographicSize = 5;
                cam.transform.position = new Vector3(0, 0, -10);
                
                Debug.Log($"✓ Added Camera to {scene.name}");
            }
            
            // Fix text alpha in StoryImage scenes
            if (scenePath.Contains("StoryImage"))
            {
                StoryImageSceneController controller = GameObject.FindObjectOfType<StoryImageSceneController>();
                if (controller != null)
                {
                    // Use reflection to get private fields
                    var titleTextField = typeof(StoryImageSceneController).GetField("titleText",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var subtitleTextField = typeof(StoryImageSceneController).GetField("subtitleText",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var imageField = typeof(StoryImageSceneController).GetField("storyImageDisplay",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    TextMeshProUGUI titleText = titleTextField?.GetValue(controller) as TextMeshProUGUI;
                    TextMeshProUGUI subtitleText = subtitleTextField?.GetValue(controller) as TextMeshProUGUI;
                    UnityEngine.UI.Image storyImage = imageField?.GetValue(controller) as UnityEngine.UI.Image;
                    
                    bool anyFixed = false;
                    
                    if (titleText != null)
                    {
                        Color c = titleText.color;
                        c.a = 0f;
                        titleText.color = c;
                        EditorUtility.SetDirty(titleText);
                        anyFixed = true;
                    }
                    
                    if (subtitleText != null)
                    {
                        Color c = subtitleText.color;
                        c.a = 0f;
                        subtitleText.color = c;
                        EditorUtility.SetDirty(subtitleText);
                        anyFixed = true;
                    }
                    
                    if (storyImage != null)
                    {
                        Color c = storyImage.color;
                        c.a = 0f;
                        storyImage.color = c;
                        EditorUtility.SetDirty(storyImage);
                        anyFixed = true;
                    }
                    
                    if (anyFixed)
                    {
                        Debug.Log($"✓ Fixed text/image alpha in {scene.name}");
                    }
                }
            }
            
            // Save scene
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        
        EditorUtility.DisplayDialog(
            "Complete!",
            "All story scenes have been fixed:\n" +
            "• Cameras added\n" +
            "• Text alpha set to 0 for proper fade-in",
            "OK");
    }
}

