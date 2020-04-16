using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class GlobalRadialSteeringBehavior : RadialSteeringBehavior
    {
        [Tooltip("Use if there are few targets and/or they need to be preceptible from far away.")]
        public bool globalTagSearch;
        
        public override void AddPerception(SteeringAgent agent, SurroundingsContainer surroundings)
        {
            if (globalTagSearch)
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
