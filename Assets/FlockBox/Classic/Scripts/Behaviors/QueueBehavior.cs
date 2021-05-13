using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class QueueBehavior : SteeringBehavior
    {
        [Tooltip("Determines a point ahead to look for Agents blocking this one.")]
        public float queueDistance = 5;
        [Tooltip("The search radius around a point ahead to look for Agents blocking this one.")]
        public float queueRadius = 10;

        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            base.AddPerception(agent, surroundings);
            surroundings.AddPerceptionSphere(queueRadius, (agent.Position + (agent.Forward * queueDistance)));
        }

        protected bool WithinEffectiveRadius(SteeringAgent mine, Agent other)
        {
            if (mine == other) return false;
            return (
                Vector3.SqrMagnitude((mine.Position + (mine.Forward * queueDistance)) - other.Position) < queueRadius * queueRadius); // inside fov
        }

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            foreach (Agent a in GetFilteredAgents(surroundings, this))
            {
                //another agent is ahead
                if(WithinEffectiveRadius(mine, a))
                {
                    //use brake force
                    steer = -mine.Velocity;
                    steer = steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
                    return;
                }
            }
            steer = Vector3.zero; 
        }


#if UNITY_EDITOR
        public override void DrawPropertyGizmos(SteeringAgent agent, bool drawLabels)
        {
            base.DrawPropertyGizmos(agent, drawLabels);

            Handles.DrawWireDisc(Vector3.forward * queueDistance, Vector3.up, queueRadius);
            Gizmos.DrawWireSphere(Vector3.forward * queueDistance, queueRadius);
            Handles.DrawLine(Vector3.zero, Vector3.forward * queueDistance);

            Color c = debugColor;
            c.a = .1f * c.a;
            Handles.color = c;
            Handles.DrawSolidDisc(Vector3.forward * queueDistance, Vector3.up, queueRadius);

            if (drawLabels)
            {
                Handles.Label(Vector3.forward * queueDistance, new GUIContent("Queue Distance"));
                Handles.Label(Vector3.forward *(queueDistance + queueRadius), new GUIContent("Queue Radius"));
            }
        }

#endif
    }
}