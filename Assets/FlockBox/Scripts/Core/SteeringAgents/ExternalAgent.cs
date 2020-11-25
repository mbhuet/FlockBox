using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine.FlockBox
{
    public class ExternalAgent : Agent
    {
        public FlockBox neighborhood;

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
        }
    }
}
