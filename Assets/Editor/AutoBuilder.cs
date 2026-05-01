using UnityEditor;
using UnityEngine;
using System.Linq;

public class AutoBuilder
{
    [MenuItem("Build/Build Android APK")]
    public static void BuildAPK()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes enabled in Build Settings!");
            return;
        }

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = scenes;
        options.locationPathName = @"C:\Users\BukhtawerKhalid\Desktop\HenAndChicken.apk";
        options.target = BuildTarget.Android;
        options.options = BuildOptions.None;

        Debug.Log("Starting Android APK build to Desktop...");
        var report = BuildPipeline.BuildPlayer(options);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("APK built successfully at: " + options.locationPathName);
        }
        else
        {
            Debug.LogError("APK build failed. Please make sure Android Build Support is installed in Unity Hub.");
        }
    }
}
