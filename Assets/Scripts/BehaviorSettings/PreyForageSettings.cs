using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PreyForageSettings : BehaviorSettings {

    public AlignmentBehavior alignmment = new AlignmentBehavior();
    public CohesionBehavior cohesion = new CohesionBehavior();
    public SeparationBehavior separation = new SeparationBehavior();
    public WanderBehavior wander = new WanderBehavior();

#if UNITY_EDITOR
    [MenuItem("Assets/Create/BehaviorSettings/Prey/Prey_Forage")]
    public static void CreateMyAsset()
    {
        PreyForageSettings asset = ScriptableObject.CreateInstance<PreyForageSettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewPreyForageSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}
