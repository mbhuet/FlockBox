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
        [SerializeField, FormerlySerializedAs("neighborhood"), HideInInspector, System.Obsolete("use Agent.FlockBox")]
        private FlockBox _autoJoinFlockBox;
#if FLOCKBOX_DOTS
        private Entity _synchedEntity;
#endif

        protected void OnValidate()
        {
            if (FlockBox == null && _autoJoinFlockBox != null) FlockBox = _autoJoinFlockBox;
        }

        protected override void OnJoinFlockBox(FlockBox flockBox)
        {
#if FLOCKBOX_DOTS
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
#endif
        }
    }
}
