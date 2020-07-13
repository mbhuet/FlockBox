using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CloudFine
{
    public class ContainmentBehavior : ForecastSteeringBehavior, IConvertToSteeringBehaviorComponentData<ContainmentData>
    {
        public override bool CanUseTagFilter { get { return false; } }
        public override bool CanToggleActive { get { return false; } }

        private Vector3 containedPosition;
        private Vector3 unclampedPosition;

        public void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, Vector3 worldDimensions, float containmentMargin)
        {
            unclampedPosition = mine.Position + mine.Velocity * lookAheadSeconds;
            containedPosition = unclampedPosition;

            float distanceToBorder = float.MaxValue;

            if (worldDimensions.x > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, Mathf.Min(mine.Position.x, worldDimensions.x - mine.Position.x));
                containedPosition.x = Mathf.Clamp(containedPosition.x, containmentMargin, worldDimensions.x - containmentMargin);
            }
            else
            {
                containedPosition.x = 0;
                distanceToBorder = 0;
            }

            if (worldDimensions.y > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, Mathf.Min(mine.Position.y, worldDimensions.y - mine.Position.y));
                containedPosition.y = Mathf.Clamp(containedPosition.y, containmentMargin, worldDimensions.y - containmentMargin);
            }
            else
            {
                containedPosition.y = 0;
                distanceToBorder = 0;
            }

            if (worldDimensions.z > 0)
            {
                distanceToBorder = Mathf.Min(distanceToBorder, Mathf.Min(mine.Position.z, worldDimensions.z - mine.Position.z));
                containedPosition.z = Mathf.Clamp(containedPosition.z, containmentMargin, worldDimensions.z - containmentMargin);
            }
            else
            {
                containedPosition.z = 0;
                distanceToBorder = 0;
            }

            if (containedPosition == unclampedPosition)
            {
                steer = Vector3.zero;
                return;
            }
            if (distanceToBorder <= 0) distanceToBorder = .001f;

            mine.GetSeekVector(out steer, containedPosition);
            steer *= containmentMargin / distanceToBorder;
        }

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            steer = Vector3.zero;
        }





        public ContainmentData Convert()
        {
            return new ContainmentData { Weight = weight, LookAheadSeconds = lookAheadSeconds, Margin = 10, Dimensions = new float3(100,100,100), LookAheadSeconds = 1};
        }

        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);
    }
}