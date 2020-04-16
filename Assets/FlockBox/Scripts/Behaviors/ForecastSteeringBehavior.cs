using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public override void DrawPerceptionGizmo(SteeringAgent agent)
        {
            base.DrawPerceptionGizmo(agent);
            UnityEditor.Handles.color = debugColor;
            Vector3 endpoint = agent.Forward * agent.activeSettings.maxSpeed * lookAheadSeconds;
            if (Application.isPlaying)
            {
                endpoint = agent.Forward * agent.Velocity.magnitude * lookAheadSeconds;
            }
            UnityEditor.Handles.DrawLine(Vector3.zero, endpoint);
            DrawForecastPerceptionEndCapGizmo(agent, endpoint);
        }

        protected virtual void DrawForecastPerceptionEndCapGizmo(SteeringAgent agent, Vector3 endpoint)
        {
        }
#endif
    }
}
