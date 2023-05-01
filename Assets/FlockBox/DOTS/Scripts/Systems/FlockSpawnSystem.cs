using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace CloudFine.FlockBox.DOTS
{
    public partial struct FlockSpawnSystem : ISystem
    {
        private bool _hasSpawned;

        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_hasSpawned) return;

            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (data, spawnPopulations) in SystemAPI.Query<FlockBoxData, DynamicBuffer<FlockBoxSpawnPopulationData>>())
            {
                for(int i =0; i<spawnPopulations.Length; i++)
                {                    
                    var pop = spawnPopulations[i];
                    NativeArray<Entity> entities = new NativeArray<Entity>(pop.Population, Allocator.Temp);
                    ecb.Instantiate(pop.Prefab, entities);

                    var random = new Unity.Mathematics.Random((uint)Time.realtimeSinceStartup);
                    float buffer = data.wrapEdges ? 0 : data.boundaryBuffer;
                    float3 dimensions = data.WorldDimensions;

                    for (int j = 0; j<entities.Length; j++)
                    {
                        Entity e = entities[j];
                        AgentData agent = new AgentData
                        {
                            Position = new float3(
                                random.NextFloat(buffer, dimensions.x - buffer),
                                random.NextFloat(buffer, dimensions.y - buffer),
                                random.NextFloat(buffer, dimensions.z - buffer)
                                ),
                            //TODO scale to maxSpeed
                            Velocity = new float3 (
                                random.NextFloat(),
                                random.NextFloat(),
                                random.NextFloat()
                                ),
                            UniqueID = AgentData.TakeNextId()
                        };

                        //TODO this overwrites tag, fill, radius, etc
                        ecb.SetComponent(e, agent);

                        ecb.AddComponent(e, new FlockMatrixData { WorldToFlockMatrix = data.WorldToLocalMatrix });
                        ecb.AddComponent(e, new BoundaryData { Dimensions = data.WorldDimensions, Margin = data.boundaryBuffer, Wrap = data.wrapEdges });
                        ecb.AddSharedComponentManaged(e, new FlockData { FlockInstanceID = data.FlockBoxID });
                    }
                }
            }

            _hasSpawned = true;
        }
    }
}