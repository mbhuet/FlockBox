using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    public class ExternalAgent : Agent
    {
        public FlockBox neighborhood;
        private Vector3 _lastPosition;

        protected override FlockBox FindNeighborhood()
        {
            return neighborhood;
        }

        protected override void JoinNeighborhood(FlockBox neighborhood)
        {
            myNeighborhood = neighborhood;
        }

        protected override void LateUpdate()
        {
            if (isAlive && transform.hasChanged)
            {
                if (neighborhood != null)
                {
                    Position = myNeighborhood.transform.InverseTransformPoint(transform.position);
                    Velocity = myNeighborhood.transform.InverseTransformDirection((transform.position - _lastPosition)/Time.deltaTime);
                    ValidateVelocity();
                    if(Velocity != Vector3.zero)
                    {
                        Forward = Velocity.normalized;
                    }
                }
                if (ValidatePosition())
                {
                    FindNeighborhoodBuckets();
                }
                else
                {
                    RemoveFromAllNeighborhoods();
                }
                transform.hasChanged = false;
            }
            _lastPosition = transform.position;

        }
    }
}
