#if FLOCKBOX_DOTS
using System;
using Unity.Entities;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    public struct AgentData : IComponentData, IEquatable<AgentData>
    {
        public byte Tag;
        public float Radius;

        public bool Fill;

        public float3 Position;
        public float3 Velocity;
        public float3 Forward;

        public bool Sleeping;
        public int UniqueID;

        public bool TagInMask(int mask)
        {
            return ((1 << Tag & mask) != 0);
        }


        public bool FindRayIntersection(float3 rayOrigin, float3 rayDirection, float rayDist, float rayRadius, ref float t)
        {
            rayRadius += Radius;

            float3 oc = rayOrigin - Position;
            float a = math.dot(rayDirection, rayDirection);
            float b = 2f * math.dot(oc, rayDirection);
            float c = math.dot(oc, oc) - rayRadius * rayRadius;
            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
            {
                return false;
            }
            else
            {
                float numerator = -b - math.sqrt(discriminant);
                if (numerator > 0)
                {
                    t = numerator / (2f * a);
                    return true;
                }

                numerator = -b + math.sqrt(discriminant);
                if (numerator > 0)
                {
                    //currently inside sphere
                    t = 0;
                    //t = numerator / (2f * a);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Equals(AgentData other)
        {
            return math.all(Position == other.Position);
        }
    }
}
#endif