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

        private Vector3 worldPosDelta;

        


        protected override void UpdateTransform()
        {
            Vector3 oldWorldPos = transform.position;
            worldPosDelta = Vector3.zero;

            if (RaycastToTerrain(Position, out terrainHit))
            {
                transform.position = terrainHit.point + Vector3.up * shape.radius;
                worldPosDelta = transform.position - oldWorldPos;
            }
            else
            {
                transform.localPosition = Position;
            }

            if (worldPosDelta.magnitude > 0)
            {
                transform.rotation = Quaternion.LookRotation(worldPosDelta.normalized, Vector3.up);
            }
            else if (Velocity.magnitude > 0)
            {
                transform.localRotation = Quaternion.LookRotation(Velocity.normalized, Vector3.up);
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