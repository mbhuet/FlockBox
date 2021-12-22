#if FLOCKBOX_DOTS
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct SteeringData : IComponentData
    {
        public float MaxSpeed;
        public float MaxForce;

        /// <summary>
        /// Get steering vector to seek desired position
        /// </summary>
        /// <param name="desiredPosition"></param>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public float3 GetSeekVector(float3 desiredPosition, float3 position, float3 velocity)
        {
            return GetSteerVector(desiredPosition - position, velocity);
        }

        /// <summary>
        /// Get steering vector for desired forward direction
        /// </summary>
        /// <param name="desiredForward"></param>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public float3 GetSteerVector(float3 desiredForward, float3 velocity)
        {
            if (math.all(desiredForward == float3.zero)) return float3.zero;
            float3 steer = math.normalize(desiredForward) * MaxSpeed - velocity;
            if (math.all(steer == float3.zero)) return float3.zero;
            steer = math.normalize(steer) * math.min(math.length(steer), MaxForce);
            return steer;
        }

        public float3 DesiredVelocityForArrival(float3 currentPosition, float3 arrivePosition, float stopRadius, float maxSpeed)
        {
            return math.normalize(arrivePosition - currentPosition)
                * math.lerp(0, maxSpeed, math.lengthsq(arrivePosition - currentPosition) / (stopRadius * stopRadius));
        }
    }
}
#endif