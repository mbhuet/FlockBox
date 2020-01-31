using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CloudFine
{
    [System.Serializable]
    public abstract class SteeringBehavior : ScriptableObject
    {
        public bool IsActive { get { return isActive; } }
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

        //supress "field declared but not used" warning. foldout is used by PropertyDrawer
#pragma warning disable 0169
        [SerializeField, HideInInspector]
        private bool foldout = true;
#pragma warning restore 0169

        public abstract void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings);
        public virtual void AddPerception(SurroundingsContainer surroundings) { }
        public virtual bool CanUseTagFilter { get { return true; } }
        public virtual bool CanToggleActive { get { return true; } }

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
