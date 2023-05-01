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
                            Velocity = new float3 (
                                random.NextFloat(),
                                random.NextFloat(),
                                random.NextFloat()
                                )
                        };
                        ecb.SetComponent(e, agent);

                        ecb.AddComponent(e, new FlockMatrixData { WorldToFlockMatrix = data.WorldToLocalMatrix });
                        ecb.AddComponent(e, new BoundaryData { Dimensions = data.WorldDimensions, Margin = data.boundaryBuffer, Wrap = data.wrapEdges });
                        ecb.AddSharedComponentManaged(e, new FlockData { FlockInstanceID = data.FlockBoxID });
                        /*
                        
                        Entity entity = agents[i];
                        AgentData agent = entityManager.GetComponentData<AgentData>(entity);
                        if (entityManager.HasComponent<SteeringData>(entity))
                        {
                            SteeringData steering = entityManager.GetComponentData<SteeringData>(entity);
                            agent.Velocity = UnityEngine.Random.insideUnitSphere * steering.MaxSpeed;
                        }
                        agent.Position = RandomPosition();
                        agent.UniqueID = AgentData.GetUniqueID();
                        entityManager.SetComponentData(entity, agent);

                        */
                    }
                }
            }

            _hasSpawned = true;
        }
    }
}