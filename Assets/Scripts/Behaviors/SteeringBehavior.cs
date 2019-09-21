using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


//Each SteeringBehavior will be instantiated ONCE
//That instance will be used by all SteeringAgents

//[DefineCategory("Debug", 3f, "drawVectorLine", "vectorColor")]
[System.Serializable]
public abstract class SteeringBehavior : ScriptableObject{
    public Action<bool> OnActiveStatusChange;

    private bool m_isActive = true;
    public bool isActive
    {
        get { return m_isActive; }
        private set
        {
            if(value != m_isActive)
            {
                if (OnActiveStatusChange != null) OnActiveStatusChange.Invoke(value);
            }
            m_isActive = value;
        }
    }

    public float weight = 1;
    public float effectiveRadius = 10;
    public bool useTagFilter;
    public List<string> filterTags;
    public bool drawVectorLine;
    public Color vectorColor = Color.white;

    public abstract void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings);

    protected bool WithinEffectiveRadius(SteeringAgent mine, AgentWrapped other)
    {
        if (mine == other.agent) return false;
        return (Vector3.SqrMagnitude(mine.position - other.wrappedPosition) < effectiveRadius * effectiveRadius);
    }

    public static LinkedList<AgentWrapped> GetFilteredAgents(SurroundingsInfo surroundings, SteeringBehavior behavior)// params string[] filterTags)
    {
        Dictionary<string, LinkedList<AgentWrapped>> agentDict = surroundings.sortedAgents;
        if(!behavior.useTagFilter)
        {
            return surroundings.allAgents;
        }
        LinkedList<AgentWrapped> filteredAgents = new LinkedList<AgentWrapped>();

        LinkedList<AgentWrapped> agentsOut = new LinkedList<AgentWrapped>();
        foreach (string tag in behavior.filterTags)
        {
            if (agentDict.TryGetValue(tag, out agentsOut))
            {
                foreach (AgentWrapped agent in agentsOut)
                {
                    //Debug.Log(agent.agent.name + " in filtered list");
                    filteredAgents.AddLast(agent);
                }

            }
        }
        return filteredAgents;
    }

#if UNITY_EDITOR
    /// <summary>
    /// return draw height
    /// </summary>
    /// <returns>remove</returns>
    public virtual bool OnInspectorGUI()
    {
        bool remove = false;
        GUILayout.BeginVertical("BOX");
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        isActive = EditorGUILayout.Toggle(GetType().ToString(), isActive);

        if (GUILayout.Button("Remove", GUILayout.Width(60)))
        {
            remove = true;
        }
        GUILayout.EndHorizontal();
        if (isActive)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            GUILayout.BeginVertical("BOX");
            weight = EditorGUILayout.Slider("Weight", weight, 0, 1);
            effectiveRadius = EditorGUILayout.FloatField("Effective Radius ",effectiveRadius);

            useTagFilter = GUILayout.Toggle(useTagFilter, "Use Tag Filter");
            if (useTagFilter)
            {
                GUILayout.BeginVertical("BOX");
                if (filterTags == null) filterTags = new List<string>();
                for(int i = 0; i< filterTags.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    filterTags[i] = EditorGUILayout.TagField(filterTags[i]);
                    if (GUILayout.Button("X"))
                    {
                        filterTags.RemoveAt(i);
                    }
                    GUILayout.EndHorizontal();
                }
                if(GUILayout.Button("Add Tag"))
                {
                    filterTags.Add("");
                }
                GUILayout.EndVertical();
            }
            GUILayout.Space(10);
            drawVectorLine = GUILayout.Toggle(drawVectorLine, "Draw Steering Vector");
            if (drawVectorLine)
            {
                vectorColor = EditorGUILayout.ColorField("Vector Color", vectorColor);
            }
            //Texture2D texture = Resources.Load<Texture2D>("Sprites/Icons/" + m_ItemDatabase.itemDatabase[i].itemName);
            //GUILayout.Label(texture);
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(5);
        GUILayout.EndVertical();
        return remove;
    }
#endif

}
