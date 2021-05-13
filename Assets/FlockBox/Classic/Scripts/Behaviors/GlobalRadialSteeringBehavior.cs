using UnityEngine;

namespace CloudFine.FlockBox
{
    public abstract class GlobalRadialSteeringBehavior : RadialSteeringBehavior
    {
        [Tooltip("[Optimization] Will search globally for targets by Tag instead of distance. Use if there are few targets and/or they need to be preceptible from far away. ")]
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
