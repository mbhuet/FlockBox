using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BoidBehaviorSettings : BehaviorSettings {

    public AlignmentBehavior alignmment = new AlignmentBehavior();
    public CohesionBehavior cohesion = new CohesionBehavior();
    public SeparationBehavior separation = new SeparationBehavior();
    public WanderBehavior wander = new WanderBehavior();
    public AvoidanceBehavior avoid = new AvoidanceBehavior();

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
