#if FLOCKBOX_DOTS
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
        public float3 ValidateDirection(float3 direction)
        {
            if (Dimensions.x == 0) direction.x = 0;
            if (Dimensions.y == 0) direction.y = 0;
            if (Dimensions.z == 0) direction.z = 0;
            return direction;
        }

    }
}
#endif