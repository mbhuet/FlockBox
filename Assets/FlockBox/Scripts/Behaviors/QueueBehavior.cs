using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine
{
    [System.Serializable]
    public class QueueBehavior : SteeringBehavior
    {
        public float queueRadius = 10;
        public float queueDistance = 5;

        private Shape _perception;
        private Shape Perception
        {
            get
            {
                if(_perception == null)
                {
                    _perception = new Shape();
                    _perception.type = Shape.ShapeType.SPHERE;
                }
                return _perception;
            }
        }
        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            base.AddPerception(agent, surroundings);
            Perception.radius = queueRadius;
            surroundings.AddPerceptionShape(Perception, (agent.Position + (agent.Forward * queueDistance)));
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
        public override void DrawPerceptionGizmo(SteeringAgent agent, bool labels)
        {
            base.DrawPerceptionGizmo(agent, labels);

            Handles.DrawWireDisc(Vector3.forward * queueDistance, Vector3.up, queueRadius);
            Gizmos.DrawWireSphere(Vector3.forward * queueDistance, queueRadius);
            Handles.DrawLine(Vector3.zero, Vector3.forward * queueDistance);

            Color c = debugColor;
            c.a = .1f * c.a;
            Handles.color = c;
            Handles.DrawSolidDisc(Vector3.forward * queueDistance, Vector3.up, queueRadius);

        }

#endif
    }
}