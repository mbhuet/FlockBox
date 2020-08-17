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

        protected override void OnCreate()
        {
            base.OnCreate();
            flockQuery = GetEntityQuery(typeof(FlockData));

        }

        protected override void OnUpdate()
        {
            return;
            //throw new System.NotImplementedException();
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
