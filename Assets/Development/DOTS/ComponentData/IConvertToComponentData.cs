using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public interface IConvertToComponentData
{
    void AddEntityData(Entity entity, EntityManager entityManager);
    void EntityCommandBufferAdd(Entity entity, EntityCommandBuffer buf);
    void EntityCommandBufferRemove(Entity entity, EntityCommandBuffer buf);
    void EntityCommandBufferSet(Entity entity, EntityCommandBuffer buf);
}

public interface IConvertToComponentData<T> : IConvertToComponentData where T : struct, IComponentData
{
    T Convert();
}
