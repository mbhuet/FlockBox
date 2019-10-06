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

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            Vector3 localForward = mine.Forward;

            int dimensions = 3;
            if (surroundings.worldDimensions.x <= 0)
            {
                dimensions--;
                localForward.x = 0;
            }
            if (surroundings.worldDimensions.y <= 0)
            {
                dimensions--;
                localForward.y = 0;
            }
            if (surroundings.worldDimensions.z <= 0)
            {
                dimensions--;
                localForward.z = 0;
            }
            localForward.Normalize();

            if (dimensions < 2)
            {
                steer = Vector3.zero;
                return;
            }

            float wanderYaw = Mathf.PerlinNoise((Time.time * wanderIntensity), mine.gameObject.GetInstanceID()) - .5f;

            if (dimensions == 2)
            {
                steer = 
                    Quaternion.AngleAxis(wanderYaw * wanderScope, mine.transform.up) 
                    * localForward
                    * mine.activeSettings.maxForce;
                return;
            }

            float wanderPitch = Mathf.PerlinNoise((Time.time * wanderIntensity) + 99f, mine.gameObject.GetInstanceID()) - .5f;

            steer = 
                Quaternion.AngleAxis(wanderYaw * wanderScope, mine.transform.InverseTransformDirection(mine.transform.up)) 
                * Quaternion.AngleAxis(wanderPitch * wanderScope, mine.transform.InverseTransformDirection(mine.transform.right)) 
                * localForward
                * mine.activeSettings.maxForce;

          
        }

    }

    
}
