using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using Vexe.Runtime.Types;
#if UNITY_EDITOR
using UnityEditor;
#endif



public class VisualSettings : BaseScriptableObject {

    
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Visual Settings")]
    public static void CreateMyAsset()
    {
        BehaviorSettings asset = ScriptableObject.CreateInstance<BehaviorSettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewVisualSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
}
#endif


}

