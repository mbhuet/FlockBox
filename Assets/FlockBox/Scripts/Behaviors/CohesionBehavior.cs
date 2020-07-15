using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class CohesionBehavior : RadialSteeringBehavior, IConvertToSteeringBehaviorComponentData<CohesionData>
    {
        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            //steer used as midpoint to prevent garbage
            steer = Vector3.zero;
            float count = 0;
            foreach (Agent other in GetFilteredAgents(surroundings, this))
            {
                if (WithinEffectiveRadius(mine, other))
                {
                    steer += (other.Position);
                    count ++;
                }
            }
            if (count > 0)
            {
                steer /= (count);
                mine.GetSeekVector(out steer, steer);
            }
            else
            {
                steer = Vector3.zero;
            }
        }


        public CohesionData Convert()
        {
            return new CohesionData
            {
                Active = IsActive,
                Weight = weight,
                Radius = effectiveRadius,
                TagMask = (useTagFilter ? TagMaskUtility.GetTagMask(filterTags) : 0)
            };
        }

        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);

    }
}