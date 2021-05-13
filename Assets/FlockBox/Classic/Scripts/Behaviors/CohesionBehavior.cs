using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public partial class CohesionBehavior : RadialSteeringBehavior
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
    }
}