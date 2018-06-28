using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PreySettings : BehaviorSettings {

    public AlignmentBehavior alignmment = new AlignmentBehavior();
    public CohesionBehavior cohesion = new CohesionBehavior();
    public SeparationBehavior separation = new SeparationBehavior();
    public FleeBehavior flee = new FleeBehavior();
    public WanderBehavior wander = new WanderBehavior();
    public SeekBehavior seek = new SeekBehavior();

#if UNITY_EDITOR
    [MenuItem("Assets/Create/BehaviorSettings/Prey/Prey_Forage")]
    public static void CreateMyAsset()
    {
        PreySettings asset = ScriptableObject.CreateInstance<PreySettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewPreyForageSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}
