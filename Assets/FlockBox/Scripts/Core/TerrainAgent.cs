using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class TerrainAgent : SteeringAgent
    {
        public LayerMask terrainLayerMask;
        private Ray terrainRay;
        private RaycastHit terrainHit;
        RaycastHit currentPostHit;
        RaycastHit goalPosHit;

        public float raycastRange = 1000f;

        private Vector3 slopedVelocity;

        protected override void UpdateVelocity()
        {
            base.UpdateVelocity();

            RaycastToTerrain(Position, out currentPostHit);

            RaycastToTerrain(Position + Velocity * Time.deltaTime, out goalPosHit);

            slopedVelocity = (goalPosHit.point - currentPostHit.point).normalized * Velocity.magnitude;
        }


        protected override void UpdateTransform()
        {
            if (RaycastToTerrain(Position, out terrainHit))
            {
                transform.position = terrainHit.point + Vector3.up * shape.radius;
            }

            if (slopedVelocity.magnitude > 0)
            {
                transform.localRotation = Quaternion.LookRotation(slopedVelocity.normalized, Vector3.up);
            }

            Forward = transform.forward;

        }

        private bool RaycastToTerrain(Vector3 origin, out RaycastHit hit)
        {
            origin = transform.parent.TransformPoint(origin);

           return Physics.Raycast(origin + Vector3.up * raycastRange * .5f, Vector3.down, out hit, raycastRange, terrainLayerMask);
        }

    }
}