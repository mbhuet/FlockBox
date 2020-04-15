using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class GlobalRadialSteeringBehavior : RadialSteeringBehavior
    {

        public bool globalSearch;
        
        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            if (globalSearch)
            {
                foreach (string tag in filterTags)
                {
                    surroundings.AddGlobalSearchTag(tag);
                }
            }
            else
            {
                base.AddPerception(agent, surroundings);
            }
        }

    }
}
