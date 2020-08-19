using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(PerceptionSystemGroup))]
    public class NeighborPerceptionSystem : SystemBase
    {
        protected EntityQuery flockQuery;
        private List<FlockData> flocks;

        protected override void OnCreate()
        {
            flocks = new List<FlockData>();
            flockQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<FlockData>(), ComponentType.ReadWrite<AgentData>()},
            });

        }


        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(flocks);

            // Each variant of the Boid represents a different value of the SharedComponentData and is self-contained,
            // meaning Boids of the same variant only interact with one another. Thus, this loop processes each
            // variant type individually.
            for (int flockIndex = 0; flockIndex < flocks.Count; flockIndex++)
            {
                var settings = flocks[flockIndex];
                flockQuery.AddSharedComponentFilter(settings);

                var boidCount = flockQuery.CalculateEntityCount();

                if (boidCount == 0)
                {
                    // Early out. If the given variant includes no Boids, move on to the next loop.
                    // For example, variant 0 will always exit early bc it's it represents a default, uninitialized
                    // Boid struct, which does not appear in this sample.
                    flockQuery.ResetFilter();
                    continue;
                }

                FlockBox flockData = settings.Flock;


                NativeArray<AgentData> map = flockQuery.ToComponentDataArray<AgentData>(Allocator.TempJob);


                var fillJobHandle = Entities
                    .WithSharedComponentFilter(settings)
                    .WithReadOnly(map)
                    .ForEach((ref DynamicBuffer<NeighborData> neighbors, ref AgentData agent, ref PerceptionData perception) =>
                    {
                        
                        neighbors.Clear();

                        for (int i=0; i< map.Length; i++)
                        {
                            if (math.length(map[i].Position - agent.Position) < 10)// perception.perceptionRadius)
                            {
                                neighbors.Add(map[i]);
                            }
                        }

                        perception.Clear();
                        
                    })
                    .ScheduleParallel(Dependency);

                Dependency = fillJobHandle;
                map.Dispose(Dependency);

                //NativeMultiHashMap<int, NativeArray<AgentData>> cells = new NativeMultiHashMap<int, NativeArray<AgentData>>(flockData.TotalCells, Allocator.TempJob);

                // DO THINGS HERE

                //hash job
                //each agent gets one index in the map
                //agent's occupying cells (hashed) are added to map at that index

                //merge job
                //map is read in, read only
                //new map is created, one index per cell

                //steer job
                //merged map is read in, read only
                //cell within perception are added to neighbor buffer

                /*
                var initialCellAlignmentJobHandle = Entities
                    .WithSharedComponentFilter(settings)
                    .ForEach((int entityInQueryIndex, in LocalToWorld localToWorld) =>
                    {
                        //cellAlignment[entityInQueryIndex] = localToWorld.Forward;
                    })
                    .ScheduleParallel(Dependency);
*/

                // We pass the job handle and add the dependency so that we keep the proper ordering between the jobs
                // as the looping iterates. For our purposes of execution, this ordering isn't necessary; however, without
                // the add dependency call here, the safety system will throw an error, because we're accessing multiple
                // pieces of boid data and it would think there could possibly be a race condition.

                flockQuery.AddDependency(Dependency);
                flockQuery.ResetFilter();
            }
            flocks.Clear();
        
        }
    }
}
