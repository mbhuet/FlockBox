using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        public override void DrawPerceptionGizmo(SteeringAgent agent, bool labels)
        {
            base.DrawPerceptionGizmo(agent, labels);
            Vector3 startHoriz = Quaternion.Euler(0, -fieldOfView / 2f, 0) * Vector3.forward;
            Vector3 startVert = Quaternion.Euler( -fieldOfView / 2f, 0, 0) * Vector3.forward;

            Vector3 endHoriz = Quaternion.Euler(0, fieldOfView / 2f, 0) * Vector3.forward;
            Vector3 endVert = Quaternion.Euler(fieldOfView / 2f, 0, 0) * Vector3.forward;

            

            Handles.DrawWireArc(Vector3.zero, Vector3.up, startHoriz, fieldOfView, effectiveRadius);
            Handles.DrawWireArc(Vector3.zero, Vector3.right, startVert, fieldOfView, effectiveRadius);

            if (fieldOfView < 360)
            {
                Handles.DrawLine(Vector3.zero, startHoriz * effectiveRadius);
                Handles.DrawLine(Vector3.zero, endHoriz * effectiveRadius);
                Handles.DrawLine(Vector3.zero, startVert * effectiveRadius);
                Handles.DrawLine(Vector3.zero, endVert * effectiveRadius);
            }

            Color c = debugColor;
            c.a = c.a*.1f;
            Handles.color = c;
            Handles.DrawSolidArc(Vector3.zero, Vector3.up, startHoriz, fieldOfView, effectiveRadius);

            if (labels)
            {
                Handles.Label(startHoriz * effectiveRadius, new GUIContent("Field Of View"));
                Handles.Label(Vector3.forward * effectiveRadius, new GUIContent("Effective Radius"));
            }
        }
#endif

    }
}
