using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


//Each SteeringBehavior will be instantiated ONCE
//That instance will be used by all SteeringAgents

//[DefineCategory("Debug", 3f, "drawVectorLine", "vectorColor")]
[System.Serializable]
public abstract class SteeringBehavior : ScriptableObject{
    public delegate void BehaviorEvent();
    public BehaviorEvent OnActiveStatusChange;

    public bool isActive = true;

    public float weight = 1;
    public float effectiveRadius = 10;
    public bool useTagFilter;
    public string[] filterTags;
    public bool drawVectorLine;
    public Color vectorColor;

    public abstract Vector3 GetSteeringBehaviorVector(SteeringAgent mine, SurroundingsInfo surroundings);



    private void InvokeActiveChangedEvent(bool active)
    {
        if (OnActiveStatusChange != null) OnActiveStatusChange();
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
        isActive = GUILayout.Toggle(isActive, GetType().ToString());
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
            //Texture2D texture = Resources.Load<Texture2D>("Sprites/Icons/" + m_ItemDatabase.itemDatabase[i].itemName);
            //GUILayout.Label(texture);
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        GUILayout.Space(5);
        GUILayout.EndVertical();
        return remove;
    }
    
}
