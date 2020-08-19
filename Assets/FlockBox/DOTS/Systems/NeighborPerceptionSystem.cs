using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;


namespace CloudFine.FlockBox.DOTS
{
    [UpdateInGroup(typeof(PerceptionSystemGroup))]
    public class NeighborPerceptionSystem : SystemBase
    {
        protected EntityQuery flockQuery;
        private List<FlockData> flocks;

        protected override void OnCreate()
        {
            flockQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<FlockData>() },
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

                // DO THINGS HERE






                // We pass the job handle and add the dependency so that we keep the proper ordering between the jobs
                // as the looping iterates. For our purposes of execution, this ordering isn't necessary; however, without
                // the add dependency call here, the safety system will throw an error, because we're accessing multiple
                // pieces of boid data and it would think there could possibly be a race condition.

                flockQuery.AddDependency(Dependency);
                flockQuery.ResetFilter();
            }
            flocks.Clear();
        
        }



        [BurstCompile]
        struct NeighborPerceptionJob : IJobForEach_BCC<NeighborData, PerceptionData, AgentData>
        {
            //[DeallocateOnJobCompletion]
            //[NativeDisableParallelForRestriction]
            [ReadOnly]
            public NativeHashMap<int, NativeList<AgentData>> map;
            public float cellSize;

            public void Execute(DynamicBuffer<NeighborData> neighbors, ref PerceptionData perception, ref AgentData agent)
            {
                neighbors.Clear();

                //use map to fill in b0

                perception.Clear();
            }
        }

        [BurstCompile]
        struct SpatialHashJob : IJobForEach_C<AgentData>
        {
            public NativeHashMap<int, NativeList<AgentData>> map;
            public float cellSize;

            public void Execute(ref AgentData agent)
            {
                int hash = 0;

                if (!map.ContainsKey(hash))
                {
                    map.TryAdd(hash, new NativeList<AgentData>());
                }
                map[hash].Add(agent);
            }


        }




        protected JobHandle OnUpdate(JobHandle inputDeps)
        {

            //Get all FlockBoxes
            EntityManager.GetAllUniqueSharedComponentData<FlockData>(flocks);

            //Iterate through FlockBoxes
            for (int flockIndex = 0; flockIndex < flocks.Count; flockIndex++)
            {
                FlockData flock = flocks[flockIndex];
                float cellSize = flock.Flock.CellSize;

                flockQuery.SetSharedComponentFilter(flock);

                int agentCount = flockQuery.CalculateChunkCount();
                if (agentCount == 0)
                {
                    // Early out.
                    flockQuery.ResetFilter();
                    continue;
                }


                var hashMap = new NativeMultiHashMap<int, int>(agentCount, Allocator.TempJob);



                //FILL IN SPATIAL HASH MAP
                SpatialHashJob hashJob = new SpatialHashJob
                {
                    //map = hashMap,
                    cellSize = cellSize,
                };
                inputDeps = hashJob.Schedule(flockQuery, inputDeps);


                //Fill each Agent's neighbors buffer using the completed spatial map
                NeighborPerceptionJob perceptionJob = new NeighborPerceptionJob
                {
                    //map = hashMap,
                    cellSize = cellSize,
                };
                inputDeps = perceptionJob.Schedule(flockQuery, inputDeps);
            }

            //hashMap.Dispose();
            return inputDeps;
        }
    }
}
