using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class ForecastSteeringBehavior : SteeringBehavior
    {
        public float lookAheadSeconds = 1;

        public override void AddPerception(ref SurroundingsInfo surroundings)
        {
            base.AddPerception(ref surroundings);
            if(lookAheadSeconds > surroundings.lookAheadSeconds)
            {
                surroundings.lookAheadSeconds = lookAheadSeconds;
            }
        }
    }
}
