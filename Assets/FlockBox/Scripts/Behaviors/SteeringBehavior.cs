using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CloudFine
{
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
        [HideInInspector]
        public bool foldout = true;

        public abstract void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings);
        public virtual void AddPerception(SurroundingsContainer surroundings) { }
        public virtual bool CanUseTagFilter => true;
        public virtual bool CanToggleActive => true;

        private static List<Agent> _filterCache = new List<Agent>();
        public static List<Agent> GetFilteredAgents(SurroundingsContainer surroundings, SteeringBehavior behavior)
        {
            if (!behavior.useTagFilter) return surroundings.allAgents;

            _filterCache.Clear();

            foreach (Agent other in surroundings.allAgents)
            {
                for(int i =0; i<behavior.filterTags.Length; i++)
                {
                    if (other.CompareTag(behavior.filterTags[i]))
                    {
                        _filterCache.Add(other);
                        break;
                    }
                }
            }
            return _filterCache;

        }
    }
}
