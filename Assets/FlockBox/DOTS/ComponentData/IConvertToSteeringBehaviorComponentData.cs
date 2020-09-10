using Unity.Entities;

namespace CloudFine.FlockBox.DOTS
{
    public interface IConvertToComponentData
    {
        void AddEntityData(Entity entity, EntityManager entityManager);
        void SetEntityData(Entity entity, EntityManager entityManager);
        void RemoveEntityData(Entity entity, EntityManager entityManager);
    }

    public interface IConvertToSteeringBehaviorComponentData<T> : IConvertToComponentData where T : struct, IComponentData
    {
        T Convert();
    }
}