using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[InitializeOnLoad]
public class DOTSToggleEditor : Editor
{
#if UNITY_2020_1_OR_NEWER
    private const string dotsToggleMenuPath = "FlockBox/Enable DOTS";
    private const string dotsPackageMenuPath = "FlockBox/Import DOTS Packages";
#else
    private const string dotsToggleMenuPath = "FlockBox/Enable DOTS (2020.1+)";
    private const string dotsPackageMenuPath = "FlockBox/Import DOTS Packages (2020.1+)";
#endif
    private const string prefsKey = "FLOCKBOX_DOTS_TOGGLE";
    private const string dotsDefine = "FLOCKBOX_DOTS";

    private static readonly string[] packages =
    {
        "com.unity.burst@1.4.4",
        "com.unity.entities@0.17.0-preview.41",
        "com.unity.mathematics@1.2.1",
        "com.unity.physics@0.6.0-preview.3",
        "com.unity.rendering.hybrid@0.11.0-preview.42",
        "com.unity.dots.editor@0.12.0-preview.6",
    };

    static DOTSToggleEditor()
    {
        UpdateScriptingDefines(IsEnabled);
    }

    public static bool IsEnabled
    {
        get
        {
#if UNITY_2020_1_OR_NEWER
            return EditorPrefs.GetBool(prefsKey, false);
#else
            return false;
#endif
        }
        set { EditorPrefs.SetBool(prefsKey, value); }
    }

    [MenuItem(dotsToggleMenuPath)]
    private static void ToggleAction()
    {
        IsEnabled = !IsEnabled;

        if (IsEnabled && !AllPackagesAlreadyImported())
        {
            DisplayPackageInstallPrompt();
        }

        UpdateScriptingDefines(IsEnabled);
    }

    [MenuItem(dotsPackageMenuPath)]
    private static void ImportPackages()
    {
        if (AllPackagesAlreadyImported())
        {
            DisplayPackagesAlreadyInstalledDialog();
        }
        else
        {
            DisplayPackageInstallPrompt();
        }
    }

    private static void UpdateScriptingDefines(bool isEnabled)
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var group = BuildPipeline.GetBuildTargetGroup(target);

        string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        List<string> allDefines = definesString.Split(';').ToList();
        if (isEnabled)
        {
            if (!allDefines.Contains(dotsDefine))
            {
                allDefines.Add(dotsDefine);
            }
        }
        else
        {
            allDefines.Remove(dotsDefine);
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
    }

    [MenuItem(dotsToggleMenuPath, true)]
    private static bool ToggleActionValidate()
    {
        Menu.SetChecked(dotsToggleMenuPath, IsEnabled);
#if UNITY_2020_1_OR_NEWER
        return true;
#else
        return false;
#endif
    }

    [MenuItem(dotsPackageMenuPath, true)]
    private static bool ImportPackagesValidate()
    {
#if UNITY_2020_1_OR_NEWER
        return true;
#else
    return false;
#endif        
    }

    private static string GetPackageList()
    {
        string str = "";
        foreach(string packageName in packages)
        {
            str += packageName + "\n";
        }
        return str;
    }


    private static void DisplayPackagesAlreadyInstalledDialog()
    {
        EditorUtility.DisplayDialog("FlockBox", "The Following DOTS packages are already imported. \n\n" + GetPackageList(), "OK");
    }

    private static void DisplayPackageInstallPrompt()
    {

        if(EditorUtility.DisplayDialog("FlockBox", "Import the following packages? \n\n" + GetPackageList(), "OK", "Cancel"))
        {
            InstallDOTSPackages();
        }
    }


    private static bool AllPackagesAlreadyImported()
    {
        List<string> packagesToImport = packages.ToList();
        var request = Client.List();
        while (!request.IsCompleted)
        {

        }

        PackageCollection collection = request.Result;
        foreach(UnityEditor.PackageManager.PackageInfo info in collection.ToArray())
        {
            if (packagesToImport.Contains(info.packageId))
            {
                packagesToImport.Remove(info.packageId);
            }
        }

        return packagesToImport.Count == 0;
    }

    private static void InstallDOTSPackages()
    {
        for (int i = 0; i<packages.Length; i++)
        {
            string package = packages[i];
            var request = Client.Add(package);
            while (!request.IsCompleted)
            {
                EditorUtility.DisplayProgressBar("FlockBox", "Fetching " + package, (float)i / packages.Length);
            }
            EditorUtility.ClearProgressBar();
        }
    }
}
