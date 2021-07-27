#if FLOCKBOX_DOTS
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


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
                All = new[] { 
                    ComponentType.ReadOnly<FlockData>(), 
                    ComponentType.ReadOnly<AgentData>() },
            });

        }


        protected override void OnUpdate()
        {
            EntityManager.GetAllUniqueSharedComponentData(flocks);

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
                int cellCap = flockBox.CellCapacity;
                float sleepChance = flockBox.sleepChance;

                //We need to calculate the exact capacity the spatial hashmap is going to need
                //if each agent was guaranteed to only occupy 1 cell, this would not be necessary
                //but some agents (AgentData.Fill = true) will need occupy multiple cells
                int mapCapacity = 0;
                Entities
                    .WithName("CalculateMapCapacity")
                    .WithSharedComponentFilter(settings)
                    .ForEach((in AgentData agent) =>
                    {
                        if (agent.Fill)
                        {
                            //the bounding box around a filled sphere will occupy at least 9 cells
                            //center cell (1) + extension in either direction (x2)
                            int boundingCubeSideLength = 1 + (1 + (int)((agent.Radius - .01f) / cellSize))*2;
                            mapCapacity += boundingCubeSideLength * boundingCubeSideLength * boundingCubeSideLength;
                        }
                        else
                        {
                            mapCapacity++;
                        }
                    }).Run();

                var spatialHashMap = new NativeMultiHashMap<int, AgentData>(mapCapacity, Allocator.TempJob);
                var tagHashMap = new NativeMultiHashMap<byte, AgentData>(agentCount, Allocator.TempJob);

                var rnd = new Unity.Mathematics.Random((uint)(Time.ElapsedTime * 1000 +1));

                //Randomly distribute sleeping
                var sleepJobHandle = Entities
                    .WithName("RandomSleep")
                    .WithSharedComponentFilter(settings)
                    .ForEach((ref AgentData agent) =>
                    {
                        agent.Sleeping = (rnd.NextDouble() < sleepChance);
                    })
                    .ScheduleParallel(Dependency);


                //clear neighbors
                var clearNeighborsJobHandle = Entities
                    .WithName("ClearNeighbors")
                    .WithSharedComponentFilter(settings)
                    .ForEach((ref DynamicBuffer<NeighborData> neighbors) =>
                    {
                        neighbors.Clear();
                    }).ScheduleParallel(Dependency);


                Dependency = JobHandle.CombineDependencies(Dependency, sleepJobHandle, clearNeighborsJobHandle);
                
                var parallelSpatialHashMap = spatialHashMap.AsParallelWriter();
                var parallelTagHashMap = tagHashMap.AsParallelWriter();


                var hashPositionsJobHandle = Entities
                    .WithName("BuildSpatialHashMap")
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


                var findNeighborsTagJobHandle = Entities
                    .WithName("FindNeighborsTag")
                    .WithSharedComponentFilter(settings)
                    .WithReadOnly(tagHashMap)
                    .ForEach((ref DynamicBuffer<NeighborData> neighbors, in PerceptionData perception, in AgentData agent) =>
                    {
                        if (!agent.Sleeping)
                        {
                            //Check for global search tags
                            int mask = perception.globalSearchTagMask;
                            if (mask != 0)
                            {
                                AgentData neighbor;
                                for (byte tag = 0; tag < sizeof(int); tag++)
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
                            }
                        }
                    }).ScheduleParallel(Dependency);

                Dependency = findNeighborsTagJobHandle;


                var findNeighborsJobHandle = Entities
                    .WithName("FindNeighborsSpatial")
                    .WithSharedComponentFilter(settings)
                    .WithReadOnly(spatialHashMap)
                    .ForEach((ref DynamicBuffer<NeighborData> neighbors, ref PerceptionData perception, in AgentData agent) =>
                    {
                        if (!agent.Sleeping)
                        {
                            var cells = new NativeList<int>(Allocator.Temp);

                            //check cells within perception range sphere
                            if (perception.perceptionRadius > 0)
                            {
                                int cellRange = 1 + (int)((perception.perceptionRadius - .01f) / cellSize);
                                var centerCell = new int3(math.floor(agent.Position / cellSize));

                                for (int x = centerCell.x - cellRange; x <= centerCell.x + cellRange; x++)
                                {
                                    for (int y = centerCell.y - cellRange; y <= centerCell.y + cellRange; y++)
                                    {
                                        for (int z = centerCell.z - cellRange; z <= centerCell.z + cellRange; z++)
                                        {
                                            if (x < 0 || x > dimensions_x
                                                    || y < 0 || y > dimensions_y
                                                    || z < 0 || z > dimensions_z)
                                            {
                                                continue;
                                            }
                                            var cell = (int)math.hash(new int3(x, y, z));
                                            if (!cells.Contains(cell))
                                            {
                                                cells.Add(cell);
                                            }
                                        }
                                    }
                                }
                            }


                            //check cells within lookahead seconds
                            if (perception.lookAheadSeconds > 0)
                            {
                                int3 p0 = (int3)math.floor(agent.Position/cellSize);
                                int3 p1 = (int3)math.floor((agent.Position + agent.Velocity * perception.lookAheadSeconds)/cellSize);
                                int3 delta = math.abs(p1 - p0);
                                int3 sign = (int3)math.sign(p1 - p0);                            

                                int deltaMax = math.cmax(delta);
                                p1.x = p1.y = p1.z = deltaMax / 2; /* error offset */

                                var startCell = (int)math.hash(p0);
                                if (!cells.Contains(startCell))
                                {
                                    cells.Add(startCell);
                                }


                                for (int i = deltaMax; i > 0; i--)
                                {
                                    if (dimensions_x > 0 && (p0.x < 0 || p0.x >= dimensions_x)) break;
                                    if (dimensions_y > 0 && (p0.y < 0 || p0.y >= dimensions_y)) break;
                                    if (dimensions_z > 0 && (p0.z < 0 || p0.z >= dimensions_z)) break;

                                    var lookCell = (int)math.hash(p0);
                                    if (!cells.Contains(lookCell))
                                    {
                                        cells.Add(lookCell);
                                    }

                                    if (i == 1) break;

                                    p1.x -= delta.x; if (p1.x < 0) { p1.x += deltaMax; p0.x += sign.x; }
                                    p1.y -= delta.y; if (p1.y < 0) { p1.y += deltaMax; p0.y += sign.y; }
                                    p1.z -= delta.z; if (p1.z < 0) { p1.z += deltaMax; p0.z += sign.z; }
                                }
                            }



                            int capBreak = 0;
                            AgentData neighbor;
                            for (int i = 0; i < cells.Length; i++)
                            {
                                capBreak = 0;
                                if (spatialHashMap.TryGetFirstValue(cells[i], out neighbor, out var iterator))
                                {
                                    do
                                    {
                                        //TODO find a way to avoid duplicates?
                                        if (true)//!neighbors.Contains(neighbor))
                                        {
                                            neighbors.Add(neighbor);
                                            if (!neighbor.Fill)
                                            {
                                                capBreak++;
                                            }
                                        }
                                    } while (spatialHashMap.TryGetNextValue(out neighbor, ref iterator) && capBreak < cellCap);
                                }
                            }
                        }
                    })
                    .ScheduleParallel(Dependency);

                Dependency = findNeighborsJobHandle;

                var clearPerpectionJob = Entities
                    .WithSharedComponentFilter(settings)
                    .WithName("ClearPerceptionsJob")
                    .ForEach((ref PerceptionData perception) =>
                        perception.Clear()
                    ).Schedule(Dependency);

                Dependency = clearPerpectionJob;


                Dependency = spatialHashMap.Dispose(Dependency);
                Dependency = tagHashMap.Dispose(Dependency);

                flockQuery.AddDependency(Dependency);
                flockQuery.ResetFilter();
            }
            flocks.Clear();
        
        }
    }
}
#endif