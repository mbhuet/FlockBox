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
    }
}
