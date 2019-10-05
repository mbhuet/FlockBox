using UnityEngine;
using UnityEditor;



public static class DefaultInspector_EditorExtension
{
    public static bool DrawDefaultInspectorWithoutScriptField(this Editor Inspector)
    {
        EditorGUI.BeginChangeCheck();

        Inspector.serializedObject.Update();

        SerializedProperty Iterator = Inspector.serializedObject.GetIterator();

        Iterator.NextVisible(true);

        while (Iterator.NextVisible(false))
        {
            EditorGUILayout.PropertyField(Iterator, true);
        }

        Inspector.serializedObject.ApplyModifiedProperties();

        return (EditorGUI.EndChangeCheck());
    }
}