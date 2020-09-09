using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct BoundaryData : IComponentData
    {
        public float3 Dimensions;
        public float Margin;
        public bool Wrap;


        public void ValidatePosition(ref float3 position)
        {
            if (Wrap)
            {
                position = math.fmod(position, Dimensions);
                if (position.x < 0) position.x += Dimensions.x;
                if (position.y < 0) position.y += Dimensions.y;
                if (position.z < 0) position.z += Dimensions.z;

            }
            else
            {
                position = math.clamp(position, float3.zero, Dimensions);
            }
        }
        public void ValidateVelocity(ref float3 velocity)
        {
            if (Dimensions.x == 0) velocity.x = 0;
            if (Dimensions.y == 0) velocity.y = 0;
            if (Dimensions.z == 0) velocity.z = 0;           
        }
    }
}
