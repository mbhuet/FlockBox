using Unity.Entities;
using UnityEngine;
using CloudFine.FlockBox.DOTS;

namespace CloudFine.FlockBox
{
    [DOTSCompatible]
    [System.Serializable]
    public class AlignmentBehavior : RadialSteeringBehavior, IConvertToSteeringBehaviorComponentData<AlignmentData>
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
            return new AlignmentData
            {
                Active = IsActive,
                Weight = weight, 
                Radius = effectiveRadius,
                TagMask = (useTagFilter ? TagMaskUtility.GetTagMask(filterTags) : int.MaxValue)
            };
        }

        public void AddEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.AddEntityData(this, entity, entityManager);
        public void SetEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.SetEntityData(this, entity, entityManager);
        public void RemoveEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.RemoveEntityData(this, entity, entityManager);
    }
}