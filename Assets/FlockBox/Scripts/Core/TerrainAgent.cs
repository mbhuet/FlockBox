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

        
        protected override void UpdateVelocity()
        {
            base.UpdateVelocity();
            //project onto terrain, get new velocity

            RaycastHit currentPostHit;
            Physics.Raycast(Position + Vector3.up * 500f, Vector3.down * 1000f, out currentPostHit, 1000, terrainLayerMask);

            RaycastHit goalPosHit;
            Physics.Raycast((Position + Velocity) + Vector3.up * 500f, Vector3.down * 1000f, out goalPosHit, 1000, terrainLayerMask);


            
                Velocity = (goalPosHit.point - currentPostHit.point).normalized * Velocity.magnitude;
 


        }
        

        protected override void UpdatePosition()
        {
            base.UpdatePosition();

            terrainRay.origin = (Position) + Vector3.up * 500f;
            terrainRay.direction = Vector3.down;
            //Debug.DrawRay(terrainRay.origin, terrainRay.direction * 1000, Color.cyan, 10);
            if (Physics.Raycast(terrainRay, out terrainHit, 1000, terrainLayerMask))
            {
                Position = terrainHit.point + Vector3.up * shape.radius;
            }

        }


    }
}