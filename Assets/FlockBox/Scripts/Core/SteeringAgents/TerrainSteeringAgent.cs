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

            if(Physics.Raycast(transform.parent.TransformPoint(Position) + Vector3.up * _raycastDistance * .5f, Vector3.down, out _terrainHit, _raycastDistance, _terrainLayerMask))
            {
                _worldPosDelta = _terrainHit.point - transform.position;
                transform.position = _terrainHit.point;// + Vector3.up * shape.radius;
                Position = transform.localPosition;
            }
            else
            {
                transform.localPosition = Position;
            }

            if (_worldPosDelta.magnitude > 0)
            {
                transform.rotation = Quaternion.LookRotation(_worldPosDelta.normalized, Vector3.up);
            }
            else if (Velocity.magnitude > 0)
            {
                transform.localRotation = Quaternion.LookRotation(Velocity.normalized, Vector3.up);
            }

            Forward = transform.localRotation * Vector3.forward;

        }

    }
}