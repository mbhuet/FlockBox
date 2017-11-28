using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Reflection;
using System.Linq;
using Vexe.Runtime.Types;

[System.Serializable]
public class BehaviorInfo
{
	[ShowSerializableType(typeof(SteeringBehavior)), OnChanged("Foo")]
	public SerializableType behaviorType;

    [fSlider(0, 10f)]
    public float weight = 1;
    public float effectiveRadius = 10;

	[Hide]
	bool showAnimCurve = false;
	[VisibleWhen("showAnimCurve")]
	public ClipAnimationInfoCurve curve;

	void Foo(SerializableType behaviorType){
		string behaviorTypeName = behaviorType.ToString ();
		if (behaviorTypeName == "CohesionBehavior") {
			showAnimCurve = true;
		} else
			showAnimCurve = false;
	}
    
}

public class BehaviorSettings : BaseScriptableObject {

    public float maxForce = 10;    // Maximum steering force
    public float maxSpeed = 2;    // Maximum speed

	[Display(Seq.PerItemRemove)]
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


}

