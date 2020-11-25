using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CloudFine.FlockBox.DOTS
{
    /// <summary>
    /// This System demonstrates how ComponentData can be read to determine the correct state for an Agent Entity, and apply new BehaviorSettings to that Entity
    /// 
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class AgentStateDemoSystem : SystemBase
    {
        private BehaviorSettings stateASettings;
        private BehaviorSettings stateBSettings;

        protected override void OnCreate()
        {
            Addressables.LoadAssetAsync<BehaviorSettings>("StateDemoA").Completed += OnLoadDoneA;
            Addressables.LoadAssetAsync<BehaviorSettings>("StateDemoB").Completed += OnLoadDoneB;
        }

        private void OnLoadDoneA(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<BehaviorSettings> obj)
        {
            stateASettings = obj.Result;
        }
        private void OnLoadDoneB(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<BehaviorSettings> obj)
        {
            stateBSettings = obj.Result;
        }

        protected override void OnUpdate()
        {
            Dependency = Entities
                .ForEach((ref AgentStateData stateData, in AgentData agentData, in BoundaryData boundaryData) =>
                {
                    //this example will change an agent's state based on whether it is in the top or bottom half of the boundary
                    int targetState = agentData.Position.y > boundaryData.Dimensions.y / 2f ? 1 : 0;
                    if(stateData.State != targetState)
                    {
                        stateData.State = targetState;
                    }
                })
                .ScheduleParallel(Dependency);

            Entities
                .WithStructuralChanges() //changing behaviors will always require structural changes
                .WithChangeFilter<AgentStateData>() //only apply behaviors to entities whose state data has changed.
                .ForEach((Entity entity, in AgentStateData stateData) =>
                {
                    if(stateData.State == 0)
                    {
                        if (stateASettings)
                        {
                            stateASettings.ApplyToEntity(entity, EntityManager);
                        }
                    }
                    if(stateData.State == 1)
                    {
                        if (stateBSettings)
                        {
                            Debug.Log("here B");
                            stateBSettings.ApplyToEntity(entity, EntityManager);
                        }
                    }
                })
                .Run();     
        }
    }
}
