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
        public override bool CanUseTagFilter => false;

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            steer = Quaternion.Euler(
                        (Mathf.PerlinNoise((Time.time * wanderIntensity), mine.gameObject.GetInstanceID()) - .5f) * wanderScope,
                        (Mathf.PerlinNoise((Time.time * wanderIntensity) + 99f, mine.gameObject.GetInstanceID()) - .5f) * wanderScope,
                        (Mathf.PerlinNoise((Time.time * wanderIntensity) + 199f, mine.gameObject.GetInstanceID()) - .5f) * wanderScope
                        )
                    * mine.Forward * mine.activeSettings.maxForce;
        }

    }

    
}
