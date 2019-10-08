using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class ContainmentBehavior : ForecastSteeringBehavior
    {
        public override bool CanUseTagFilter => false;
        public override bool CanToggleActive => false;

        public void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, Vector3 worldDimensions, float containmentBuffer)
        {
            Vector3 bufferedPosition = mine.Position + mine.Velocity * lookAheadSeconds;
            float distanceToBorder = float.MaxValue;

            if (worldDimensions.x > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, mine.Position.x, worldDimensions.x - mine.Position.x);
                bufferedPosition.x = Mathf.Clamp(bufferedPosition.x, containmentBuffer, worldDimensions.x - containmentBuffer);
            }
            else
            {
                bufferedPosition.x = 0;
            }
            if (worldDimensions.y > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, mine.Position.y, worldDimensions.y - mine.Position.y);
                bufferedPosition.y = Mathf.Clamp(bufferedPosition.y, containmentBuffer, worldDimensions.y - containmentBuffer);
            }
            else
            {
                bufferedPosition.y = 0;
            }
            if (worldDimensions.z > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, mine.Position.z, worldDimensions.z - mine.Position.z);
                bufferedPosition.z = Mathf.Clamp(bufferedPosition.z, containmentBuffer, worldDimensions.z - containmentBuffer);
            }
            else
            {
                bufferedPosition.z = 0;
            }
            if (bufferedPosition == mine.Position + mine.Velocity)
            {
                steer = Vector3.zero;
                return;
            }
            if (distanceToBorder <= 0) distanceToBorder = .001f;

            mine.GetSeekVector(out steer, bufferedPosition);
            steer *= containmentBuffer / distanceToBorder;
        }

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            steer = Vector3.zero;
        }
    }
}