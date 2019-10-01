using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class ForecastSteeringBehavior : SteeringBehavior
    {
        public float lookAheadSeconds = 1;
    }
}
