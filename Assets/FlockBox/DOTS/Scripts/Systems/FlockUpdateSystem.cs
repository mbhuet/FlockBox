#if FLOCKBOX_DOTS
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(PerceptionSystemGroup))]
    public class FlockUpdateSystem : SystemBase
    {
        protected EntityQuery m_Query;
        private List<FlockBox> toUpdate = new List<FlockBox>();

        protected override void OnCreate()
        {
            m_Query = GetEntityQuery(ComponentType.ReadOnly<FlockData>());
            FlockBox.OnValuesModified += OnSettingsChanged;
        }

        protected override void OnDestroy()
        {
            FlockBox.OnValuesModified -= OnSettingsChanged;
        }

        protected override void OnUpdate()
        {
            foreach(FlockBox changed in toUpdate)
            {
                FlockData data = new FlockData { Flock = changed };
                float3 dimensions = changed.WorldDimensions;
                float margin = changed.boundaryBuffer;
                bool wrap = changed.wrapEdges;

                Dependency = Entities
                    .WithSharedComponentFilter(data)
                    .ForEach((ref BoundaryData boundary) => {
                        boundary.Dimensions = dimensions;
                        boundary.Margin = margin;
                        boundary.Wrap = wrap;
                    }).ScheduleParallel(Dependency);
            }
            toUpdate.Clear();
        }

        private void OnSettingsChanged(FlockBox changed)
        {
            toUpdate.Add(changed);
        }
    }
}
#endif