#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class EldervaleAndroidBuilder
{
    public static void Build()
    {
        EnsureUniversalRenderPipeline();
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

    static void EnsureUniversalRenderPipeline()
    {
        const string assetPath = "Assets/Settings/ELDERVALE_URP.asset";
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(assetPath);
        if (pipeline == null)
        {
            System.IO.Directory.CreateDirectory("Assets/Settings");
            var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, "Assets/Settings/ELDERVALE_Renderer.asset");
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(pipeline, assetPath);
            AssetDatabase.SaveAssets();
        }
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
    }
}
#endif
