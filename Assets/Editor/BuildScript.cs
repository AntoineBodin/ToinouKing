using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildScript
{
    [MenuItem("Tools/Build WebGL for GHPages")]
    public static void BuildWebGLGithubPages()
    {
        BuildWithProfile("GHPages");
    }

    [MenuItem("Tools/Build WebGL for itch io")]
    public static void BuildWebGLItchio()
    {
        BuildWithProfile("ItchIo");
    }

    private static void BuildWithProfile(string buildProfileAssetName)
    {
        var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>($"Assets/BuildProfiles/{buildProfileAssetName}.asset");
        if (profile == null)
        {
            Debug.LogError("Build Profile not found");
            return;
        }

        PlayerSettings.WebGL.compressionFormat = profile.compressionFormat;

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            locationPathName = profile.outputPath,
            target = profile.targetPlatform,
            scenes = new[] { "Assets/Scenes/MainGame.unity" }
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Build failed");
        }
    }
}