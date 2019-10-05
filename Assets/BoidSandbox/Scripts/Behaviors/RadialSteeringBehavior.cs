using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class RadialSteeringBehavior : SteeringBehavior
    {
        public float effectiveRadius = 10;
        [Range(0f,360f)]
        public float fov = 360;

        protected bool WithinEffectiveRadius(SteeringAgent mine, AgentWrapped other)
        {
            if (mine == other.agent) return false;
            return (
                Vector3.SqrMagnitude(mine.Position - other.wrappedPosition) < effectiveRadius * effectiveRadius //inside radius
                && Vector3.Angle(mine.Forward, other.wrappedPosition - mine.Position) <= fov); // inside fov
        }

        public override void AddPerception(ref SurroundingsInfo surroundings)
        {
            base.AddPerception(ref surroundings);
            if(effectiveRadius > surroundings.perceptionRadius)
            {
                surroundings.perceptionRadius = effectiveRadius;
            }
        }

#if UNITY_EDITOR

        public virtual void DrawGUI(UnityEditor.SerializedObject serializedObject)
        {
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("effectiveRadius"));
            UnityEditor.SerializedProperty useTag = serializedObject.FindProperty("useTagFilter");
            UnityEditor.EditorGUILayout.PropertyField(useTag);
            if (useTag.boolValue)
            {
                UnityEditor.SerializedProperty tags = serializedObject.FindProperty("filterTags");
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.BeginVertical("BOX", GUILayout.ExpandWidth(true));

                for (int i = 0; i < tags.arraySize; i++)
                {
                    GUILayout.BeginHorizontal();

                    tags.GetArrayElementAtIndex(i).stringValue = UnityEditor.EditorGUILayout.TagField(tags.GetArrayElementAtIndex(i).stringValue);
                    if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(14)))
                    {
                        tags.DeleteArrayElementAtIndex(i);
                    }
                    GUILayout.EndHorizontal();

                }
                if (GUILayout.Button("+ Add Tag", GUILayout.Width(80)))
                {
                    tags.arraySize = tags.arraySize + 1;
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

        }
#endif
    }

   
}
