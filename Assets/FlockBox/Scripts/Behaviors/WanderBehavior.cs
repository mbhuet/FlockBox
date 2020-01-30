using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class WanderBehavior : SteeringBehavior
    {
        [Range(0,360f)]
        public float wanderScope = 90;
        public float wanderIntensity = 1;
        public override bool CanUseTagFilter { get { return false; } }

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            float uniqueId = mine.gameObject.GetInstanceID() * .001f;
            uniqueId = uniqueId*uniqueId;
            steer = Quaternion.Euler(
                        (Mathf.PerlinNoise((Time.time * wanderIntensity), uniqueId) - .5f) * wanderScope,
                        (Mathf.PerlinNoise((Time.time * wanderIntensity) + uniqueId, uniqueId) - .5f) * wanderScope,
                        (Mathf.PerlinNoise((Time.time * wanderIntensity) + uniqueId*2, uniqueId) - .5f) * wanderScope
                        )
                    * mine.Forward * mine.activeSettings.maxForce;
        }

    }

    
}
