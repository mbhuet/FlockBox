using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(PerceptionSystemGroup))]
    public class FlockUpdateSystem : JobComponentSystem
    {
        protected EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(typeof(FlockData));
            FlockBox.OnValuesModified += OnSettingsChanged;
        }

        protected override void OnDestroy()
        {
            FlockBox.OnValuesModified -= OnSettingsChanged;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return inputDeps;
        }

        private void OnSettingsChanged(FlockBox changed)
        {
            BoundaryData boundary = new BoundaryData { Dimensions = changed.WorldDimensions, Margin = changed.boundaryBuffer, Wrap = changed.wrapEdges };
            m_Query.SetSharedComponentFilter(new FlockData { Flock = changed });
            NativeArray<Entity> entities = m_Query.ToEntityArray(Allocator.TempJob);
            foreach (Entity entity in entities)
            {
                EntityManager.SetComponentData(entity, boundary);
            }
            entities.Dispose();
        }


    }
}
