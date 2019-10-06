﻿using System;
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
            Vector3 lastForward = mine.Forward.normalized;
            Vector3 rotAxis = Vector3.one;
            if (surroundings.worldDimensions.x <= 0)
            {
                rotAxis.y = 0;
                rotAxis.z = 0;
                lastForward.x = 0;
            }
            if (surroundings.worldDimensions.y <= 0)
            {
                rotAxis.x = 0;
                rotAxis.z = 0;
                lastForward.y = 0;
            }
            if (surroundings.worldDimensions.z <= 0)
            {
                rotAxis.x = 0;
                rotAxis.y = 0;
                lastForward.z = 0;
            }

            Vector3 wanderVector = Quaternion.AngleAxis(
                (Mathf.PerlinNoise(Time.time * wanderIntensity, mine.gameObject.GetInstanceID()) - .5f) * wanderScope, 
                rotAxis) * lastForward;

            steer = wanderVector * mine.activeSettings.maxForce;
        }

    }

    
}