using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace CloudFine.FlockBox
{
    [System.Serializable]
    public abstract class SteeringBehavior : ScriptableObject
    {
        public bool IsActive { get { return isActive; } }
        [SerializeField, HideInInspector, Tooltip("Determines if this behavior will apply any steering force.")]
        private bool isActive = true;
        [HideInInspector, Tooltip("Relative strength of this behavior's steering force.")]
        public float weight = 1;
        [HideInInspector, Tooltip("Limit this behavior's awareness to Agents with certain Tags.")]
        public bool useTagFilter;
        [HideInInspector]
        public string[] filterTags = new string[0];

#pragma warning disable 0649
        [SerializeField, HideInInspector, Tooltip("[Debug] Enable debug visualizations for this behavior.")]
        private bool drawDebug;
        [SerializeField, HideInInspector]
        public Color debugColor = Color.white;
        [SerializeField, HideInInspector, Tooltip("[Debug] Draw line representing this behavior's steering force. (Will only appear in Play mode).")]
        private bool debugDrawSteering;
        [SerializeField, HideInInspector, Tooltip("[Debug] Draw visualizations representing this behavior's awareness of the environment.")]
        private bool debugDrawProperties;
#pragma warning restore 0649

        public Action<SteeringBehavior> OnValueChanged;

        public bool DrawProperties
        {
            get
            {
                return drawDebug && debugDrawProperties;
            }
        }
        public bool DrawSteering
        {
            get
            {
                return drawDebug && debugDrawSteering;
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        private void OnValidate()
        {
            MarkAsChanged();
        }

        public void MarkAsChanged()
        {
            if (OnValueChanged != null) OnValueChanged(this);
        }



        //supress "field declared but not used" warning. foldout is used by PropertyDrawer
#pragma warning disable 0414
        [SerializeField, HideInInspector]
        private bool foldout = true;
#pragma warning restore 0414

        public abstract void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings);
        public virtual void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings) { }
        public virtual bool CanUseTagFilter { get { return true; } }
        public virtual bool CanToggleActive { get { return true; } }

        private static HashSet<Agent> _filterCache = new HashSet<Agent>();
        public static HashSet<Agent> GetFilteredAgents(SurroundingsContainer surroundings, SteeringBehavior behavior)
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



#if UNITY_EDITOR
        public virtual void DrawPropertyGizmos(SteeringAgent agent, bool drawLabels)
        {
            Handles.matrix = agent.transform.localToWorldMatrix;
            Gizmos.matrix = agent.transform.localToWorldMatrix;
            Gizmos.color = debugColor;
            Handles.color = debugColor;
        }

        protected void DrawCylinderGizmo(float clearAheadRadius, float clearAheadDistance)
        {
            Handles.color = debugColor;
            Color areaFill = debugColor;
            areaFill.a *= .1f;

            Handles.DrawLine(Vector3.zero, Vector3.forward * clearAheadDistance);
            Handles.DrawWireDisc(Vector3.forward * clearAheadDistance, Vector3.forward, clearAheadRadius);
            Handles.DrawWireDisc(Vector3.zero, Vector3.forward, clearAheadRadius);

            Vector3[] verts = new Vector3[]
            {
                Vector3.left * clearAheadRadius,
                Vector3.left * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.right * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.right *clearAheadRadius
            };

            Handles.DrawSolidRectangleWithOutline(verts, areaFill, debugColor);

            Handles.DrawLine(Vector3.up * clearAheadRadius,
                Vector3.up * clearAheadRadius + Vector3.forward * clearAheadDistance);

            Handles.DrawLine(Vector3.down * clearAheadRadius + Vector3.forward * clearAheadDistance,
                Vector3.down * clearAheadRadius);
        }

#endif
    }
}
