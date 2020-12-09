using System;
using System.Collections.Generic;
using UnityEditor;

class MyEditorScript
{
    static string[] SCENES = FindEnabledEditorScenes();

    static string APP_NAME = "YourProject";
    static string TARGET_DIR = "target";

    [MenuItem("Custom/CI/Build Mac OS X")]
    static void PerformMacOSXBuild()
    {
        string target_dir = APP_NAME + ".app";
        GenericBuild(SCENES, TARGET_DIR + "/" + target_dir, BuildTarget.StandaloneOSXIntel, BuildOptions.None);
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> EditorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            EditorScenes.Add(scene.path);
        }
        return EditorScenes.ToArray();
    }

    static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildOptions build_options)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(build_target);
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.options = build_options;
        options.target = build_target;
        options.scenes = scenes;
        UnityEditor.Build.Reporting.BuildReport res = BuildPipeline.BuildPlayer(options);
        if (res.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            throw new Exception("BuildPlayer failure: " + res);
        }
    }
}