using UnityEngine;
using UnityEngine.Serialization;

#if FLOCKBOX_DOTS
using Unity.Transforms;
using CloudFine.FlockBox.DOTS;
using Unity.Entities;
#endif

namespace CloudFine.FlockBox
{
    public class ExternalAgent : Agent
    {
#pragma warning disable 0649
        [SerializeField, FormerlySerializedAs("neighborhood"), HideInInspector, System.Obsolete("use Agent.FlockBox")]
        private FlockBox _autoJoinFlockBox;
#pragma warning restore 0649
#if FLOCKBOX_DOTS
        private Entity _synchedEntity;
#endif

        protected void OnValidate()
        {
#pragma warning disable 0618 
            if (FlockBox == null && _autoJoinFlockBox != null) FlockBox = _autoJoinFlockBox;
#pragma warning restore 0618
#if FLOCKBOX_DOTS
            RefreshSyncedEntityData();
#endif
        }

#if FLOCKBOX_DOTS

        public void RefreshSyncedEntityData()
        {
            if (_synchedEntity != Entity.Null)
            {
                World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData<AgentData>(_synchedEntity, ConvertToAgentData());
            }
        }

        protected override void OnJoinFlockBox(FlockBox flockBox)
        {
            if (flockBox.DOTSEnabled)
            {
                EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                if (_synchedEntity == Entity.Null)
                {
                    _synchedEntity = entityManager.CreateEntity();

                    entityManager.AddComponentData<LocalToWorld>(_synchedEntity, new LocalToWorld());
                    entityManager.AddComponentObject(_synchedEntity, this.transform);
                    entityManager.AddComponentData<CopyTransformFromGameObject>(_synchedEntity, new CopyTransformFromGameObject { });

                    entityManager.AddSharedComponentData<FlockData>(_synchedEntity, new FlockData { Flock = flockBox });
                    entityManager.AddComponentData<FlockMatrixData>(_synchedEntity, new FlockMatrixData { WorldToFlockMatrix = transform.worldToLocalMatrix });
                    entityManager.AddComponentData<AgentData>(_synchedEntity, ConvertToAgentData());
                }
                else
                {
                    entityManager.SetSharedComponentData<FlockData>(_synchedEntity, new FlockData { Flock = flockBox });
                    entityManager.SetComponentData<FlockMatrixData>(_synchedEntity, new FlockMatrixData { WorldToFlockMatrix = transform.worldToLocalMatrix });
                    entityManager.SetComponentData<AgentData>(_synchedEntity, ConvertToAgentData());
                }
            }
        }
#endif

    }
}
