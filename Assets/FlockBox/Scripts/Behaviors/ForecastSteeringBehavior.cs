using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CloudFine
{
    public abstract class ForecastSteeringBehavior : SteeringBehavior
    {
        public float lookAheadSeconds = 1;

        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            base.AddPerception(agent, surroundings);
            surroundings.SetMinLookAheadSeconds(lookAheadSeconds);
        }


#if UNITY_EDITOR
        public override void DrawPerceptionGizmo(SteeringAgent agent, bool labels)
        {
            base.DrawPerceptionGizmo(agent, labels);
            float distance = agent.activeSettings.maxSpeed * lookAheadSeconds;
            if (Application.isPlaying)
            {
                distance = agent.Velocity.magnitude * lookAheadSeconds;
            }
            DrawForecastPerceptionGizmo(agent, distance);
            if (labels)
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
