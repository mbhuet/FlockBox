using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class ContainmentBehavior : ForecastSteeringBehavior
    {
        public override bool CanUseTagFilter => false;
        public override bool CanToggleActive => false;

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsInfo surroundings)
        {
            Vector3 bufferedPosition = mine.Position + mine.Velocity * lookAheadSeconds;
            float distanceToBorder = float.MaxValue;

            if (surroundings.worldDimensions.x > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, mine.Position.x, surroundings.worldDimensions.x - mine.Position.x);
                bufferedPosition.x = Mathf.Clamp(bufferedPosition.x, surroundings.containmentBuffer, surroundings.worldDimensions.x - surroundings.containmentBuffer);
            }
            else
            {
                bufferedPosition.x = 0;
            }
            if(surroundings.worldDimensions.y > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, mine.Position.y, surroundings.worldDimensions.y - mine.Position.y);
                bufferedPosition.y = Mathf.Clamp(bufferedPosition.y, surroundings.containmentBuffer, surroundings.worldDimensions.y- surroundings.containmentBuffer);
            }
            else
            {
                bufferedPosition.y = 0;
            }
            if (surroundings.worldDimensions.z > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, mine.Position.z, surroundings.worldDimensions.z - mine.Position.z);
                bufferedPosition.z = Mathf.Clamp(bufferedPosition.z, surroundings.containmentBuffer, surroundings.worldDimensions.z - surroundings.containmentBuffer);
            }
            else
            {
                bufferedPosition.z = 0;
            }
            if(bufferedPosition == mine.Position + mine.Velocity)
            {
                steer = Vector3.zero;
                return;
            }
            if (distanceToBorder <= 0) distanceToBorder = .001f;

            mine.GetSeekVector(out steer, bufferedPosition);
            steer *= surroundings.containmentBuffer / distanceToBorder;

        }



    }
}