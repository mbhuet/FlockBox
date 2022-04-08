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
            foreach (FlockBox changed in toUpdate)
            {
                FlockData data = new FlockData { Flock = changed };
                float3 dimensions = changed.WorldDimensions;
                float margin = changed.boundaryBuffer;
                bool wrap = changed.wrapEdges;
                bool worldSpace = changed.useWorldSpace;
                float4x4 wtf = changed.transform.worldToLocalMatrix;

                var boundaryUpdateJob = Entities
                    .WithSharedComponentFilter(data)
                    .ForEach((ref BoundaryData boundary) => {
                        boundary.Dimensions = dimensions;
                        boundary.Margin = margin;
                        boundary.Wrap = wrap;
                    }).ScheduleParallel(Dependency);

                JobHandle flockMatrixUpdateJob = default;

                if (worldSpace)
                {
                    flockMatrixUpdateJob = Entities
                        .WithSharedComponentFilter(data)
                        .ForEach((ref FlockMatrixData flock, ref AgentData agent) =>
                        {
                            //Compensate for flockbox movement
                            agent.Position = math.transform(wtf, flock.FlockToWorldPoint(agent.Position));
                            agent.Velocity = math.transform(wtf, flock.FlockToWorldDirection(agent.Velocity));
                            agent.Forward = math.transform(wtf, flock.FlockToWorldDirection(agent.Forward));
                            flock.WorldToFlockMatrix = wtf;
                        }).ScheduleParallel(Dependency);
                }
                else
                {
                    flockMatrixUpdateJob = Entities
                        .WithSharedComponentFilter(data)
                        .ForEach((ref FlockMatrixData flock) =>
                        {
                            flock.WorldToFlockMatrix = wtf;
                        }).ScheduleParallel(Dependency);
                }

                Dependency = JobHandle.CombineDependencies(boundaryUpdateJob, flockMatrixUpdateJob);
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