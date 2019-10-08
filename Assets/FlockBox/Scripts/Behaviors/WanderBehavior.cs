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
            float wanderYaw = (Mathf.PerlinNoise((Time.time * wanderIntensity), mine.gameObject.GetInstanceID()) - .5f);
            float wanderPitch = (Mathf.PerlinNoise((Time.time * wanderIntensity) + 99f, mine.gameObject.GetInstanceID()) - .5f);
            steer =
                Quaternion.Euler(wanderYaw * wanderScope, wanderYaw * wanderScope, 0) * mine.Forward * mine.activeSettings.maxForce;
        }

    }

    
}
