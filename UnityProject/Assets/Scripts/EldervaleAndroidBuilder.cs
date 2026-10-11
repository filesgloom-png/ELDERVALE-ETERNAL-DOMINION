#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class EldervaleAndroidBuilder
{
    public static void Build()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.companyName = "Filesgloom";
        PlayerSettings.productName = "ELDERVALE: ETERNAL DOMINION";
        PlayerSettings.applicationIdentifier = "com.filesgloom.eldervaledominion";
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            locationPathName = "Build/ELDERVALE-ETERNAL-DOMINION.apk",
            target = BuildTarget.Android,
            options = BuildOptions.None
        };
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Unity Android build failed: " + report.summary.result);
        Debug.Log("Android ARM64 APK built: " + report.summary.totalSize + " bytes");
    }
}
#endif
