using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CloudFine
{
    public abstract class GlobalBehavior : SteeringBehavior
    {
        public override void AddPerception(SurroundingsContainer surroundings)
        {
            base.AddPerception(surroundings);
            foreach (string tag in filterTags)
            {
                if (!surroundings.globalSearchTags.Contains(tag))
                {
                    surroundings.globalSearchTags.Add(tag);
                }
            }
        }
    }
}
