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
                All = new[] { ComponentType.ReadOnly<FlockData>(), ComponentType.ReadWrite<AgentData>() },
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

                var agentCount = flockQuery.CalculateEntityCount();

                if (agentCount == 0)
                {
                    // Early out. If the given variant includes no Boids, move on to the next loop.
                    // For example, variant 0 will always exit early bc it's it represents a default, uninitialized
                    // Boid struct, which does not appear in this sample.
                    flockQuery.ResetFilter();
                    continue;
                }

                FlockBox flockBox = settings.Flock;
                float cellSize = flockBox.CellSize;
                int dimensions_x = flockBox.DimensionX;
                int dimensions_y = flockBox.DimensionY;
                int dimensions_z = flockBox.DimensionZ;

                var spatialHashMap = new NativeMultiHashMap<int, AgentData>(agentCount, Allocator.TempJob);
                var tagHashMap = new NativeMultiHashMap<byte, AgentData>(agentCount, Allocator.TempJob);


                var parallelSpatialHashMap = spatialHashMap.AsParallelWriter();
                var parallelTagHashMap = tagHashMap.AsParallelWriter();

                var hashPositionsJobHandle = Entities
                    .WithSharedComponentFilter(settings)
                    .ForEach((in AgentData agent) =>
                    {
                        //keep track of all agents by tag
                        parallelTagHashMap.Add(agent.Tag, agent);

                        if (agent.Fill)
                        {
                            int cellRange = 1 + (int)((agent.Radius - .01f) / cellSize);
                            var centerCell = new int3(math.floor(agent.Position / cellSize));

                            for (int x = centerCell.x - cellRange; x <= centerCell.x + cellRange; x++)
                            {
                                for (int y = centerCell.y - cellRange; y <= centerCell.y + cellRange; y++)
                                {
                                    for (int z = centerCell.z - cellRange; z <= centerCell.z + cellRange; z++)
                                    {
                                        if (       x < 0 || x > dimensions_x
                                                || y < 0 || y > dimensions_y
                                                || z < 0 || z > dimensions_z)
                                        {
                                            continue;
                                        }
                                        parallelSpatialHashMap.Add((int)math.hash(new int3(x,y,z)), agent);

                                    }
                                }
                            }
                        }
                        else
                        {
                            parallelSpatialHashMap.Add((int)math.hash(new int3(math.floor(agent.Position / cellSize))), agent);
                        }                        
                    })
                    .ScheduleParallel(Dependency);

                Dependency = hashPositionsJobHandle;


                var findNeighborsJobHandle = Entities
                    .WithSharedComponentFilter(settings)
                    .WithReadOnly(spatialHashMap)
                    .WithReadOnly(tagHashMap)
                    .ForEach((ref DynamicBuffer<NeighborData> neighbors, ref AgentData agent, ref PerceptionData perception) =>
                    {
                        AgentData neighbor;
                        neighbors.Clear();

                        //Check for global search tags
                        int mask = perception.globalSearchTagMask;
                        for (byte tag = 0; tag<sizeof(int); tag++)
                        {
                            if ((1 << tag & mask) != 0)
                            {
                                if (tagHashMap.TryGetFirstValue(tag, out neighbor, out var iterator))
                                {
                                    do
                                    {
                                        neighbors.Add(neighbor);
                                    } while (tagHashMap.TryGetNextValue(out neighbor, ref iterator));
                                }
                            }
                        }


                        //check cells in perception range
                        var hash = (int)math.hash(new int3(math.floor(agent.Position / cellSize)));

                        var cells = new NativeList<int>(Allocator.Temp);
                        cells.Add(hash);
                        

                        for (int i = 0; i < cells.Length; i++)
                        {
                            if (spatialHashMap.TryGetFirstValue(cells[i], out neighbor, out var iterator))
                            {
                                do
                                {
                                    neighbors.Add(neighbor);

                                } while (spatialHashMap.TryGetNextValue(out neighbor, ref iterator));
                            }
                        }
                        perception.Clear();

                    })
                    .ScheduleParallel(Dependency);

                Dependency = findNeighborsJobHandle;

                Dependency = spatialHashMap.Dispose(Dependency);
                Dependency = tagHashMap.Dispose(Dependency);

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
