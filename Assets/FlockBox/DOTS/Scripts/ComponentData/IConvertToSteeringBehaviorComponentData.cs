#if FLOCKBOX_DOTS
using Unity.Entities;

namespace CloudFine.FlockBox.DOTS
{
    public interface IConvertToComponentData
    {
        bool HasEntityData(Entity entity, EntityManager entityManager);
        void AddEntityData(Entity entity, EntityManager entityManager);
        void SetEntityData(Entity entity, EntityManager entityManager);
        void RemoveEntityData(Entity entity, EntityManager entityManager);
    }

    public interface IConvertToSteeringBehaviorComponentData<T> : IConvertToComponentData where T : unmanaged, IComponentData
    {
        T Convert();

        bool HasEntityData(Entity entity, EntityManager entityManager) => IConvertToComponentDataExtension.HasEntityData(this, entity, entityManager);

    }
}
#endif