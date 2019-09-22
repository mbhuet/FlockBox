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
            GUILayout.Space(20);
            EditorGUILayout.BeginVertical("BOX");
            EditorGUILayout.Slider(serializedObject.FindProperty("weight"), 0f,1f);
            //EditorGUILayout.Toggle()

            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectiveRadius"));


            bool useTagFilter = true;// GUILayout.Toggle(useTagFilter, "Use Tag Filter");
            if (useTagFilter)
            {
                SerializedProperty tags = serializedObject.FindProperty("filterTags");
                GUILayout.BeginVertical("BOX");

                for (int i=0; i<tags.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();

                    tags.GetArrayElementAtIndex(i).stringValue = EditorGUILayout.TagField(tags.GetArrayElementAtIndex(i).stringValue);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        behavior.RemoveTag(i);
                    }
                    GUILayout.EndHorizontal();

                }
                if (GUILayout.Button("Add Tag"))
                {
                    behavior.AddTag();
                    //tags.InsertArrayElementAtIndex(tags.arraySize);
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
