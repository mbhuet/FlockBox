using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PredatorBehaviorSettings : BehaviorSettings
{

    public AlignmentBehavior alignmment = new AlignmentBehavior();
    public CohesionBehavior cohesion = new CohesionBehavior();
    public SeparationBehavior separation = new SeparationBehavior();
    public WanderBehavior wander = new WanderBehavior();
    public PursuitBehavior pursuit = new PursuitBehavior();


#if UNITY_EDITOR
    [MenuItem("Assets/Create/BehaviorSettings/Predator/Predator")]
    public static void CreateMyAsset()
    {
        PredatorBehaviorSettings asset = ScriptableObject.CreateInstance<PredatorBehaviorSettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewPredatorSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}
