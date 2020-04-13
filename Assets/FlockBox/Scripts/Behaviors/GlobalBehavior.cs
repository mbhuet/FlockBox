using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class GlobalBehavior : SteeringBehavior
    {
        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            base.AddPerception(agent, surroundings);
            foreach (string tag in filterTags)
            {
                surroundings.AddGlobalSearchTag(tag);
            }
        }
    }
}
