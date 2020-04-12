using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public class TerrainSteeringAgent : SteeringAgent
    {
        public LayerMask _terrainLayerMask;
        /// <summary>
        /// Should be a little more than the maximum elevation of your Terrain. Otherwise, agents spawned beneath the highest points may not find the surface.
        /// </summary>
        public float _raycastDistance = 1000f;

        private RaycastHit _terrainHit;
        private Vector3 _worldPosDelta;

        protected override void UpdateTransform()
        {
            _worldPosDelta = Vector3.zero;

            if(Physics.Raycast(myNeighborhood.transform.TransformPoint(Position) + Vector3.up * _raycastDistance * .5f, Vector3.down, out _terrainHit, _raycastDistance, _terrainLayerMask))
            {
                _worldPosDelta = _terrainHit.point - transform.position;
                Position = myNeighborhood.transform.InverseTransformPoint(_terrainHit.point);
            }
            transform.localPosition = SmoothedPosition(Position);


            if (_worldPosDelta.magnitude > 0)
            {
                Vector3 terrainForward = myNeighborhood.transform.InverseTransformDirection(_worldPosDelta);
                transform.localRotation = SmoothedRotation(terrainForward);
                Forward = terrainForward.normalized;
            }
            else if (Velocity.magnitude > 0)
            {
                transform.localRotation = SmoothedRotation(Velocity);
                Forward = Velocity.normalized;
            }
            else
            {
                Forward = transform.localRotation * Vector3.forward;
            }
        }

    }
}