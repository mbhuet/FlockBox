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
        public float raycastRange = 1000f;
        
        protected override void UpdateVelocity()
        {
            base.UpdateVelocity();
            //project onto terrain, get new velocity

            RaycastHit currentPostHit;
            RaycastToTerrain(Position, out currentPostHit);
            //Physics.Raycast(Position + Vector3.up * 500f, Vector3.down * 1000f, out currentPostHit, 1000, terrainLayerMask);

            RaycastHit goalPosHit;
            RaycastToTerrain(Position + Velocity, out goalPosHit);
            //Physics.Raycast((Position + Velocity) + Vector3.up * 500f, Vector3.down * 1000f, out goalPosHit, 1000, terrainLayerMask);


            
                Velocity = (goalPosHit.point - currentPostHit.point).normalized * Velocity.magnitude;
 


        }
        

        protected override void UpdatePosition()
        {
            base.UpdatePosition();

            terrainRay.origin = (Position) + Vector3.up * 500f;
            terrainRay.direction = Vector3.down;
            //Debug.DrawRay(terrainRay.origin, terrainRay.direction * 1000, Color.cyan, 10);

            if (RaycastToTerrain(Position, out terrainHit))
                //Physics.Raycast(terrainRay, out terrainHit, 1000, terrainLayerMask))
            {
                Position = terrainHit.point + Vector3.up * shape.radius;
            }

        }

        private bool RaycastToTerrain(Vector3 origin, out RaycastHit hit)
        {
            return Physics.Raycast(origin + Vector3.up * raycastRange * .5f, Vector3.down, out hit, raycastRange, terrainLayerMask);
        }

    }
}