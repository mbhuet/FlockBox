using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
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

                    for(int j = 0; j<entities.Length; j++)
                    {
                        Entity e = entities[j];
                        AgentData agent = new AgentData
                        {
                            Position = Vector3.zero,
                            Velocity = Vector3.forward,
                            Forward = Vector3.forward,
                        };
                        ecb.SetComponent(e, agent);

                        ecb.AddComponent(e, new FlockMatrixData { });
                        ecb.AddComponent(e, new BoundaryData { Dimensions = data.WorldDimensions, Margin = data.boundaryBuffer, Wrap = data.wrapEdges });
                        ecb.AddComponent(e, new DummyData { });

                        ecb.AddSharedComponentManaged(e, new FlockData { });
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

                        entityManager.AddComponentData<FlockMatrixData>(entity, new FlockMatrixData { WorldToFlockMatrix = transform.worldToLocalMatrix });
                        entityManager.AddSharedComponentManaged<FlockData>(entity, new FlockData { FlockInstanceID = this.GetFlockBoxID() });
                        entityManager.AddComponentData<FlockMatrixData>(entity, new FlockMatrixData {WorldToFlockMatrix = transform.worldToLocalMatrix });
                        entityManager.AddComponentData<BoundaryData>(entity, new BoundaryData { Dimensions = WorldDimensions, Margin = boundaryBuffer, Wrap = wrapEdges });
                        
                        */
                    }
                }
            }

            _hasSpawned = true;
        }
    }
}