using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using Vexe.Runtime.Types;
using System.Reflection;
using System.Linq;


[System.Serializable]
public class BehaviorInfo
{
    [PerItem, Popup("BehaviorSettings.FindSteeringBehaviorImplementationNames")]
    public string behaviorTypeName = "";
    [fSlider(0, 10f)]
    public float weight = 1;
    public float effectiveRadius = 1;
    
}

public class BehaviorSettings : BaseScriptableObject {

    public float maxForce = 10;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed

    public BehaviorInfo[] behaviors = { };
    

    [MenuItem("Assets/Create/Behavior Settings")]
    public static void CreateMyAsset()
    {
        BehaviorSettings asset = ScriptableObject.CreateInstance<BehaviorSettings>();
        AssetDatabase.CreateAsset(asset, "Assets/NewBehaviorSettings.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    static List<String> FindSteeringBehaviorImplementationNames()
    {
        List<string> allBehaviorTypes = new List<string>();
        foreach (Type type in
            Assembly.GetAssembly(typeof(SteeringBehavior)).GetTypes()
            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(SteeringBehavior))))
        {
            allBehaviorTypes.Add(type.ToString());
        }
        return allBehaviorTypes;
    }


}

