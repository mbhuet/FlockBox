using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace CloudFine
{
    public class ContainmentBehavior : ForecastSteeringBehavior, IConvertToComponentData<ContainmentData>
    {
        public override bool CanUseTagFilter { get { return false; } }
        public override bool CanToggleActive { get { return false; } }

        private Vector3 bufferedPosition;
        private float[] minArray = new float[3];
        public void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, Vector3 worldDimensions, float containmentBuffer)
        {
            bufferedPosition = mine.Position + mine.Velocity * lookAheadSeconds;
            float distanceToBorder = float.MaxValue;

            if (worldDimensions.x > 0)
            {
                minArray[0] = distanceToBorder;
                minArray[1] = mine.Position.x;
                minArray[2] = worldDimensions.x - mine.Position.x;
                distanceToBorder = Mathf.Min(minArray);
                bufferedPosition.x = Mathf.Clamp(bufferedPosition.x, containmentBuffer, worldDimensions.x - containmentBuffer);
            }
            else
            {
                bufferedPosition.x = 0;
            }
            if (worldDimensions.y > 0)
            {
                minArray[0] = distanceToBorder;
                minArray[1] = mine.Position.y;
                minArray[2] = worldDimensions.y - mine.Position.y;
                distanceToBorder = Mathf.Min(minArray);
                bufferedPosition.y = Mathf.Clamp(bufferedPosition.y, containmentBuffer, worldDimensions.y - containmentBuffer);
            }
            else
            {
                bufferedPosition.y = 0;
            }
            if (worldDimensions.z > 0)
            {
                minArray[0] = distanceToBorder;
                minArray[1] = mine.Position.z;
                minArray[2] = worldDimensions.z - mine.Position.z;
                distanceToBorder = Mathf.Min(minArray);
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


        public override void AddComponentData(Entity entity, EntityManager dstManager)
        {
            dstManager.AddComponentData(entity, Convert());
        }


        public ContainmentData Convert()
        {
            return new ContainmentData { };
        }
    }
}