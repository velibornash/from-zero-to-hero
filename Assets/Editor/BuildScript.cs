using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class BuildScript
{
    public static void BuildMac()
    {
        BuildPlayer(BuildTarget.StandaloneOSX);
    }

    public static void BuildWebGL()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            Debug.LogError("WebGL build target not supported — install WebGL module via Unity Hub");
            return;
        }
        BuildPlayer(BuildTarget.WebGL);
    }

    static void BuildPlayer(BuildTarget target)
    {
        var suffix = target == BuildTarget.WebGL ? "" : ".app";
        var buildPath = $"Build/Village{suffix}";

        BuildPipeline.BuildPlayer(
            new BuildPlayerOptions {
                scenes = new[] { "Assets/Village.unity" },
                locationPathName = buildPath,
                target = target,
                options = BuildOptions.None
            }
        );

        Debug.Log($"Build completed: {buildPath}");
    }
}
