using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CloudFine
{
    public abstract class RadialSteeringBehavior : SteeringBehavior
    {
        public float effectiveRadius = 10;
        [Range(0f, 360f), FormerlySerializedAs("fov")]
        public float fieldOfView = 360;

        protected bool WithinEffectiveRadius(SteeringAgent mine, Agent other)
        {
            if (mine == other) return false;
            return (
                Vector3.SqrMagnitude(mine.Position - other.Position) < effectiveRadius * effectiveRadius //inside radius
                && Vector3.Angle(mine.Forward, other.Position - mine.Position) <= fieldOfView); // inside fov
        }

        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            base.AddPerception(agent, surroundings);
            surroundings.SetMinPerceptionRadius(effectiveRadius);
        }

#if UNITY_EDITOR
        public override void DrawPerceptionGizmo(SteeringAgent agent)
        {
            base.DrawPerceptionGizmo(agent);
            Color c = debugColor;
            UnityEditor.Handles.color = c;
            Vector3 startHoriz = Quaternion.Euler(0, -fieldOfView / 2f, 0) * Vector3.forward;
            Vector3 startVert = Quaternion.Euler( -fieldOfView / 2f, 0, 0) * Vector3.forward;

            UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.up, startHoriz, fieldOfView, effectiveRadius);
            UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.right, startVert, fieldOfView, effectiveRadius);

            c.a = c.a*.1f;
            UnityEditor.Handles.color = c;
            UnityEditor.Handles.DrawSolidArc(Vector3.zero, Vector3.up, startHoriz, fieldOfView, effectiveRadius);
        }
#endif

    }
}
