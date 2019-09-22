using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SteeringBehavior), true)]
public class SteeringBehaviorDrawer : PropertyDrawer
{
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
      
        SteeringBehavior behavior = property.objectReferenceValue as SteeringBehavior;
        SerializedObject serializedObject = new SerializedObject(behavior);

        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isActive"));


        if (behavior.IsActive)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(30);
            EditorGUILayout.BeginVertical("BOX");
            EditorGUILayout.Slider(serializedObject.FindProperty("weight"), 0f,1f);
            //EditorGUILayout.Toggle()

            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectiveRadius"));


            bool useTagFilter = false;// GUILayout.Toggle(useTagFilter, "Use Tag Filter");
            List<string> filterTags = new List<string>();
            if (useTagFilter)
            {
                GUILayout.BeginVertical("BOX");
                if (filterTags == null) filterTags = new List<string>();
                for (int i = 0; i < filterTags.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    filterTags[i] = EditorGUILayout.TagField(filterTags[i]);
                    if (GUILayout.Button("X"))
                    {
                        filterTags.RemoveAt(i);
                    }
                    GUILayout.EndHorizontal();
                }
                if (GUILayout.Button("Add Tag"))
                {
                    filterTags.Add("");
                }
                GUILayout.EndVertical();
            }
           



            GUILayout.Space(10);
            bool drawVectorLine = false;// GUILayout.Toggle(drawVectorLine, "Draw Steering Vector");
            if (drawVectorLine)
            {
                //vectorColor = EditorGUILayout.ColorField("Vector Color", vectorColor);
            }
             


            //Texture2D texture = Resources.Load<Texture2D>("Sprites/Icons/" + m_ItemDatabase.itemDatabase[i].itemName);
            //GUILayout.Label(texture);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        // Draw label
        //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        serializedObject.ApplyModifiedProperties();
    }

}
