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

        private Vector3 containedPosition;
        private float[] minArray = new float[3];
        public void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, Vector3 worldDimensions, float containmentMargin)
        {
            containedPosition = mine.Position + mine.Velocity * lookAheadSeconds;
            float distanceToBorder = float.MaxValue;

            if (worldDimensions.x > 0)
            {
                minArray[0] = distanceToBorder;
                minArray[1] = mine.Position.x;
                minArray[2] = worldDimensions.x - mine.Position.x;
                distanceToBorder = Mathf.Min(minArray);
                containedPosition.x = Mathf.Clamp(containedPosition.x, containmentMargin, worldDimensions.x - containmentMargin);
            }
            else
            {
                containedPosition.x = 0;
                distanceToBorder = 0;
            }
            if (worldDimensions.y > 0)
            {
                minArray[0] = distanceToBorder;
                minArray[1] = mine.Position.y;
                minArray[2] = worldDimensions.y - mine.Position.y;
                distanceToBorder = Mathf.Min(minArray);
                containedPosition.y = Mathf.Clamp(containedPosition.y, containmentMargin, worldDimensions.y - containmentMargin);
            }
            else
            {
                containedPosition.y = 0;
                distanceToBorder = 0;
            }
            if (worldDimensions.z > 0)
            {
                minArray[0] = distanceToBorder;
                minArray[1] = mine.Position.z;
                minArray[2] = worldDimensions.z - mine.Position.z;
                distanceToBorder = Mathf.Min(minArray);
                containedPosition.z = Mathf.Clamp(containedPosition.z, containmentMargin, worldDimensions.z - containmentMargin);
            }
            else
            {
                containedPosition.z = 0;
                distanceToBorder = 0;
            }
            if (containedPosition == mine.Position + mine.Velocity)
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
            return new ContainmentData { };
        }

        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);
        public void EntityCommandBufferAdd(Entity entity, EntityCommandBuffer buf) => IConvertToComponentDataExtension.EntityCommandBufferAdd(this, entity, buf);
        public void EntityCommandBufferRemove(Entity entity, EntityCommandBuffer buf) => IConvertToComponentDataExtension.EntityCommandBufferRemove(this, entity, buf);
        public void EntityCommandBufferSet(Entity entity, EntityCommandBuffer buf) => IConvertToComponentDataExtension.EntityCommandBufferSet(this, entity, buf);
    }
}