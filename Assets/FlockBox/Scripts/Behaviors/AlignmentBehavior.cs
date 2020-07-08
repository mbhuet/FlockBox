using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace CloudFine
{
    [System.Serializable]
    public class AlignmentBehavior : RadialSteeringBehavior, IConvertToComponentData<AlignmentData>
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            foreach (Agent other in GetFilteredAgents(surroundings, this))
            {
                if (WithinEffectiveRadius(mine, other))
                {
                    float modFactor = 1;
                    sum += (other.Velocity) * modFactor;
                    count++;
                }
            }
            if (count > 0)
            {
                sum /= ((float)count);
                mine.GetSteerVector(out steer, sum);
            }
            else
            {
                steer = Vector3.zero;
            }
        }


        public AlignmentData Convert()
        {
            return new AlignmentData { Radius = effectiveRadius, TagMask = TagMaskUtility.GetTagMask(filterTags) };
        }

        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void EntityCommandBufferAdd(Entity entity, EntityCommandBuffer buf) => IConvertToComponentDataExtension.EntityCommandBufferAdd(this, entity, buf);
        public void EntityCommandBufferRemove(Entity entity, EntityCommandBuffer buf) => IConvertToComponentDataExtension.EntityCommandBufferRemove(this, entity, buf);
        public void EntityCommandBufferSet(Entity entity, EntityCommandBuffer buf) => IConvertToComponentDataExtension.EntityCommandBufferSet(this, entity, buf);
    }
}