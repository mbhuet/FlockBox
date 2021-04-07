using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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

        public float3 GetWorldPosition(in LocalToWorld ltw, in LocalToParent ltp)
        {
            return FlockToWorldPoint(ltw, ltp, Position);
        }

        public float3 GetWorldForward(in LocalToWorld ltw, in LocalToParent ltp)
        {
            return FlockToWorldDirection(ltw, ltp, Forward);
        }


        //Flock To Local
        public static float3 FlockToLocalPoint(in LocalToParent flockLtp, float3 flockPoint)
        {
            return math.transform(math.inverse(flockLtp.Value), flockPoint);
        }

        public static float3 FlockToLocalDirection(in LocalToParent flockLtp, float3 flockDirection)
        {
           return math.rotate(math.inverse(flockLtp.Value), flockDirection);
        }


        //Local To Flock
        public static float3 LocalToFlockPoint(in LocalToParent flockLtp, float3 localPoint)
        {
            return math.transform(flockLtp.Value, localPoint);
        }

        public static float3 LocalToFlockDirection(in LocalToParent flockLtp, float3 localDirection)
        {
            return math.rotate(flockLtp.Value, localDirection);
        }


        //Flock To World
        public static float3 FlockToWorldPoint(in LocalToWorld agentLtw, in LocalToParent flockLtp, float3 flockPoint)
        {
            return math.transform(agentLtw.Value, FlockToLocalPoint(flockLtp, flockPoint));
        }
        
        public static float3 FlockToWorldDirection(in LocalToWorld agentLtw, in LocalToParent flockLtp, float3 flockDirection)
        {
            return math.rotate(agentLtw.Rotation, FlockToLocalDirection(flockLtp, flockDirection));
        }


        //World To Local
        public static float3 WorldToLocalPoint(in LocalToWorld agentLtw, float3 worldPoint)
        {
            return math.transform(math.inverse(agentLtw.Value), worldPoint);
        }

        public static float3 WorldToLocalDirection(in LocalToWorld agentLtw, float3 worldDirection)
        {
            return math.rotate(math.inverse(agentLtw.Value), worldDirection);
        }


        //World To FLock
        public static float3 WorldToFlockPoint(in LocalToWorld agentLtw, in LocalToParent flockLtp, float3 worldPoint)
        {
            return LocalToFlockPoint(flockLtp, WorldToLocalPoint(agentLtw, worldPoint));
        }

        public static float3 WorldToFlockDirection(in LocalToWorld agentLtw, in LocalToParent flockLtp, float3 worldDirection)
        {
            return LocalToFlockDirection(flockLtp, WorldToLocalPoint(agentLtw, worldDirection));
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