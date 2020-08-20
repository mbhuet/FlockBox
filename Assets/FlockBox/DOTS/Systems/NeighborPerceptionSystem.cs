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

        public static int[] GetOccupyingCells(float3 Position, float cellSize, int dimensions_x, int dimensions_y, int dimensions_z)
        {
            //TODO check Point vs Sphere shape
            return new int[] { WorldPositionToHash(Position.x, Position.y, Position.z, cellSize, dimensions_x, dimensions_y, dimensions_z) };
        }

        public static int[] GetPercievedCells(float3 Position, float cellSize, int dimensions_x, int dimensions_y, int dimensions_z, ref PerceptionData perception)
        {
            return new int[] { WorldPositionToHash(Position.x, Position.y, Position.z, cellSize, dimensions_x, dimensions_y, dimensions_z) };
        }

        private static int CellPositionToHash(int x, int y, int z, int dimensions_x, int dimensions_y, int demensions_z)
        {
            if (x < 0 || y < 0 || z < 0) return -1;

            return (
                 x
               + y * (dimensions_x + 1) // +1 in case dimension is 0, will still produce unique hash
               + z * (dimensions_x + 1) * (dimensions_y + 1));
        }

        private static int WorldPositionToHash(float x, float y, float z, float cellSize, int dimensions_x, int dimensions_y, int dimensions_z)
        {
            return CellPositionToHash((int)(x / cellSize), (int)(y / cellSize), (int)(z / cellSize), dimensions_x, dimensions_y, dimensions_z);
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

                var hashMap = new NativeMultiHashMap<int, AgentData>(agentCount, Allocator.TempJob);


                var parallelHashMap = hashMap.AsParallelWriter();
                var hashPositionsJobHandle = Entities
                    .WithSharedComponentFilter(settings)
                    .ForEach((in AgentData agent) =>
                    {
                        var hash = (int)math.hash(new int3(math.floor(agent.Position / cellSize)));


                        var cells = new NativeList<int>(Allocator.Temp);
                        cells.Add(hash);
                        
                        for (int i = 0; i < cells.Length; i++)
                        {
                            parallelHashMap.Add(cells[i], agent);
                        }
                    })
                    .ScheduleParallel(Dependency);

                Dependency = hashPositionsJobHandle;


                var fillJobHandle = Entities
                    .WithSharedComponentFilter(settings)
                    .WithReadOnly(hashMap)
                    .ForEach((ref DynamicBuffer<NeighborData> neighbors, ref AgentData agent, ref PerceptionData perception) =>
                    {

                        neighbors.Clear();
                        var hash = (int)math.hash(new int3(math.floor(agent.Position / cellSize)));

                        var cells = new NativeList<int>(Allocator.Temp);
                        cells.Add(hash);
                        
                        AgentData value;

                        for (int i = 0; i < cells.Length; i++)
                        {
                            if (hashMap.TryGetFirstValue(cells[i], out value, out var iterator))
                            {
                                do
                                {
                                    neighbors.Add(value);

                                } while (hashMap.TryGetNextValue(out value, ref iterator));
                            }
                        }
                        perception.Clear();

                    })
                    .ScheduleParallel(Dependency);


                Dependency = fillJobHandle;



                var disposeJobHandle = hashMap.Dispose(Dependency);


                /*

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
                */

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
