﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class CohesionBehavior : RadialSteeringBehavior, IConvertToComponentData
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

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new CohesionData { Radius = effectiveRadius, TagMask = TagMaskUtility.GetTagMask(filterTags) });
        }
    }
}