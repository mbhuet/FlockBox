#if FLOCKBOX_DOTS
using Unity.Entities;

namespace CloudFine.FlockBox.DOTS
{
    public static class IConvertToComponentDataExtension
    {
        public static bool HasEntityData<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            return (entityManager.HasComponent<T>(entity));
        }
        public static void AddEntityData<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            entityManager.AddComponentData<T>(entity, value.Convert());
        }
        public static void SetEntityData<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            entityManager.SetComponentData<T>(entity, value.Convert());
        }
        public static void RemoveEntityData<T>(this IConvertToSteeringBehaviorComponentData<T> value, EntityQuery query, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            entityManager.RemoveChunkComponentData<T>(query);
        }
        public static void RemoveEntityData<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, EntityManager entityManager) where T : unmanaged, IComponentData
        {
            entityManager.RemoveComponent<T>(entity);
        }
        public static void EntityCommandBufferAdd<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, EntityCommandBuffer buf) where T : unmanaged, IComponentData
        {
            buf.AddComponent<T>(entity);
        }
        public static void EntityCommandBufferRemove<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, EntityCommandBuffer buf) where T : unmanaged, IComponentData
        {
            buf.RemoveComponent<T>(entity);
        }
        public static void EntityCommandBufferSet<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, EntityCommandBuffer buf) where T : unmanaged, IComponentData
        {
            buf.SetComponent<T>(entity, value.Convert());
        }

        public static void AddEntityData<T>(this IConvertToSteeringBehaviorComponentData<T> value, Entity entity, IBaker baker) where T : unmanaged, IComponentData
        {
            baker.AddComponent(entity, value.Convert());
        }
    }
}
#endif
