using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
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
        private Vector3 _lastPosition;

        protected override void ApplySteeringAcceleration()
        {
            _lastPosition = Position;
            base.ApplySteeringAcceleration();
        }

        protected override void UpdateTransform()
        {
            _worldPosDelta = Vector3.zero;

            if(Physics.Raycast(WorldPosition + Vector3.up * _raycastDistance * .5f, Vector3.down, out _terrainHit, _raycastDistance, _terrainLayerMask))
            {
                _worldPosDelta = _terrainHit.point - FlockBoxToWorldPosition(_lastPosition);
                Position = WorldToFlockBoxPosition(_terrainHit.point);
            }
            transform.position = FlockBoxToWorldPosition(Position);

            if (_worldPosDelta.magnitude > 0)
            {
                Vector3 terrainForward = WorldToFlockBoxDirection(_worldPosDelta);
                transform.rotation = LookRotation(FlockBoxToWorldDirection(terrainForward));
                Forward = terrainForward;
            }
            else if (Velocity.magnitude > 0)
            {
                transform.rotation = LookRotation(FlockBoxToWorldDirection(Velocity));
                Forward = Velocity;
            }
            else
            {
                Forward = WorldToFlockBoxDirection(transform.rotation * Vector3.forward);
            }
        }

    }
}