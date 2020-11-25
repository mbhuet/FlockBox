using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct BoundaryData : IComponentData
    {
        public float3 Dimensions;
        public float Margin;
        public bool Wrap;


        public float3 ValidatePosition(float3 position)
        {
            if (Wrap)
            {
                //if any dimension is 0, using fmod will cause an error
                if (Dimensions.x > 0)
                {
                    position.x = math.fmod(position.x, Dimensions.x);
                    if (position.x < 0)
                    {
                        position.x += Dimensions.x;
                    }
                }
                else
                {
                    position.x = 0;
                }

                if(Dimensions.y > 0)
                {
                    position.y = math.fmod(position.y, Dimensions.y);
                    if (position.y < 0)
                    {
                        position.y += Dimensions.y;
                    }
                }
                else
                {
                    position.y = 0;
                }

                if(Dimensions.z > 0)
                {
                    position.z = math.fmod(position.z, Dimensions.z);
                    if (position.z < 0)
                    {
                        position.z += Dimensions.z;
                    }
                }
                else
                {
                    position.z = 0;
                }
            }
            else
            {
                position = math.clamp(position, float3.zero, Dimensions);
            }
            return position;
        }
        public float3 ValidateVelocity(float3 velocity)
        {
            if (Dimensions.x == 0) velocity.x = 0;
            if (Dimensions.y == 0) velocity.y = 0;
            if (Dimensions.z == 0) velocity.z = 0;
            return velocity;
        }

    }
}
