using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class ForecastSteeringBehavior : SteeringBehavior
    {
        public float lookAheadSeconds = 1;

        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            base.AddPerception(agent, surroundings);
            surroundings.SetMinLookAheadSeconds(lookAheadSeconds);
        }
    }
}
