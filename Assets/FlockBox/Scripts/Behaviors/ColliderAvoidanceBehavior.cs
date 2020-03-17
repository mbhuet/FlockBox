using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    [System.Serializable]
    public class ColliderAvoidanceBehavior : ForecastSteeringBehavior
    {

        public LayerMask mask;
        RaycastHit hit;

        public override bool CanUseTagFilter => false;

        public override void GetSteeringBehaviorVector(out Vector3 steer, SteeringAgent mine, SurroundingsContainer surroundings)
        {
            Ray myRay = new Ray(mine.Position, mine.Forward);
            float rayDist = surroundings.lookAheadSeconds * mine.Velocity.magnitude;            

            if (!ObstacleInPath(myRay, mine.shape.radius, rayDist, ref hit, mask))
            {
                steer = Vector3.zero;
                return;
            }
            steer = ObstacleRays(myRay, mine.shape.radius, rayDist, ref hit, mask);
            
            steer = steer.normalized * mine.activeSettings.maxForce - mine.Velocity;

        }

        bool ObstacleInPath(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit, LayerMask mask)
        {
            if (Physics.SphereCast(ray.origin, rayRadius, ray.direction, out hit, perceptionDistance, mask))
            {
                return true;
            }
            else { }
            return false;
        }


        Vector3 forward;
        Quaternion rot;
        Vector3[] rayDirections;

        Vector3 ObstacleRays(Ray ray, float rayRadius, float perceptionDistance, ref RaycastHit hit, LayerMask mask)
        {
            rayDirections = BoidHelper.directions;
            forward = ray.direction;
            rot = Quaternion.LookRotation(forward);

            for (int i = 0; i < rayDirections.Length; i++)
            {
                Vector3 dir = rot * (rayDirections[i]);
                ray.direction = dir;
                if (!Physics.SphereCast(ray, rayRadius, out hit, perceptionDistance, mask))
                {
                    return dir;
                }
            }

            return forward;
        }
    }
}