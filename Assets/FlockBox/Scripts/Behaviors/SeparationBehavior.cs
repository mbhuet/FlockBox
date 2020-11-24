﻿using Unity.Entities;
using UnityEngine;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    [System.Serializable]
    public class SeparationBehavior : RadialSteeringBehavior, IConvertToSteeringBehaviorComponentData<SeparationData>
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            steer = Vector3.zero;
            int count = 0;
            foreach (Agent other in GetFilteredAgents(surroundings, this))
            {

                if (WithinEffectiveRadius(mine, other))
                {
                    Vector3 diff = mine.Position - other.Position;
                    if (diff.sqrMagnitude < .001f) diff = UnityEngine.Random.insideUnitCircle * .01f;
                    steer += (diff.normalized / diff.magnitude);
                    count++;
                }
            }
            if (count > 0)
            {
                steer /= ((float)count);
            }

            if (steer.magnitude > 0)
            {
                mine.GetSteerVector(out steer, steer);
            }
        }



        public SeparationData Convert()
        {
            return new SeparationData
            {
                Active = IsActive,
                Weight = weight,
                Radius = effectiveRadius,
                TagMask = (useTagFilter ? TagMaskUtility.GetTagMask(filterTags) : int.MaxValue)
            };
        }

        public bool HasEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.HasEntityData(this, entity, entityManager);
        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);
    }
}