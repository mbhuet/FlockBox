using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class RadialSteeringBehavior : SteeringBehavior
    {
        public float effectiveRadius = 10;
        [Range(0f,360f)]
        public float fov = 360;

        protected bool WithinEffectiveRadius(SteeringAgent mine, Agent other)
        {
            if (mine == other) return false;
            return (
                Vector3.SqrMagnitude(mine.Position - other.Position) < effectiveRadius * effectiveRadius //inside radius
                && Vector3.Angle(mine.Forward, other.Position - mine.Position) <= fov); // inside fov
        }

        public override void AddPerception(SurroundingsContainer surroundings)
        {
            base.AddPerception(surroundings);
            if(effectiveRadius > surroundings.perceptionRadius)
            {
                surroundings.perceptionRadius = effectiveRadius;
            }
        }
    }
}
