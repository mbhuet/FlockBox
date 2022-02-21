using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class SeekCenterBehavior : SteeringBehavior
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            mine.GetSeekVector(out steer, mine.FlockBox.WorldDimensions / 2f);
        }
    }
}