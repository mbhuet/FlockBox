using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class ArriveBehavior : SeekBehavior
    {
        [Tooltip("Distance at which brake force will be applied to bring the agent to a stop.")]
        public float stoppingDistance = 10;


        public static Vector3 DesiredVelocityForArrival(SteeringAgent mine, Vector3 arrivePosition, float stopRadius)
        {
            return (arrivePosition - mine.Position).normalized
                * Mathf.Lerp(0, mine.activeSettings.maxSpeed, (arrivePosition - mine.Position).sqrMagnitude / (stopRadius * stopRadius));
        }

        protected override Vector3 GetSteeringVectorForTarget(SteeringAgent mine, Agent target)
        {
            Vector3 desired_velocity = DesiredVelocityForArrival(mine, target.Position, stoppingDistance);
            Vector3 steer = desired_velocity - mine.Velocity;
            return steer.normalized * Mathf.Min(steer.magnitude, mine.activeSettings.maxForce);
        }

#if UNITY_EDITOR
        public override void DrawPropertyGizmos(SteeringAgent agent, bool drawLabels)
        {
            base.DrawPropertyGizmos(agent, drawLabels);

            Handles.color = debugColor;
            Handles.DrawWireDisc(Vector3.zero, Vector3.up, stoppingDistance);
            
            if (drawLabels)
            {
                Handles.Label(Vector3.forward * stoppingDistance, new GUIContent("Stopping Distance"));
            }
        }
#endif
    }
}