using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    [System.Serializable]
    public class PursuitBehavior : SeekBehavior
    {
        protected override Vector3 GetSteeringVectorForTarget(SteeringAgent mine, Agent target)
        {
            Vector3 distance = target.Position - mine.Position;
            float est_timeToIntercept = distance.magnitude / mine.activeSettings.maxSpeed;
            Vector3 predictedInterceptPosition = target.Position + target.Velocity * est_timeToIntercept;

            mine.GetSeekVector(out Vector3 steer, predictedInterceptPosition);
            return steer;
        }
    }
}