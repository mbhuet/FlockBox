using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CloudFine
{
    //Each SteeringBehavior will be instantiated ONCE
    //That instance will be used by all SteeringAgents

    //[DefineCategory("Debug", 3f, "drawVectorLine", "vectorColor")]
    [System.Serializable]
    public abstract class SteeringBehavior : ScriptableObject
    {
        public bool IsActive => isActive;
        [SerializeField, HideInInspector]
        private bool isActive = true;
        [HideInInspector]
        public float weight = 1;
        [HideInInspector]
        public bool useTagFilter;
        [HideInInspector]
        public string[] filterTags = new string[0];
        [HideInInspector]
        public bool drawDebug;
        [HideInInspector]
        public Color debugColor = Color.white;

        public abstract void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings);
        public virtual void AddPerception(ref SurroundingsInfo surroundings) { }


        public static List<AgentWrapped> GetFilteredAgents(SurroundingsInfo surroundings, SteeringBehavior behavior)// params string[] filterTags)
        {
            if (!behavior.useTagFilter) return surroundings.allAgents;

            List<AgentWrapped> filtered = new List<AgentWrapped>();
            foreach (AgentWrapped other in surroundings.allAgents)
            {
                if (Array.IndexOf(behavior.filterTags, other.agent.tag) >= 0)
                {
                    filtered.Add(other);
                }
            }
            return filtered;

        }




    }
}
