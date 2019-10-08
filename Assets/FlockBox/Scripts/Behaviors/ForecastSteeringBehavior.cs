using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class ForecastSteeringBehavior : SteeringBehavior
    {
        public float lookAheadSeconds = 1;

        public override void AddPerception(SurroundingsContainer surroundings)
        {
            base.AddPerception(surroundings);
            if(lookAheadSeconds > surroundings.lookAheadSeconds)
            {
                surroundings.lookAheadSeconds = lookAheadSeconds;
            }
        }
    }
}
