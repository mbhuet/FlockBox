using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class DOTSToggleEditor : Editor
{
#if UNITY_2020_1_OR_NEWER
    private const string menuPath = "FlockBox/Enable DOTS";
#else
    private const string menuPath = "FlockBox/Enable DOTS (2020.1+)";
#endif
    private const string prefsKey = "FLOCKBOX_DOTS_TOGGLE";
    private const string dotsDefine = "FLOCKBOX_DOTS";

    static DOTSToggleEditor()
    {
        UpdateScriptingDefines(IsEnabled);
    }

    public static bool IsEnabled
    {
        get
        {
#if UNITY_2020_1_OR_NEWER
        return EditorPrefs.GetBool(prefsKey,true);
#else
            return false;
#endif
        }
        set { EditorPrefs.SetBool(prefsKey, value); }
    }

    [MenuItem(menuPath)]
    private static void ToggleAction()
    {
        IsEnabled = !IsEnabled;
        UpdateScriptingDefines(IsEnabled);
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

    [MenuItem(menuPath, true)]
    private static bool ToggleActionValidate()
    {
        Menu.SetChecked(menuPath, IsEnabled);
#if UNITY_2020_1_OR_NEWER
        return true;
#else
        return false;
#endif
    }
}
