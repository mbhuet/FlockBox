using System;
using System.Collections.Generic;
using UnityEditor;

class MyEditorScript
{
    static string[] SCENES = FindEnabledEditorScenes();

    static string APP_NAME = "FlockBox";
    static string TARGET_DIR = "CIBuilds";

    [MenuItem("Custom/CI/Build Windows")]
    static void PerformWindowsBuild()
    {
        string target_dir = APP_NAME + ".exe";
        GenericBuild(SCENES, TARGET_DIR + "/" + target_dir, BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone, BuildOptions.None);
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

    static void GenericBuild(string[] scenes, string target_dir, BuildTarget build_target, BuildTargetGroup group, BuildOptions build_options)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(group, build_target);
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.options = build_options;
        options.target = build_target;
        options.scenes = scenes;
        options.locationPathName = target_dir;
        UnityEditor.Build.Reporting.BuildReport res = BuildPipeline.BuildPlayer(options);
        UnityEngine.Debug.Log(res);
        if (res.summary.result == UnityEditor.Build.Reporting.BuildResult.Failed)
        {
            throw new Exception("BuildPlayer failure: " + res);
        }
    }
}