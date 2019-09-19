using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif



public class SocialBehaviorSettings : BehaviorSettings {



    public AlignmentBehavior alignmment = new AlignmentBehavior();
    public CohesionBehavior cohesion = new CohesionBehavior();
    public SeparationBehavior separation = new SeparationBehavior();
    public AvoidanceBehavior avoidance = new AvoidanceBehavior();
    public WanderBehavior wander = new WanderBehavior();
    public SocialStatusBehavior socialStatus = new SocialStatusBehavior();
    public RelationshipBehavior friendships = new RelationshipBehavior();
    public SeekBehavior seek = new SeekBehavior();

    //public EmptyBehavior empty = new EmptyBehavior();

#if UNITY_EDITOR
    [MenuItem("Assets/Create/BehaviorSettings/SocialSettings")]
    public static void CreateMyAsset()
    {
        SocialBehaviorSettings asset = ScriptableObject.CreateInstance<SocialBehaviorSettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewSocialSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}

