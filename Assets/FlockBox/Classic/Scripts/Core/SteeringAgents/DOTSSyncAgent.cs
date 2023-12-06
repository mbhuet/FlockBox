using UnityEngine;
using UnityEngine.Serialization;

#if FLOCKBOX_DOTS
using Unity.Transforms;
using CloudFine.FlockBox.DOTS;
using Unity.Entities;
#endif

namespace CloudFine.FlockBox
{
    public class DOTSSyncAgent : Agent
    {
#if FLOCKBOX_DOTS

        private Entity _synchedEntity;

        protected void OnValidate()
        {
            RefreshSyncedEntityData();
        }

        /// <summary>
        /// Must be called when any of this Agent's properties have changed. Will update data on the synched Entity.
        /// </summary>
        public void RefreshSyncedEntityData()
        {
            if (_synchedEntity != Entity.Null)
            {
                World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData<AgentData>(_synchedEntity, ConvertToAgentData());
            }
        }

        protected override void OnJoinFlockBox(FlockBox flockBox)
        {
            //if (flockBox.DOTSEnabled)
            {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (_synchedEntity == Entity.Null)
                {
                    _synchedEntity = entityManager.CreateEntity();

                    entityManager.AddComponentData<LocalToWorld>(_synchedEntity, new LocalToWorld());
                    //TODO replace with new system to update from a gameobject
                    //entityManager.AddComponentObject(_synchedEntity, this.transform);
                    //entityManager.AddComponentData<CompanionLink>(_synchedEntity, new CompanionLink { });

                    entityManager.AddSharedComponentManaged<FlockData>(_synchedEntity, new FlockData { FlockInstanceID = flockBox.GetFlockBoxID() });
                    entityManager.AddComponentData<FlockMatrixData>(_synchedEntity, new FlockMatrixData { WorldToFlockMatrix = transform.worldToLocalMatrix });
                    entityManager.AddComponentData<AgentData>(_synchedEntity, ConvertToAgentData());
                }
                else
                {
                    entityManager.SetSharedComponentManaged<FlockData>(_synchedEntity, new FlockData { FlockInstanceID = flockBox.GetFlockBoxID() });
                    entityManager.SetComponentData<FlockMatrixData>(_synchedEntity, new FlockMatrixData { WorldToFlockMatrix = transform.worldToLocalMatrix });
                    entityManager.SetComponentData<AgentData>(_synchedEntity, ConvertToAgentData());
                }
            }
        }
#endif

    }
}
