using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoidBehaviorSettings : BehaviorSettings {


#if UNITY_EDITOR
    [MenuItem("Assets/Create/BehaviorSettings/Boid")]
    public static void CreateMyAsset()
    {
        BoidBehaviorSettings asset = ScriptableObject.CreateInstance<BoidBehaviorSettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewBoidSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}
