

namespace CloudFine.FlockBox
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
