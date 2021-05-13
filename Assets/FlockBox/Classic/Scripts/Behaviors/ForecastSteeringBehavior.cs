using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine.FlockBox
{
    public abstract class ForecastSteeringBehavior : SteeringBehavior
    {
        [Tooltip("Seconds ahead this behavior is able to perceive in the current direction of travel.")]
        public float lookAheadSeconds = 1;

        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            base.AddPerception(agent, surroundings);
            surroundings.SetMinLookAheadSeconds(lookAheadSeconds);
        }


#if UNITY_EDITOR
        public override void DrawPropertyGizmos(SteeringAgent agent, bool drawLabels)
        {
            base.DrawPropertyGizmos(agent, drawLabels);
            float distance = agent.activeSettings.maxSpeed * lookAheadSeconds;
            if (Application.isPlaying)
            {
                distance = agent.Velocity.magnitude * lookAheadSeconds;
            }
            DrawForecastPerceptionGizmo(agent, distance);
            if (drawLabels)
            {
                Handles.Label(Vector3.forward * distance, new GUIContent("Look Ahead"));
            }

        }

        protected virtual void DrawForecastPerceptionGizmo(SteeringAgent agent, float distance)
        {
            Handles.DrawLine(Vector3.zero, Vector3.forward * distance);
        }
#endif
    }
}
